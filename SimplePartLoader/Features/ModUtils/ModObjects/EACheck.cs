using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    [DisallowMultipleComponent]
    internal class EACheck : MonoBehaviour
    {
        internal int frameCount = 0;
        internal bool checkDone = false;
        void Update()
        {
            if (checkDone)
                return;
            
            frameCount++;
            if (!SteamManager.Initialized)
                return;
            
            ulong steamID = Steamworks.SteamUser.GetSteamID().m_SteamID;
            Debug.Log("[ModUtils/EACheck]: Identified user: " + steamID);
            
            foreach (ModInstance mi in ModUtils.ModInstances)
            {
                if (mi.RequiresSteamCheck)
                {
                    mi.Check(steamID);
                }
            }

            checkDone = true;
        }
    }
}
