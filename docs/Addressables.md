# Addressables (Yandex Object Storage)

Remote content for The Lord of the Bow WebGL / Yandex Games.

## One-time Unity setup

1. Open the project (Addressables package installs from `Packages/manifest.json`).
2. Menu: **Game → Addressables → Setup Groups And Mark Assets**  
   (also switches Play Mode to **Use Asset Database**).
3. If Play still looks for `Library/.../aa/WebGL/settings.json`:  
   **Game → Addressables → Use Asset Database Play Mode**  
   or Addressables Groups window → Play Mode Script → **Use Asset Database (fastest)**.
4. In **Profiles**, set `Remote.LoadPath` = `https://storage.yandexcloud.net/<BUCKET_NAME>/[BuildTarget]`.
5. For WebGL player only: **Addressables Groups → Build → New Build → Default Build Script**, then upload `ServerData/WebGL/` to the bucket.  
   Later content updates: **Update a Previous Build**.

**Do not** use Play Mode **Use Existing Build** in Editor until Addressables content for the active platform (WebGL) has been built — that is what caused `settings.json` / RuntimeData is null.

## Groups

| Group | Path | Contents |
|-------|------|----------|
| Remote_Configs | Remote | Level_1…10, UpgradCard SO, ShopCatalog + shop item SOs |
| Remote_Characters | Remote | GG, enemies, XP config |
| Remote_Combat | Remote | bullets, PickHealth |
| Remote_World | Remote | Section_1/2/3_Ground (+ Polygon deps) |
| Remote_FX | Remote | GenericDeath, HealOnceBurst (+ thin ETFX) |
| Remote_Icons | Remote | IMG_0603, `Assets/Game/Art/Shop` item icons |
| Remote_Audio | Remote | BGM, Button, Heal, Upgrade, Death/Spawn ETFX wavs |
| Remote_Smoke | Remote | Smoke/TestAsset |

Shop catalog is **not** serialized on `RootLifetimeScope`. Boot loads `Configs/Shop/ShopCatalog` after Addressables init; item icons use `AssetReferenceSprite`. Shop UI prefabs stay local.

Local in player build: Boot, YG, UI panels/buttons/joystick, fonts, Tutor, Hyper_Casual, Level_Tutorial scene, LevelCatalog, GameStartupConfig, Reward, AudioMixer.

## Yandex Object Storage

1. Create a bucket (public-read for objects used by the game).
2. CORS example:

```json
[
  {
    "AllowedOrigins": ["*"],
    "AllowedMethods": ["GET", "HEAD"],
    "AllowedHeaders": ["*"],
    "MaxAgeSeconds": 3600
  }
]
```

3. Upload contents of `ServerData/WebGL/` (catalog, `.hash`, bundles) to the bucket under `WebGL/` (or match your LoadPath layout).
4. In Yandex Games partner cabinet, allow CSP host: `https://storage.yandexcloud.net` (and bucket host if different).

## Boot flow

`Bootstrapper`: Addressables init → catalog update → download labels → load `Configs/Shop/ShopCatalog` → wait YG saves → `LevelService` loads remote `LevelConfig` → local `Level_Tutorial` scene → pools warmup via `AssetReference`.

## Cost guardrails

- Do **not** mark whole `Epic Toon FX`, `FlatKit`, `PolygonWesternFrontier`, `maanetorn`, or `Hyper_Casual_UI` folders.
- Prefer **Update a Previous Build**; delete obsolete bundles from the bucket after a successful update.
- Watch Yandex Cloud egress (free tier: ~1 GB storage + 100 GB traffic/month).
- Do not ship orphan grounds (`Ground_1/2/3`), unused `Elf.blend`, or unused scene `Level_1.unity` as remote content.

## Smoke test

Address `Smoke/TestAsset` → `Assets/Game/Configs/Addressables/SmokeTest.txt`.  
In Editor/Development builds Bootstrapper logs smoke success/failure.
