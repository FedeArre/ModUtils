using Assets.SimpleLocalization;
using Newtonsoft.Json;
using SimplePartLoader.Objects.Furniture;
using SimplePartLoader.Objects.Furniture.Saving;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    internal class FurnitureManager
    {
        private static Hashtable FurnitureList = new Hashtable();
        internal static List<SaleFurniture> SaleItems = new List<SaleFurniture>();

        private static FurnitureWrapper SaveData = new FurnitureWrapper();
        
        internal static string SavePath = $"{Application.persistentDataPath}/save1/modSaves/";
        internal static string FileName = "ModUtilsFurniture.json";
        
        public static Hashtable Furnitures
        {
            get { return FurnitureList; }
        }
        
        public static void LoadFurniture()
        {
            ModUtils.GetPlayer().AddComponent<PlayerFurniturePickup>();
            
            GameObject spawnSpot = new GameObject("SPAWN_POINT_FURNITURE_MODUTILS");
            spawnSpot.transform.position = new Vector3(660.2147f, 56.3729f, -83.6104f);
            
            foreach(SaleFurniture sf in SaleItems)
            {
                GameObject furniture = GameObject.Instantiate(sf.Furniture.Prefab);

                furniture.layer = LayerMask.NameToLayer("Items");
                
                GameObject.Destroy(furniture.GetComponent<ModUtilsFurniture>());
                GameObject.DestroyImmediate(furniture.GetComponent<Rigidbody>());

                CustomFurnitureSaleItem si = furniture.AddComponent<CustomFurnitureSaleItem>();
                si.Price = sf.Furniture.Price;
                si.Prefab = sf.Furniture.Prefab;
                si.Name = sf.Furniture.Name;
                si.Tip = sf.Furniture.Tip;

                if (sf.Spawn != null)
                    si.SpawnSpot = sf.Spawn.Get();
                else
                    si.SpawnSpot = spawnSpot.transform;
                
                furniture.transform.position = sf.Pos;
                furniture.transform.eulerAngles = sf.Rot;

                furniture.name = "MODUTILS_SALEFURNITURE_" + sf.Furniture.Name;
                
                // Localization of the part
                foreach (var dictionary in LocalizationManager.Dictionary)
                {
                    if (dictionary.Value.ContainsKey(sf.Furniture.Prefab.name)) // Ignore case where the name is shared so the translation already exists
                        continue;

                    dictionary.Value.Add(sf.Furniture.Prefab.name, sf.Furniture.Name);
                }
            }

            // Check if new save.
            if (PlayerPrefs.GetFloat("LoadLevel") == 0f) // New game check
            {
                if (File.Exists(SavePath + FileName))
                    File.Delete(SavePath + FileName);

                return;
            }

            if (!File.Exists(SavePath + FileName))
                return;

            Debug.Log($"[ModUtils/Furniture/Loader]: Starting furniture loader.");
            // We load the data now
            using (StreamReader r = new StreamReader(SavePath + FileName))
            {
                string json = r.ReadToEnd();

                if (String.IsNullOrEmpty(json))
                    return;

                SaveData = JsonConvert.DeserializeObject<FurnitureWrapper>(json);
            }
            
            Debug.Log($"[ModUtils/Furniture/Loader]: Trying to load {SaveData.Furnitures.Count} furnitures.");
            // And then with the data loaded we load the Furnitures
            foreach (FurnitureData fd in SaveData.Furnitures)
            {
                Furniture f = (Furniture) Furnitures[fd.PrefabName];
                if(f != null)
                {
                    GameObject furniture = GameObject.Instantiate(f.Prefab);
                    furniture.name = f.Prefab.name;

                    furniture.transform.position = new Vector3(fd.X, fd.Y, fd.Z);
                    furniture.transform.eulerAngles = new Vector3(fd.rX, fd.rY, fd.rZ);

                    /*if (furniture.GetComponent<Rigidbody>() && f.BehaveAsFurniture)
                        GameObject.DestroyImmediate(furniture.GetComponent<Rigidbody>());*/
                }
                else
                {
                    Debug.Log("[ModUtils/Furniture/Loader]: A furniture was loaded but not found in-game! Name: " + fd.PrefabName);
                }
            }
            Debug.Log($"[ModUtils/Furniture/Loader]: Furniture load finished.");
        }

        public static void SaveFurniture()
        {
            SaveData = new FurnitureWrapper();
            
            ModUtilsFurniture[] AllFurnitures = UnityEngine.Object.FindObjectsOfType<ModUtilsFurniture>();
            Debug.Log($"[ModUtils/Furniture/Saver]: Trying to save {AllFurnitures.Length} furnitures.");
            
            foreach (ModUtilsFurniture f in AllFurnitures)
            {
                FurnitureData fw = new FurnitureData();
                
                fw.PrefabName = f.furnitureRef.PrefabName;

                Vector3 fix = ModUtils.UnshiftCoords(f.transform.position);
                fw.X = fix.x;
                fw.Y = fix.y;
                fw.Z = fix.z;
                
                fw.rX = f.transform.eulerAngles.x;
                fw.rY = f.transform.eulerAngles.y;
                fw.rZ = f.transform.eulerAngles.z;

                SaveData.Furnitures.Add(fw);
            }

            if (!Directory.Exists(SavePath))
                Directory.CreateDirectory(SavePath);

            using (TextWriter tw = new StreamWriter(SavePath + FileName))
            {
                tw.Write(JsonConvert.SerializeObject(SaveData));
            }
            
            Debug.Log($"[ModUtils/Furniture/Saver]: Succesfully saved {AllFurnitures.Length} furnitures.");
        }
    }
}
