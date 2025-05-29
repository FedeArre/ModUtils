using HarmonyLib;
using SimplePartLoader;
using System;
using System.Collections.Generic;
using UnityEngine;

[HarmonyPatch(typeof(Shader), nameof(Shader.Find))]
public static class UrpShaderCompatibilityHook
{
    private static readonly Dictionary<string, string> ShaderNameMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "Standard", "Universal Render Pipeline/Lit" },
        { "Standard (Specular setup)", "Universal Render Pipeline/Lit" },
        { "Azerilo/Double Sided Standard", "Universal Render Pipeline/Lit" }
    };

    private static readonly Dictionary<string, Shader> CachedShaders = new Dictionary<string, Shader>();

    public static bool Prefix(string name, ref Shader __result)
    {
        if (!ModMain.UrpCompatibility.Checked)
        {
            return true;
        }

        if (string.IsNullOrEmpty(name))
        {
            return true;
        }

        if (ShaderNameMappings.TryGetValue(name, out string urpShaderName))
        {
            Shader shaderToReturn;
            if (CachedShaders.TryGetValue(urpShaderName, out shaderToReturn))
            {
                return shaderToReturn;
            }
            else
            {
                shaderToReturn = Shader.Find(urpShaderName);

                if (shaderToReturn != null)
                {

                    CachedShaders[urpShaderName] = shaderToReturn;
                }
                else
                {
                    return true;
                }
            }

            __result = shaderToReturn;
            return false;
        }

        return true;
    }
}