﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SimplePartLoader.Utils
{
    public static class Extension
    {
        // From Unity Forums
        public const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;

        public static T GetCopyOf<T>(this Component comp, T other) where T : Component
        {
            Type type = comp.GetType();
            if (type != other.GetType()) return null; // type mis-match

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

            IEnumerable<PropertyInfo> pinfos = type.GetProperties(bindingFlags);

            foreach (Type derivedType in derivedTypes)
            {
                pinfos = pinfos.Concat(derivedType.GetProperties(bindingFlags));
            }

            pinfos = from property in pinfos
                     where !(type == typeof(Rigidbody) && property.Name == "inertiaTensor") // Special case for Rigidbodies inertiaTensor which isn't catched for some reason.
                     where !property.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(ObsoleteAttribute))
                     select property;
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

            IEnumerable<FieldInfo> finfos = type.GetFields(bindingFlags);

            foreach (var finfo in finfos)
            {

                foreach (Type derivedType in derivedTypes)
                {
                    if (finfos.Any(e => e.Name == $"shared{char.ToUpper(finfo.Name[0])}{finfo.Name.Substring(1)}"))
                    {
                        continue;
                    }
                    finfos = finfos.Concat(derivedType.GetFields(bindingFlags));
                }
            }

            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }

            finfos = from field in finfos
                     where field.CustomAttributes.Any(attribute => attribute.AttributeType == typeof(ObsoleteAttribute))
                     select field;
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }

            return comp as T;
        }

        public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
        {
            return go.AddComponent(toAdd.GetType()).GetCopyOf(toAdd) as T;
        }
    }
}
