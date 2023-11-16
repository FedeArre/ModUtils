using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    internal class BuildableManager
    {
        internal static Hashtable Buildables { get; } = new Hashtable();
        internal static Hashtable BuildableMaterials { get; } = new Hashtable();
        internal static BuildingParent BuildComponent = null;

        internal static void OnGameLoad()
        {
            // tools._buildingParent is null at this point
            BuildComponent = GameObject.Find("SceneManager/BuildingParent").GetComponent<BuildingParent>();

            // Create lists for every type, initialize them as empty lists
            Queue<Buildable>[] CategorizedBuildables = new Queue<Buildable>[Enum.GetNames(typeof(BuildableType)).Length];
            for (int i = 0; i < CategorizedBuildables.Length; i++) CategorizedBuildables[i] = new Queue<Buildable>();

            foreach (DictionaryEntry item in Buildables)
            {
                Buildable b = (Buildable)item.Value;

                CategorizedBuildables[(int)b.Type].Enqueue(b);
            }

            // Resize game arrays, inject our models
            int[] CategorizedBuildablesIndexes = new int[Enum.GetNames(typeof(BuildableType)).Length];
            CategorizedBuildablesIndexes[(int)BuildableType.WALL] = BuildComponent.WallParts.Length;
            CategorizedBuildablesIndexes[(int)BuildableType.DOOR] = BuildComponent.DoorParts.Length;
            CategorizedBuildablesIndexes[(int)BuildableType.ROOF] = BuildComponent.RoofParts.Length;

            Array.Resize(ref BuildComponent.WallParts, BuildComponent.WallParts.Length + CategorizedBuildables[(int)BuildableType.WALL].Count);
            Array.Resize(ref BuildComponent.DoorParts, BuildComponent.DoorParts.Length + CategorizedBuildables[(int)BuildableType.DOOR].Count);
            Array.Resize(ref BuildComponent.RoofParts, BuildComponent.RoofParts.Length + CategorizedBuildables[(int)BuildableType.ROOF].Count);

            for (int i = CategorizedBuildablesIndexes[(int)BuildableType.WALL]; i < BuildComponent.WallParts.Length; i++)
                BuildComponent.WallParts[i] = CategorizedBuildables[(int)BuildableType.WALL].Dequeue().Prefab;

            for (int i = CategorizedBuildablesIndexes[(int)BuildableType.DOOR]; i < BuildComponent.DoorParts.Length; i++)
                BuildComponent.DoorParts[i] = CategorizedBuildables[(int)BuildableType.DOOR].Dequeue().Prefab;

            for (int i = CategorizedBuildablesIndexes[(int)BuildableType.ROOF]; i < BuildComponent.RoofParts.Length; i++)
                BuildComponent.RoofParts[i] = CategorizedBuildables[(int)BuildableType.ROOF].Dequeue().Prefab;

            // Material setup load
            foreach (DictionaryEntry item in BuildableMaterials)
            {
                BuildableMaterial bm = (BuildableMaterial)item.Value;

                bm.UsedMaterial.name = bm.PrefabName;
            }
        }
        internal static void LoadBoxes(GameObject reference)
        {
            reference = reference.GetComponent<SaleItem>().Item;

            Mesh mesh = reference.GetComponent<MeshFilter>().sharedMesh;

            Mesh boxMesh = reference.transform.Find("Box (2)").GetComponent<MeshFilter>().sharedMesh;
            Material boxMat = reference.transform.Find("Box (2)").GetComponent<MeshRenderer>().material;

            Mesh planeMesh = reference.transform.Find("Plane").GetComponent<MeshFilter>().sharedMesh;

            foreach (DictionaryEntry item in BuildableMaterials)
            {
                BuildableMaterial bm = (BuildableMaterial) item.Value;

                // Clean up child
                foreach(Transform t in bm.Item.GetComponentsInChildren<Transform>())
                {
                    if (t != bm.Item.transform) GameObject.DestroyImmediate(t.gameObject);
                }

                foreach(Component c in bm.Item.GetComponents<Component>())
                {
                    if (!(c is Transform)) GameObject.DestroyImmediate(c);
                }

                bm.Item.layer = LayerMask.NameToLayer("Items");
                bm.Item.tag = "Item";
                bm.Item.name = "FinishingMaterials";

                bm.Item.AddComponent<MeshFilter>().mesh = mesh;
                bm.Item.AddComponent<Rigidbody>();
                bm.Item.AddComponent<DISABLER>();
                bm.Item.AddComponent<SaveItem>();
                
                BoxCollider bc = bm.Item.AddComponent<BoxCollider>();
                bc.center = new Vector3(0.0006f, 0.0042f, -0.0013f);
                bc.size = new Vector3(0.2145f, 0.139f, 0.1075f);

                PickupTool pt = bm.Item.AddComponent<PickupTool>();
                pt.CanPutInBox = true;
                pt.material = bm.UsedMaterial;
                pt.MaterialChanger = true;
                pt.paintlife = 10;

                // Box object
                GameObject box = new GameObject("Box");
                box.transform.SetParent(bm.Item.transform);

                box.AddComponent<MeshFilter>().mesh = boxMesh;
                box.AddComponent<MeshRenderer>().material = boxMat;
                box.transform.localPosition = new Vector3(0, -0.0633f, 0);
                box.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);

                // Plane material preview
                GameObject plane = new GameObject("Plane");
                plane.transform.SetParent(bm.Item.transform);

                plane.transform.localPosition = new Vector3(0.0057f, 0.0295f, -0.0772f);
                plane.transform.localEulerAngles = new Vector3(90f, 180f, 0f);
                plane.transform.localScale = new Vector3(0.015f, 0.01f, 0.015f);

                MeshRenderer planeRenderer = plane.AddComponent<MeshRenderer>();
                plane.AddComponent<MeshFilter>().mesh = planeMesh;
                planeRenderer.material = bm.UsedMaterial;

                pt.previewLabel = planeRenderer;
            }
        }

        internal static void OnNewMapEnabled()
        {
            new GameObject("DelayedLoaderBuildable").AddComponent<DelayedBuildableLoader>();
        }

        internal static void OnNewMapLoad()
        {
            GameObject referenceSaleItem = GameObject.Find("Unloadables/HardwareStore/SHOPITEMS/FinishingMaterial");
            GameObject shopItems = GameObject.Find("Unloadables/HardwareStore/SHOPITEMS");
            GameObject spawnSpot = GameObject.Find("Unloadables/HardwareStore/ItemSpawn");

            TimedActions ta = GameObject.Find("Unloadables/HardwareStore").GetComponent<TimedActions>();

            LoadBoxes(referenceSaleItem);
            
            // Create sale items
            Mesh mesh = referenceSaleItem.GetComponent<MeshFilter>().sharedMesh;
            Material[] materials = referenceSaleItem.GetComponent<MeshRenderer>().materials;

            foreach(DictionaryEntry item in BuildableMaterials)
            {
                GameObject saleItem = new GameObject("FinishingMaterials");
                BuildableMaterial bm = (BuildableMaterial) item.Value;

                saleItem.layer = LayerMask.NameToLayer("Items");
                saleItem.tag = "Item";

                saleItem.AddComponent<MeshFilter>().sharedMesh = mesh;
                MeshRenderer rend = saleItem.AddComponent<MeshRenderer>();

                rend.materials = materials;
                rend.material = bm.UsedMaterial;

                SaleItem si = saleItem.AddComponent<SaleItem>();
                si.boughtMaterial = bm.UsedMaterial;
                si.Item = bm.Item;
                si.Price = 100;
                si.SpawnSpot = spawnSpot;
                si.TimedActions = ta;

                saleItem.transform.SetParent(shopItems.transform);
                saleItem.transform.localPosition = bm.Position;
                saleItem.transform.localEulerAngles = bm.Rotation;

                BoxCollider bc = saleItem.AddComponent<BoxCollider>();
                bc.center = new Vector3(-0.45f, 0, 0);
                bc.size = new Vector3(0.1f, 2f, 1f);
            }
        }
    }
}
