# Changelog

All notable changes to this package are recorded here. The format follows
Keep a Changelog, and the package follows Semantic Versioning.

## [1.0.0] - 2026-09-21

First release. Static editor-time baking from Unity Splines to
upload-safe VRChat world content.

- Baked spline data asset (positions, tangents, up vectors, arc-length
  table) converted from any SplineContainer spline. The asset is an
  editor-only intermediate; the bake window copies plain arrays onto
  Udon components because Udon cannot read custom asset types at
  runtime.
- Animate output: baked AnimationClip with Animator wiring (default),
  with loop modes and start offsets.
- Animate fallback: VRCTween path driver for position-only motion with
  native timing.
- Opt-in Udon evaluator for scrubbing and distance queries.
- Instantiate output: seeded prefab scatter by count or spacing.
- Extrude output: mesh snapshot from SplineExtrude to a Mesh asset.
- Bake window with a demo generator that needs no Splines install.
- Build guard that blocks VRChat uploads while live Splines components
  remain in the scene.
- Edit-mode tests covering bake math without the Splines package.
