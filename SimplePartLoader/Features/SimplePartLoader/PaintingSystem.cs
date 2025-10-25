using PaintIn3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    [Obsolete("Please use the new GamePainting API instead. This is kept for compatibility purposes")]
    public class PaintingSystem
    {
        static Material ChromeMaterial = null;
        static Material PaintRustMaterial = null;
        static Material DirtMaterial = null;
        static Material BaseMaterial = null;
        internal static Material CullBaseMaterial = null;
        internal static Shader BackfaceShader = null;
        public enum Types
        {
            FullPaintingSupport = 1,
            OnlyPaint,
            OnlyPaintAndRust,
            OnlyDirt,
            OnlyPaintAndDirt
        }

        public enum PartPaintResolution
        {
            Low = 1,
            Medium = 2,
            High = 3
        }

        public static void CheckHighResolutionPaint(Part p, P3dPaintableTexture texture)
        {
            if(p.Mod != null)
            {
                switch(p.Mod.Settings.PaintResolution)
                {
                    case PartPaintResolution.High:
                        texture.Width = 2048;
                        texture.Height = 2048;
                        break;
                    case PartPaintResolution.Medium:
                        texture.Width = 1024;
                        texture.Height = 1024;
                        break;
                    case PartPaintResolution.Low:
                        texture.Width = 512;
                        texture.Height = 512;
                        break;
                }
            }
        }

        [Obsolete("Do not use this method - Use GamePainting.SetupPart!")]
        public static Material GetDirtMaterial()
        {
            return GamePainting.GetPaintMaterial();
        }

        [Obsolete("Do not use this method - Use GamePainting.SetupPart!")]
        public static Material GetPaintRustMaterial()
        {
            return GamePainting.GetPaintMaterial();
        }

        [Obsolete("Do not use this method - Use GamePainting.GetBlackMaterial!")]
        public static Material GetBodymatMaterial()
        {
            return GetBodymatMaterial(false);
        }

        [Obsolete("Do not use this method - Use GamePainting.GetBlackMaterial!")]
        public static Material GetBodymatMaterial(bool useBackfaceShader = false)
        {
            return GamePainting.GetBlackMaterial();
        }

        [Obsolete("Do not use this method - Use GamePainting.GetChromeMaterial!")]
        public static Material GetChromeMaterial()
        {
            return GamePainting.GetChromeMaterial();
        }

        [Obsolete("Use SetupPart method instead")]
        /// <summary>
        /// Sets the painting materials for a given Part. If index is -1 no material is affected for that type
        /// </summary>
        /// <param name="p">The part to get the painting</param>
        /// <param name="bodymatIndex">Not to be confused with the paintable material. Black material that hides the one-sided paintable material</param>
        /// <param name="paintRustIndex">Should be always at index 0, paintable material</param>
        /// <param name="dirtIndex">Should be always at index 1, dirt material</param>
        public static void SetMaterialsForObject(Part p, int bodymatIndex = -1, int paintRustIndex = -1, int dirtIndex = -1)
        {
            SetMaterialsForObject(p, bodymatIndex, paintRustIndex, dirtIndex, false);
        }

        [Obsolete("Use SetupPart method instead. This method not longer works")]
        public static void SetMaterialsForObject(Part p, int bodymatIndex = -1, int paintRustIndex = -1, int dirtIndex = -1, bool force = true) { }

        public static void SetupPart(Part p, PaintingSystem.Types type, bool dontUseBlack = false)
        {
            switch (type)
            {
                case Types.FullPaintingSupport:
                    SetMaterialsForObject(p, dontUseBlack ? -1 : 2, 0, 1, true);
                    p.EnablePartPainting(type);
                    break;
                case Types.OnlyPaint:
                    SetMaterialsForObject(p, dontUseBlack ? -1 : 2, 0, -1, true);
                    p.EnablePartPainting(type);
                    break;
                case Types.OnlyDirt:
                    SetMaterialsForObject(p, -1, -1, 1, true);
                    p.EnablePartPainting(type);
                    break;
                case Types.OnlyPaintAndDirt:
                    SetMaterialsForObject(p, dontUseBlack ? -1 : 2, 0, 1, true);
                    p.EnablePartPainting(type);
                    break;
                case Types.OnlyPaintAndRust:
                    SetMaterialsForObject(p, dontUseBlack ? -1 : 2, 0, -1, true);
                    p.EnablePartPainting(type);
                    break;
            }
        }

        internal static void TryApplyL2Overwrite(Part p)
        {
            TryApplyL2Overwrite(p.Prefab);
        }

        internal static void TryApplyL2Overwrite(GameObject go)
        {
            L2PaintOverwrite comp = go.GetComponent<L2PaintOverwrite>();
            if (!comp)
                return;

            Material matToChange = go.GetComponent<Renderer>().material;
            if (!matToChange || matToChange.shader.name != "Thunderbyte/RustDirt2Layers URP")
                return;

            if(comp._texcoord)
                matToChange.SetTexture("_texcoord", comp._texcoord);

            if (comp._GrungeMap)
                matToChange.SetTexture("_GrungeMap", comp._GrungeMap);

            if (comp._L1ColorMap)
                matToChange.SetTexture("_L1ColorMap", comp._L1ColorMap);

            if (comp._L1EmissionMap)
                matToChange.SetTexture("_L1EmissionMap", comp._L1EmissionMap);

            if (comp._L1MetallicRustDustSmoothness)
                matToChange.SetTexture("_L1MetallicRustDustSmoothness", comp._L1MetallicRustDustSmoothness);

            if (comp._L1Normal)
                matToChange.SetTexture("_L1Normal", comp._L1Normal);

            if (comp._L2EmissionMap)
                matToChange.SetTexture("_L2EmissionMap", comp._L2EmissionMap);

            if (comp._L2MetallicRustDustSmoothness)
                matToChange.SetTexture("_L2MetallicRustDustSmoothness", comp._L2MetallicRustDustSmoothness);

            if (comp._L2Normal)
                matToChange.SetTexture("_L2Normal", comp._L2Normal);
        }
    }
}
