using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace SimplePartLoader.Features
{
    public class SharedRaycasts
    {
        private SharedRaycasts() { }

        internal static List<SharedRaycast> ExistingRequests = new List<SharedRaycast>();
        internal static bool EnableSharedRaycasting = false;

        public static SharedRaycastCallData Add(LayerMask layerMask, float maxDistance, Action<RaycastHit> callback)
        {
            if(callback == null)
            {
                Debug.LogError("[ModUtils/SharedRaycasting/Error]: Null callback was provided!");
                return null;
            }

            EnableSharedRaycasting = true;

            SharedRaycastCallData srcd = new SharedRaycastCallData(maxDistance, callback);
            foreach(var req in ExistingRequests)
            {
                if(req.LayerMask == layerMask)
                {
                    req.Subscribed.Add(srcd);
                    return srcd;
                }
            }

            SharedRaycast sr = new SharedRaycast();
            sr.LayerMask = layerMask;
            sr.Subscribed = new List<SharedRaycastCallData> { srcd };
            return srcd;
        }

        internal static void Update()
        {
            foreach(var req in ExistingRequests)
            {
                RaycastHit rcHit;
                if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.position, out rcHit, 20f, req.LayerMask))
                {
                    foreach(var suscriber in req.Subscribed)
                    {
                        if(rcHit.distance <= suscriber.MaxDistance)
                        {
                            try
                            {
                                suscriber.Callback.Invoke(rcHit);
                            }
                            catch(Exception ex)
                            {
                                Debug.LogError("[ModUtils/SharedRaycasting/Error]: An exception occurred on SharedRaycasting, details below");
                                Debug.LogError($"[ModUtils/SharedRaycasting/Error]: {ex.Message} / {ex.Source}");
                                Debug.LogError($"[ModUtils/SharedRaycasting/Error]: Inner: {ex.InnerException}");
                                Debug.LogError($"[ModUtils/SharedRaycasting/Error]: StackTrace: {ex.StackTrace}");
                            }
                        }
                    }
                }
            }
        }
    }

    internal class SharedRaycast
    {
        public LayerMask LayerMask;
        public List<SharedRaycastCallData> Subscribed;
    }

    public class SharedRaycastCallData
    {
        internal float MaxDistance;
        internal Action<RaycastHit> Callback;

        internal SharedRaycastCallData(float distance, Action<RaycastHit> callback)
        {
            MaxDistance = distance;
            Callback = callback;
        }
    }
}
