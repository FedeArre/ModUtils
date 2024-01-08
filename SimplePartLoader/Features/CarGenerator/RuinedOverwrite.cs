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
            StartCoroutine(OverwriteExistingRuined());
            /*int value = UnityEngine.Random.Range(1, 4);
            if (value != 2 || MainCarGenerator.RuinedCars.Count == 0)
            {
                GameObject.Destroy(gameObject);
            }
            else
            {
                StartCoroutine(OverwriteExistingRuined());
            }*/
        }

        IEnumerator OverwriteExistingRuined()
        {
            // Await 7 frames
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            GameObject carToSpawn = MainCarGenerator.RuinedCars[UnityEngine.Random.Range(0, MainCarGenerator.RuinedCars.Count)].carPrefab;

            if (CustomLogger.DebugEnabled)
                CustomLogger.AddLine("RuinedFind", "Replacing existing ruined find for mod car " + carToSpawn);

            MainCarProperties[] cars = UnityEngine.Object.FindObjectsOfType<MainCarProperties>();
            foreach (MainCarProperties mcp in cars)
            {
                if (mcp.Owner == "None")
                {
                    Vector3 creationPos = mcp.SpawnPoint + new Vector3(0f, 3.5f, 0f);
                    GameObject.Destroy(mcp.gameObject);

                    yield return new WaitForEndOfFrame();

                    GameObject instanciated = GameObject.Instantiate(carToSpawn, creationPos, Quaternion.identity);
                    MainCarProperties mcpNew = instanciated.GetComponent<MainCarProperties>();
                    mcpNew.SpawnPoint = creationPos;
                    mcpNew.CreatingRuinedFind();

                    yield return new WaitForEndOfFrame();
                    yield return new WaitForEndOfFrame();
                    yield return new WaitForEndOfFrame();
                    yield return new WaitForEndOfFrame();

                    instanciated.transform.position = creationPos;
                    Debug.Log(creationPos);
                    break;
                }
            }

            GameObject.Destroy(gameObject);
        }
    }
}
