# Changelog

All notable changes to this package are recorded here. The format follows
Keep a Changelog, and the package follows Semantic Versioning.

## [1.0.0] - 2026-09-21

First release. Static editor-time baking from Unity Splines to
upload-safe VRChat world content.

- Baked spline data (positions, tangents, up vectors, arc-length table)
  converted from any SplineContainer spline, held in memory during
  baking. The bake window copies plain arrays onto Udon components
  because Udon cannot read custom types at runtime.
- Animate output: baked AnimationClip with Animator wiring (default),
  with loop modes.
- Animate fallback: VRCTween path driver for position-only motion with
  native timing.
- Opt-in Udon evaluator for scrubbing and distance queries.
- Create Demo Spline menu item (Tools > VRCUnitySplines) that needs no
  Splines install.
- Live Splines components stay in the scene for editing: they are
  stripped automatically while a build or upload runs, and disabled
  around editor play mode so baked outputs run unopposed.
- Edit-mode tests covering bake and evaluator math without the Splines
  package.
