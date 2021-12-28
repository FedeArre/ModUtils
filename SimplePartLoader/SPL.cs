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
                HexNut hx = childs[i].GetComponent<HexNut>();

                if (hx || childs[i].GetComponent<FlatNut>())
                {
                    childs[i].gameObject.AddComponent<CarProperties>();
                    childs[i].gameObject.AddComponent<DISABLER>();

                    childs[i].gameObject.layer = LayerMask.NameToLayer(hx ? "Bolts" : "FlatBolts"); // Add bolts if they have HexNut component or FlatBolts if has FlatNut component.
                    if (!childs[i].GetComponent<BoxCollider>())
                        childs[i].gameObject.AddComponent<BoxCollider>();
                }
            }

            Part p = new Part(prefab, prefabCarProp, prefabPartInfo);
            PartManager.modLoadedParts.Add(p);

            Saver.modParts.Add(p.CarProps.PrefabName, prefab);

            GameObject.DontDestroyOnLoad(prefab); // We make sure that our prefab is not deleted in the first scene change

            return p; // We provide the Part instance so the developer can setup the transparents
        }

        public static void SetupCarPartFromDummy(Part p, string partName, string carName)
        {
            // We first delete all the components from our part.
            foreach (Component comp in p.Prefab.GetComponents<Component>())
            {
                if(!(comp is Transform))
                {
                    GameObject.Destroy(comp);
                }
            }

            // Then we look up for the car part and store it
            GameObject carPart = null, carsParent = GameObject.Find("CarsParent");
            foreach(GameObject car in carsParent.GetComponent<CarList>().Cars)
            {
                if(car.name == carName)
                {
                    Transform[] childs = car.transform.GetComponentsInChildren<Transform>();
                    foreach(Transform child in childs)
                    {
                        if(child.name == partName)
                        {
                            carPart = child.gameObject;
                            break;
                        }
                    }
                }
            }

            if (!carPart) return;

            // Now we copy all the components from the car part into the prefab
            foreach (Component comp in carPart.GetComponents<Component>())
            {
                if (!(comp is Transform))
                {
                    CopyComponent(comp, p.Prefab);
                }
            }
        }

        internal static Component CopyComponent(Component original, GameObject destination)
        {
            System.Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            // Copied fields can be restricted with BindingFlags
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy;
        }

        /*public enum Language
        {
            English,
            Portuguese,
            German,
            Russian,
            Hungarian,
            French,
            Spanish,
            Polish,
            Swedish,
            Czech
        }

        internal static string GetLanguageByType(Language lang)
        {
            switch (lang)
            {
                case Language.English:
                    return "English";

                case Language.Portuguese:
                    return "Portuguese";

                case Language.German:
                    return "German";

                case Language.Russian:
                    return "Russian";

                case Language.Hungarian:
                    return "Hungarian";

                case Language.French:
                    return "Francais";

                case Language.Spanish:
                    return "Español";

                case Language.Polish:
                    return "Polish";

                case Language.Swedish:
                    return "Swedish";

                case Language.Czech:
                    return "Čeština";

                default:
                    return "English";
            }
        }*/
    }
}
