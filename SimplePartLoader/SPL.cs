using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    public class SPL
    {
        static bool ENABLE_DEBUG;

        public static void LoadPart(AssetBundle bundle, string prefabName)
        {
            if (!bundle)
                throw new Exception("Tried to create a part without valid AssetBundle");

            if (String.IsNullOrWhiteSpace(prefabName))
                throw new Exception("Tried to create a part without prefab name");

            if (Saver.modParts.ContainsKey(prefabName))
                throw new Exception($"Tried to create an already existing prefab ({prefabName})");

            GameObject prefab = bundle.LoadAsset<GameObject>(prefabName);
            if (!prefab)
                throw new Exception($"Tried to create a prefab but it was not found in the AssetBundle ({prefabName})");

            CarProperties prefabCarProp = prefab.GetComponent<CarProperties>();
            Partinfo prefabPartInfo = prefab.GetComponent<Partinfo>();
            if (!prefabCarProp || !prefabPartInfo)
                throw new Exception("An essential component is missing!");

            prefab.AddComponent<Pickup>().canHold = true;
            prefab.AddComponent<DISABLER>();


        }

        public static void SetupTransparent(Part part, string attachesTo, Vector3 transparentLocalPos, Vector3 transaprentLocalRot)
        {

        }

        internal static void SendDebugMessage(string message)
        {
            if(ENABLE_DEBUG)
                Debug.LogError(message);
        }
    }
}
