
using UnityEngine;

namespace SimplePartLoader
{
    public class ModMain : Mod
    {
        // Looking for docs? https://fedearre.github.io/my-garage-modding-docs/
        public override string ID => "SimplePartLoader";
        public override string Name => "SimplePartLoader";
        public override string Author => "Federico Arredondo";
        public override string Version => "dev";

        public override void OnLoad()
        {
            PartManager.OnLoadCalled();
        }

        public override void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                // We have to load all our transparents into the game's cars prefabs. If we don't do this our part would not be attachable in any bought car.
                GameObject carList = GameObject.Find("CarsParent");
                GameObject[] cars = carList.GetComponent<CarList>().Cars;

                foreach (GameObject car in cars)
                {
                    Transform[] childs = car.GetComponentsInChildren<Transform>();

                    foreach (Transform child in childs)
                    {
                        Debug.LogError($"DEBUG: {child} in car {car.name}");
                    }
                }
            }
        }
    }
}