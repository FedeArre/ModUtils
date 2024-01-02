using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    internal class ModStatusReport
    {
        public ModInstance Mod { get; set; }
        public bool PartFailed = false;

        public ModStatusReport(ModInstance mod)
        {
            Mod = mod;
        }

        public string GenerateReport(bool showOnlyWrong)
        {
            string reportText = $"ModUtils - Mod instance status report - {Mod.Name}";

            reportText += $"\nParts: {Mod.Parts.Count}";
            reportText += $"\nFurnitures: {Mod.Furnitures.Count}";
            reportText += $"\nBuildables: {Mod.Buildables.Count}";
            reportText += $"\nMaterials (build.): {Mod.BuildableMaterials.Count}";
            reportText += $"\nCars: {Mod.Cars.Count}";

            if(Mod.Parts.Count != 0)
            {
                reportText += $"\n\nPer part report: ";
                foreach (Part p in Mod.Parts)
                {
                    string text = GeneratePartReport(p, showOnlyWrong);
                    if (text != "")
                        reportText += "\n" + text;
                }
                if (showOnlyWrong && !PartFailed)
                    reportText += "\n All part tests are OK";
            }

            if(Mod.Cars.Count != 0)
            {
                reportText += $"\n\nPer car report: ";
            }

            return reportText ;
        }

        public string GetPartReportName(int id)
        {
            switch(id)
            {
                case 0: return "Basic components";
                case 1: return "No mesh collider";
                case 2: return "Mesh collider is not trigger, CarProps triger is set to true";
                case 3: return "CarProps and Mesh Collider are trigger, but children have wrong layer";
                case 4: return "Mesh collider is Triger, CarProps triger is set to false";
                case 5: return "Reference issue, check extra info";
                case 6: return "No attaching method in part";
                default: return "Unknown issue?";
            }
        }

        public string GeneratePartReport(Part p, bool showOnlyWrong)
        {
            // Reports by ID - True means it failed the check!
            bool[] partsChecks = new bool[7];
            string extraInfoReferences = "";

            partsChecks[0] = !(p.Prefab.GetComponent<CarProperties>() && p.Prefab.GetComponent<Partinfo>());

            MeshCollider mc = p.Prefab.GetComponent<MeshCollider>();
            partsChecks[1] = !mc;
           
            if (p.CarProps.triger)
            {
                partsChecks[2] = !mc.isTrigger;

                foreach (Collider c in p.Prefab.GetComponentsInChildren<Collider>())
                {
                    if (!c.isTrigger && c.gameObject.layer == LayerMask.NameToLayer("Default"))
                    {
                        partsChecks[3] = true;
                    }
                }
            }
            else
            {
                partsChecks[4] = mc.isTrigger;
            }

            foreach (MonoBehaviour c in p.Prefab.GetComponentsInChildren<MonoBehaviour>())
            {
                if (c != null)
                {
                    Type type = c.GetType();

                    if (type == null) continue;

                    FieldInfo[] fields = type.GetFields();
                    foreach (FieldInfo field in fields)
                    {
                        if (field == null) continue;

                        if (field.FieldType == typeof(Transform))
                        {
                            Transform transformValue = (Transform)field.GetValue(c);
                            if (transformValue && transformValue.root && !transformValue.root.GetComponent<SPL_Part>())
                            {
                                partsChecks[5] = true;
                                extraInfoReferences += $"Reference issue on {c.name} (part is {c.transform.root.name}) - Found reference to {transformValue.name} (root {transformValue.root.name}) - Root is not mod part!\n";
                            }

                        }
                        else if (field.FieldType == typeof(GameObject))
                        {
                            GameObject goValue = (GameObject)field.GetValue(c);
                            if (goValue && goValue.transform && goValue.transform.root && !goValue.transform.root.GetComponent<SPL_Part>())
                            {
                                partsChecks[5] = true;
                                extraInfoReferences += $"Reference issue on {c.name} (part is {c.transform.root.name}) - Found reference to {goValue.transform.name} (root {goValue.transform.root.name}) - Root is not mod part!\n";
                            }
                        }
                    }
                }
            }

            if(p.Prefab.GetComponent<Pickup>() || p.Prefab.GetComponent<PickupDoor>())
            {
                int attachingCount = p.Prefab.GetComponentsInChildren<HexNut>().Length + p.Prefab.GetComponentsInChildren<FlatNut>().Length + p.Prefab.GetComponentsInChildren<BoltNut>().Length + p.Prefab.GetComponentsInChildren<WeldCut>().Length;
                if (attachingCount == 0)
                    partsChecks[6] = true;
            }

            bool issuesFound = false;
            for(int i = 0; i < partsChecks.Length; i++)
            {
                if (partsChecks[i])
                {
                    PartFailed = true;
                    issuesFound = true;
                }
            }

            if (!issuesFound && showOnlyWrong) return "";

            // Generate report text
            string prefabGenOkString = " ";
            if(p.PartType == PartTypes.DUMMY_PREFABGEN)
            {
                prefabGenOkString = p.PrefabGenLoaded ? " (OK) " : " (NOT OK) ";
            }
            string resultText = $"------------------------------------------------------------------------------\n- {p.CarProps.PrefabName} - {p.GetTypeName()}{prefabGenOkString}- Tests:";

            for(int i = 0; i < partsChecks.Length; i++)
            {
                resultText += $"\n  - {GetPartReportName(i)} - Result: " + (partsChecks[i] ? "FAILED" : "Ok");
                if(i == 5 && partsChecks[i]) // Handle special referencing showing
                {
                    resultText += $"\nExtra information about this test: " + extraInfoReferences;
                }
            }

            return resultText;
        }
    }
}
