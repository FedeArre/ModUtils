using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.CarGen
{
    public class CatalogMassUpdate
    {
        internal static Dictionary<string, List<string>> CarChangesToDo = new Dictionary<string, List<string>>();
        internal static Dictionary<string, List<string>> EngineChangesToDo = new Dictionary<string, List<string>>();

        internal static void ApplyChanges()
        {
            foreach(GameObject go in PartManager.gameParts)
            {
                if (CarChangesToDo.ContainsKey(go.name))
                {
                    Partinfo partinfo = go.GetComponent<Partinfo>();
                    if (partinfo)
                    {
                        int sizeBeforeModify = partinfo.FitsToCar.Length;
                        Array.Resize(ref partinfo.FitsToCar, sizeBeforeModify + CarChangesToDo[go.name].Count);
                        foreach(string s in CarChangesToDo[go.name])
                        {
                            partinfo.FitsToCar[sizeBeforeModify] = s;
                            sizeBeforeModify++;
                        }
                    }
                }

                if (EngineChangesToDo.ContainsKey(go.name))
                {
                    Partinfo partinfo = go.GetComponent<Partinfo>();
                    if (partinfo)
                    {
                        int sizeBeforeModify = partinfo.FitsToEngine.Length;
                        Array.Resize(ref partinfo.FitsToEngine, sizeBeforeModify + EngineChangesToDo[go.name].Count);
                        foreach (string s in EngineChangesToDo[go.name])
                        {
                            partinfo.FitsToEngine[sizeBeforeModify] = s;
                            sizeBeforeModify++;
                        }
                    }
                }
            }
        }

        public static void AddCarChanges(Func<Partinfo, bool> condition, string nameToAdd)
        {
            foreach(GameObject go in PartManager.gameParts)
            {
                Partinfo pi = go.GetComponent<Partinfo>();
                if(pi)
                {
                    if(condition(pi) == true)
                    {
                        if (CarChangesToDo.ContainsKey(go.name))
                        {
                            if(!CarChangesToDo[go.name].Contains(nameToAdd))
                                CarChangesToDo[go.name].Add(nameToAdd);
                        }
                        else
                        {
                            CarChangesToDo.Add(go.name, new List<string> { nameToAdd } );
                        }
                    }
                }
            }
        }
        public static void AddEngineChanges(Func<Partinfo, bool> condition, string nameToAdd)
        {
            foreach (GameObject go in PartManager.gameParts)
            {
                Partinfo pi = go.GetComponent<Partinfo>();
                if (pi)
                {
                    if (condition(pi) == true)
                    {
                        if (EngineChangesToDo.ContainsKey(go.name))
                        {
                            if (!EngineChangesToDo[go.name].Contains(nameToAdd))
                                EngineChangesToDo[go.name].Add(nameToAdd);
                        }
                        else
                        {
                            EngineChangesToDo.Add(go.name, new List<string> { nameToAdd });
                        }
                    }
                }
            }
        }
    }
}
