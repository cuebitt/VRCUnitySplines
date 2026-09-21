# VRCUnitySplines

Unity's Splines package does not survive a VRChat upload. The types are not whitelisted, and the Burst/Jobs math it relies on cannot run in Udon. This package works around that by baking everything in the editor. You build your splines with Unity's normal Splines tools, press bake, and what lands in the world is plain clips, meshes, and prefabs. Nothing upload-unsafe remains.

## What you need

- Unity 2022.3 with the VRChat Worlds SDK (3.10.4 or newer, since the tween fallback uses VRCTween)
- `com.unity.splines` 2.6.1, which this package pulls in as an editor-only dependency
- UdonSharp, which ships with the Worlds SDK

## Install

Add the VCC listing for this repo (see the repo website), then install VRCUnitySplines from the Creator Companion. Or clone this repo and open it directly. It is a normal Unity project with the package embedded.

## Quick start

1. Build your path with GameObject > Spline, like you normally would.
2. Open VRCUnitySplines > Bake Window and point it at the SplineContainer.
3. Press Bake Data. You get a `VRCBakedSplineData` asset holding resampled positions, tangents, up vectors, and an arc-length table, all in the container's local space. Keep baked followers under the same transform and they track it if the parent moves.
4. Pick an output below, then delete the live SplineContainer and Splines components from the scene. The build guard stops the upload and names names if you forget.

No Splines install handy? Press Create Demo. It builds a small S-curve bake from scratch so you can try the outputs first.

## Outputs

Baked animation clips are the default way to move things. The baker turns frames into position and rotation keys with linear tangents. Frames are spaced evenly by arc length, so even key timing gives constant speed for free. Playback, looping, and speed all run on a plain Animator. No Udon runs at all, which is the point. This matches what VRChat recommends: native playback instead of per-frame script motion.

The VRCTween driver is the fallback. It feeds the baked positions to `TweenLocalPath` as a CatmullRom path, so timing still runs natively. It only drives position, so use it when orientation does not matter or when you want tween controls (delays, loops, completion callbacks) without clips.

The Udon evaluator (`VRCSplineEvaluator`) is opt-in for the few cases that need runtime control: scrubbing a normalized time, querying a position at a distance, that sort of thing. It lerps baked frames on demand. Leave `driveEveryFrame` off unless you have a reason. Udon runs hundreds of times slower than C#, so per-frame evaluation is a budget you spend deliberately.

Prefab scatter bakes to plain GameObjects. You pick count or spacing, offset and rotation ranges, a scale range, and a seed. The result is deterministic and editable like anything else in the scene.

Extrude snapshot copies Unity's SplineExtrude output into a Mesh asset and points `targetMesh` at it, so the road or tube becomes a normal mesh. Configure the extrude in Splines first, bake, then remove the Splines components.

## What is skipped in v1

Anything that needs live spline math at runtime stays out: `SplineData` channels, knot linking behavior, multi-container path blending, nearest-point queries, and runtime knot edits. The link result and tangent modes are all resolved at bake time, so what you see in the editor is what uploads.

Motion is local-only, same as all tween and animation approaches in VRChat. If you need it networked later, sync state yourself and trigger playback on all clients.

## Layout

- `Runtime/`: `VRCBakedSplineData`, `VRCSplineTweenDriver`, `VRCSplineEvaluator`
- `Editor/`: container baker, clip baker, scatter baker, extrude baker, bake window, build guard
- `Tests/Editor/`: edit-mode tests that run with no Splines package present

## License

MIT, Copyright (c) 2026 Cuebitt. See LICENSE.
