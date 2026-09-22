# VRCUnitySplines

Unity has a built-in [Splines](https://docs.unity3d.com/Packages/com.unity.splines@2.9/manual/index.html) package, but it's not whitelisted by VRChat ([yet](https://feedback.vrchat.com/udon/p/expose-spline-components-for-world-creation)). This package, VRCUnitySplines, allows you to use Unity Splines in a VRChat World project by baking the splines in the editor. This way, only whitelisted runtime components are used.

## How it works

An editor script reads each `SplineContainer` spline through the public Splines API and resamples it in memory: positions, tangents, up vectors, and an arc-length table, all in the container's local space.

For the tween driver and evaluator the bake window copies plain arrays directly onto the components so that Udon can read them at runtime. Clips, scatter, and extruded meshes are baked into their corresponding asset or configuration.

The live Splines components are stripped automatically while a build or upload runs, and disabled around editor play mode, so you can keep them in the scene for further editing.

## Requirements

- Unity 2022.3 with the VRChat Worlds SDK, 3.10.4 or newer (the tween fallback uses VRCTween)
- `com.unity.splines` 2.6.1, pulled in automatically as an editor-only dependency
- UdonSharp, which ships with the Worlds SDK

## Add the package to Creator Companion

Releases publish a VPM listing from this repo, so install and updates flow through VCC:

1. Copy the listing URL: `https://cuebitt.github.io/VRCUnitySplines`
2. Open the Creator Companion, go to Settings, then the Packages tab.
3. Press Add Repository and paste the URL. Confirm and close Settings.
4. Open your world project, press Manage Project, find VRCUnitySplines in the list, and press the plus to install.

## Usage

1. Build your path with GameObject > Spline, like you normally would.
2. Open VRCUnitySplines > Bake Window and point it at the SplineContainer.
3. Press Bake Data. Keep baked followers under the same transform and they track it if the parent moves.
4. Pick an output. Leave the Splines components in place for later edits, they are stripped on build and disabled in play mode automatically.

You can use the `Create Demo` button in the bake window to add a demo spline that you can use to test this package.

## Output

Baked animation clips are the default output of the spline animation baker. These run outside of Udon, so they are efficent and performant when compared to Udon-based per-frame movement.

A VRCTween driver can alternatively be used. This also runs outside Udon, so it should come with a minimal performance cost. You can use this if you'd prefer to move a GameObject programmatically instead of using an Animator. The baked positions are fed into `TweenLocalPath`.

The Udon-based spline animation evaluator is available. You can use this when you want to scrub through the animation or query a position at a distance. This is much slower than the previous two, and is not recommended unless you specifically need it (you probably don't). Leave `driveEveryFrame` off unless you have a reason to use it.

Prefab scatter bakes to plain GameObjects by count or spacing, with seeded offsets and a scale range. Extrude snapshot copies `SplineExtrude` output into a Mesh asset, turning the road or tube into a normal mesh. These are just normal GameObjects and meshes, so they shouldn't introduce any additonal performance cost.

## Not Included

VRCUnitySplines bakes splines

Anything needing live spline math at runtime stays out: `SplineData` channels, knot linking behavior, multi-container path blending, nearest-point queries, and runtime knot edits. Motion is also local-only, like all tween and animation approaches in VRChat. If you need it networked, sync the state yourself and trigger playback on every client.

## License

MIT.
