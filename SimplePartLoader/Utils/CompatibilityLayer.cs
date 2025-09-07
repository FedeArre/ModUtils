using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.Utils
{
    public class CompatibilityLayer
    {
        public static Material ConvertMaterial(Material mat)
        {
            if (mat.shader.name != "Standard" &&
                mat.shader.name != "Azerilo/Double Sided Standard" &&
                mat.shader.name != "Standard (Specular setup)")
            {
                return mat;
            }

            // Store original properties before shader change
            var color = mat.color;
            var mainTexture = mat.mainTexture;
            var normalMap = mat.GetTexture("_BumpMap");
            var heightMap = mat.GetTexture("_ParallaxMap");
            var occlusionMap = mat.GetTexture("_OcclusionMap");
            var emissionMap = mat.GetTexture("_EmissionMap");
            var detailMask = mat.GetTexture("_DetailMask");
            var detailAlbedo = mat.GetTexture("_DetailAlbedoMap");
            var detailNormal = mat.GetTexture("_DetailNormalMap");

            // Store scalar properties (with safe property checking)
            var normalScale = mat.HasProperty("_BumpScale") ? mat.GetFloat("_BumpScale") : 1.0f;
            var heightScale = mat.HasProperty("_Parallax") ? mat.GetFloat("_Parallax") : 0.02f;
            var occlusionStrength = mat.HasProperty("_OcclusionStrength") ? mat.GetFloat("_OcclusionStrength") : 1.0f;
            var emissionColor = mat.HasProperty("_EmissionColor") ? mat.GetColor("_EmissionColor") : Color.black;
            var detailNormalScale = mat.HasProperty("_DetailNormalMapScale") ? mat.GetFloat("_DetailNormalMapScale") : 1.0f;
            var uvSec = mat.HasProperty("_UVSec") ? mat.GetFloat("_UVSec") : 0f;

            // Handle different workflow types
            bool isSpecularWorkflow = mat.shader.name == "Standard (Specular setup)";
            bool doubleSided = mat.shader.name == "Azerilo/Double Sided Standard";

            Texture metallicGlossMap = null;
            Texture specGlossMap = null;
            float metallic = 0f;
            float smoothness = 0.5f;
            Color specularColor = Color.white;
            float glossMapScale = 1.0f;
            float smoothnessTextureChannel = 0f; // 0 = Metallic Alpha, 1 = Albedo Alpha

            if (isSpecularWorkflow)
            {
                specGlossMap = mat.GetTexture("_SpecGlossMap");
                specularColor = mat.HasProperty("_SpecColor") ? mat.GetColor("_SpecColor") : Color.white;
                smoothness = mat.HasProperty("_Glossiness") ? mat.GetFloat("_Glossiness") : 0.5f;
                glossMapScale = mat.HasProperty("_GlossMapScale") ? mat.GetFloat("_GlossMapScale") : 1.0f;
            }
            else
            {
                metallicGlossMap = mat.GetTexture("_MetallicGlossMap");
                metallic = mat.HasProperty("_Metallic") ? mat.GetFloat("_Metallic") : 0f;
                smoothness = mat.HasProperty("_Glossiness") ? mat.GetFloat("_Glossiness") : 0.5f;
                glossMapScale = mat.HasProperty("_GlossMapScale") ? mat.GetFloat("_GlossMapScale") : 1.0f;

                // Check if smoothness is in albedo alpha channel
                if (mat.IsKeywordEnabled("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A"))
                {
                    smoothnessTextureChannel = 1f;
                }
            }

            // Store other properties with safe checking
            var cutoff = mat.HasProperty("_Cutoff") ? mat.GetFloat("_Cutoff") : 0.5f;
            var mode = mat.HasProperty("_Mode") ? mat.GetFloat("_Mode") : 0f;
            var specularHighlights = mat.HasProperty("_SpecularHighlights") ? mat.GetFloat("_SpecularHighlights") : 1f;
            var glossyReflections = mat.HasProperty("_GlossyReflections") ? mat.GetFloat("_GlossyReflections") : 1f;

            // Store UV tiling and offset before shader change
            var mainTextureScale = mat.GetTextureScale("_MainTex");
            var mainTextureOffset = mat.GetTextureOffset("_MainTex");

            // Change shader to URP Lit
            mat.shader = Shader.Find("Universal Render Pipeline/Lit");

            // Set workflow mode (0 = Specular, 1 = Metallic)
            mat.SetFloat("_WorkflowMode", isSpecularWorkflow ? 0f : 1f);

            // Set base properties
            if (mainTexture)
            {
                mat.SetTexture("_BaseMap", mainTexture);
            }
            mat.SetColor("_BaseColor", color);

            // Set UV tiling and offset
            mat.SetTextureScale("_BaseMap", mainTextureScale);
            mat.SetTextureOffset("_BaseMap", mainTextureOffset);

            // Set culling mode
            mat.SetFloat("_Cull", doubleSided ? 0 : 2);

            // Handle metallic/specular workflow
            if (isSpecularWorkflow)
            {
                if (specGlossMap)
                {
                    mat.SetTexture("_SpecGlossMap", specGlossMap);
                }
                mat.SetColor("_SpecColor", specularColor);
                mat.SetFloat("_Smoothness", smoothness);
                mat.SetFloat("_GlossMapScale", glossMapScale);
            }
            else
            {
                if (metallicGlossMap)
                {
                    mat.SetTexture("_MetallicGlossMap", metallicGlossMap);
                }
                mat.SetFloat("_Metallic", metallic);
                mat.SetFloat("_Smoothness", smoothness);
                mat.SetFloat("_GlossMapScale", glossMapScale);
                mat.SetFloat("_SmoothnessTextureChannel", smoothnessTextureChannel);
            }

            // Set normal map
            if (normalMap)
            {
                mat.SetTexture("_BumpMap", normalMap);
                mat.SetFloat("_BumpScale", normalScale);
            }

            // Set height map (parallax mapping)
            if (heightMap)
            {
                mat.SetTexture("_ParallaxMap", heightMap);
                mat.SetFloat("_Parallax", heightScale);
            }

            // Set occlusion map
            if (occlusionMap)
            {
                mat.SetTexture("_OcclusionMap", occlusionMap);
                mat.SetFloat("_OcclusionStrength", occlusionStrength);
            }

            // Set emission
            if (emissionMap)
            {
                mat.SetTexture("_EmissionMap", emissionMap);
            }
            mat.SetColor("_EmissionColor", emissionColor);

            // Set detail maps
            if (detailMask)
            {
                mat.SetTexture("_DetailMask", detailMask);
            }
            if (detailAlbedo)
            {
                mat.SetTexture("_DetailAlbedoMap", detailAlbedo);
                // URP uses _DetailAlbedoMapScale, but Standard doesn't have this property
                // Set to 1.0 as default for URP
                mat.SetFloat("_DetailAlbedoMapScale", 1.0f);
            }
            if (detailNormal)
            {
                mat.SetTexture("_DetailNormalMap", detailNormal);
                mat.SetFloat("_DetailNormalMapScale", detailNormalScale);
            }

            // Set specular highlights and environment reflections
            mat.SetFloat("_SpecularHighlights", specularHighlights);
            mat.SetFloat("_EnvironmentReflections", glossyReflections);

            // Set alpha cutoff
            mat.SetFloat("_Cutoff", cutoff);

            // Handle rendering modes
            if (mode == 0) // Opaque
            {
                mat.SetFloat("_Surface", 0); // Opaque
                mat.SetFloat("_Blend", 0);   // Alpha
                mat.SetFloat("_AlphaClip", 0);
                mat.SetFloat("_SrcBlend", 1); // One
                mat.SetFloat("_DstBlend", 0); // Zero
                mat.SetFloat("_ZWrite", 1);
                mat.renderQueue = 2000;
                mat.SetOverrideTag("RenderType", "Opaque");
            }
            else if (mode == 1) // Cutout
            {
                mat.SetFloat("_Surface", 0); // Opaque
                mat.SetFloat("_Blend", 0);   // Alpha
                mat.SetFloat("_AlphaClip", 1);
                mat.SetFloat("_SrcBlend", 1); // One
                mat.SetFloat("_DstBlend", 0); // Zero
                mat.SetFloat("_ZWrite", 1);
                mat.renderQueue = 2450;
                mat.SetOverrideTag("RenderType", "TransparentCutout");
            }
            else if (mode == 2) // Fade
            {
                mat.SetFloat("_Surface", 1); // Transparent
                mat.SetFloat("_Blend", 0);   // Alpha
                mat.SetFloat("_AlphaClip", 0);
                mat.SetFloat("_SrcBlend", 5);  // SrcAlpha
                mat.SetFloat("_DstBlend", 10); // OneMinusSrcAlpha
                mat.SetFloat("_ZWrite", 0);
                mat.renderQueue = 3000;
                mat.SetOverrideTag("RenderType", "Transparent");
            }
            else if (mode == 3) // Transparent
            {
                mat.SetFloat("_Surface", 1); // Transparent
                mat.SetFloat("_Blend", 1);   // Premultiply
                mat.SetFloat("_AlphaClip", 0);
                mat.SetFloat("_SrcBlend", 1);  // One
                mat.SetFloat("_DstBlend", 10); // OneMinusSrcAlpha
                mat.SetFloat("_ZWrite", 0);
                mat.renderQueue = 3000;
                mat.SetOverrideTag("RenderType", "Transparent");
            }

            // Enable/disable keywords based on textures and settings
            if (normalMap)
                mat.EnableKeyword("_NORMALMAP");
            else
                mat.DisableKeyword("_NORMALMAP");

            if (emissionMap || emissionColor != Color.black)
                mat.EnableKeyword("_EMISSION");
            else
                mat.DisableKeyword("_EMISSION");

            if (occlusionMap)
                mat.EnableKeyword("_OCCLUSIONMAP");
            else
                mat.DisableKeyword("_OCCLUSIONMAP");

            if (metallicGlossMap || specGlossMap)
                mat.EnableKeyword("_METALLICSPECGLOSSMAP");
            else
                mat.DisableKeyword("_METALLICSPECGLOSSMAP");

            if (heightMap)
                mat.EnableKeyword("_PARALLAXMAP");
            else
                mat.DisableKeyword("_PARALLAXMAP");

            if (mode == 1) // Cutout
                mat.EnableKeyword("_ALPHATEST_ON");
            else
                mat.DisableKeyword("_ALPHATEST_ON");

            if (mode == 3) // Transparent with premultiply
                mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            else
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");

            if (isSpecularWorkflow)
                mat.EnableKeyword("_SPECULAR_SETUP");
            else
                mat.DisableKeyword("_SPECULAR_SETUP");

            if (smoothnessTextureChannel == 1f)
                mat.EnableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
            else
                mat.DisableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");

            if (specularHighlights == 0f)
                mat.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            else
                mat.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");

            if (glossyReflections == 0f)
                mat.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            else
                mat.DisableKeyword("_ENVIRONMENTREFLECTIONS_OFF");

            // Handle detail maps keywords
            if (detailAlbedo || detailNormal)
            {
                // Standard shader uses _DETAIL_MULX2, URP uses _DETAIL_MULX2 and _DETAIL_SCALED
                // Default to _DETAIL_MULX2 for compatibility
                mat.EnableKeyword("_DETAIL_MULX2");
                mat.DisableKeyword("_DETAIL_SCALED");
            }
            else
            {
                mat.DisableKeyword("_DETAIL_MULX2");
                mat.DisableKeyword("_DETAIL_SCALED");
            }

            return mat;
        }

        public static void UpdateMaterialsOfRenderer(MeshRenderer mr)
        {
            var mats = mr.materials;

            for(int i = 0; i < mats.Length; i++)
            {
                mats[i] = ConvertMaterial(mats[i]);
            }

            mr.materials = mats;
        }
    }
}
