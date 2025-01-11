using PaintIn3D;
using SimplePartLoader.Utils;
using System;
using System.Collections;
using UnityEngine;
using SimplePartLoader;
using System.Collections.Generic;
using SimplePartLoader.Objects;

namespace SimplePartLoader
{
    public class Part
    {
        public GameObject Prefab;
        public CarProperties CarProps;
        public Partinfo PartInfo;
        public Renderer Renderer;

        internal GameObject OriginalGameobject;

        internal string Name;
        internal bool Paintable;

        internal Hashtable languages = new Hashtable();

        internal bool SavingEnabled;
        internal bool PrefabGenLoaded = false;

        public bool UseBetterCopy;
        public bool BoltDisplacement = true;

        public ShaderSettings ForceShaderStatus = ShaderSettings.NONE;

        internal bool IssueExternalReport = false;
        internal string ReportedIssue = string.Empty;

        private PartTypes Type;
        private ModInstance modInstance;

        public bool RotateThumbnail;
        public List<string> Properties = new List<string>();

        public ModInstance Mod
        {
            get { return modInstance; }
        }
        
        public PartTypes PartType
        {
            get { return Type; }
            internal set { Type = value; }
        }

        public Part(GameObject prefab, CarProperties carProp, Partinfo partinfo, Renderer renderer, ModInstance modInstance)
        {
            Prefab = prefab;
            CarProps = carProp;
            PartInfo = partinfo;
            Renderer = renderer;
            this.modInstance = modInstance;

            if(modInstance != null)
            {
                UseBetterCopy = modInstance.Settings.PreciseCloning;
            }
        }

        [Obsolete("SetupTransparent will be removed on the future, use AddTransparent instead!")]
        public void SetupTransparent(string attachesTo, Vector3 transparentLocalPos, Quaternion transaprentLocalRot, bool testingModeEnable = false)
        {
            TransparentData td = new TransparentData(PartInfo.RenamedPrefab, attachesTo, transparentLocalPos, transaprentLocalRot, testingModeEnable);
            MeshFilter mf = Prefab.GetComponent<MeshFilter>();
            if(mf)
            {
                td.MeshToUse = mf.mesh;
            }

            td.Owner = this;
            PartManager.transparentData.Add(td);
        }

        [Obsolete("SetupTransparent will be removed on the future, use AddTransparent instead!")]
        public void SetupTransparent(string attachesTo, Vector3 transparentLocalPos, Quaternion transaprentLocalRot, Vector3 scale, bool testingModeEnable = false)
        {
            TransparentData td = new TransparentData(PartInfo.RenamedPrefab, attachesTo, transparentLocalPos, transaprentLocalRot, scale, testingModeEnable);
            MeshFilter mf = Prefab.GetComponent<MeshFilter>();
            if (mf)
            {
                td.MeshToUse = mf.mesh;
            }

            td.Owner = this;
            PartManager.transparentData.Add(td);
        }
        
        public T GetComponent<T>()
        {
            return Prefab.GetComponent<T>();
        }

        public GameObject GetDummyOriginal()
        {
            return OriginalGameobject;
        }

        public TransparentData AddTransparent(string attachesTo, Vector3 transparentLocalPos, Quaternion transaprentLocalRot, bool testingModeEnable = false)
        {
            if(attachesTo == PartInfo.RenamedPrefab)
            {
                ReportIssue($"Tried to generate part with same name as renamed prefab ({attachesTo})");
                return null;
            }

            TransparentData td = new TransparentData(PartInfo.RenamedPrefab, attachesTo, transparentLocalPos, transaprentLocalRot, testingModeEnable);
            MeshFilter mf = Prefab.GetComponent<MeshFilter>();
            if (mf)
            {
                td.MeshToUse = mf.mesh;
            }

            td.Owner = this;
            PartManager.transparentData.Add(td);
            return td;
        }

        public void Localize(string language, string newTranslation)
        {
            languages[language] = newTranslation;
        }

        public void UsePrytoolAttachment()
        {
            if(Prefab.GetComponent<Pickup>())
                DestroyConsiderSetting(Prefab.GetComponent<Pickup>());

            if (!Prefab.GetComponent<PickupWindow>())
            {
                RemoveAttachmentsFromPart();

                Prefab.AddComponent<PickupWindow>();
                Prefab.AddComponent<RemoveWindow>();

                Prefab.layer = LayerMask.NameToLayer("Windows");
                //Prefab.tag = "Window";
            }
        }

        public void EnableDataSaving()
        {
            if (SavingEnabled)
                return;

            SavingEnabled = true;
            Prefab.AddComponent<SaveData>();
        }

        public void UseHandAttachment()
        {
            if (Prefab.GetComponent<Pickup>())
                DestroyConsiderSetting(Prefab.GetComponent<Pickup>());

            if (!Prefab.GetComponent<PickupHand>())
            {
                RemoveAttachmentsFromPart();

                Prefab.AddComponent<PickupHand>();
                Prefab.AddComponent<RemoveWindow>();

                Prefab.layer = LayerMask.NameToLayer("Windows");
                Prefab.tag = "Window";
            }
        }

        public void MakeOpenable(OpeningType type)
        {
            if (Prefab.GetComponent<Pickup>())
                DestroyConsiderSetting(Prefab.GetComponent<Pickup>());

            if (!Prefab.GetComponent<PickupDoor>())
            {
                RemoveAttachmentsFromPartIgnoringNuts();

                Prefab.AddComponent<PickupDoor>();
                Prefab.AddComponent<OpenDoor>();

                Prefab.layer = LayerMask.NameToLayer("OpenableParts");
                Prefab.tag = "Untagged";

                UpdatePartinfoBasedOnType(type);
                GenerateHingePivot(type);
            }
        }

        internal void ReportIssue(string issue)
        {
            IssueExternalReport = true;
            ReportedIssue += issue + "\n";
        }

        public void UpdateHingePivotPosition(Vector3 newLocalPosition)
        {
            Transform t = Prefab.transform.Find("HingePivot");
            if(t)
            {
                t.localPosition = newLocalPosition;
            }
        }

        public Transform[] GetTransforms()
        {
            return Prefab.GetComponentsInChildren<Transform>();
        }
        
        /*[Obsolete("EnablePartPainting using SPL.PaintingSupportedTypes will be removed on Modutils 1.5. Use PaintingSystem.Types instead!")]
        public void EnablePartPainting(SPL.PaintingSupportedTypes type, int paintMaterial = -1)
        {
            PaintingSystem.Types newType = (PaintingSystem.Types)type;
            EnablePartPainting(newType, paintMaterial);
        }
        */
        public void EnablePartPainting(PaintingSystem.Types type, int paintMaterial = -1)
        {
            switch (type)
            {
                case PaintingSystem.Types.FullPaintingSupport:
                    PaintingSystem.EnableFullSupport(this);
                    break;

                case PaintingSystem.Types.OnlyPaint:
                    PaintingSystem.EnablePaintOnly(this, paintMaterial);
                    break;

                case PaintingSystem.Types.OnlyPaintAndRust:
                    PaintingSystem.EnablePaintAndRust(this);
                    break;

                case PaintingSystem.Types.OnlyDirt:
                    PaintingSystem.EnableDirtOnly(this);
                    break;

                case PaintingSystem.Types.OnlyPaintAndDirt:
                    PaintingSystem.EnablePaintAndDirt(this);
                    break;

                default:
                    CustomLogger.AddLine("Parts", $"An invalid type has been sent to Part.EnablePainting, part: " + Prefab.name);
                    break;
            }
        }

        private void RemoveAttachmentsFromPart()
        {
            foreach (WeldCut wc in Prefab.GetComponentsInChildren<WeldCut>())
            {
                DestroyConsiderSetting(wc.gameObject);
            }

            foreach (BoltNut bn in Prefab.GetComponentsInChildren<BoltNut>())
            {
                DestroyConsiderSetting(bn.gameObject);
            }

            foreach (HexNut hn in Prefab.GetComponentsInChildren<HexNut>())
            {
                DestroyConsiderSetting(hn.gameObject);
            }

            if (Prefab.GetComponent<RemoveWindow>())
                DestroyConsiderSetting(Prefab.GetComponent<RemoveWindow>());

            if(Prefab.GetComponent<PickupHand>())
                DestroyConsiderSetting(Prefab.GetComponent<PickupHand>());

            if(Prefab.GetComponent<PickupWindow>())
                DestroyConsiderSetting(Prefab.GetComponent<PickupWindow>());
        }

        private void RemoveAttachmentsFromPartIgnoringNuts()
        {
            foreach (WeldCut wc in Prefab.GetComponentsInChildren<WeldCut>())
            {
                DestroyConsiderSetting(wc.gameObject);
            }

            if (Prefab.GetComponent<RemoveWindow>())
                DestroyConsiderSetting(Prefab.GetComponent<RemoveWindow>());

            if (Prefab.GetComponent<PickupHand>())
                DestroyConsiderSetting(Prefab.GetComponent<PickupHand>());

            if (Prefab.GetComponent<PickupWindow>())
                DestroyConsiderSetting(Prefab.GetComponent<PickupWindow>());
        }

        private void DestroyConsiderSetting(Component c)
        {
            if (modInstance != null)
            {
                if (modInstance.Settings.EnableImmediateDestroys)
                    GameObject.DestroyImmediate(c);
                else
                    GameObject.Destroy(c);
            }
            else
            {
                GameObject.Destroy(c);
            }
        }
        private void DestroyConsiderSetting(GameObject c)
        {
            if (modInstance != null)
            {
                if (modInstance.Settings.EnableImmediateDestroys)
                    GameObject.DestroyImmediate(c);
                else
                    GameObject.Destroy(c);
            }
            else
            {
                GameObject.Destroy(c);
            }
        }

        private void UpdatePartinfoBasedOnType(OpeningType type)
        {
            PartInfo.HoodHalf = false;
            PartInfo.Hood = false;
            PartInfo.Rdoor = false;
            PartInfo.Ldoor = false;
            PartInfo.Trunk = false;

            switch (type)
            {
                case OpeningType.TRUNK:
                    PartInfo.Trunk = true;
                    break;

                case OpeningType.DOOR_LEFT:
                    PartInfo.Ldoor = true;
                    break;

                case OpeningType.DOOR_RIGHT:
                    PartInfo.Rdoor = true;
                    break;

                case OpeningType.HOOD:
                    PartInfo.Hood = true;
                    break;

                case OpeningType.HOOD_HALF:
                    PartInfo.HoodHalf = true;
                    break;
            }
        }

        public void RemovePaintCounters()
        {
            foreach (P3dChangeCounter counter in Prefab.GetComponentsInChildren<P3dChangeCounter>())
                GameObject.Destroy(counter);
        }

        public void SetStandardShader()
        {
            Material[] objectMats = Renderer.materials;
            Shader standardShader = Shader.Find("Standard");

            foreach (Material m in objectMats)
                m.shader = standardShader;

            Renderer.materials = objectMats;
        }

        private void GenerateHingePivot(OpeningType type)
        {
            Transform t = Prefab.transform.Find("HingePivot");
            if (t)
            {
                SPL.DevLog(this, "Found hinge pivot automatically, using it as reference");
            }
            else
            {
                GameObject hingePivot = new GameObject("HingePivot");
                t = hingePivot.transform;
                t.SetParent(Prefab.transform);

                t.localPosition = Vector3.zero;
            }

            t.gameObject.layer = LayerMask.NameToLayer("OpenableParts");

            PartInfo.HingePivot = t.gameObject;
        }

        public string GetTypeName()
        {
            switch(Type)
            {
                case PartTypes.FULL_PART: return "Full part";
                case PartTypes.DUMMY_PREFABGEN: return "Prefab generator";
                default: return "Dummy";
            }
        }

        public bool HasProperty(string property)
        {
            return Properties.Contains(property);
        }

        /// <summary>
        /// Adds a start option to the part. This does not work on prefab generator parts.
        /// </summary>
        /// <param name="option">The start option instance</param>
        public void AddStartOption(StartOption option)
        {
            if(CarProps == null)
            {
                CustomLogger.AddLine("Parts", $"Tried to add start option to a part that is not a full part, part: {Prefab.name}");
                return;
            }

            // This is terrible. There is no better option as the game has 9 different variables per part for the start options instead of an array!
            if (!CarProps.StartOption1)
                CarProps.StartOption1 = option.Prefab;
            else if (!CarProps.StartOption2)
                CarProps.StartOption2 = option.Prefab;
            else if (!CarProps.StartOption3)
                CarProps.StartOption3 = option.Prefab;
            else if (!CarProps.StartOption4)
                CarProps.StartOption4 = option.Prefab;
            else if (!CarProps.StartOption5)
                CarProps.StartOption5 = option.Prefab;
            else if(!CarProps.StartOption6)
                CarProps.StartOption6 = option.Prefab;
            else if(!CarProps.StartOption7)
                CarProps.StartOption7 = option.Prefab;
            else if (!CarProps.StartOption8)
                CarProps.StartOption8 = option.Prefab;
            else if (!CarProps.StartOption9)
                CarProps.StartOption9 = option.Prefab;
            else
            {
                CustomLogger.AddLine("Parts", $"Tried to add start option to a part that already has the maximum of possible start options (9), part: {Prefab.name}");
                return;
            }

            CustomLogger.AddLine("Parts", $"Succesfully set start option {option.PartToCopy}");
        }
    }

    
    public enum PartTypes
    {
        FULL_PART = 1,
        DUMMY_PREFABGEN,
        DUMMY
    }

    public enum ShaderSettings
    {
        NONE = 0,
        FORCE_BACKSIDE,
        FORCE_DOUBLESIDED
    }

    public enum OpeningType
    {
        TRUNK = 0,
        DOOR_LEFT,
        DOOR_RIGHT,
        HOOD,
        HOOD_HALF
    }
}
