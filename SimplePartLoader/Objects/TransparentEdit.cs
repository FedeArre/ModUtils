using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    internal class TransparentEdit : MonoBehaviour
    {
        GameObject secondaryObject;
        public TransparentData transparentData;

        string dataShown = string.Empty;

        bool editingRotation = false;

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

        void Update()
        {
            // Show data from our transparent object.
            dataShown  = $"{gameObject.name} data";
            dataShown += $"\nActual mode: {(editingRotation ? "Rotation" : "Position")}";
            dataShown += $"\nMultiplier status: {(Input.GetKey(KeyCode.LeftShift) ? "Pressed" : "Not pressed")}";
            dataShown += $"\nLocal position: {gameObject.transform.localPosition.ToString("F3")}";
            dataShown += $"\nLocal rotation: {gameObject.transform.localEulerAngles.ToString("F3")}"; // F3 means 3 digit precision.
            dataShown += $"\nLocal scale: {gameObject.transform.localScale.ToString("F3")}";

            if (Input.GetKeyDown(KeyCode.Keypad0)) // Multiplier
            {
                editingRotation = !editingRotation;
            } 

            if (Input.GetKeyDown(KeyCode.Keypad1)) // X-
            {
                if (editingRotation)
                    gameObject.transform.localEulerAngles = actualRot - new Vector3( (Input.GetKey(KeyCode.LeftShift) ? 1f : 0.1f), 0f, 0f);
                else
                    gameObject.transform.localPosition = actualPos - new Vector3( (Input.GetKey(KeyCode.LeftShift) ? 0.05f : 0.005f), 0f, 0f);
            }
            else if (Input.GetKeyDown(KeyCode.Keypad2)) // X 90
            {
                if (editingRotation)
                    gameObject.transform.localEulerAngles = actualRot - new Vector3((Input.GetKey(KeyCode.LeftShift) ? 90f : -90f), 0f, 0f);
                else
                    gameObject.transform.localPosition = actualPos - new Vector3((Input.GetKey(KeyCode.LeftShift) ? 90f : -90f), 0f, 0f);
            }
            else if (Input.GetKeyDown(KeyCode.Keypad3)) // X+
            {
                if (editingRotation)
                    gameObject.transform.localEulerAngles = actualRot + new Vector3((Input.GetKey(KeyCode.LeftShift) ? 1f : 0.1f), 0f, 0f);
                else
                    gameObject.transform.localPosition = actualPos + new Vector3((Input.GetKey(KeyCode.LeftShift) ? 0.05f : 0.005f), 0f, 0f);
            }
            else if (Input.GetKeyDown(KeyCode.Keypad4)) // Y-
            {
                if (editingRotation)
                    gameObject.transform.localEulerAngles = actualRot - new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) ? 1f : 0.1f), 0f);
                else
                    gameObject.transform.localPosition = actualPos - new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) ? 0.05f : 0.005f), 0f);
            }
            else if (Input.GetKeyDown(KeyCode.Keypad5)) // Y 90
            {
                if (editingRotation)
                    gameObject.transform.localEulerAngles = actualRot - new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) ? 90f : -90f), 0f);
                else
                    gameObject.transform.localPosition = actualPos - new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) ? 90f : -90f), 0f);
            }
            else if (Input.GetKeyDown(KeyCode.Keypad6)) // Y+
            {
                if (editingRotation)
                    gameObject.transform.localEulerAngles = actualRot + new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) ? 1f : 0.1f), 0f);
                else
                    gameObject.transform.localPosition = actualPos + new Vector3(0f, (Input.GetKey(KeyCode.LeftShift) ? 0.05f : 0.005f), 0f);
            }
            else if (Input.GetKeyDown(KeyCode.Keypad7)) // Z-
            {
                if (editingRotation)
                    gameObject.transform.localEulerAngles = actualRot - new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) ? 1f : 0.1f));
                else
                    gameObject.transform.localPosition = actualPos - new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) ? 0.05f : 0.005f));
            }
            else if (Input.GetKeyDown(KeyCode.Keypad5)) // Z 90
            {
                if (editingRotation)
                    gameObject.transform.localEulerAngles = actualRot - new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) ? 90f : -90f));
                else
                    gameObject.transform.localPosition = actualPos - new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) ? 90f : -90f));
            }
            else if (Input.GetKeyDown(KeyCode.Keypad9)) // Z+
            {
                if (editingRotation)
                    gameObject.transform.localEulerAngles = actualRot + new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) ? 1f : 0.1f));
                else
                    gameObject.transform.localPosition = actualPos + new Vector3(0f, 0f, (Input.GetKey(KeyCode.LeftShift) ? 0.05f : 0.005f));
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                secondaryObject.GetComponent<Renderer>().enabled = !secondaryObject.GetComponent<Renderer>().enabled;
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
