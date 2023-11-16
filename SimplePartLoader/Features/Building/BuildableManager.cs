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
            for(int i = 0; i < CategorizedBuildables.Length; i++) CategorizedBuildables[i] = new Queue<Buildable>();

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
        }

        internal static void OnNewMapEnabled()
        {
            GameObject referenceSaleItem = GameObject.Find("Unloadables/HardwareStore/SHOPITEMS/FinishingMaterial");

        }
    }
}
