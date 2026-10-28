# Ninja shop skin

Payment: Yandex IAP product `skin_ninja`. The existing `skin_stub_01` save ID and asset GUID are retained, so previous coin purchases remain owned.
Default Elf uses an empty equipped-skin ID and is always available for free.

## Artwork

Generated with the built-in image generation tool using `Ninja_Model_Render.png`
(render of the actual configured Unity prefab) as the authoritative reference and
the supplied IMG_1132.PNG turnaround as secondary design context.
Final asset: `Assets/Game/Art/Shop/Icon_Ninja.png`, Sprite Single, alpha transparency,
512px import limit, no mipmaps; Addressables `Remote_Icons`.
No new artwork or model was created for Elf.

### Prompt

Create one square 1024x1024 transparent-background PNG character cutout for a mobile game's shop card. The FIRST reference is the actual in-game model render and is authoritative: preserve this exact ninja's mesh-like silhouette, short chibi proportions, oversized hood, eyes, tan visible skin, black mask, charcoal clothing, brown straps, belt and boots, and simple painted game textures. SECOND reference is secondary design context only; do not replace the model's appearance with that drawing's proportions or painterly style. Show one ninja in full body, appealing confident dynamic three-quarter running pose similar to the first reference, face clearly visible, all fingers and feet inside frame, about 80% image height with comfortable margins. Polish lighting only: soft studio key light, restrained cool rim light, clean matte materials and clear readable shapes, recognizable as a polished render of this same low-poly game model, not generic AI concept art. No text, no frame, no pedestal, no scenery, no floor, no props or weapons, no invented accessories, no extra straps, no photorealism, no plastic shine, no painterly brush strokes, no excess detail, no sparkles. True transparent alpha background; do not draw checkerboard. Keep character recognizable at small shop-card size.

## Setup and checks

- `Tools/Shop/Setup Ninja Skin` delegates to the complete character setup and rebuilds all character prefabs, catalog wiring and Addressables entries.
- `Tools/Shop/Validate Skin Inventory And Assets` checks prices, persistence and asset wiring in Edit Mode.
- `Game.Tests.Editor.SkinIntegrationChecks.RunAsyncChecks()` tests delayed/failed loads and releases in Play Mode using isolated test objects and fake services.
- Ninja uses the supplied Run FBX with textures from ZIP; OBJ/GLB are not runtime dependencies.
- Materials, animation and model are dependencies of `Prefabs/Skins/Ninja` in `Remote_Characters`.
- Remote delivery still requires replacing `BUCKET_NAME` in the existing Addressables profile and uploading the built catalog/bundles. The old angle-bracket placeholder was normalized because it caused an illegal-path error when building the remote catalog on Windows. Catalog paths now reference the existing Remote profile variables.

## Validation performed

- The original 50-coin purchase validation applied to the first Ninja release. Ninja now uses Yandex IAP while retaining the same inventory ID for compatibility.
- Isolated asynchronous checks passed: delayed load, stale selection, failure and retry, destruction during loading and balanced releases.
- Play Mode: actual textured Ninja, looped running, movement, damage feedback and arrow firing checked. A separate player instance retained Ninja through death and revival at 50 HP. The next-level transition completed with Ninja equipped (the current level configurations share the tutorial scene).
- Shop inspected at 1080x1920 and 1920x1080. Landscape card height is constrained to the viewport; title, status and button minimum sizes prevent clipped controls.
- Final screenshots: `Shop_Portrait_Final.png`, `Shop_Landscape_Final.png`; model reference: `Ninja_Model_Render.png`.
- WebGL Addressables build result is recorded in `AddressablesBuildResult.json`. Runtime content includes Ninja FBX, textures, material and Animator dependencies; static GLB/OBJ are not included.
- Final build succeeded in 11.72 seconds. The Addressables report window logged a report-loading error, but the generated JSON report is present and parseable with an empty `BuildError`; the remote catalog and hash were produced successfully.
- Cloud persistence and real CDN loading were not exercised. Persistence was validated with isolated serialized saves, without purchasing on the user's account.
