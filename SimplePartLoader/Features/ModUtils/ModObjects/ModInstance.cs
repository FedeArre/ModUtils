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

        internal List<ISetting> ModSettings = new List<ISetting>();

        internal Action OnSettingsLoad;
        internal bool SettingsLoaded;
        internal bool CardLoaded;

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
                    CustomLogger.AddLine("Parts", $"Duplicate entry {part.CarProps.PrefabName} on Saver.modParts. Part loading is {prefabName}");
                
                Saver.modParts.Add(part.CarProps.PrefabName, prefab);

                if (part.Mod.Settings.AutomaticFitsToCar != null)
                {
                    part.PartInfo.FitsToCar = part.Mod.Settings.AutomaticFitsToCar;
                }

                if (part.Mod.Settings.AutomaticFitsToEngine != null)
                {
                    part.PartInfo.FitsToEngine = part.Mod.Settings.AutomaticFitsToEngine;
                }
                
                PartProperties pp = part.GetComponent<PartProperties>();
                if(pp)
                {
                    pp.Properties.ForEach(property =>
                    {
                        part.Properties.Add(property);
                    });

                    GameObject.Destroy(pp);
                }

                return part; // We provide the Part instance so the developer can setup the transparents
            }
            
            Part p = new Part(prefab, null, null, null, this);
            PrefabGenerator prefabGen = prefab.GetComponent<PrefabGenerator>();
            if (prefabGen)
            {
                p.Name = prefabGen.PrefabName;

                if (Saver.modParts.ContainsKey(p.Name))
                    CustomLogger.AddLine("Parts", $"Duplicate entry {p.Name} on Saver.modParts. Part loading is {prefabName}");
                
                Saver.modParts.Add(p.Name, prefab);

                PartManager.prefabGenParts.Add(p);
                loadedParts.Add(p);

                p.PartType = PartTypes.DUMMY_PREFABGEN;

                PartProperties pp = p.GetComponent<PartProperties>();
                if (pp)
                {
                    pp.Properties.ForEach(property =>
                    {
                        p.Properties.Add(property);
                    });

                    GameObject.Destroy(pp);
                }
            }
            else
            {
                p.Name = prefabName; 
                
                if (Saver.modParts.ContainsKey(p.Name))
                    CustomLogger.AddLine("Parts", $"Duplicate entry {p.Name} on Saver.modParts. Part loading is {prefabName}");

                Saver.modParts.Add(prefabName, prefab);
                
                PartManager.dummyParts.Add(p);
                loadedParts.Add(p);

                p.PartType = PartTypes.DUMMY;

                PartProperties pp = p.GetComponent<PartProperties>();
                if (pp)
                {
                    pp.Properties.ForEach(property =>
                    {
                        p.Properties.Add(property);
                    });

                    GameObject.Destroy(pp);
                }
            }

            return p;
        }

        public Furniture LoadFurniture(AssetBundle bundle, string prefabName)
        {
            // Safety checks
            if (!bundle)
                CustomLogger.AddLine("Furnitures", $"Tried to create a furniture without valid AssetBundle");

            if (String.IsNullOrWhiteSpace(prefabName))
                CustomLogger.AddLine("Furnitures", $"Tried to create a part without prefab name");

            if (Saver.modParts.ContainsKey(prefabName))
                CustomLogger.AddLine("Furnitures", $"Tried to create an already existing prefab ({prefabName})");

            GameObject prefab = bundle.LoadAsset<GameObject>(prefabName);
            if (!prefab)
                CustomLogger.AddLine("Furnitures", $"Tried to create a prefab but it was not found in the AssetBundle ({prefabName})");

            FurnitureGenerator furnitureGen = prefab.GetComponent<FurnitureGenerator>();
            if (!furnitureGen)
                CustomLogger.AddLine("Furnitures", $"{prefabName} has no Furniture Generator component");
            
            GameObject.DontDestroyOnLoad(prefab); // We make sure that our prefab is not deleted in the first scene change

            if(FurnitureManager.Furnitures.ContainsKey(furnitureGen.PrefabName))
            {
                CustomLogger.AddLine("Furnitures", $"{furnitureGen.PrefabName} prefab name is already on use!");
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
                CustomLogger.AddLine("EACheck", ex);
            }

            if(!allowed)
            {
                CustomLogger.AddLine("EACheck", $"User is not allowed to use mod " + Mod.Name);
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
                CustomLogger.AddLine("CarGenerator", $"Tried to create a car without valid AssetBundle");

            if (String.IsNullOrWhiteSpace(carObject) || String.IsNullOrWhiteSpace(emptyObject) || String.IsNullOrWhiteSpace(transparentsObject))
                CustomLogger.AddLine("CarGenerator", $"Tried to create a car without car / empty / transparents name");

            GameObject carPrefab = bundle.LoadAsset<GameObject>(carObject);
            GameObject emptyCarPrefab = bundle.LoadAsset<GameObject>(emptyObject);
            GameObject transparentsPrefab = bundle.LoadAsset<GameObject>(transparentsObject);
            
            if (!carPrefab)
                CustomLogger.AddLine("CarGenerator", $"Tried to create a prefab but it was not found in the AssetBundle ({carObject})");
            
            if (!emptyCarPrefab)
                CustomLogger.AddLine("CarGenerator", $"Tried to create a prefab but it was not found in the AssetBundle ({emptyObject})");
            
            if (!transparentsPrefab)
                CustomLogger.AddLine("CarGenerator", $"Tried to create a prefab but it was not found in the AssetBundle ({transparentsObject})");

            CarGenerator carGen = carPrefab.GetComponent<CarGenerator>();
            if(!carGen)
                CustomLogger.AddLine("CarGenerator", $"{carObject} has no Car Generator component");

            Car car = new Car(carPrefab, emptyCarPrefab, transparentsPrefab);
            car.loadedBy = this;

            Cars.Add(car);

            MainCarGenerator.RegisteredCars.Add(car);
            return car;
        }

        public void EnableDebug(bool saveDissasembler = false)
        {
            ErrorMessageHandler.GetInstance().DebugEnabled.Add(Mod.ID);
            CustomLogger.DebugEnabled = true;

            if (saveDissasembler)
            {
                ErrorMessageHandler.GetInstance().Dissasembler.Add(Mod.ID);
                CustomLogger.SaveDissasamble = true;
            }
        }

        public Buildable LoadBuildable(AssetBundle bundle, string prefabName)
        {
            // Safety checks
            if (!bundle)
                CustomLogger.AddLine("Buildables", $"Tried to create a buildable without valid AssetBundle");

            if (String.IsNullOrWhiteSpace(prefabName))
                CustomLogger.AddLine("Buildables", $"Tried to create a buildable without valid prefab name");

            GameObject prefab = bundle.LoadAsset<GameObject>(prefabName);

            if (!prefab)
                CustomLogger.AddLine("Buildables", $"Tried to create a prefab but it was not found in the AssetBundle ({prefabName})");

            BuildableGenerator buildGen = prefab.GetComponent<BuildableGenerator>();
            if (!buildGen)
                CustomLogger.AddLine("Buildables", $"{prefabName} does not have Buildable Generator component!");

            if (BuildableManager.Buildables.Contains(buildGen.PrefabName))
                CustomLogger.AddLine("Buildables", $"{buildGen.PrefabName} (from {prefabName}) prefab name is already registered in ModUtils!");

            if (Saver.modParts.Contains(buildGen.PrefabName))
                CustomLogger.AddLine("Buildables", $"{buildGen.PrefabName} (from {prefabName}) prefab name is already registered in game saver!");

            Buildable b = new Buildable(buildGen.PrefabName, prefab, (BuildableType) buildGen.Type);
            b.loadedBy = this;

            prefab.AddComponent<SaveItem>().PrefabName = buildGen.PrefabName;
            prefab.tag = "Building";

            if (b.Type == BuildableType.DOOR && buildGen.OpenPosition && buildGen.ClosedPosition)
            {
                OpenGarage garage = prefab.AddComponent<OpenGarage>();
                garage.OpenOnOThisClick = true;
                garage.GateClosed = buildGen.ClosedPosition.gameObject;
                garage.GateOpen = buildGen.OpenPosition.gameObject;
            }

            BuildableManager.Buildables.Add(buildGen.PrefabName, b);
            Saver.modParts.Add(buildGen.PrefabName, prefab);

            Buildables.Add(b);
            return b;
        }

        public BuildableMaterial LoadBuildableMaterial(AssetBundle bundle, string prefabName, string materialName, Vector3 shopPosition, Vector3 shopRotation)
        {
            // Safety checks
            if (!bundle)
                CustomLogger.AddLine("BuildablesMats", $"Tried to create a buildable material without valid AssetBundle");

            if (String.IsNullOrWhiteSpace(prefabName))
                CustomLogger.AddLine("BuildablesMats", $"Tried to create a buildable material without valid prefab name");

            GameObject prefab = bundle.LoadAsset<GameObject>(prefabName);
            if (!prefab)
                CustomLogger.AddLine("BuildablesMats", $"Tried to create a buildable material prefab but it was not found in the AssetBundle {prefabName}");

            Material mat = bundle.LoadAsset<Material>(materialName);
            if (!mat)
                CustomLogger.AddLine("BuildablesMats", $"Tried to create a buildable material but it was not found in the AssetBundle {materialName}");

            if (BuildableManager.BuildableMaterials.Contains(materialName))
                CustomLogger.AddLine("BuildablesMats", $"{materialName} (from {prefabName}) material name is already registered in ModUtils!");

            if (Saver.modParts.Contains(materialName))
                CustomLogger.AddLine("BuildablesMats", $"{materialName} (from {prefabName}) material name is already registered in game saver!");

            if (shopPosition == null || shopRotation == null)
                CustomLogger.AddLine("BuildablesMats", $"{materialName} (from {prefabName}) material does not have shop position or shop rotation!");

            BuildableMaterial bm = new BuildableMaterial(materialName, mat, this, shopPosition, shopRotation, prefab);

            BuildableManager.BuildableMaterials.Add(materialName, bm);
            Saver.modParts.Add(materialName, mat);

            BuildableMaterials.Add(bm);
            return bm;
        }

        public void LoadCustomMeshObject(AssetBundle bundle, string prefabName)
        {
            // Safety checks
            if (!bundle)
                CustomLogger.AddLine("CustomMeshes", $"Tried to load custom meshes object without valid AssetBundle");

            if (String.IsNullOrWhiteSpace(prefabName))
                CustomLogger.AddLine("CustomMeshes", $"Tried to load a custom meshes object without valid prefab name");

            GameObject prefab = bundle.LoadAsset<GameObject>(prefabName);
            if (!prefab)
                CustomLogger.AddLine("CustomMeshes", $"Tried to load a custom meshes object prefab but it was not found in the AssetBundle {prefabName}");

            CustomMeshes cm = prefab.GetComponent<CustomMeshes>();
            if (!cm)
                CustomLogger.AddLine("CustomMeshes", $"Tried to load a custom meshes object but component was not found in the GameObject {prefabName}");

            if(CustomMeshHandler.IsEngineNameUsed(cm))
            {
                CustomLogger.AddLine("CustomMeshes", $"Repeated engine name found, name {cm.EngineName}");
            }

            CustomMeshHandler.Meshes.Add(cm);
        }


        // Mod settings update
        public Label AddLabelToUI(string text)
        {
            Label label = new Label(text);
            ModSettings.Add(label);

            return label;
        }
        public void AddSpacerToUI()
        {
            Spacer s = new Spacer();
            ModSettings.Add(s);
        }

        public TextInput AddTextInputToUI(string saveId, string text, string defaultValue = "", Action<string> onValueChange = null)
        {
            TextInput textInput = new TextInput(saveId, text, defaultValue, onValueChange);
            ModSettings.Add(textInput);

            return textInput;
        }

        public ModDropdown AddDropdownToUI(string saveId, string text, string[] options, int defaultOption = 0, Action<int> onValueChange = null)
        {
            ModDropdown dropdown = new ModDropdown(saveId, text, options, defaultOption, onValueChange);
            ModSettings.Add(dropdown);
            return dropdown;
        }

        public ModSlider AddSliderToUI(string saveId, float minValue, float maxValue, float value, bool wholeNumbers = true, Action<float> onValueChange = null)
        {
            if(minValue > maxValue || value < minValue || value > maxValue)
            {
                CustomLogger.AddLine("ModUtilsUI", $"Wrong setup of slider in {Mod.ID} - discarding setting!");
                return null;
            }

            ModSlider s = new ModSlider(saveId, minValue, maxValue, value, wholeNumbers, onValueChange);
            ModSettings.Add(s);
            return s;
        }

        public ModButton AddButtonToUI(string text, Action onButtonClick = null)
        {
            ModButton mb = new ModButton(text, onButtonClick);
            ModSettings.Add(mb);
            return mb;
        }

        public Checkbox AddCheckboxToUI(string saveId, string text, bool value, Action<bool> onValueChange = null) 
        {
            Checkbox ch = new Checkbox(saveId, text, value, onValueChange);
            ModSettings.Add(ch);
            return ch;
        }
        public Keybind AddKeybindToUI(string saveId, KeyCode key, KeyCode multiplier = KeyCode.None)
        {
            Keybind kb = new Keybind(saveId, key, multiplier);
            ModSettings.Add(kb);
            return kb;
        }

        public void SetSettingsLoadedFunction(Action func)
        {
            OnSettingsLoad = func;
        }

        internal List<ISetting> GetSaveablesSettings()
        {
            List<ISetting> settings = new List<ISetting>();

            foreach(ISetting setting in ModSettings)
            {
                if(setting is Checkbox || setting is ModSlider || setting is ModDropdown || setting is TextInput || setting is Keybind)
                {
                    settings.Add(setting);
                }
            }

            return settings.Count == 0 ? null : settings;
        }
    }
}
