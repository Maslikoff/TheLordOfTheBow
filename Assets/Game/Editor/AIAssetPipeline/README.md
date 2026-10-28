# AI Asset Pipeline

Open **Game → AI Asset Pipeline** in Unity.

## Local prerequisites

- Define `OPENAI_API_KEY` and `TRIPO_API_KEY` as environment variables before starting Unity.
- Blender is auto-detected at `C:\Program Files\Blender Foundation\...`; override its executable in the window when needed.
- API keys are never serialized into project assets or the active job file.

## Flow

1. Enter an asset name, description, and kind. Animation names are optional.
2. Generate and approve a transparent PNG. An opaque border blocks Tripo submission.
3. Wait for the Tripo model. Static assets continue directly to FBX and prefab creation.
4. For animated humans, Tripo creates a Mixamo-compatible rig. For animated animals, Mesh2Motion supplies the rig.
5. In Mesh2Motion select the requested animations, export a single GLB, and import it back through the window.
6. The finished assets are placed under `Assets/Game/Generated/<asset>`.

An active job is persisted under `Library/AIAssetPipeline`. Paid POST requests are never automatically repeated when the server acceptance state is unknown.

If Tripo accepted a paid request but Unity did not receive its response, copy the `task_...` identifier from Tripo Studio into the window's manual recovery field. Interrupted OpenAI image requests cannot be resumed by ID; the window requires an explicit confirmation before creating another paid image request.
