# [Part](api/part.md).Localize

*public void Localize(string language, string newTranslation)*

#### Description
Allows to localize a part to the specified language.

#### Parameters
Name | Description | Type | Default
---- | ---- | ----  | ---- 
language | The language of the translation | string | None
newTranslation | The translated string for the specified language | string | None

#### Example
```csharp
using SimplePartLoader;

public ModMain()
{
    Part example_part = SPL.LoadPart(MyAssetBundle, "ExamplePart");
    // If we don't localize our part SPL will automatically handle this by setting the translation for every language to the current CarProperties.PartName
    example_part.CarProps.PartName = "Beatiful part"; // Now every language will see the part as "Beatiful part".

    example_part.Localize("English", "Example part"); // Now every language will see the part as "Example part" since english is the fallback language.

    example_part.Localize("Polish", "Great part"); // Now the polish users will see the part as "Great part" while the rest of languages will see "Example part".
}
```