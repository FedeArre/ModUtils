using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SimplePartLoader.Utils
{
    internal static class Extension
    {
        // From Unity Forums
        public const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;

        // Allows using CopyComponentData as an extension method.
        public static T GetCopyOf<T>(this Component comp, T other, bool preciseCloning) where T : Component
        {
            Type type = comp.GetType();
            if (type != other.GetType()) return null; // type mis-match

            Functions.CopyComponentData(comp, other, preciseCloning);

            return comp as T;
        }

        public static T AddComponent<T>(this GameObject go, T toAdd, bool preciseCloning) where T : Component
        {
            return go.AddComponent(toAdd.GetType()).GetCopyOf(toAdd, preciseCloning) as T;
        }
    }
}
