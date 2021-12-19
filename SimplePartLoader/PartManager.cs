using Assets.SimpleLocalization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimplePartLoader
{
    internal class PartManager
    {
        public static List<Part> modLoadedParts = new List<Part>();

        static bool firstLoad = false;

        public static void OnLoadCalled()
        {
            // Parts catalog - We need to first add our custom parts into the Junkyard part list since the parts catalog uses it as reference.
            GameObject junkyardListParent = GameObject.Find("PartsParent");
            JunkPartsList jpl = junkyardListParent.GetComponent<JunkPartsList>();
            int sizeBeforeModify = jpl.Parts.Length;

            Array.Resize(ref jpl.Parts, sizeBeforeModify + modLoadedParts.Count); // We resize the array only once.

            // Now we add our transparents into the game

        }

        internal static GameObject GetTransparentReadyObject(Part p, TransparentData t)
        {
            GameObject transparentObject = GameObject.CreatePrimitive(PrimitiveType.Cube);

            GameObject.Destroy(transparentObject.GetComponent<BoxCollider>());

            transparentObject.name = p.PartInfo.RenamedPrefab; // Renamed prefab is the one that the game uses for looking for transparent. Prefab name in car props is used for identify which prefab has to be loaded.

            transparentObject.transform.localPosition = t.LocalPos;
            transparentObject.transform.localScale = t.Scale;
            transparentObject.transform.localRotation = t.LocalRot;

            transparentObject.tag = "transparentpart";
            transparentObject.layer = LayerMask.NameToLayer("TransparentParts");

            transparents transparentComponent = transparentObject.AddComponent<transparents>();
            // We add dummy data so the component doesn't crash.
            transparentComponent.ATTACHABLES = new transparents.AttachingObjects[0];
            transparentComponent.DEPENDANTS = new transparents.dependantObjects[0];

            if (p.TestingEnabled)
                transparentObject.AddComponent<TransparentEdit>().transparentData = t;

            return transparentObject;
        }
    }
}
