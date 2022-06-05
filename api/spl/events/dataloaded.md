# [SPL](api/spl.md).DataLoaded

*public static event DataLoadedDelegate DataLoaded*

#### Description
Event called when SimplePartLoader loads the custom data saved of parts. This callback is called a frame after Continue() so you can already interact with your parts (As shown in the example)

#### Example
```csharp
using SimplePartLoader;

Part ExtraBattery;

public ModMain()
{
    ExtraBattery = SPL.LoadDummy(MyAssetBundle, "ExtraBatteryForCar");

    SPL.DataLoaded += OnDataLoad;
}

public void OnDataLoad()
{
    // We loop through all the objects that have the custom data feature enabled.
    foreach (SaveData data in UnityEngine.Object.FindObjectsOfType<SaveData>())
    {
        // We check if this data is from our object.
        if(data.PrefabName == "ExtraBatteryForCar")
        {
            // We check if the data exists on our object.
            if(data.Data.ContainsKey("BatteryCharge"))
            {
                // We assign the data to our object.
                data.gameObject.GetComponent<BatteryComponent>().BatteryChargeLevel = Int32.Parse(data.Data["BatteryCharge"]);
            }
        }
    }
}
```