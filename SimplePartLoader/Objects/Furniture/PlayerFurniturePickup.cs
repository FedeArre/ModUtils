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
        bool DoingHack = false, HackLastFrame = false;
        
        void LateUpdate()
        {
            RaycastHit rcHit;
            if (tools.tool == 1 || DoingHack)
            {
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out rcHit, 2f, ~LayerMask.NameToLayer("Items")))
                {
                    if (rcHit.collider.transform.root.name.StartsWith("MODUTILS_FURNITURE_H_"))
                    {
                        DoingHack = true;
                    }
                    else if (DoingHack)
                        DoingHack = false;
                }
                else if (DoingHack)
                    DoingHack = false;
            }

            if (DoingHack)
            {
                tools.tool = 22;
                HackLastFrame = true;
            }

            if (HackLastFrame && !DoingHack)
            {
                tools.tool = 1;
                HackLastFrame = false;
            }
        }
    }
}
