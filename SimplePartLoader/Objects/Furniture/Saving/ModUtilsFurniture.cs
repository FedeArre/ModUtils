using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.Objects.Furniture.Saving
{
    internal class ModUtilsFurniture : MonoBehaviour
    {
        public string PrefabName;
        public SimplePartLoader.Furniture furnitureRef;
        bool InTrailer = false;

        FixedJoint Joint = null;
        bool CanPickup = false;
        bool PreventFreeze = false;
        
        void Start()
        {
            furnitureRef = (SimplePartLoader.Furniture) FurnitureManager.Furnitures[PrefabName];
            if(furnitureRef == null)
            {
                CustomLogger.AddLine("Furnitures", $"Could not find reference for furniture with prefab name " + PrefabName);
            }
        }

        public void OnDrop()
        {
            if(furnitureRef.TrailerAttaching && Joint)
            {
                return;
            }

            CanPickup = true;
            PreventFreeze = false;
            
            if (furnitureRef.FreezeType == FurnitureGenerator.FreezeTypeEnum.Instant)
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb)
                { 
                    rb.isKinematic = true;
                }
                return;
            }
            
            StartCoroutine(FreezeRoutine());
        }

        public void OnPickup()
        {   
            PreventFreeze = true;
            CanPickup = false;
            
            StopCoroutine(FreezeRoutine());
            
            if (Joint)
                GameObject.Destroy(Joint);
        }

        public void OnBuy()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb)
            {
                rb.isKinematic = false;
                StartCoroutine(FreezeRoutine());
            }
        }
        
        IEnumerator FreezeRoutine()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if(rb)
            {
                yield return new WaitForSeconds(1);
                while(rb.velocity.magnitude > 0.05f || InTrailer)
                {
                    yield return 0;
                }

                if(furnitureRef.FreezeType != FurnitureGenerator.FreezeTypeEnum.NoFreeze && !PreventFreeze)
                    rb.isKinematic = true;

                if(!PreventFreeze)
                    CanPickup = false;
            }
        }
        
        public void OnTriggerEnter(Collider other)
	    {
		    if (CanPickup && other.gameObject.name == "InsideCol" && transform.name != "CarLift")
		    {
                InTrailer = true;
                if (!Joint)
                {
                    Joint = gameObject.AddComponent<FixedJoint>();
                    Joint.connectedBody = other.attachedRigidbody;
                    Joint.enableCollision = false;

                    Rigidbody rb = GetComponent<Rigidbody>();
                    if (rb)
                    {
                        rb.isKinematic = false;
                    }
                }
            }
	    }

	    public void OnTriggerExit(Collider other)
	    {
		    if (other.gameObject.name == "InsideCol")
		    {
                InTrailer = false;
		    }
	    }

        public void ChildDestroyed()
        {
            GameObject.Destroy(this);
        }
    }
}
