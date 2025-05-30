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

            var color = mat.color;
            var mainTexture = mat.mainTexture;
            var normalMap = mat.GetTexture("_BumpMap");
            var heightMap = mat.GetTexture("_ParallaxMap");
            var occlusionMap = mat.GetTexture("_OcclusionMap");
            var emissionMap = mat.GetTexture("_EmissionMap");
            var detailMask = mat.GetTexture("_DetailMask");
            var detailAlbedo = mat.GetTexture("_DetailAlbedoMap");
            var detailNormal = mat.GetTexture("_DetailNormalMap");

            var normalScale = mat.GetFloat("_BumpScale");
            var heightScale = mat.GetFloat("_Parallax");
            var occlusionStrength = mat.GetFloat("_OcclusionStrength");
            var emissionColor = mat.GetColor("_EmissionColor");
            var detailNormalScale = mat.GetFloat("_DetailNormalMapScale");
            var detailAlbedoScale = mat.GetFloat("_DetailAlbedoMapScale");

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
                specularColor = mat.GetColor("_SpecColor");
                smoothness = mat.GetFloat("_Glossiness");
                glossMapScale = mat.GetFloat("_GlossMapScale");
            }
            else
            {
                metallicGlossMap = mat.GetTexture("_MetallicGlossMap");
                metallic = mat.GetFloat("_Metallic");
                smoothness = mat.GetFloat("_Glossiness");
                glossMapScale = mat.GetFloat("_GlossMapScale");

                if (mat.IsKeywordEnabled("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A"))
                {
                    smoothnessTextureChannel = 1f;
                }
            }

            var cutoff = mat.GetFloat("_Cutoff");
            var mode = mat.GetFloat("_Mode");
            var specularHighlights = mat.GetFloat("_SpecularHighlights");
            var glossyReflections = mat.GetFloat("_GlossyReflections");

            var mainTextureScale = mat.GetTextureScale("_MainTex");
            var mainTextureOffset = mat.GetTextureOffset("_MainTex");

            mat.shader = Shader.Find("Universal Render Pipeline/Lit");

            mat.SetFloat("_WorkflowMode", isSpecularWorkflow ? 0f : 1f);

            if (mainTexture)
            {
                mat.SetTexture("_BaseMap", mainTexture);
            }
            mat.SetColor("_BaseColor", color);

            mat.SetTextureScale("_BaseMap", mainTextureScale);
            mat.SetTextureOffset("_BaseMap", mainTextureOffset);

            mat.SetFloat("_Cull", doubleSided ? 0 : 2);

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

            if (normalMap)
            {
                mat.SetTexture("_BumpMap", normalMap);
                mat.SetFloat("_BumpScale", normalScale);
            }

            if (heightMap)
            {
                mat.SetTexture("_ParallaxMap", heightMap);
                mat.SetFloat("_Parallax", heightScale);
            }

            if (occlusionMap)
            {
                mat.SetTexture("_OcclusionMap", occlusionMap);
                mat.SetFloat("_OcclusionStrength", occlusionStrength);
            }

            if (emissionMap)
            {
                mat.SetTexture("_EmissionMap", emissionMap);
            }
            mat.SetColor("_EmissionColor", emissionColor);

            if (detailMask)
            {
                mat.SetTexture("_DetailMask", detailMask);
            }
            if (detailAlbedo)
            {
                mat.SetTexture("_DetailAlbedoMap", detailAlbedo);
                mat.SetFloat("_DetailAlbedoMapScale", detailAlbedoScale);
            }
            if (detailNormal)
            {
                mat.SetTexture("_DetailNormalMap", detailNormal);
                mat.SetFloat("_DetailNormalMapScale", detailNormalScale);
            }

            mat.SetFloat("_SpecularHighlights", specularHighlights);
            mat.SetFloat("_EnvironmentReflections", glossyReflections);

            mat.SetFloat("_Cutoff", cutoff);

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
                if (detailAlbedoScale == 2f)
                    mat.EnableKeyword("_DETAIL_MULX2");
                else
                    mat.EnableKeyword("_DETAIL_SCALED");
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
