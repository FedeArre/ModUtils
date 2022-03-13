# [Part](api/part.md).UsePrytoolAttachment

*public void UsePrytoolAttachment()*

#### Description
Makes a part use the prytool attachment system from the game.

#### Example
```csharp
using SimplePartLoader;

public ModMain()
{
    Part example_part = SPL.LoadPart(MyAssetBundle, "ExamplePart");
    example_part.UsePrytoolAttachment();

    // Note that if a part has any WeldCut / HexNut / FlatNut it will be deleted.
}
```