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
        GameObject transparentObject;
        GameObject secondaryObject;
        public TransparentData transparentData;

        string dataShown = string.Empty;

        bool editingRotation = false;

        Vector3 actualPos;
        Vector3 actualRot;
        void Start()
        {
            transparentObject = gameObject;
            secondaryObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject.Destroy(secondaryObject.GetComponent<BoxCollider>());
            secondaryObject.transform.SetParent(transparentObject.transform);

            secondaryObject.transform.localPosition = Vector3.zero;
            secondaryObject.transform.localScale = transparentData.Scale;
            secondaryObject.transform.localRotation = Quaternion.identity;

            actualPos = transparentObject.transform.localPosition;
            actualRot = transparentObject.transform.localRotation.eulerAngles;
        }

        void Update()
        {
            // Show data from our transparent object.
            dataShown  = $"{transparentObject.name} data";
            dataShown += $"\nActual mode: {(editingRotation ? "Rotation" : "Position")}";
            dataShown += $"\nMultiplier status: {(Input.GetKey(KeyCode.LeftShift) ? "Pressed" : "Not pressed")}";
            dataShown += $"\nLocal position: {transparentObject.transform.localPosition.ToString("F3")}";
            dataShown += $"\nLocal scale: {transparentObject.transform.localScale.ToString("F3")}";
            dataShown += $"\nLocal rotation: {transparentObject.transform.localRotation.ToString("F3")}"; // F3 means 3 digit precision.

            if (Input.GetKeyDown(KeyCode.Keypad0)) // Multiplier
            {
                editingRotation = !editingRotation;
            } 

            if (Input.GetKeyDown(KeyCode.Keypad1)) // X-
            {
                if (editingRotation)
                    transparentObject.transform.localRotation.eulerAngles.Set(actualRot.x - (Input.GetKey(KeyCode.LeftShift) ? 0.1f : 0.01f), actualRot.y, actualRot.z);
                else
                    transparentObject.transform.localPosition.Set(actualPos.x - (Input.GetKey(KeyCode.LeftShift) ? 0.1f : 0.01f), actualPos.y, actualPos.z);
            }
            else if (Input.GetKeyDown(KeyCode.Keypad3)) // X+
            {
                if (editingRotation)
                    transparentObject.transform.localRotation.eulerAngles.Set(actualRot.x + (Input.GetKey(KeyCode.LeftShift) ? 0.1f : 0.01f), actualRot.y, actualRot.z);
                else
                    transparentObject.transform.localPosition.Set(actualPos.x + (Input.GetKey(KeyCode.LeftShift) ? 0.1f : 0.01f), actualPos.y, actualPos.z);
            }
            else if (Input.GetKeyDown(KeyCode.Keypad4)) // Y-
            {
                if (editingRotation)
                    transparentObject.transform.localRotation.eulerAngles.Set(actualRot.x, actualRot.y - (Input.GetKey(KeyCode.LeftShift) ? 0.1f : 0.01f), actualRot.z);
                else
                    transparentObject.transform.localPosition.Set(actualPos.x, actualPos.y - (Input.GetKey(KeyCode.LeftShift) ? 0.1f : 0.01f), actualPos.z);
            }
            else if (Input.GetKeyDown(KeyCode.Keypad6)) // Y+
            {
                if (editingRotation)
                    transparentObject.transform.localRotation.eulerAngles.Set(actualRot.x, actualRot.y + (Input.GetKey(KeyCode.LeftShift) ? 0.1f : 0.01f), actualRot.z);
                else
                    transparentObject.transform.localPosition.Set(actualPos.x, actualPos.y + (Input.GetKey(KeyCode.LeftShift) ? 0.1f : 0.01f), actualPos.z);
            }
            else if (Input.GetKeyDown(KeyCode.Keypad7)) // Z-
            {
                if (editingRotation)
                    transparentObject.transform.localRotation.eulerAngles.Set(actualRot.x, actualRot.y, actualRot.z - (Input.GetKey(KeyCode.LeftShift) ? 0.1f : 0.01f));
                else
                    transparentObject.transform.localPosition.Set(actualPos.x, actualPos.y, actualPos.z - (Input.GetKey(KeyCode.LeftShift) ? 0.1f : 0.01f));
            }
            else if (Input.GetKeyDown(KeyCode.Keypad9)) // Z+
            {
                if (editingRotation)
                    transparentObject.transform.localRotation.eulerAngles.Set(actualRot.x, actualRot.y, actualRot.z + (Input.GetKey(KeyCode.LeftShift) ? 0.1f : 0.01f));
                else
                    transparentObject.transform.localPosition.Set(actualPos.x, actualPos.y, actualPos.z + (Input.GetKey(KeyCode.LeftShift) ? 0.1f : 0.01f));
            } 
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                secondaryObject.GetComponent<Renderer>().enabled = false;
            }

            if (actualPos != transparentObject.transform.localPosition || actualRot != transparentObject.transform.localRotation.eulerAngles)
            {
                Partinfo[] componentsInChildren = transparentObject.GetComponentsInChildren<Partinfo>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    Partinfo comp = componentsInChildren[i];
                    comp.remove(false);
                    comp.attach();
                }
            }

            actualPos = transparentObject.transform.localPosition;
            actualRot = transparentObject.transform.localRotation.eulerAngles;
        }

        void OnGUI()
        {
            dataShown = GUI.TextArea(new Rect(50, 50, 400, 300), dataShown);
        }
    }
}
