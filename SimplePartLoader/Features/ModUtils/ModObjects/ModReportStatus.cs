using Rewired;
using SimplePartLoader.CarGen;
using SimplePartLoader.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

namespace SimplePartLoader
{
    internal class ModReportStatus : MonoBehaviour
    {
        public ModInstance m_mod;
        public bool m_onlyWrongStuff;
        bool m_anyPartFailed;

        string m_reportText = string.Empty;
        void Start()
        {
            m_reportText = $"ModUtils - Mod instance status report - {m_mod.Name}";

            m_reportText += $"\nParts: {m_mod.Parts.Count}";
            m_reportText += $"\nFurnitures: {m_mod.Furnitures.Count}";
            m_reportText += $"\nBuildables: {m_mod.Buildables.Count}";
            m_reportText += $"\nMaterials (build.): {m_mod.BuildableMaterials.Count}";
            m_reportText += $"\nCars: {m_mod.Cars.Count}";

            if (m_mod.Parts.Count != 0)
            {
                m_reportText += $"\n\nPer part report: ";
                foreach (Part p in m_mod.Parts)
                {
                    string text = GeneratePartReport(p, m_onlyWrongStuff);
                    if (text != "")
                        m_reportText += "\n" + text;
                }
                if (m_onlyWrongStuff && !m_anyPartFailed)
                    m_reportText += "\n All part tests are OK";
            }

            if (m_mod.Cars.Count != 0)
            {
                StartCoroutine(LoadCars());
            }
            else if(ModUtils.GetPlayerCurrentCar())
            {
                StartCoroutine(LoadCar(ModUtils.GetPlayerCurrentCar().gameObject));
            }
            else
            {
                WriteToFile();
            }
        }

        IEnumerator LoadCar(GameObject car)
        {
            yield return 0;

            string referenceIssues = GenerateReport(car);

            if (!string.IsNullOrEmpty(referenceIssues))
                m_reportText += $"Reported references issues:\n{referenceIssues}";

            WriteToFile();
        }

        IEnumerator LoadCars()
        {
            foreach(Car carObj in m_mod.Cars)
            {
                GameObject car = GameObject.Instantiate(carObj.carPrefab);
                MainCarProperties mcp = car.GetComponent<MainCarProperties>();

                mcp.SpawnPoint = Vector3.zero;
                mcp.CreatingStock(0);

                car.name = carObj.carGeneratorData.CarName;

                // Wait 11 frames (7+4 because i like the number. Joke, 7 caused car to not be fully loaded yet)
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();

                string referenceIssues = GenerateReport(car);

                if (!carObj.IssueExternalReport && string.IsNullOrEmpty(referenceIssues))
                    m_reportText += $"\n------------------------------------------------------------------------------\n- {carObj.carGeneratorData.CarName} - No issues reported!\n";
                else
                {
                    m_reportText += $"\n------------------------------------------------------------------------------\n- {carObj.carGeneratorData.CarName}";
                    if (!string.IsNullOrEmpty(carObj.ReportedIssue))
                        m_reportText += $" - Reported issues:\n{carObj.ReportedIssue}\n";
                    else
                        m_reportText += $"\n";

                    if (!string.IsNullOrEmpty(referenceIssues))
                        m_reportText += $"Reported references issues:\n{referenceIssues}";
                }
            }

            WriteToFile();
        }


        string GenerateReport(GameObject car)
        {
            Transform sceneMananger = GameObject.Find("SceneManager").transform;
            Transform player = GameObject.Find("Player").transform;

            string referenceIssues = "";
            foreach (MonoBehaviour c in car.GetComponentsInChildren<MonoBehaviour>())
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
                            if (transformValue && transformValue.root && transformValue.root.name != car.name)
                            {
                                if (transformValue.root == sceneMananger.root || transformValue.root == player.root) continue;

                                referenceIssues += $"- {c.name} | C:{c.GetType().Name} A:{field.Name} ({Functions.GetTransformPath(c.transform)}) references {transformValue.name} (parent {transformValue.parent})\n";
                            }
                        }
                        else if (field.FieldType == typeof(GameObject))
                        {
                            GameObject goValue = (GameObject)field.GetValue(c);

                            if (goValue && goValue.transform && goValue.transform.root && goValue.transform.root.name != car.name)
                            {
                                if (goValue.transform.root == sceneMananger.root || goValue.transform.root == player.root || (c.GetType().Name == "CarProperties" && field.Name == "PREFAB") || (c.GetType().Name == "Partinfo" && field.Name.ToLower() == "player")) continue;

                                referenceIssues += $"- {c.name} | C:{c.GetType().Name} A:{field.Name} ({Functions.GetTransformPath(c.transform)}) references {goValue.transform.name} (parent {goValue.transform.parent})\n";
                            }
                        }
                    }
                }
            }

            foreach (MyBoneSCR scr in car.GetComponentsInChildren<MyBoneSCR>())
            {
                if (scr.stretchToTarget && string.IsNullOrEmpty(scr.StrechToName))
                    referenceIssues += ($"- {scr.name} bone (parent {scr.transform.parent.name}) has StrechToName null\n");
                else if (scr.stretchToTarget && !scr.thisTransform)
                    referenceIssues += ($"- {scr.name} bone (parent {scr.transform.parent.name}) has StrechToName set to {scr.StrechToName} but could not be found\n");
            }

            m_reportText += $"\n---------TRIGGER PARTS FOUND----------------------\n";

            foreach (CarProperties cp in car.GetComponentsInChildren<CarProperties>())
            {
                if (cp.triger)
                {
                    m_reportText += $"Part {cp.PrefabName} has trigger collider set on, checking colliders & status:\n";
                    foreach (Collider c in cp.gameObject.GetComponentsInChildren<Collider>())
                    {
                        m_reportText += $"{Functions.GetTransformPath(c.transform)} - SCALE: {c.transform.localScale}\n";
                    }
                    m_reportText += $"\n";
                }
            }

            m_reportText += $"\n---------MISMATCHED TRIGERS PARTS FOUND----------------------\n";
            bool found = false, otherFound = false;
            foreach (CarProperties cp in car.GetComponentsInChildren<CarProperties>())
            {
                found = false;
                if (cp.SinglePart && !cp.triger)
                {
                    foreach (Collider c in cp.gameObject.GetComponentsInChildren<Collider>())
                    {
                        if (c.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast") && c.transform.parent == cp.transform && !c.isTrigger)
                        {
                            m_reportText += "found " + c.gameObject.name;
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        m_reportText += $"- {cp.PrefabName} - Child colliders found with no triger collider setup reported!\n";
                    }
                }

                if(cp.SinglePart && cp.triger)
                {
                    foreach (Collider c in cp.gameObject.GetComponentsInChildren<Collider>())
                    {
                        if (c.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast") && c.transform.parent == cp.transform && !c.GetComponent<CarProperties>())
                        {
                            otherFound = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        m_reportText += $"- {cp.PrefabName} - Child colliders found with but no CarProperties component found on one of them!\n";
                    }
                }
            }
            m_reportText += "\n";

            return referenceIssues;
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
                                extraInfoReferences += $"- {c.name} | C:{c.GetType().Name} A:{field.Name} ({Functions.GetTransformPath(c.transform)}) references {transformValue.name} (root {transformValue.root.name})\n";
                            }

                        }
                        else if (field.FieldType == typeof(GameObject))
                        {
                            GameObject goValue = (GameObject)field.GetValue(c);
                            if (goValue && goValue.transform && goValue.transform.root && !goValue.transform.root.GetComponent<SPL_Part>())
                            {
                                partsChecks[5] = true;
                                extraInfoReferences += $"- {c.name} | C:{c.GetType().Name} A:{field.Name} ({Functions.GetTransformPath(c.transform)}) references {goValue.transform.name} (root {goValue.transform.root.name})\n";
                            }
                        }
                    }
                }
            }

            if (p.Prefab.GetComponent<Pickup>() || p.Prefab.GetComponent<PickupDoor>())
            {
                int attachingCount = p.Prefab.GetComponentsInChildren<HexNut>().Length + p.Prefab.GetComponentsInChildren<FlatNut>().Length + p.Prefab.GetComponentsInChildren<BoltNut>().Length + p.Prefab.GetComponentsInChildren<WeldCut>().Length;
                if (attachingCount == 0)
                    partsChecks[6] = true;
            }

            bool issuesFound = false;
            for (int i = 0; i < partsChecks.Length; i++)
            {
                if (partsChecks[i])
                {
                    m_anyPartFailed = true;
                    issuesFound = true;
                }
            }

            // Generate report text
            string prefabGenOkString = " ";
            if (p.PartType == PartTypes.DUMMY_PREFABGEN)
            {
                prefabGenOkString = p.PrefabGenLoaded ? " (OK) " : " (NOT OK) ";
            }

            string resultText = $"------------------------------------------------------------------------------\n- {p.CarProps.PrefabName} - {p.GetTypeName()}{prefabGenOkString}- Possible issues:";

            for (int i = 0; i < partsChecks.Length; i++)
            {
                if (showOnlyWrong)
                {
                    if (!partsChecks[i]) continue;
                    resultText += $"\n  - {GetPartReportName(i)}";
                    if (i == 5 && partsChecks[i]) // Handle special referencing showing
                    {
                        resultText += $"\nExtra information: \n" + extraInfoReferences;
                    }
                }
                else
                {
                    resultText += $"\n  - {GetPartReportName(i)} - Result: " + (partsChecks[i] ? "FAILED" : "Ok");
                    if (i == 5 && partsChecks[i]) // Handle special referencing showing
                    {
                        resultText += $"\nExtra information about this test: \n" + extraInfoReferences;
                    }
                }
            }

            if (p.IssueExternalReport)
            {
                resultText += $"\n  - Reported issues in part from other ModUtils modules: \n" + p.ReportedIssue;
            }

            if (!issuesFound && !p.IssueExternalReport && showOnlyWrong) return "";

            return resultText;
        }
        public string GetPartReportName(int id)
        {
            switch (id)
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
        public void WriteToFile()
        {
            File.WriteAllText("./Mods/__ModReport.txt", m_reportText);

            GameObject.Destroy(gameObject);
        }
    }
}
