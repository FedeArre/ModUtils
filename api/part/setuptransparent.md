# [Part](api/part.md).SetupTransparent

*public void SetupTransparent(string attachesTo, Vector3 transparentLocalPos, Quaternion transaprentLocalRot, bool testingModeEnable = false)*

*public void SetupTransparent(string attachesTo, Vector3 transparentLocalPos, Quaternion transaprentLocalRot, Vector3 scale, bool testingModeEnable = false)*

#### Description
Creates a transparent (Attachment point) in the indicated part on the given position.

#### Parameters
Name | Description | Type | Default
---- | ---- | ----  | ---- 
attachesTo | Name of the game object that the transparent will be attached to | string | None
transparentLocalPos | The local position of the transparent | Vector3 | None
transparentLocalRot | The local rotation of the transparent | Quaternion | None
scale | The scale of the transparent. Is not recommended to change but may be required on certain scenarios. | Vector3 | Vector3.one
testingModeEnable | Enable the transparent in-game editor for the transparent | bool | false

#### Example
```csharp
using SimplePartLoader;

public ModMain()
{
    Part example_part = SPL.LoadPart(MyAssetBundle, "ExamplePart");
    example_part.SetupTransparent("TrunkDoor06", Vector3.up, Quaternion.identity);
}
```