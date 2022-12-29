using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    public class SaleFurniture
    {
        private Furniture SellingFurniture;
        private Vector3 Position;
        private Vector3 Rotation;
        private SpawnSpot Spot;

        public static readonly Vector3 DEFAULT_MODSHOP_LOCATION = new Vector3(655.4157f, 55.2303f, -43.4625f);
        
        public Furniture Furniture
        {
            get { return SellingFurniture; }
            internal set { SellingFurniture = value; }
        }

        public Vector3 Pos
        {
            get { return Position; }
            set { Position = value; }
        }

        public Vector3 Rot
        {
            get { return Rotation; }
            set { Rotation = value; }
        }
        public SpawnSpot Spawn
        {
            internal get { return Spot; }
            set { Spot = value; }
        }

    }
}
