using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.Objects
{
    internal class ReworkedTransparentEdit : MonoBehaviour
    {
        GameObject secondaryObject;
        public TransparentData transparentData;

        string dataShown = string.Empty;

        bool editingRotation = false;
        int currentlyEditing = 0; // 0: X - 1: Y - 2: Z

        Vector3 actualPos;
        Vector3 actualRot;

        void Start()
        {
            secondaryObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject.Destroy(secondaryObject.GetComponent<BoxCollider>());
            secondaryObject.transform.SetParent(gameObject.transform);

            secondaryObject.transform.localPosition = transparentData.LocalPos;
            secondaryObject.transform.localScale = transparentData.Scale;
            secondaryObject.transform.localRotation = transparentData.LocalRot;

            actualPos = gameObject.transform.localPosition;
            actualRot = gameObject.transform.localRotation.eulerAngles;
        }

        string GetCurrentlyEditingAxis()
        {
            switch (currentlyEditing)
            {
                case 0: return "X";
                case 1: return "Y";
                case 2: return "Z";
            }

            return "brokee";
        }
        void Update()
        {
            // Show data from our transparent object.
            dataShown = $"{gameObject.name} data";
            dataShown += $"\nActual mode: {(editingRotation ? "Rotation" : "Position")}";
            dataShown += $"\nMultiplier status: {(Input.GetKey(KeyCode.LeftShift) ? "Pressed" : "Not pressed")}";
            dataShown += $"\nLocal position: {gameObject.transform.localPosition.ToString("F3")}";
            dataShown += $"\nLocal rotation: {gameObject.transform.localEulerAngles.ToString("F3")}"; // F3 means 3 digit precision.
            dataShown += $"\nLocal scale: {gameObject.transform.localScale.ToString("F3")}";
            dataShown += $"\nCurrently editing axis: {GetCurrentlyEditingAxis()}";

            if (ModMain.Multiplier.WasPressed()) // Multiplier
            {
                editingRotation = !editingRotation;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (currentlyEditing == 0)
                {
                    if (editingRotation)
                        gameObject.transform.localEulerAngles = actualRot - new Vector3((Input.GetKey(KeyCode.LeftShift) ? 1f : 0.1f), 0f, 0f);
                    else
                        gameObject.transform.localPosition = actualPos - new Vector3((Input.GetKey(KeyCode.LeftShift) ? 0.05f : 0.005f), 0f, 0f);
                }
                else if (currentlyEditing == 1)
                {
                    if (editingRotation)
                        gameObject.transform.localEulerAngles = actualRot - new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) ? 1f : 0.1f), 0f);
                    else
                        gameObject.transform.localPosition = actualPos - new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) ? 0.05f : 0.005f), 0f);
                }
                else if (currentlyEditing == 2)
                {
                    if (editingRotation)
                        gameObject.transform.localEulerAngles = actualRot - new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) ? 1f : 0.1f));
                    else
                        gameObject.transform.localPosition = actualPos - new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) ? 0.05f : 0.005f));
                }
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (currentlyEditing == 0)
                {
                    if (editingRotation)
                        gameObject.transform.localEulerAngles = actualRot + new Vector3((Input.GetKey(KeyCode.LeftShift) ? 1f : 0.1f), 0f, 0f);
                    else
                        gameObject.transform.localPosition = actualPos + new Vector3((Input.GetKey(KeyCode.LeftShift) ? 0.05f : 0.005f), 0f, 0f);
                }
                else if (currentlyEditing == 1)
                {
                    if (editingRotation)
                        gameObject.transform.localEulerAngles = actualRot + new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) ? 1f : 0.1f), 0f);
                    else
                        gameObject.transform.localPosition = actualPos + new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) ? 0.05f : 0.005f), 0f);
                }
                else if (currentlyEditing == 2)
                {
                    if (editingRotation)
                        gameObject.transform.localEulerAngles = actualRot + new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) ? 1f : 0.1f));
                    else
                        gameObject.transform.localPosition = actualPos + new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) ? 0.05f : 0.005f));
                }
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (currentlyEditing == 0)
                {
                    if (editingRotation)
                        gameObject.transform.localEulerAngles = actualRot - new Vector3((Input.GetKey(KeyCode.LeftShift) ? 90f : -90f), 0f, 0f);
                    else
                        gameObject.transform.localPosition = actualPos - new Vector3((Input.GetKey(KeyCode.LeftShift) ? 90f : -90f), 0f, 0f);
                }
                else if (currentlyEditing == 1)
                {
                    if (editingRotation)
                        gameObject.transform.localEulerAngles = actualRot - new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) ? 90f : -90f), 0f);
                    else
                        gameObject.transform.localPosition = actualPos - new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) ? 90f : -90f), 0f);
                }
                else if (currentlyEditing == 2)
                {
                    if (editingRotation)
                        gameObject.transform.localEulerAngles = actualRot - new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) ? 90f : -90f));
                    else
                        gameObject.transform.localPosition = actualPos - new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) ? 90f : -90f));
                }
            }
            else if (Input.GetKeyDown(KeyCode.V))
            {
                secondaryObject.GetComponent<Renderer>().enabled = !secondaryObject.GetComponent<Renderer>().enabled;
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                currentlyEditing = 0;
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                currentlyEditing = 1;
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                currentlyEditing = 2;
            }

            if (actualPos != gameObject.transform.localPosition || actualRot != gameObject.transform.localRotation.eulerAngles)
            {
                Partinfo[] componentsInChildren = gameObject.GetComponentsInChildren<Partinfo>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    Partinfo comp = componentsInChildren[i];
                    comp.remove(false);
                    comp.attach();
                }
            }

            actualPos = gameObject.transform.localPosition;
            actualRot = gameObject.transform.localRotation.eulerAngles;
            secondaryObject.transform.localPosition = actualPos;
            secondaryObject.transform.localEulerAngles = actualRot;
        }

        void OnGUI()
        {
            dataShown = GUI.TextArea(new Rect(50, 50, 400, 150), dataShown);
        }
    }
}