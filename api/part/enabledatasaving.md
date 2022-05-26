# [Part](api/part.md).EnableDataSaving

*public void EnableDataSaving()*

#### Description
Makes a part use the data saving feature.

#### Example
```csharp
using SimplePartLoader;

public ModMain()
{
    Part example_part = SPL.LoadPart(MyAssetBundle, "ExamplePart");
    example_part.EnableDataSaving();

    // If you are using dummy parts, use EnableDataSaving AFTER copying an object into your dummy!
    // Example:
    // SPL.CopyPartToPrefab(example_part, "TrunkDoor06");
    // example_part.EnableDataSaving();
}
```