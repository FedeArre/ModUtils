using Newtonsoft.Json;
using SimplePartLoader.CarGen;
using SimplePartLoader.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    public class ModInstance
    {
        private Mod thisMod;

        private List<Part> loadedParts;
        private List<Furniture> loadedFurniture;
        private List<Car> loadedCars;
        private List<Buildable> loadedBuildables;
        private List<BuildableMaterial> loadedBuildableMats;

        private ModSettings settings;

        internal bool RequiresSteamCheck = false;
        internal bool Checked = false;
        
        internal bool CheckedAndAllowed = true;

        internal bool Thumbnails = false;

        public List<Part> Parts
        {
            get { return loadedParts; }
            internal set { loadedParts = value; }
        }

        public List<Furniture> Furnitures
        {
            get { return loadedFurniture; }
            internal set { loadedFurniture = value; }
        }
        public List<Car> Cars
        {
            get { return loadedCars; }
            internal set { loadedCars = value; }
        }
        public List<Buildable> Buildables
        {
            get { return loadedBuildables; }
            internal set { loadedBuildables = value; }
        }

        public List<BuildableMaterial> BuildableMaterials
        {
            get { return loadedBuildableMats; }
            internal set { loadedBuildableMats = value; }
        }
        
        public Mod Mod
        {
            get { return thisMod; }
        }

        public ModSettings Settings
        {
            get { return settings; }
        }

        public string Name
        {
            get { return Mod.Name;  }
        }

        public bool CheckAllow
        {
            get { return CheckedAndAllowed; }
        }

        internal ModInstance(Mod mod)
        {
            thisMod = mod;

            loadedParts = new List<Part>();
            loadedFurniture = new List<Furniture>();
            loadedCars = new List<Car>();
            loadedBuildables = new List<Buildable>();
            loadedBuildableMats = new List<BuildableMaterial>();

            settings = new ModSettings(this);
        }

        public Part Load(AssetBundle bundle, string prefabName)
        {
            // Safety checks
            if (!bundle)
                SPL.SplError("Tried to create a part without valid AssetBundle");

            if (String.IsNullOrWhiteSpace(prefabName))
                SPL.SplError("Tried to create a part without prefab name");

            if (Saver.modParts.ContainsKey(prefabName))
                SPL.SplError($"Tried to create an already existing prefab ({prefabName})");

            GameObject prefab = bundle.LoadAsset<GameObject>(prefabName);
            if (!prefab)
                SPL.SplError($"Tried to create a prefab but it was not found in the AssetBundle ({prefabName})");

            GameObject.DontDestroyOnLoad(prefab); // We make sure that our prefab is not deleted in the first scene change
            prefab.transform.localScale = Vector3.one; // We set the scale to 1,1,1 to prevent any weird scaling issues

            // Now we determinate the type of part we are loading.
            // If it has CarProperties and Partinfo is a full part.
            // If it has PrefabGenerator ww know is a dummy with prefab gen.
            // If nothing applies is a dummy part.
            CarProperties prefabCarProp = prefab.GetComponent<CarProperties>();
            Partinfo prefabPartInfo = prefab.GetComponent<Partinfo>();

            if(prefabCarProp && prefabPartInfo)
            {
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
                Functions.BoltingSetup(prefab);

                // Now we create the part and add it to the list.
                Part part = new Part(prefab, prefabCarProp, prefabPartInfo, prefab.GetComponent<Renderer>(), this);
                PartManager.modLoadedParts.Add(part);
                loadedParts.Add(part);
                
                part.PartType = PartTypes.FULL_PART;

                if (Saver.modParts.ContainsKey(part.CarProps.PrefabName))
                    Debug.LogError($"[ModUtils/SPL/Error]: Duplicate entry {part.CarProps.PrefabName} on Saver.modParts. Part loading is {prefabName}");
                
                Saver.modParts.Add(part.CarProps.PrefabName, prefab);

                if (part.Mod.Settings.AutomaticFitsToCar != null)
                {
                    part.PartInfo.FitsToCar = part.Mod.Settings.AutomaticFitsToCar;
                }

                if (part.Mod.Settings.AutomaticFitsToEngine != null)
                {
                    part.PartInfo.FitsToEngine = part.Mod.Settings.AutomaticFitsToEngine;
                }
                
                return part; // We provide the Part instance so the developer can setup the transparents
            }
            
            Part p = new Part(prefab, null, null, null, this);
            PrefabGenerator prefabGen = prefab.GetComponent<PrefabGenerator>();
            if (prefabGen)
            {
                p.Name = prefabGen.PrefabName;

                if (Saver.modParts.ContainsKey(p.Name))
                    Debug.LogError($"[ModUtils/SPL/Error]: Duplicate entry {p.Name} on Saver.modParts. Part loading is {prefabName}");
                
                Saver.modParts.Add(p.Name, prefab);

                PartManager.prefabGenParts.Add(p);
                loadedParts.Add(p);

                p.PartType = PartTypes.DUMMY_PREFABGEN;
            }
            else
            {
                p.Name = prefabName; 
                
                if (Saver.modParts.ContainsKey(p.Name))
                    Debug.LogError($"[ModUtils/SPL/Error]: Duplicate entry {p.Name} on Saver.modParts. Part loading is {prefabName}");

                Saver.modParts.Add(prefabName, prefab);
                
                PartManager.dummyParts.Add(p);
                loadedParts.Add(p);

                p.PartType = PartTypes.DUMMY;
            }

            return p;
        }

        public Furniture LoadFurniture(AssetBundle bundle, string prefabName)
        {
            // Safety checks
            if (!bundle)
                Debug.Log("[ModUtils/Furniture/Error]: Tried to create a furniture without valid AssetBundle");

            if (String.IsNullOrWhiteSpace(prefabName))
                Debug.Log("[ModUtils/Furniture/Error]: Tried to create a part without prefab name");

            if (Saver.modParts.ContainsKey(prefabName))
                Debug.Log($"[ModUtils/Furniture/Error]: Tried to create an already existing prefab ({prefabName})");

            GameObject prefab = bundle.LoadAsset<GameObject>(prefabName);
            if (!prefab)
                Debug.Log($"[ModUtils/Furniture/Error]: Tried to create a prefab but it was not found in the AssetBundle ({prefabName})");

            FurnitureGenerator furnitureGen = prefab.GetComponent<FurnitureGenerator>();
            if (!furnitureGen)
                Debug.Log($"[ModUtils/Furniture/Error]: {prefabName} has no Furniture Generator component");
            
            GameObject.DontDestroyOnLoad(prefab); // We make sure that our prefab is not deleted in the first scene change

            if(FurnitureManager.Furnitures.ContainsKey(furnitureGen.PrefabName))
            {
                Debug.Log($"[ModUtils/Furniture/Error]: {furnitureGen.PrefabName} prefab name is already on use!");
                return null;
            }
            
            Furniture furn = new Furniture(prefab, furnitureGen);

            furn.Mod = this;

            loadedFurniture.Add(furn);
            FurnitureManager.Furnitures.Add(furn.PrefabName, furn);
            
            return furn;
        }

        public void EnableEarlyAccessCheck()
        {
            CheckedAndAllowed = false;
            RequiresSteamCheck = true;
        }

        public void GenerateThumbnails()
        {
            Thumbnails = true;
            PartManager.ThumbnailGeneratorEnabled = true;
            ErrorMessageHandler.GetInstance().ThumbnaiLGeneratorEnabled = true;
        }
        
        internal async void Check(ulong SteamID)
        {
            if (!RequiresSteamCheck)
                return;

            EarlyAccessJson eaJson = new EarlyAccessJson();
            eaJson.ModId = Mod.ID;
            eaJson.SteamId = SteamID.ToString();

            bool allowed = false;
            
            try
            {
                string json = JsonConvert.SerializeObject(eaJson);
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await KeepAlive.GetInstance().client.PostAsync(ModMain.API_URL + "/eacheck", content);
                string contents = await result.Content.ReadAsStringAsync();

                if (contents == "true")
                    allowed = true;

                CheckedAndAllowed = allowed;
                Checked = true;
            }
            catch(Exception ex)
            {
                Debug.LogError("[ModUtils/EACheck/Error]: An exception occured");
                Debug.LogError(ex);
            }

            if(!allowed)
            {
                Debug.Log("[ModUtils/EACheck]: User is not allowed to use this mod - " + Mod.Name);
                ErrorMessageHandler.GetInstance().DisabledModList.Add(Mod.Name);
                
                foreach (Part p in Parts)
                {
                    if (PartManager.modLoadedParts.Contains(p))
                        PartManager.modLoadedParts.Remove(p);

                    if (PartManager.dummyParts.Contains(p))
                        PartManager.dummyParts.Remove(p);

                    if (PartManager.prefabGenParts.Remove(p))
                        PartManager.prefabGenParts.Remove(p);

                    if (p.CarProps)
                    {
                        Saver.modParts[p.CarProps.PrefabName] = null;
                    }
                    else
                    {
                        PrefabGenerator pg = p.Prefab.GetComponent<PrefabGenerator>();
                        if (pg)
                            Saver.modParts[pg.PrefabName] = null;
                        else
                            Saver.modParts[p.Name] = null;
                    }
                    
                    GameObject.Destroy(p.Prefab);
                }

                foreach(Furniture f in Furnitures)
                {
                    FurnitureManager.Furnitures.Remove(f);
                }

                foreach(Buildable b in Buildables)
                {
                    BuildableManager.Buildables[b.PrefabName] = null;
                }

                Parts.Clear();
                Furnitures.Clear();
                Buildables.Clear();
            }
        }

        public Car LoadCar(AssetBundle bundle, string carObject, string emptyObject, string transparentsObject)
        {
            // Safety checks
            if (!bundle)
                Debug.LogError("[ModUtils/CarGen/Error]: Tried to create a car without valid AssetBundle");

            if (String.IsNullOrWhiteSpace(carObject) || String.IsNullOrWhiteSpace(emptyObject) || String.IsNullOrWhiteSpace(transparentsObject))
                Debug.LogError("[ModUtils/CarGen/Error]: Tried to create a car without car / empty / transparents name");

            GameObject carPrefab = bundle.LoadAsset<GameObject>(carObject);
            GameObject emptyCarPrefab = bundle.LoadAsset<GameObject>(emptyObject);
            GameObject transparentsPrefab = bundle.LoadAsset<GameObject>(transparentsObject);
            
            if (!carPrefab)
                Debug.LogError($"[ModUtils/CarGen/Error]: Tried to create a prefab but it was not found in the AssetBundle ({carObject})");
            
            if (!emptyCarPrefab)
                Debug.LogError($"[ModUtils/CarGen/Error]: Tried to create a prefab but it was not found in the AssetBundle ({emptyObject})");
            
            if (!transparentsPrefab)
                Debug.LogError($"[ModUtils/CarGen/Error]: Tried to create a prefab but it was not found in the AssetBundle ({transparentsObject})");

            CarGenerator carGen = carPrefab.GetComponent<CarGenerator>();
            if(!carGen)
                Debug.LogError($"[ModUtils/CarGen/Error]: {carObject} has no Car Generator component");

            Car car = new Car(carPrefab, emptyCarPrefab, transparentsPrefab);
            car.loadedBy = this;

            Cars.Add(car);

            MainCarGenerator.RegisteredCars.Add(car);
            return car;
        }

        public Buildable LoadBuildable(AssetBundle bundle, string prefabName)
        {
            // Safety checks
            if (!bundle)
                Debug.LogError("[ModUtils/Buildables/Error]: Tried to create a buildable without valid AssetBundle");

            if (String.IsNullOrWhiteSpace(prefabName))
                Debug.LogError("[ModUtils/Buildables/Error]: Tried to create a buildable without valid prefab name");

            GameObject prefab = bundle.LoadAsset<GameObject>(prefabName);

            if (!prefab)
                Debug.LogError($"[ModUtils/Buildables/Error]: Tried to create a prefab but it was not found in the AssetBundle ({prefabName})");

            BuildableGenerator buildGen = prefab.GetComponent<BuildableGenerator>();
            if (!buildGen)
                Debug.LogError($"[ModUtils/Buildables/Error]: {prefabName} does not have Buildable Generator component!");

            if (BuildableManager.Buildables.Contains(buildGen.PrefabName))
                Debug.LogError($"[ModUtils/Buildables/Error]: {buildGen.PrefabName} (from {prefabName}) prefab name is already registered in ModUtils!");

            if (Saver.modParts.Contains(buildGen.PrefabName))
                Debug.LogError($"[ModUtils/Buildables/Error]: {buildGen.PrefabName} (from {prefabName}) prefab name is already registered in game saver!");

            Buildable b = new Buildable(buildGen.PrefabName, prefab, (BuildableType) buildGen.Type);
            b.loadedBy = this;

            prefab.AddComponent<SaveItem>().PrefabName = buildGen.PrefabName;
            prefab.tag = "Building";

            BuildableManager.Buildables.Add(buildGen.PrefabName, b);
            Saver.modParts.Add(buildGen.PrefabName, prefab);

            Buildables.Add(b);
            return b;
        }

        public BuildableMaterial LoadBuildableMaterial(AssetBundle bundle, string prefabName, string materialName, Vector3 shopPosition, Vector3 shopRotation)
        {
            // Safety checks
            if (!bundle)
                Debug.LogError("[ModUtils/Buildables/Mats/Error]: Tried to create a buildable material without valid AssetBundle");

            if (String.IsNullOrWhiteSpace(prefabName))
                Debug.LogError("[ModUtils/Buildables/Mats/Error]: Tried to create a buildable material without valid prefab name");

            GameObject prefab = bundle.LoadAsset<GameObject>(prefabName);
            if (!prefab)
                Debug.LogError($"[ModUtils/Buildables/Mats/Error]: Tried to create a buildable material prefab but it was not found in the AssetBundle {prefabName}");

            Material mat = bundle.LoadAsset<Material>(materialName);
            if (!mat)
                Debug.LogError($"[ModUtils/Buildables/Mats/Error]: Tried to create a buildable material but it was not found in the AssetBundle {materialName}");

            if (BuildableManager.BuildableMaterials.Contains(materialName))
                Debug.LogError($"[ModUtils/Buildables/Mats/Error]: {materialName} (from {prefabName}) material name is already registered in ModUtils!");

            if (Saver.modParts.Contains(materialName))
                Debug.LogError($"[ModUtils/Buildables/Mats/Error]: {materialName} (from {prefabName}) material name is already registered in game saver!");

            if (shopPosition == null || shopRotation == null)
                Debug.LogWarning($"[ModUtils/Buildables/Mats/Warning]: {materialName} (from {prefabName}) material does not have shop position or shop rotation!");

            BuildableMaterial bm = new BuildableMaterial(materialName, mat, this, shopPosition, shopRotation, prefab);

            BuildableManager.BuildableMaterials.Add(materialName, bm);
            Saver.modParts.Add(materialName, mat);

            BuildableMaterials.Add(bm);
            return bm;
        }
    }
}
