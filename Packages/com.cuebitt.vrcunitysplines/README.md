# VRCUnitySplines

Bakes Unity Splines into upload-safe VRChat world content: animation clips, prefab scatter, and extruded meshes. You build with normal Splines tools, press bake, then delete the live Splines components before uploading. A build guard stops the upload if you forget.

Needs Unity 2022.3, Worlds SDK 3.10.4 or newer, and `com.unity.splines` 2.6.1 (pulled in automatically). Install through the Creator Companion listing, or clone the repo and open it directly. Full docs live in the repo root README.

Bake with VRCUnitySplines > Bake Window. Clips are the default motion output, the VRCTween driver is the fallback, and the Udon evaluator is opt-in for scrubbing and queries. For the driver and evaluator, fill their baked arrays from the same window, they run on component fields only.

v1 skips anything needing live spline math at runtime: `SplineData` channels, knot linking, path blending, nearest-point queries, and knot edits. Motion is local-only.

MIT, Copyright (c) 2026 Cuebitt. See LICENSE.
