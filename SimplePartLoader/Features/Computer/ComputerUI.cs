using EnviroSamples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SimplePartLoader;
using SimplePartLoader.Objects.Furniture;

namespace SimplePartLoader.Features
{
    public class ComputerUI
    {
        internal static GameObject UI_Prefab, ComputerModelPrefab, UI_ErrorPrefab;

        internal static GameObject CurrentUI, Computer, NewTable;
        internal static Canvas UI_Canvas;

        public static bool PlayerAtComputer { get; internal set; }

        internal static void SetupComputer(GameObject windowPrefab, GameObject iconPrefab)
        {
            ComputerLogic.RegisterApp("App launcher", windowPrefab, iconPrefab, true);
        }

        internal static void LoadComputerTable()
        {
            GameObject table = GameObject.Find("Desk Phone").transform.root.gameObject;
            NewTable = GameObject.Instantiate(table);

            GameObject.Destroy(NewTable.GetComponent<SavePosition>());

            for(int i = 0; i < NewTable.transform.childCount; i++)
                GameObject.Destroy(NewTable.transform.GetChild(i).gameObject);

            NewTable.transform.position = new Vector3(161.8864f, 62.4578f, 554.8682f);
            NewTable.transform.eulerAngles = new Vector3(0f, 90.0872f, 0f);
            NewTable.transform.localScale = new Vector3(0.24f, 0.55f, 0.36f);

            Computer = GameObject.Instantiate(ComputerModelPrefab);
            Computer.transform.SetParent(NewTable.transform);
            NewTable.name = "Computer table";

            Computer.transform.localPosition = new Vector3(-1.8f, 1.2f, 0.6f);
            Computer.transform.localEulerAngles = new Vector3(270f, 270f, 0f);
            Computer.transform.localScale = new Vector3(0.005f, 0.01f, 0.005f);

            Computer.transform.Find("ModUtilsComputer").gameObject.layer = LayerMask.NameToLayer("Items");
        }

        internal static void Open()
        {
            if(!CurrentUI)
            {
                CurrentUI = GameObject.Instantiate(UI_Prefab);
                UI_Canvas = CurrentUI.GetComponent<Canvas>();
                UI_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                ComputerLogic.Setup(CurrentUI);
            }

            CurrentUI.SetActive(true);

            PlayerAtComputer = true;

            ModUtils.Player.GetComponent<FirstPersonAIO>().ControllerPause();
            ModUtils.CursorCanvas.SetActive(false);
        }

        internal static void Close()
        {
            PlayerAtComputer = false;

            ComputerLogic.OnComputerScreenClose();

            ModUtils.Player.GetComponent<FirstPersonAIO>().ControllerUnPause();
            ModUtils.CursorCanvas.SetActive(true);

            CurrentUI.SetActive(false);

            /*UI_Canvas.renderMode = RenderMode.WorldSpace;
            //UI_Canvas.worldCamera = Camera.main;

            CurrentUI.transform.SetParent(Computer.transform);
            CurrentUI.transform.localPosition = new Vector3(-71.5573f, 152.962f, 152.7598f);
            CurrentUI.transform.localEulerAngles = new Vector3(0f, 270f, 270f);
            CurrentUI.transform.localScale = new Vector3(0.153f, 0.19f, 1f);*/
        }

        internal static void RaycastHandler(RaycastHit rcHit)
        {
            if (PlayerAtComputer)
                return;
            
            if(rcHit.collider.name == "ModUtilsComputer")
            {
                PlayerFurniturePickup.Instance.TipToShow = "Press E to use the computer";
                PlayerFurniturePickup.Instance.ShowText = true;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Open();
                }
            }
        }

        internal static void Continue()
        {
            if (DataHandler.GetData("ModUtils_ComputerPos_X") != null)
            {
                float pX = (float) Convert.ChangeType(DataHandler.GetData("ModUtils_ComputerPos_X"), typeof(float));
                float pY = (float) Convert.ChangeType(DataHandler.GetData("ModUtils_ComputerPos_Y"), typeof(float));
                float pZ = (float) Convert.ChangeType(DataHandler.GetData("ModUtils_ComputerPos_Z"), typeof(float));

                float rX = (float) Convert.ChangeType(DataHandler.GetData("ModUtils_ComputerRot_X"), typeof(float));
                float rY = (float) Convert.ChangeType(DataHandler.GetData("ModUtils_ComputerRot_Y"), typeof(float));
                float rZ = (float) Convert.ChangeType(DataHandler.GetData("ModUtils_ComputerRot_Z"), typeof(float));

                NewTable.transform.position = new Vector3(pX, pY, pZ);
                NewTable.transform.eulerAngles = new Vector3(rX, rY, rZ);
            }
        }

        internal static void Save()
        {
            if (!NewTable)
            {
                DataHandler.RemoveData("ModUtils_ComputerPos_X");
                DataHandler.RemoveData("ModUtils_ComputerPos_Y");
                DataHandler.RemoveData("ModUtils_ComputerPos_Z");
                DataHandler.RemoveData("ModUtils_ComputerRot_X");
                DataHandler.RemoveData("ModUtils_ComputerRot_Y");
                DataHandler.RemoveData("ModUtils_ComputerRot_Z");
                return;
            }

            Vector3 pos = ModUtils.UnshiftCoords(NewTable.transform.position);

            DataHandler.AddData("ModUtils_ComputerPos_X", pos.x);
            DataHandler.AddData("ModUtils_ComputerPos_Y", pos.y);
            DataHandler.AddData("ModUtils_ComputerPos_Z", pos.z);

            DataHandler.AddData("ModUtils_ComputerRot_X", NewTable.transform.eulerAngles.x);
            DataHandler.AddData("ModUtils_ComputerRot_Y", NewTable.transform.eulerAngles.y);
            DataHandler.AddData("ModUtils_ComputerRot_Z", NewTable.transform.eulerAngles.z);
        }
    }
}
