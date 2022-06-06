# Localization

SimplePartLoader allows to localize every part individually through the [Part.Localize](api/part/localize.md) function, by default your part name (CarProperties.PartName) is used as part name to show in all the languages.

```csharp
using SimplePartLoader;

public ModMain()
{
    Part examplePart = SPL.LoadPart(MyGreatBundle, "ExamplePart");
    examplePart.CarProps.PartName = "Great part!";
}
```

On the code above, the part will be displayed as "Great part!" in all languages, but there is an exception to this.
```csharp
using SimplePartLoader;

public ModMain()
{
    Part examplePart = SPL.LoadPart(MyGreatBundle, "ExamplePart");
    examplePart.CarProps.PartName = "Great part!";
    examplePart.Localize("English", "Now on english");
}
```

Now, our part will be displayed as "Now on english" on every language since english is the fallback language of SimplePartLoader and will be used if available. You can also localize your part to specific languages.

```csharp
using SimplePartLoader;

public ModMain()
{
    Part examplePart = SPL.LoadPart(MyGreatBundle, "ExamplePart");
    examplePart.CarProps.PartName = "Great part!";
    examplePart.Localize("Portuguese", "Other part");
    examplePart.Localize("English", "Now on english");

    // The part will appear as "Now on english" to all languages except portuguese since it will be shown as "Other part".
}
```

As you might notice, the language identifiers in the game are not written always on english, so this table will help you.

Language | In-game name
------- | ----------
English | English
Portuguese | Portuguese
German | German
Russian | Russian
Hungarian | Hungarian
French | Francais
Spanish | Español
Polish | Polish
Swedish | Swedish
Czech | Čeština
Italian | Italiano