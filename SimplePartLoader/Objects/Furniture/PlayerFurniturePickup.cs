using Assets.SimpleLocalization;
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
        string EngineText;

        bool ShowText;

        UnityEngine.UI.Text PriceText;
        TMPro.TMP_Text LookingText;
        TMPro.TMP_Text TipsText;
        TMPro.TMP_Text FitEngineText;

        LayerMask Items = LayerMask.GetMask("Items");

        Transform LookingLastFrame = null;
        CustomFurnitureSaleItem cachedCsfi = null;

        void Start()
        {
            PlayerHand = GameObject.Find("hand").transform;

            LookingText = ModUtils.GetPlayerTools().LookingText;
            PriceText = ModUtils.GetPlayerTools().PriceText;
            TipsText = ModUtils.GetPlayerTools().TipsText;
            FitEngineText = ModUtils.GetPlayerTools().LookingFitEngineText;
        }

        void Update()
        {
            RaycastHit rcHit;
            ShowText = false;

            if ((tools.tool == 22 || tools.tool == 1) && tools.helditem == "Nothing")
            {
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out rcHit, 2f, Items))
                {
                    if (rcHit.collider.transform.name.StartsWith("MODUTILS_FURNITURE_")) // If looking at ModUtils furniture
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            if (rcHit.collider.transform.name[19] == 'F' && tools.tool != 22)
                                return;

                            PlayerHand.position = rcHit.point;

                            CurrentlyHoldingFurniture = rcHit.collider.transform;
                            CurrentlyHoldingFurniture.SetParent(PlayerHand);
                            CurrentlyHoldingFurniture.GetComponent<Rigidbody>().isKinematic = true;
                            CurrentlyHoldingFurniture.GetComponent<ModUtilsFurniture>().OnPickup();

                            foreach (Collider c in CurrentlyHoldingFurniture.GetComponentsInChildren<Collider>())
                            {
                                if (c.name.StartsWith("MODUTILS_FURNITURECOLL_") || c.name.StartsWith("MODUTILS_FURNITURE_"))
                                    c.enabled = false;
                            }
                        }
                    }
                    else if (rcHit.collider.transform.name.StartsWith("MODUTILS_FURNITURECOLL_")) // If looking at ModUtils furniture children collider
                    {
                        Transform furnitureRoot = rcHit.collider.transform.parent;
                        if (furnitureRoot.name.StartsWith("MODUTILS_FURNITURE_")) // If is a furniture
                        {
                            ShowText = true;
                            LookText = LocalizationManager.Localize(furnitureRoot.name);
                            TipToShow = "";
                            PriceToShow = "";
                            EngineText = "";
                            if(Input.GetMouseButtonDown(0))
                            {
                                if (furnitureRoot.name[19] == 'F' && tools.tool != 22)
                                    return;

                                PlayerHand.position = rcHit.point;

                                CurrentlyHoldingFurniture = furnitureRoot;
                                CurrentlyHoldingFurniture.SetParent(PlayerHand);
                                CurrentlyHoldingFurniture.GetComponent<Rigidbody>().isKinematic = true;
                                CurrentlyHoldingFurniture.GetComponent<ModUtilsFurniture>().OnPickup();

                                foreach (Collider c in CurrentlyHoldingFurniture.GetComponentsInChildren<Collider>())
                                {
                                    if (c.name.StartsWith("MODUTILS_FURNITURECOLL_") || c.name.StartsWith("MODUTILS_FURNITURE_"))
                                        c.enabled = false;
                                }
                            }
                        }
                        else if (furnitureRoot.name.StartsWith("MODUTILS_SALEFURNITURE_"))
                        {
                            CustomFurnitureSaleItem cfsi;

                            if (LookingLastFrame == furnitureRoot)
                                cfsi = cachedCsfi;
                            else
                                cfsi = furnitureRoot.GetComponent<CustomFurnitureSaleItem>();

                            if (cfsi)
                            {
                                ShowText = true;
                                LookText = cfsi.Name;
                                EngineText = cfsi.Tip;
                                TipToShow = "Press left click to buy";
                                PriceToShow = cfsi.Price.ToString("C2");

                                LookingLastFrame = furnitureRoot;
                                cachedCsfi = cfsi;

                                if (Input.GetMouseButtonDown(0))
                                {
                                    cfsi.Buy();
                                }
                            }
                        }
                    }
                    else if (rcHit.collider.transform.name.StartsWith("MODUTILS_SALEFURNITURE_"))
                    {
                        CustomFurnitureSaleItem cfsi;

                        if (LookingLastFrame == rcHit.collider.transform)
                            cfsi = cachedCsfi;
                        else
                            cfsi = rcHit.collider.transform.GetComponent<CustomFurnitureSaleItem>();

                        if (cfsi)
                        {
                            ShowText = true;
                            LookText = cfsi.Name;
                            TipToShow = cfsi.Tip;
                            PriceToShow = cfsi.Price.ToString("C2");

                            LookingLastFrame = rcHit.collider.transform;
                            cachedCsfi = cfsi;

                            if (Input.GetMouseButtonDown(0))
                            {
                                cfsi.Buy();
                            }
                        }
                    }
                    else
                    {
                        LookingLastFrame = null;
                        cachedCsfi = null;
                    }
                }

                if ((Input.GetMouseButtonUp(0) && CurrentlyHoldingFurniture) || (CurrentlyHoldingFurniture && Input.GetKeyDown(KeyCode.Escape))) // Furniture dropped
                {
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
        void LateUpdate()
        {
            if (ShowText)
            {
                LookingText.text = LookText;
                PriceText.text = PriceToShow;
                TipsText.text = TipToShow;
                FitEngineText.text = EngineText;
            }
        }
    }
}