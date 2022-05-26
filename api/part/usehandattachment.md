# [Part](api/part.md).UseHandAttachment

*public void UseHandAttachment()*

#### Description
Makes a part use the hand attachment system from the game.

#### Example
```csharp
using SimplePartLoader;

public ModMain()
{
    Part example_part = SPL.LoadPart(MyAssetBundle, "ExamplePart");
    example_part.UseHandAttachment();

    // Note that if a part has any WeldCut / HexNut / FlatNut / BoltNut it will be deleted.
}
```