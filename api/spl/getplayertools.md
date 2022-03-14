# [SPL](api/spl.md).GetPlayerTools

*public static tools GetPlayerTools()*

#### Description
Returns the current tools component in the Player GameObject. Can be used on OnLoad method.

#### Example
```csharp
using SimplePartLoader;

public override void OnLoad()
{
    // Both return the same component
    // Note that SPL caches the component so is faster than looking for the Player and getting the component.
    Debug.LogError(GameObject.Find("Player").GetComponent<tools>());
    Debug.LogError(SPL.GetPlayerTools());

    // Also, do not confuse with the static variables from tools class!
    tools.money = 123; // Valid code
    SPL.GetPlayerTools().money = 123; // Invalid code
}
```