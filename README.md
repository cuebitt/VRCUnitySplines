# VRCUnitySplines

Unity's Splines package does not survive a VRChat upload. Its types are not whitelisted, and the Burst and Jobs math behind it cannot run in Udon. This repo ships a VPM package that works around that by baking everything in the editor. You build paths with Unity's normal Splines tools, press bake, and the world ends up with plain animation clips, meshes, and prefabs. Nothing upload-unsafe remains.

The package is `com.cuebitt.vrcunitysplines`, currently at 1.0.0. This repo doubles as the dev project: open it in Unity Hub and the package sits embedded under `Packages/`, ready to hack on.

## How it works

A baker reads each `SplineContainer` spline through the public Splines API and resamples it into a `VRCBakedSplineData` asset: positions, tangents, up vectors, and an arc-length table, all in the container's local space. Every output below consumes that asset, never the live spline. When you are done baking, you delete the Splines components from the scene. A build guard stops the upload and names the leftovers if you forget.

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

VCC resolves `com.unity.splines` on its own. If you would rather work from source, clone this repo and open the root folder as a Unity project instead.

## Use it

1. Build your path with GameObject > Spline, like you normally would.
2. Open VRCUnitySplines > Bake Window and point it at the SplineContainer.
3. Press Bake Data. Keep baked followers under the same transform and they track it if the parent moves.
4. Pick an output, then delete the live Splines components before uploading.

No Splines install handy? Press Create Demo in the bake window. It builds a small S-curve bake from scratch so you can try the outputs first.

## Outputs

Baked animation clips are the default way to move things. Frames sit evenly by arc length, so even key timing gives constant speed. Playback runs on a plain Animator with zero Udon, which is exactly what VRChat recommends over per-frame script motion.

The VRCTween driver is the fallback. It feeds baked positions to `TweenLocalPath`, so timing still runs natively. Position only, so reach for it when orientation does not matter or when you want tween controls without clips.

The Udon evaluator is opt-in for runtime control like scrubbing a normalized time or querying a position at a distance. Leave `driveEveryFrame` off unless you have a reason. Udon is hundreds of times slower than C#, so per-frame evaluation is a budget you spend on purpose.

Prefab scatter bakes to plain GameObjects by count or spacing, with seeded offsets and a scale range. Extrude snapshot copies `SplineExtrude` output into a Mesh asset, turning the road or tube into a normal mesh.

## Out of scope in v1

Anything needing live spline math at runtime stays out: `SplineData` channels, knot linking behavior, multi-container path blending, nearest-point queries, and runtime knot edits. Motion is also local-only, like all tween and animation approaches in VRChat. If you need it networked, sync the state yourself and trigger playback on every client.

## Repo layout

- `Packages/com.cuebitt.vrcunitysplines/`: the shippable package. `Runtime/` holds the baked data asset, the tween driver, and the opt-in evaluator. `Editor/` holds the bakers, the bake window, and the build guard. `Tests/Editor/` holds edit-mode tests that run with no Splines package present.
- `Assets/Scenes/`: dev scenes for trying things out.
- `Website/`: landing page source for the listing site.
- `.github/workflows/`: release automation. Run the Build Release action and it zips the package from the version in `package.json`, publishes the release, and rebuilds the listing. New releases need the `PACKAGE_NAME` repo variable set to `com.cuebitt.vrcunitysplines` and Pages set to deploy from GitHub Actions.

## License

MIT, Copyright (c) 2026 Cuebitt. See LICENSE at the repo root.
