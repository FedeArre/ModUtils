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
        
        void Start()
        {
            furnitureRef = (SimplePartLoader.Furniture) FurnitureManager.Furnitures[PrefabName];
            if(furnitureRef == null)
            {
                Debug.Log("[ModUtils/Furniture/Error]: Could not find reference for furniture with prefab name " + PrefabName);
            }
        }

        public void OnDrop()
        {
            if (furnitureRef.BehaveAsFurniture)
                StartCoroutine(FreezeRoutine());
        }

        public void OnPickup()
        {
            StopCoroutine(FreezeRoutine());
        }

        IEnumerator FreezeRoutine()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if(rb)
            {
                yield return new WaitForSeconds(1);
                while(rb.velocity.magnitude > 0.1f || InTrailer)
                {
                    yield return 0;
                }

                rb.isKinematic = true;
            }
        }
        
        public void OnTriggerEnter(Collider other)
	    {
		    if (other.gameObject.name == "InsideCol" && transform.name != "CarLift")
		    {
			    InTrailer = true;
		    }
	    }

	    public void OnTriggerExit(Collider other)
	    {
		    if (other.gameObject.name == "InsideCol")
		    {
			    InTrailer = false;
		    }
	    }
    }
}
