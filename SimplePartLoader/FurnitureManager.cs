using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePartLoader
{
    internal class FurnitureManager
    {
        private static Hashtable FurnitureList = new Hashtable();
        
        public static Hashtable Furnitures
        {
            get { return FurnitureList; }
        }

        public static void LoadFurniture()
        {

        }

        public static void SaveFurniture()
        {
            
        }
    }
}
