# SPL

Static class that contains all the useful methods of SimplePartLoader

# Methods <!-- {docsify-ignore} -->

Method | Description
----- | -----------
[LoadPart](api/spl/loadpart.md) | Given a prefab from an AssetBundle, creates a [Part](api/part.md).
[LoadDummy](api/spl/loaddummy.md) | Allows to load an empty prefab into SPL parts, creating a [Part](api/part.md). Used for [mesh override](guides/mesh_override.md).
[CopyPartToPrefab](api/spl/copyparttoprefab.md) | Clones a game's GameObject prefab into a [Part](api/part.md) that has been loaded using [LoadDummy](api/spl/loaddummy.md).
[ForcePartRegister](api/spl/forcepartregister.md) | Internally registers a part into SPL object list after SPL has been loaded. Used for parts that do not exist on both gamemodes.
[GetPlayer](api/spl/getplayer.md) | Returns the current Player GameObject.
[GetPlayerTools](api/spl/getplayertools.md) | Returns the tools component of the Player. Note that is not the same as the static fields of the tools class.

# Events <!-- {docsify-ignore} -->

Event | Description
----- | -----------
[FirstLoad](api/spl/firstload.md) | Invoked on game's first load only. Used for calling [CopyPartToPrefab](api/spl/copyparttoprefab.md).
[LoadFinish](api/spl.loadfinish.md) | Invoked when game has finished loading. Used for calling [ForcePartRegister](api/spl/forcepartregister.md).