# Character skins and enemy visuals

`Tools/Characters/Setup All Skins And Enemies` is the repeatable setup command. It imports the animated FBX files as Generic rigs, loops the Run clip without root motion, creates FlatKit cel-shaded materials with character-only outlines and Animator Controllers, normalizes the visuals, updates the player and enemy prefabs, rebuilds the skin-only catalog, registers Addressables and writes reference renders to `Design/Characters/Renders`.

`Tools/Characters/Apply FlatKit To Character Materials` reapplies the shared FlatKit look to the seven player skins and three enemy materials without rebuilding prefabs, catalog entries or Addressables groups.

## Shop configuration

| ID | Display name | Payment |
|---|---|---|
| `skin_default_elf` | Elf | Free default |
| `skin_elf_female` | Elf Maiden | 25 coins |
| `skin_knight` | Knight | 110 coins |
| `skin_wizard` | Wizard | 110 coins |
| `skin_dwarf` | Dwarf | 110 coins |
| `skin_stub_01` | Ninja | Yandex IAP `skin_ninja` |
| `skin_valkyrie` | Valkyrie | Yandex IAP `skin_valkyrie` |

The Ninja keeps its legacy `skin_stub_01` inventory ID so previous owners retain it. Yandex product prices are configured in the Yandex Games catalog and displayed from PluginYG2 at runtime.

## Shop artwork

The built-in ImageGen tool was used for the shop artwork. Each Unity prefab render was the authoritative reference. Final PNG files are stored in `Assets/Game/Art/Shop` as `Icon_Elf`, `Icon_ElfFemale`, `Icon_Knight`, `Icon_Wizard`, `Icon_Dwarf` and `Icon_Valkyrie`. For Elf Female, the generated pass removes the black render background while preserving the model, pose and transparent card composition.

### Shared prompt

Use case: stylized-concept. Asset type: square transparent PNG character cutout for a mobile game's shop card. The referenced image is the actual in-game model render and is authoritative. Preserve this exact character's silhouette, stylized proportions, face, hair or helmet, clothing, armor, colors and simple painted game textures. Show exactly one full-body character in a confident dynamic three-quarter running pose, face clearly visible where applicable, with every extremity and costume element inside frame, about 80% of image height with comfortable margins. Render as a polished version of this same low-poly game model with brighter soft studio key light, restrained cool rim light, clean matte materials and readable forms. True transparent alpha background. No text, frame, floor, scenery, props, weapons, new accessories, altered costume, photorealism, plastic shine, painterly brush strokes, sparks or excess detail.

Character-specific details supplied with the shared prompt:

- Elf: pointed ears, tied blond hair, green hooded cloak and tunic, brown belts and boots.
- Knight: closed dark steel helmet, black and gunmetal plate armor, brown belt and pouches.
- Wizard: floppy navy pointed hat, round nose, gray split beard, navy robe and brown belt.
- Dwarf: pointed dark helmet, reddish-brown split beard and moustache, dark armor and heavy boots.
- Valkyrie: winged gold helmet with blue gem, blond braids, white and gold armor, blue split cape.

## Test currency

During Play Mode, use `Tools/Shop/Add 1000 Test Coins`. The amount is the `TestCoinGrant` constant in `Assets/Game/Scripts/Editor/ShopTestTools.cs`. The command lives in an Editor-only assembly and is not compiled into WebGL builds.

## Addressables

Skin and enemy visual prefabs use `Remote_Characters`, shop definitions use `Remote_Configs`, and icons use `Remote_Icons`. Models, textures, materials and Animator Controllers are prefab dependencies. The static OBJ/MTL files from the ZIP archives are intentionally not copied into the project.

Remote loading still requires replacing `BUCKET_NAME` in the Addressables profile and uploading the generated WebGL catalog and bundles.

## Validation

Validated in Unity 2022.3.62f2 on 2026-09-26:

- seven-item skin-only catalog, exact coin prices and Yandex product IDs;
- legacy Ninja ownership, save round-trip and immediate equip after both coin and IAP purchases;
- delayed Addressables loads, rapid selection changes, failure/retry, destroyed player and balanced releases;
- looped animations without root motion, textured player/enemy visuals, hidden projectile/hat tabs;
- Play Mode shop layout and editor-only `+1000` coin command;
- WebGL Addressables build completed with new `Remote_Characters`, `Remote_Icons` and `Remote_Configs` bundles.
