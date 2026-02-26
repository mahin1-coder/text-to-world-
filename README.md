# Unity + Python Text-to-World Bridge

This workspace contains a minimal text-to-world bridge between a Python CLI and a Unity scene.

## ✅ Python CLI
The CLI parses commands with regex and writes a JSON payload to `~/VirtualWorldBridge/command.json`.

**Supported commands:**
- `spawn <type> <id> at x y z`
- `move <id> to x y z`
- `set_material <id> <material>`
- `delete <id>`

Example:
- `python3 Tools/virtual_world_cli.py "spawn cube crate01 at 0 1 2"`

## ✅ Unity Setup
1. Open your scene.
2. Create an empty GameObject named `WorldRoot` (optional; it will be created automatically if missing).
3. Create an empty GameObject named `CommandReceiver` and attach `CommandReceiver.cs`.
4. Enter Play mode and send commands from the CLI.

Spawned objects:
- Use Unity primitives.
- Automatically attach `SurfaceMaterial`.
- Parent under `WorldRoot`.

## Files
- `Assets/Scripts/CommandReceiver.cs`
- `Assets/Scripts/SurfaceMaterial.cs`
- `Tools/virtual_world_cli.py`
