using SimplePartLoader.CarGen;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader.Features.CarGenerator
{
    internal class RuinedOverwrite : MonoBehaviour
    {
        void Start()
        {
            int value = UnityEngine.Random.Range(1, 4);
            if (value != 2 || MainCarGenerator.RuinedCars.Count == 0)
            {
                GameObject.Destroy(gameObject);
            }
            else
            {
                StartCoroutine(OverwriteExistingRuined());
            }
        }

        IEnumerator OverwriteExistingRuined()
        {
            yield return 0;
            yield return 0;

            GameObject carToSpawn = MainCarGenerator.RuinedCars[UnityEngine.Random.Range(0, MainCarGenerator.RuinedCars.Count)].carPrefab;

            if (CustomLogger.DebugEnabled)
                CustomLogger.AddLine("RuinedFind", "Replacing existing ruined find for mod car");

            MainCarProperties[] cars = UnityEngine.Object.FindObjectsOfType<MainCarProperties>();
            foreach (MainCarProperties mcp in cars)
            {
                if (mcp.Owner == "None")
                {
                    Vector3 creationPos = mcp.transform.position + new Vector3(0f, 3.5f, 0f);
                    GameObject.Destroy(mcp);

                    GameObject instanciated = GameObject.Instantiate(carToSpawn, creationPos, Quaternion.identity);
                    MainCarProperties mcpNew = instanciated.GetComponent<MainCarProperties>();
                    mcpNew.CreatingRuinedFind();
                    mcpNew.SpawnPoint = creationPos;

                    break;
                }
            }

            GameObject.Destroy(gameObject);
        }
    }
}
