using PaintIn3D;
using SimplePartLoader.CarGen;
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
        /// <summary>
        /// Enables the debug mode for car building functions
        /// </summary>
        public static bool ENABLE_CARBUILDING_DEBUG = false;

        /// <summary>
        /// Copies the car from the given car prefab to the given car object
        /// </summary>
        /// <param name="originalCar">The original car</param>
        /// <param name="prefab">The prefab that will store the car clone</param>
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

            Debug.Log($"[ModUtils/CarBuilding]: Car {originalCar.name} cloned to prefab");
        }

        /// <summary>
        /// Copies the given GameObject into the specified transform as child of it
        /// </summary>
        /// <param name="partToAdd">The part to add</param>
        /// <param name="location">The trasnform that will be the parent of the part</param>
        public static void CopyPartIntoTransform(GameObject partToAdd, Transform location)
        {
            DevLog($"Copying part {partToAdd.name} into {location.name}");

            GameObject addedPart = new GameObject();
            addedPart.transform.SetParent(location);

            addedPart.name = partToAdd.name;
            addedPart.layer = partToAdd.layer;
            addedPart.tag = partToAdd.tag;

            addedPart.SetActive(partToAdd.activeSelf);

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

        /// <summary>
        /// Recursive function that copies all the child hierarchy from a car part into a dummy part.
        /// </summary>
        /// <param name="partToAttach">The parent (top on hierarchy) GameObject that get the clones</param>
        /// <param name="original">The original part to be copied</param>
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

                childObject.SetActive(original.activeSelf); // EXPERIMENTAL!

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

        /// <summary>
        /// Updates all the DEPENDANTS and ATTACHABLES on the given GameObject
        /// </summary>
        /// <param name="p">The GameObject that will be updated</param>
        public static void UpdateTransparentsReferences(GameObject p, bool ignoreErrors = false)
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
                        
                        if(dp == null || dp.dependant == null)
                        {
                            continue;
                        }

                        int savePosition = dp.dependant.GetComponent<transparents>().SavePosition;
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
                            if (t2.name == dp.dependant.name && t2.SavePosition == savePosition)
                            {
                                newDependants[i].dependant = t2.gameObject;
                                referenceUpdated = true;
                                break;
                            }
                        }
                        if (!referenceUpdated && !ignoreErrors)
                        {
                            if(dp.dependant == null)
                                Debug.LogError("[ModUtils/CarBuilding/Error]: Dependant object (null) not found in " + p.name + ", on part " + t.name);
                            else
                                Debug.LogError("[ModUtils/CarBuilding/Error]: Dependant object " + dp.dependant.name + " not found in " + p.name + ", on part " + t.name);
                        }
                    }
                    t.DEPENDANTS = newDependants;
                }

                // Attachables
                if (t.ATTACHABLES != null && t.ATTACHABLES.Length > 0)
                {
                    transparents.AttachingObjects[] newAttachables = new transparents.AttachingObjects[t.ATTACHABLES.Length];
                    for (int i = 0; i < t.ATTACHABLES.Length; i++)
                    {
                        transparents.AttachingObjects dp = t.ATTACHABLES[i];
                        newAttachables[i] = new transparents.AttachingObjects();
                        referenceUpdated = false;

                        if(dp == null || dp.Attachable == null)
                        {
                            continue;
                        }

                        int savePosition = dp.Attachable.GetComponent<transparents>().SavePosition;
                        foreach (transparents t2 in p.GetComponentsInChildren<transparents>())
                        {
                            if (t2 == null)
                            {
                                continue;
                            }

                            if (dp.Attachable == null)
                            {
                                continue;
                            }
                            if (t2.name == dp.Attachable.name && t2.SavePosition == savePosition)
                            {
                                newAttachables[i].Attachable = t2.gameObject;
                                referenceUpdated = true;
                                break;
                            }
                        }
                        if (!referenceUpdated)
                        {
                            if (dp.Attachable == null)
                                Debug.LogError("[ModUtils/CarBuilding/Error]: Attachable object (null) not found in " + p.name + ", on part " + t.name);
                            else
                                Debug.LogError("[ModUtils/CarBuilding/Error]: Attachable object " + dp.Attachable.name + " not found in " + p.name + ", on part " + t.name);
                        }
                    }
                    t.ATTACHABLES = newAttachables;
                }
            }
        }

        // Compatibility method
        public static void UpdateTransparentsReferences(GameObject p)
        {
            UpdateTransparentsReferences(p, false);
        }

        /// <summary>
        /// Internal function to log stuff when debug mode is enabled
        /// </summary>
        /// <param name="str">The message to show on log</param>
        internal static void DevLog(string str)
        {
            if (ENABLE_CARBUILDING_DEBUG)
                Debug.Log("[ModUtils/CarBuilding]: " + str);
        }
    }
}
