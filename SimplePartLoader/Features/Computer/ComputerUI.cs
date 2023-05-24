using EnviroSamples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SimplePartLoader;

namespace SimplePartLoader.Features
{
    public class ComputerUI
    {
        internal static GameObject UI_Prefab, ComputerModelPrefab;

        internal static GameObject CurrentUI, Computer;

        internal static void LoadComputerTable()
        {
            GameObject table = GameObject.Find("Desk Phone").transform.root.gameObject;
            GameObject newTable = GameObject.Instantiate(table);

            GameObject.Destroy(newTable.GetComponent<SavePosition>());

            for(int i = 0; i < newTable.transform.childCount; i++)
                GameObject.Destroy(newTable.transform.GetChild(i).gameObject);

            newTable.transform.position = new Vector3(161.8864f, 62.4578f, 554.8682f);
            newTable.transform.eulerAngles = new Vector3(0f, 90.0872f, 0f);
            newTable.transform.localScale = new Vector3(0.24f, 0.55f, 0.36f);

            Computer = GameObject.Instantiate(ComputerModelPrefab);
            Computer.transform.SetParent(newTable.transform);
            Computer.transform.localPosition = new Vector3(-1.8f, 1.2f, 0.6f);
            Computer.transform.localEulerAngles = new Vector3(270f, 270f, 0f);
            Computer.transform.localScale = new Vector3(0.005f, 0.01f, 0.005f);

        }

        internal static void Open()
        {
            if (CurrentUI)
                return;

            CurrentUI = GameObject.Instantiate(UI_Prefab);
            CurrentUI.transform.SetParent(Computer.transform);
            CurrentUI.transform.localPosition = new Vector3(-71.5573f, 152.962f, 152.7598f);
            CurrentUI.transform.localEulerAngles = new Vector3(0f, 270f, 270f);
            CurrentUI.transform.localScale = new Vector3(0.25f, 0.32f, 1f);

            Transform camRef = Computer.transform.Find("ComputerPoint");

            ModUtils.Player.GetComponent<FirstPersonAIO>().ControllerPause();

            Camera.main.transform.position = camRef.position;
            Camera.main.transform.rotation = camRef.rotation;
        }
    }
}
