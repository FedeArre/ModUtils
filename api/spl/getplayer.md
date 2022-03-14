# [SPL](api/spl.md).GetPlayer

*public static GameObject GetPlayer()*

#### Description
Small shortcut that returns the current Player GameObject. Can be used on OnLoad method.

#### Example
```csharp
using SimplePartLoader;

public override void OnLoad()
{
    // Both return the same GameObject
    // Note that SPL caches the Player object so is faster than searching the object.
    Debug.LogError(GameObject.Find("Player"));
    Debug.LogError(SPL.GetPlayer());
}
```