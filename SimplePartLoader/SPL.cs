using System;
using UnityEngine;

namespace SimplePartLoader
{
    public class SPL
    {
        /// <summary>
        /// Adds a prefab as a car part into the game
        /// </summary>
        /// <param name="bundle">The bundle in which the prefab is located. Has to be loaded!</param>
        /// <param name="prefabName">The name of the prefab to be loaded</param>
        /// <exception cref="Exception">An exception will be thrown if the bundle or prefabName are invalid, if the prefab already exists or if essential components are missing</exception>
        /// <returns></returns>
        public static Part LoadPart(AssetBundle bundle, string prefabName)
        {
            // Safety checks
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

            // Automatically add some components and also assign the correct layer.
            // Pickup and DISABLER for the part - Required so they work properly!
            // Also add CarProperties to all nuts of the part, unexpected behaviour can happen if the component is missing.
            prefab.layer = LayerMask.NameToLayer("Ignore Raycast");

            Pickup prefabPickup = prefab.AddComponent<Pickup>();
            prefabPickup.canHold = true;
            prefabPickup.tempParent = GameObject.Find("hand");
            prefabPickup.SphereCOl = GameObject.Find("SphereCollider");

            prefab.AddComponent<DISABLER>();

            prefabCarProp.PREFAB = prefab; // Saving will not work without this due to a condition located in Saver.Save()
            Transform[] childs = prefab.GetComponentsInChildren<Transform>();
            for(int i = 0; i < childs.Length; i++)
            {
                Debug.LogError(childs[i].name);
                HexNut hx = childs[i].GetComponent<HexNut>();

                if (hx || childs[i].GetComponent<FlatNut>())
                {
                    childs[i].gameObject.AddComponent<CarProperties>();
                    childs[i].gameObject.layer = LayerMask.NameToLayer(hx ? "Bolts" : "FlatBolts"); // Add bolts if they have HexNut component or FlatBolts if has FlatNut component.
                }
            }

            Part p = new Part(prefab, prefabCarProp, prefabPartInfo);
            PartManager.modLoadedParts.Add(p);

            Saver.modParts.Add(p.CarProps.PrefabName, prefab);

            GameObject.DontDestroyOnLoad(prefab); // We make sure that our prefab is not deleted in the first scene change

            return p; // We provide the Part instance so the developer can setup the transparents
        }

        public static void EnableTransparentEditor()
        {
            ModMain.IsTransparentEditingEnabled = true;
        }
    }
}
