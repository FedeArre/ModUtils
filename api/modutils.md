# ModUtils

Static class that contains useful methods.

# Methods <!-- {docsify-ignore} -->

Method | Description
----- | -----------
GetPlayer | Returns the current Player GameObject
GetPlayerTools | Returns the current tools component from the Player GameObject
GetAudios | Returns the AudioManager component
PlaySound | Plays the passed AudioClip to the audio source of the game
PlayCashSound | Plays the cash AudioClip once
GetPlayerCurrentCar | Returns the car the player is currently on, if the player is not in a car returns null
GetNearestCar | Returns the nearest car, if a car is not found returns null

# Events <!-- {docsify-ignore} -->

Event | Description
----- | -----------
PlayerCarChanged | Invoked when the player current car changes