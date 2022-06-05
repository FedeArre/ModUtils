# ModUtils

Static class that contains useful methods.

# Methods <!-- {docsify-ignore} -->

Method | Description
----- | -----------
[GetPlayer](api/modutils/getplayer.md) | Returns the current Player GameObject
[GetPlayerTools](api/modutils/getplayertools.md) | Returns the current tools component from the Player GameObject
[GetAudios](api/modutils/getaudios.md) | Returns the AudioManager component
[PlaySound](api/modutils/playsound.md) | Plays the passed AudioClip to the audio source of the game
[PlayCashSound](api/modutils/playcashsound.md) | Plays the cash AudioClip once
[GetPlayerCurrentCar](api/modutils/getplayercurrentcar.md) | Returns the car the player is currently on, if the player is not in a car returns null
[GetNearestCar](api/modutils/getnearestcar.md) | Returns the nearest car, if a car is not found returns null

# Events <!-- {docsify-ignore} -->

Event | Description
----- | -----------
[PlayerCarChanged](api/modutils/events/playercarchanged.md) | Invoked when the player current car changes