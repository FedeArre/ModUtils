using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimplePartLoader
{
    public class PartExtendedCalls : MonoBehaviour
    {
        public Part OwnerPart { get; internal set; }

        public bool IsAttached { get; private set; }
        public GameObject AttachedTo { get; private set; }

        private static readonly Dictionary<Part, Action<PartExtendedCalls, GameObject>> OnAttachedHandlers = new Dictionary<Part, Action<PartExtendedCalls, GameObject>>();
        private static readonly Dictionary<Part, Action<PartExtendedCalls, GameObject>> OnDeattachedHandlers = new Dictionary<Part, Action<PartExtendedCalls, GameObject>>();

        void Start()
        {
            Debug.Log("PartExtendedCall start, awaiting 5 frames to check current status of " + name);
            StartCoroutine(DelayedStart());
        }

        IEnumerator DelayedStart()
        {
            yield return 0;
            yield return 0;
            yield return 0;
            yield return 0;
            yield return 0;

            // 5 frames later, we check if we are attached to something
            if(transform.parent != null && transform.parent.GetComponent<transparents>())
            {
                Debug.Log("PartExtendedCall detected attachment on start for " + name);
                NotifyAttached(gameObject);
            }
        }
        public static void RegisterOnAttached(Part part, Action<PartExtendedCalls, GameObject> callback)
        {
            AddHandler(OnAttachedHandlers, part, callback);
        }

        public static void UnregisterOnAttached(Part part, Action<PartExtendedCalls, GameObject> callback)
        {
            RemoveHandler(OnAttachedHandlers, part, callback);
        }

        public static void RegisterOnDeattached(Part part, Action<PartExtendedCalls, GameObject> callback)
        {
            AddHandler(OnDeattachedHandlers, part, callback);
        }

        public static void UnregisterOnDeattached(Part part, Action<PartExtendedCalls, GameObject> callback)
        {
            RemoveHandler(OnDeattachedHandlers, part, callback);
        }


        internal void NotifyAttached(GameObject target)
        {
            Debug.Log("NotifyAttached called for " + name + " to target " + target.name);
            Debug.Log("Current IsAttached: " + IsAttached + ", AttachedTo: " + (AttachedTo != null ? AttachedTo.name : "null"));
            if (IsAttached && AttachedTo == target)
                return;

            IsAttached = true;
            AttachedTo = target;
            InvokeHandler(OnAttachedHandlers, target);
        }

        internal void NotifyDetached(GameObject target)
        {
            Debug.Log("NotifyDetached called for " + name + " from target " + target.name);

            if (!IsAttached)
                return;

            var previousTarget = AttachedTo;
            IsAttached = false;
            AttachedTo = null;
            InvokeHandler(OnDeattachedHandlers, target);
        }

        private void InvokeHandler(Dictionary<Part, Action<PartExtendedCalls, GameObject>> handlers, GameObject target)
        {
            if (OwnerPart == null)
                return;

            Action<PartExtendedCalls, GameObject> handler;
            if (handlers.TryGetValue(OwnerPart, out handler))
            {
                handler?.Invoke(this, target);
            }
        }

        private static void AddHandler(Dictionary<Part, Action<PartExtendedCalls, GameObject>> handlers, Part part, Action<PartExtendedCalls, GameObject> callback)
        {
            if (part == null || callback == null)
                return;

            Action<PartExtendedCalls, GameObject> existing;
            if (handlers.TryGetValue(part, out existing))
            {
                handlers[part] = existing + callback;
            }
            else
            {
                handlers.Add(part, callback);
            }
        }

        private static void RemoveHandler(Dictionary<Part, Action<PartExtendedCalls, GameObject>> handlers, Part part, Action<PartExtendedCalls, GameObject> callback)
        {
            if (part == null || callback == null)
                return;

            Action<PartExtendedCalls, GameObject> existing;
            if (!handlers.TryGetValue(part, out existing))
                return;

            existing -= callback;
            if (existing == null)
                handlers.Remove(part);
            else
                handlers[part] = existing;
        }

    }
}
