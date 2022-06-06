# First steps

*Note that the guides expect you to have knowledge on both Unity and C#. Doing mods for a game is probably the worst way to start learning how to code!*
*Is highly recommended to also read [How parts work](guides/how_parts_work.md) to understand more about the game*

To start to develop mods using SimplePartLoader you will need to download both the version for users (.dll) and also the developer files (This is a Unity project containing all the basics thing you need to start working). You can find everything on the [downloads section](downloads.md).

Having both files downloaded you just need to add a reference to the SimplePartLoader (The .dll) into your mod project and you are ready to go!

![](../images/first_steps/1.png)

![](../images/first_steps/2.png)

![](../images/first_steps/3.png)

After adding the reference in your project, you just need to add the following on the top and you are ready to start using the SimplePartLoader features!

```csharp
using SimplePartLoader;
```