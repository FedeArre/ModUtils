using Rewired;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplePartLoader
{
    public class ModUtils
    {
        private static GameObject Player;
        private static tools PlayerTools;
        private static AudioManager AudioList;
        private static AudioSource Source;
        private static Player RewiredPlayer;
        
        private static MainCarProperties CurrentPlayerCar;

        internal static List<GameObject> Cars;

        public delegate void OnPlayerCarChangeDelegate();
        public static event OnPlayerCarChangeDelegate PlayerCarChanged;

        internal static void OnLoadCalled()
        {
            Player = GameObject.Find("Player");
            PlayerTools = Player.GetComponent<tools>();
            RewiredPlayer = ReInput.players.GetPlayer(0);
            
            GameObject PlayerHand = GameObject.Find("hand");
            AudioList = PlayerHand.GetComponent<AudioManager>();
            Source = PlayerHand.GetComponent<AudioSource>();

            Cars = new List<GameObject>();

            GameObject[] carList = GameObject.Find("CarsParent").GetComponent<CarList>().Cars;
            foreach(GameObject car in carList)
            {
                car.AddComponent<SPL_CarTracking>();
            }

            GameObject dummy = new GameObject("SPL_Dummy");
            dummy.AddComponent<SPL_CarTracking>().AddToAll();
        }

        internal static void UpdatePlayerStatus(bool isOnCar, MainCarProperties mcp = null)
        {
            if (isOnCar)
            {
                CurrentPlayerCar = mcp;
            }
            else
            {
                CurrentPlayerCar = null;
            }

            SPL.DevLog($"UpdatePlayerStatus has changed to {isOnCar} - {mcp}");
            if(PlayerCarChanged != null)
            {
                PlayerCarChanged?.Invoke();
            }
        }

        public static GameObject GetPlayer()
        {
            if(Player)
            {
                return Player;
            }
            else
            {
                Debug.LogError("[ModUtils/Utils/Error]: Tried to use GetPlayer but Player was null. Make sure that you are using it after OnLoad!");
                return null;
            }
        }

        public static tools GetPlayerTools()
        {
            if (PlayerTools)
            {
                return PlayerTools;
            }
            else
            {
                Debug.LogError("[ModUtils/Utils/Error]: Tried to use GetPlayerTools but PlayerTools was null. Make sure that you are using it after OnLoad!");
                return null;
            }
        }

        public static AudioManager GetAudios()
        {
            if (AudioList)
            {
                return AudioList;
            }
            else
            {
                Debug.LogError("[ModUtils/Utils/Error]: Tried to use GetAudios but the audio list was null. Make sure that you are using it after OnLoad!");
                return null;
            }
        }

        public static void PlaySound(AudioClip ac)
        {
            if(ac && Source)
            {
                Source.PlayOneShot(ac);
            }
            else
            {
                Debug.LogError("[ModUtils/Utils/Error]: Tried to use PlaySound but AudioClip / source was null. Make sure that you are using it after OnLoad and have a valid AudioClip!");
            }
        }

        public static Player GetKeyPlayer()
        {
            if (RewiredPlayer != null)
            {
                return RewiredPlayer;
            }
            else
            {
                Debug.LogError("[ModUtils/Utils/Error]: Tried to use GetKeyPlayer but Rewired has not started yet, make sure to GetKeyPlayer this after OnLoad!");
                return null;
            }
        }

        public static void PlayCashSound()
        {
            if (Source)
            {
                Source.PlayOneShot(AudioList.Cash);
            }
        }

        public static MainCarProperties GetPlayerCurrentCar()
        {
            return CurrentPlayerCar;
        }

        public static List<GameObject> GetCars()
        {
            return Cars;
        }

        public static CarDetails GetNearestCar()
        {
            if(Cars.Count == 0)
                return null;

            CarDetails carDetails = new CarDetails();
            GameObject NearestCar = Cars.First();
            float CurrentLowestDistance = float.MaxValue;
            Vector3 PlayerPosition = Player.transform.position;

            foreach(GameObject car in Cars)
            {
                float Distance = Vector3.Distance(PlayerPosition, car.transform.position);
                if (Distance < CurrentLowestDistance)
                {
                    CurrentLowestDistance = Distance;
                    NearestCar = car;
                }
            }

            carDetails.Car = NearestCar.GetComponent<MainCarProperties>();
            carDetails.Distance = CurrentLowestDistance;
            return carDetails;
        }

        public static Vector3 UnshiftCoords(Vector3 coordsToUnshift)
        {
            return coordsToUnshift - PlayerTools.saver.transform.root.position;
        }

        public static Vector3 ShiftCoords(Vector3 coordsToShift)
        {
            return coordsToShift + PlayerTools.saver.transform.root.position;
        }
    }
}
