using SimplePartLoader.Objects.Furniture.Saving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.Objects.Furniture
{
    internal class PlayerFurniturePickup : MonoBehaviour
    {
        Transform PlayerHand;
        Transform CurrentlyHoldingFurniture = null;

        string LookText;
        string PriceToShow;
        string TipToShow;

        bool ShowText;
        
        TMPro.TMP_Text LookingText;
        UnityEngine.UI.Text PriceText;
        TMPro.TMP_Text TipsText;

        void Start()
        {
            PlayerHand = GameObject.Find("hand").transform;

            LookingText = ModUtils.GetPlayerTools().LookingText;
            PriceText = ModUtils.GetPlayerTools().PriceText;
            TipsText = ModUtils.GetPlayerTools().TipsText;
        }

        void Update()
        {
            RaycastHit rcHit;

            if ((tools.tool == 22 || tools.tool == 1) && tools.helditem == "Nothing")
            {
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out rcHit, 2f, ~LayerMask.NameToLayer("Items")))
                {
                    Debug.Log(rcHit.collider);
                    if (rcHit.collider.transform.name.StartsWith("MODUTILS_FURNITURE_")) // If looking at ModUtils furniture
                    {
                        // TODO: Show furniture data here.

                        if(Input.GetMouseButtonDown(0))
                        {
                            Debug.Log("AAAA");
                            if (rcHit.collider.transform.name[19] == 'F' && tools.tool != 22)
                                return;

                            PlayerHand.position = rcHit.point;
                            Debug.Log("X2D");
                            CurrentlyHoldingFurniture = rcHit.collider.transform;
                            CurrentlyHoldingFurniture.SetParent(PlayerHand);
                            CurrentlyHoldingFurniture.GetComponent<Rigidbody>().isKinematic = true;
                            CurrentlyHoldingFurniture.GetComponent<ModUtilsFurniture>().OnPickup();
                            Debug.Log("XD");
                            foreach (Collider c in CurrentlyHoldingFurniture.GetComponentsInChildren<Collider>())
                            {
                                if (c.name.StartsWith("MODUTILS_FURNITURECOLL_") || c.name.StartsWith("MODUTILS_FURNITURE_"))
                                    c.enabled = false;
                            }
                            Debug.Log("XDD");
                        }
                    }
                    else if(rcHit.collider.transform.name.StartsWith("MODUTILS_FURNITURECOLL_"))
                    {
                        
                    }
                }
            }

            Debug.Log("A: " + CurrentlyHoldingFurniture);
            if (Input.GetMouseButtonUp(0) && CurrentlyHoldingFurniture) // Furniture dropped
            {
                Debug.Log("ButtonLeft");
                foreach (Collider c in CurrentlyHoldingFurniture.GetComponentsInChildren<Collider>())
                {
                    if (c.name.StartsWith("MODUTILS_FURNITURECOLL_") || c.name.StartsWith("MODUTILS_FURNITURE_"))
                        c.enabled = true;
                }

                CurrentlyHoldingFurniture.SetParent(null);
                CurrentlyHoldingFurniture.GetComponent<Rigidbody>().isKinematic = false;
                CurrentlyHoldingFurniture.GetComponent<ModUtilsFurniture>().OnDrop();
                CurrentlyHoldingFurniture = null;
            }
        }
    }
}
