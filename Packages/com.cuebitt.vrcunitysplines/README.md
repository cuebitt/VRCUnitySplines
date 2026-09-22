# VRCUnitySplines

Unity's Splines package isn't whitelisted by VRChat, so this package bakes your splines in the editor instead. Animation clips, prefab scatter, and extruded meshes end up in your world, and only whitelisted runtime components are used.

Open Tools > VRCUnitySplines > Bake Spline, point it at a SplineContainer, press Bake Data, and pick an output. Animation clips are the default. The live Splines components can stay in your scene: they are stripped during builds and disabled in play mode, so you can keep editing and re-baking like you normally would.

Requires Unity 2022.3, the VRChat Worlds SDK 3.10.4 or newer (the tween fallback uses VRCTween), and `com.unity.splines` 2.6.1, which is pulled in automatically. UdonSharp ships with the Worlds SDK. Install through the Creator Companion listing, or clone the repository and open it directly.

Runtime spline editing, `SplineData` channels, and network sync aren't included. The repository README has the full documentation.

MIT.
