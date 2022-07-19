using PaintIn3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.Utils
{
    public class CarBuilding
    {
        public static bool ENABLE_CARBUILDING_DEBUG = false;

        public static void CopyCarToPrefab(GameObject originalCar, GameObject prefab)
        {
            prefab.layer = originalCar.layer;

            foreach (Component comp in originalCar.GetComponents<Component>())
            {
                if (comp is P3dPaintable || comp is P3dPaintableTexture || comp is P3dChangeCounter || comp is P3dMaterialCloner || comp is P3dColorCounter || comp is Transform)
                    continue;

                DevLog($"Now copying component to base object ({comp})");
                prefab.AddComponent(comp.GetType()).GetCopyOf(comp);
            }

            AttachPrefabChilds(prefab, originalCar); // Call the recursive function that copies all the child hierarchy.

            Debug.LogError($"[ModUtils]: Car {originalCar.name} cloned to prefab");
        }


        public static void AttachPrefabChilds(GameObject partToAttach, GameObject original)
        {
            DevLog("Attaching childs to " + partToAttach.name);

            // Now we also do the same for the childs of the object.
            for (int i = 0; i < original.transform.childCount; i++)
            {
                DevLog("Attaching " + original.transform.GetChild(i).name);
                GameObject childObject = new GameObject();
                childObject.transform.SetParent(partToAttach.transform);

                childObject.name = original.transform.GetChild(i).name;
                childObject.layer = original.transform.GetChild(i).gameObject.layer;
                childObject.tag = original.transform.GetChild(i).tag;

                childObject.transform.localPosition = original.transform.GetChild(i).localPosition;
                childObject.transform.localRotation = original.transform.GetChild(i).localRotation;
                childObject.transform.localScale = original.transform.GetChild(i).localScale;

                foreach (Component comp in original.transform.GetChild(i).GetComponents<Component>())
                {
                    if (comp is Transform || comp == null)
                        continue;

                    if (!childObject.GetComponent(comp.GetType()))
                    {
                        childObject.AddComponent(comp.GetType()).GetCopyOf(comp);

                        DevLog("Copying component " + comp.GetType());
                    }
                    else
                    {
                        Functions.CopyComponentData(childObject.GetComponent(comp.GetType()), original.transform.GetChild(i).GetComponent(comp.GetType()));

                        DevLog("Cloning component" + comp.GetType());
                    }
                }

                if (original.transform.GetChild(i).childCount != 0)
                    AttachPrefabChilds(childObject, original.transform.GetChild(i).gameObject);
            }
        }

        internal static void DevLog(string str)
        {
            if (ENABLE_CARBUILDING_DEBUG)
                Debug.Log("[ModUtils-CarBuilding]: " + str);
        }
    }
}
