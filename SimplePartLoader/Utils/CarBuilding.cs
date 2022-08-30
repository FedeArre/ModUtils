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
                prefab.AddComponent(comp.GetType()).GetCopyOf(comp, true);
            }

            AttachPrefabChilds(prefab, originalCar); // Call the recursive function that copies all the child hierarchy.

            Debug.LogError($"[ModUtils/CarBuilding]: Car {originalCar.name} cloned to prefab");
        }

        public static void CopyPartIntoTransform(GameObject partToAdd, Transform location)
        {
            DevLog($"Copying part {partToAdd.name} into {location.name}");

            GameObject addedPart = new GameObject();
            addedPart.transform.SetParent(location);

            addedPart.name = partToAdd.name;
            addedPart.layer = partToAdd.layer;
            addedPart.tag = partToAdd.tag;

            addedPart.transform.localPosition = Vector3.zero;
            addedPart.transform.localRotation = Quaternion.identity;
            addedPart.transform.localScale = Vector3.one;

            foreach (Component comp in partToAdd.GetComponents<Component>())
            {
                if (comp is P3dPaintable || comp is P3dPaintableTexture || comp is P3dChangeCounter || comp is P3dMaterialCloner || comp is P3dColorCounter || comp is Transform)
                    continue;

                DevLog($"Now copying component to added part ({comp})");
                addedPart.AddComponent(comp.GetType()).GetCopyOf(comp, true);
            }
            
            AttachPrefabChilds(addedPart, partToAdd);
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
                        childObject.AddComponent(comp.GetType()).GetCopyOf(comp, true);

                        DevLog("Copying component " + comp.GetType());
                    }
                    else
                    {
                        Functions.CopyComponentData(childObject.GetComponent(comp.GetType()), original.transform.GetChild(i).GetComponent(comp.GetType()), true);

                        DevLog("Cloning component" + comp.GetType());
                    }
                }

                if (original.transform.GetChild(i).childCount != 0)
                    AttachPrefabChilds(childObject, original.transform.GetChild(i).gameObject);
            }
        }

        public static void UpdateTransparentsReferences(GameObject p)
        {
            bool referenceUpdated = false;
            foreach (transparents t in p.GetComponentsInChildren<transparents>())
            {
                if(t.DEPENDANTS != null && t.DEPENDANTS.Length > 0)
                {
                    transparents.dependantObjects[] newDependants = new transparents.dependantObjects[t.DEPENDANTS.Length];
                    for(int i = 0; i < t.DEPENDANTS.Length; i++)
                    {
                        transparents.dependantObjects dp = t.DEPENDANTS[i];
                        newDependants[i] = new transparents.dependantObjects();
                        referenceUpdated = false;
                        
                        foreach (transparents t2 in p.GetComponentsInChildren<transparents>())
                        {
                            if(t2 == null)
                            {
                                continue;
                            }

                            if(dp.dependant == null)
                            {
                                continue;
                            }
                            if (t2.name == dp.dependant.name)
                            {
                                newDependants[i].dependant = t2.gameObject;
                                referenceUpdated = true;
                                break;
                            }
                        }
                        if (!referenceUpdated)
                        {
                            if(dp.dependant == null)
                                Debug.LogError("[ModUtils/CarBuilding/Error]: Dependant object (null) not found in " + p.name + ", on part " + t.name);
                            else
                                Debug.LogError("[ModUtils/CarBuilding/Error]: Dependant object " + dp.dependant.name + " not found in " + p.name + ", on part " + t.name);
                        }
                    }
                    t.DEPENDANTS = newDependants;
                }
                /*
                Debug.Log("now ttacables");
                if (t.ATTACHABLES != null && t.ATTACHABLES.Length > 0)
                {
                    transparents.AttachingObjects[] newAttachables = new transparents.AttachingObjects[t.ATTACHABLES.Length];
                    Debug.Log("ENTERING");
                    for (int i = 0; i < t.ATTACHABLES.Length; i++)
                    {
                        Debug.Log("in for");
                        transparents.AttachingObjects dp = t.ATTACHABLES[i];
                        newAttachables[i] = new transparents.AttachingObjects();
                        referenceUpdated = false;

                        foreach (transparents t2 in p.GetComponentsInChildren<transparents>())
                        {
                            Debug.Log(dp.Attachable);
                            Debug.Log(newAttachables[i]);
                            Debug.Log(newAttachables[i].Attachable);
                            if (t2 == null)
                            {
                                Debug.Log("t2 is null");
                                continue;
                            }

                            if (dp.Attachable == null)
                            {
                                Debug.Log("dp.attachable is null");
                                continue;
                            }

                            if (t2.name == dp.Attachable.name)
                            {
                                newAttachables[i].Attachable = t2.gameObject;
                                referenceUpdated = true;
                                break;
                            }
                        }
                        Debug.Log("EXIT, CODE" + referenceUpdated);
                        if (!referenceUpdated)
                        {
                            if (dp.Attachable == null)
                                Debug.LogError("[ModUtils/CarBuilding/Error]: Attachable object (null) not found in " + p.name);
                            else
                                Debug.LogError("[ModUtils/CarBuilding/Error]: Attachable object " + dp.Attachable.name + " not found in " + p.name);
                        }
                    }
                    Debug.Log("main exit setting it off");
                    t.ATTACHABLES = newAttachables;
                }
                Debug.Log("bye");*/

            }
        }

        internal static void DevLog(string str)
        {
            if (ENABLE_CARBUILDING_DEBUG)
                Debug.Log("[ModUtils/CarBuilding]: " + str);
        }
    }
}
