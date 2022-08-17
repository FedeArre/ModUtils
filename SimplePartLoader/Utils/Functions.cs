using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;

namespace SimplePartLoader.Utils
{
    public class Functions
    {
        /// <summary>
        /// Internal usage only, gets a car part from his name and car
        /// </summary>
        /// <param name="partName">The name of the part</param>
        /// <returns>The prefab of the part if exists, null otherwise</returns>
        public static GameObject GetCarPart(string partName)
        {
            GameObject carPart = null;
            foreach (GameObject part in PartManager.gameParts)
            {
                if (part.name == partName)
                {
                    carPart = part;
                    break;
                }
            }

            return carPart;
        }

        /// <summary>
        /// Given a GameObject converts it into a HexNut bolt. The MarkAsNut component is required!
        /// </summary>
        /// <param name="bolt">The GameObject to be converted</param>
        public static void ConvertToHexnut(GameObject bolt)
        {
            bolt.AddComponent<CarProperties>();
            bolt.AddComponent<DISABLER>();
            HexNut hx = bolt.AddComponent<HexNut>();
            MarkAsHexnut mhx = bolt.GetComponent<MarkAsHexnut>();
            bolt.layer = LayerMask.NameToLayer("Bolts");

            bolt.GetComponent<Renderer>().material = ModUtils.NutMaterial;
            
            if (!bolt.GetComponent<BoxCollider>())
                bolt.gameObject.AddComponent<BoxCollider>();

            hx.DontDisableRenderer = mhx.DontDisableRenderer;
            hx.Type = mhx.Type;

            GameObject.Destroy(mhx);
        }

        /// <summary>
        /// Given a GameObject converts it into a FlatNut bolt. The MarkAsFlat component is required!
        /// </summary>
        /// <param name="bolt">The GameObject to be converted</param>
        public static void ConvertToFlatNut(GameObject bolt)
        {
            bolt.AddComponent<CarProperties>();
            bolt.AddComponent<DISABLER>();
            bolt.AddComponent<FlatNut>().tight = true;

            bolt.layer = LayerMask.NameToLayer("FlatBolts");

            bolt.GetComponent<Renderer>().material = ModUtils.NutMaterial;
            
            if (!bolt.GetComponent<BoxCollider>())
                bolt.gameObject.AddComponent<BoxCollider>();

            GameObject.Destroy(bolt.GetComponent<MarkAsFlatnut>());
        }

        /// <summary>
        /// Gets the absolute path to a transform
        /// </summary>
        /// <param name="transform">Transform to get the path of</param>
        /// <returns>A string containing the absolute path. It will never return null</returns>
        public static string GetTransformPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                if (transform.parent == null)
                    return path;

                path = transform.name + "/" + path;
            }

            return null;
        }

        // Function from Unity forums
        /// <summary>
        /// Copies all the component properties from a component to another
        /// </summary>
        /// <param name="other">The target component</param>
        /// <param name="comp">The target component</param>
        public static void CopyComponentData(Component comp, Component other, bool preciseCloning)
        {
            Type type = comp.GetType();

            List<Type> derivedTypes = new List<Type>();
            Type derived = type.BaseType;
            while (derived != null)
            {
                if (derived == typeof(MonoBehaviour))
                {
                    break;
                }
                derivedTypes.Add(derived);
                derived = derived.BaseType;
            }

            IEnumerable<PropertyInfo> pinfos = type.GetProperties(Extension.bindingFlags);

            foreach (Type derivedType in derivedTypes)
            {
                pinfos = pinfos.Concat(derivedType.GetProperties(Extension.bindingFlags));
            }

            if (preciseCloning)
            {
                pinfos = from property in pinfos
                         where !(type == typeof(Rigidbody) && property.Name == "inertiaTensor") // Special case for Rigidbodies inertiaTensor which isn't catched for some reason.
                         select property;
            }
            else
            {
                pinfos = from property in pinfos
                         where !(type == typeof(Rigidbody) && property.Name == "inertiaTensor") // Special case for Rigidbodies inertiaTensor which isn't catched for some reason.
                         where !property.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(ObsoleteAttribute))
                         select property;
            }

            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    if (pinfos.Any(e => e.Name == $"shared{char.ToUpper(pinfo.Name[0])}{pinfo.Name.Substring(1)}"))
                    {
                        continue;
                    }
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }

            IEnumerable<FieldInfo> finfos = type.GetFields(Extension.bindingFlags);

            foreach (var finfo in finfos)
            {

                foreach (Type derivedType in derivedTypes)
                {
                    if (finfos.Any(e => e.Name == $"shared{char.ToUpper(finfo.Name[0])}{finfo.Name.Substring(1)}"))
                    {
                        continue;
                    }
                    finfos = finfos.Concat(derivedType.GetFields(Extension.bindingFlags));
                }
            }

            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }

            finfos = from field in finfos
                     //where field.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(ObsoleteAttribute))
                     select field;
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }
        }
    }
}
