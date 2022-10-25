using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.Objects
{
    internal class LightFix : MonoBehaviour
    {
        void Start()
        {
            CarLight cl = gameObject.GetComponent<CarLight>();
            if (!cl)
                return;

            if(!cl.transform.parent || cl.transform.parent.GetComponent<transparents>())
                cl.LightRend = cl.transform.GetComponent<MeshRenderer>();
            else
                cl.LightRend = cl.transform.parent.GetComponent<MeshRenderer>();
            
            if (cl.High)
            {
                Transform t = cl.transform.parent.Find("High Beam");
                if (t)
                {
                    cl.High = t.GetComponent<Light>();
                }
            }

            if (cl.Low)
            {
                Transform t = cl.transform.parent.Find("Low Beam");
                if (t)
                {
                    cl.Low = t.GetComponent<Light>();
                }
            }

            GameObject.Destroy(this);
        }
    }
}
