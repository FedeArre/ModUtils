using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    internal class PrefabGenerator : MonoBehaviour
    {
        public string PrefabName;
        public string CopiesFrom;

        public GameObject NewMesh;

        public string PartName;

        public int NewPrice = 0;
        public bool EnablePartOnJunkyard = false;
        public bool EnablePartOnCatalog = true;
        public Texture2D CatalogImage;

        public bool SavingFeatureEnabled;
        public AttachmentTypes AttachmentType = AttachmentTypes.Default;

        public enum AttachmentTypes
        {
            Default,
            Prytool,
            Hand
        }
    }

}
