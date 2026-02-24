#!/usr/bin/env python3
import re
import json
import sys
from pathlib import Path

OUTPUT_DIR = Path.home() / "VirtualWorldBridge"
OUTPUT_FILE = OUTPUT_DIR / "command.json"

ROOMS = {
    "bar": [
        {"command": "spawn", "type": "plane", "id": "floor", "position": {"x": 0, "y": 0, "z": 0}, "scale": {"x": 12, "y": 1, "z": 10}, "material": "darkwood"},
        {"command": "spawn", "type": "cube", "id": "wall_back", "position": {"x": 0, "y": 3, "z": 5}, "scale": {"x": 12, "y": 6, "z": 0.2}, "material": "brick"},
        {"command": "spawn", "type": "cube", "id": "bar_counter", "position": {"x": 0, "y": 0.6, "z": 0}, "scale": {"x": 6, "y": 1.2, "z": 1}, "material": "wood"},
        {"command": "spawn", "type": "cube", "id": "bar_top", "position": {"x": 0, "y": 1.25, "z": 0}, "scale": {"x": 6.2, "y": 0.1, "z": 1.2}, "material": "marble"},
        {"command": "spawn", "type": "cylinder", "id": "stool1", "position": {"x": -2, "y": 0.4, "z": -1.5}, "scale": {"x": 0.4, "y": 0.4, "z": 0.4}, "material": "leather"},
        {"command": "spawn", "type": "cylinder", "id": "stool2", "position": {"x": 0, "y": 0.4, "z": -1.5}, "scale": {"x": 0.4, "y": 0.4, "z": 0.4}, "material": "leather"},
        {"command": "spawn", "type": "cylinder", "id": "stool3", "position": {"x": 2, "y": 0.4, "z": -1.5}, "scale": {"x": 0.4, "y": 0.4, "z": 0.4}, "material": "leather"},
    ],
    "living": [
        {"command": "spawn", "type": "plane", "id": "floor", "position": {"x": 0, "y": 0, "z": 0}, "scale": {"x": 10, "y": 1, "z": 10}, "material": "wood"},
        {"command": "spawn", "type": "cube", "id": "wall_north", "position": {"x": 0, "y": 2.5, "z": 5}, "scale": {"x": 10, "y": 5, "z": 0.2}, "material": "cream"},
        {"command": "spawn", "type": "cube", "id": "sofa", "position": {"x": 0, "y": 0.5, "z": 3}, "scale": {"x": 3, "y": 1, "z": 1}, "material": "brown"},
        {"command": "spawn", "type": "cube", "id": "coffee_table", "position": {"x": 0, "y": 0.3, "z": 1}, "scale": {"x": 1.5, "y": 0.4, "z": 0.8}, "material": "wood"},
        {"command": "spawn", "type": "cube", "id": "tv_stand", "position": {"x": 0, "y": 0.4, "z": -2}, "scale": {"x": 2, "y": 0.6, "z": 0.5}, "material": "black"},
        {"command": "spawn", "type": "cube", "id": "tv", "position": {"x": 0, "y": 1.2, "z": -2}, "scale": {"x": 2.5, "y": 1.4, "z": 0.1}, "material": "black"},
    ],
    "bedroom": [
        {"command": "spawn", "type": "plane", "id": "floor", "position": {"x": 0, "y": 0, "z": 0}, "scale": {"x": 8, "y": 1, "z": 8}, "material": "cream"},
        {"command": "spawn", "type": "cube", "id": "bed_frame", "position": {"x": 0, "y": 0.3, "z": 2}, "scale": {"x": 2.2, "y": 0.4, "z": 2.6}, "material": "wood"},
        {"command": "spawn", "type": "cube", "id": "mattress", "position": {"x": 0, "y": 0.55, "z": 2}, "scale": {"x": 2, "y": 0.3, "z": 2.4}, "material": "white"},
        {"command": "spawn", "type": "cube", "id": "pillow", "position": {"x": 0, "y": 0.8, "z": 3}, "scale": {"x": 1.2, "y": 0.2, "z": 0.4}, "material": "white"},
        {"command": "spawn", "type": "cube", "id": "nightstand", "position": {"x": 1.5, "y": 0.35, "z": 2.5}, "scale": {"x": 0.5, "y": 0.5, "z": 0.5}, "material": "wood"},
        {"command": "spawn", "type": "cylinder", "id": "lamp", "position": {"x": 1.5, "y": 0.85, "z": 2.5}, "scale": {"x": 0.2, "y": 0.3, "z": 0.2}, "material": "cream"},
        {"command": "spawn", "type": "cube", "id": "wardrobe", "position": {"x": -3, "y": 1.2, "z": 0}, "scale": {"x": 1.5, "y": 2.4, "z": 0.6}, "material": "darkwood"},
    ],
    "kitchen": [
        {"command": "spawn", "type": "plane", "id": "floor", "position": {"x": 0, "y": 0, "z": 0}, "scale": {"x": 8, "y": 1, "z": 8}, "material": "marble"},
        {"command": "spawn", "type": "cube", "id": "counter", "position": {"x": -3, "y": 0.5, "z": 0}, "scale": {"x": 1, "y": 1, "z": 6}, "material": "white"},
        {"command": "spawn", "type": "cube", "id": "counter_top", "position": {"x": -3, "y": 1.05, "z": 0}, "scale": {"x": 1.1, "y": 0.1, "z": 6.1}, "material": "marble"},
        {"command": "spawn", "type": "cube", "id": "fridge", "position": {"x": -3, "y": 1, "z": -2.5}, "scale": {"x": 0.9, "y": 2, "z": 0.8}, "material": "white"},
        {"command": "spawn", "type": "cube", "id": "stove", "position": {"x": -3, "y": 0.5, "z": 1.5}, "scale": {"x": 0.8, "y": 0.1, "z": 0.8}, "material": "black"},
        {"command": "spawn", "type": "cube", "id": "island", "position": {"x": 0, "y": 0.5, "z": 0}, "scale": {"x": 2, "y": 1, "z": 1.2}, "material": "wood"},
        {"command": "spawn", "type": "cube", "id": "island_top", "position": {"x": 0, "y": 1.05, "z": 0}, "scale": {"x": 2.1, "y": 0.1, "z": 1.3}, "material": "marble"},
    ],
    "office": [
        {"command": "spawn", "type": "plane", "id": "floor", "position": {"x": 0, "y": 0, "z": 0}, "scale": {"x": 8, "y": 1, "z": 8}, "material": "wood"},
        {"command": "spawn", "type": "cube", "id": "desk", "position": {"x": 0, "y": 0.4, "z": 2}, "scale": {"x": 2, "y": 0.1, "z": 1}, "material": "wood"},
        {"command": "spawn", "type": "cube", "id": "desk_legs1", "position": {"x": -0.9, "y": 0.2, "z": 2}, "scale": {"x": 0.1, "y": 0.4, "z": 0.8}, "material": "black"},
        {"command": "spawn", "type": "cube", "id": "desk_legs2", "position": {"x": 0.9, "y": 0.2, "z": 2}, "scale": {"x": 0.1, "y": 0.4, "z": 0.8}, "material": "black"},
        {"command": "spawn", "type": "cube", "id": "monitor", "position": {"x": 0, "y": 0.7, "z": 2.3}, "scale": {"x": 0.8, "y": 0.5, "z": 0.05}, "material": "black"},
        {"command": "spawn", "type": "cylinder", "id": "chair_base", "position": {"x": 0, "y": 0.25, "z": 1}, "scale": {"x": 0.5, "y": 0.25, "z": 0.5}, "material": "black"},
        {"command": "spawn", "type": "cube", "id": "chair_back", "position": {"x": 0, "y": 0.7, "z": 0.7}, "scale": {"x": 0.5, "y": 0.6, "z": 0.1}, "material": "leather"},
        {"command": "spawn", "type": "cube", "id": "bookshelf", "position": {"x": 3, "y": 1, "z": 0}, "scale": {"x": 0.4, "y": 2, "z": 1.5}, "material": "wood"},
    ],
    "cafe": [
        {"command": "spawn", "type": "plane", "id": "floor", "position": {"x": 0, "y": 0, "z": 0}, "scale": {"x": 12, "y": 1, "z": 12}, "material": "wood"},
        {"command": "spawn", "type": "cube", "id": "counter", "position": {"x": -4, "y": 0.6, "z": 0}, "scale": {"x": 1.5, "y": 1.2, "z": 4}, "material": "darkwood"},
        {"command": "spawn", "type": "cube", "id": "table1", "position": {"x": 1, "y": 0.4, "z": 2}, "scale": {"x": 1, "y": 0.05, "z": 1}, "material": "wood"},
        {"command": "spawn", "type": "cylinder", "id": "chair1a", "position": {"x": 0.3, "y": 0.3, "z": 2}, "scale": {"x": 0.35, "y": 0.3, "z": 0.35}, "material": "brown"},
        {"command": "spawn", "type": "cylinder", "id": "chair1b", "position": {"x": 1.7, "y": 0.3, "z": 2}, "scale": {"x": 0.35, "y": 0.3, "z": 0.35}, "material": "brown"},
        {"command": "spawn", "type": "cube", "id": "table2", "position": {"x": 1, "y": 0.4, "z": -2}, "scale": {"x": 1, "y": 0.05, "z": 1}, "material": "wood"},
        {"command": "spawn", "type": "cylinder", "id": "chair2a", "position": {"x": 0.3, "y": 0.3, "z": -2}, "scale": {"x": 0.35, "y": 0.3, "z": 0.35}, "material": "brown"},
        {"command": "spawn", "type": "cylinder", "id": "chair2b", "position": {"x": 1.7, "y": 0.3, "z": -2}, "scale": {"x": 0.35, "y": 0.3, "z": 0.35}, "material": "brown"},
    ],
}

def write_command(cmd):
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    with open(OUTPUT_FILE, "w") as f:
        json.dump(cmd, f, indent=2)
    print(f"Command written to {OUTPUT_FILE}")

def write_batch(commands):
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    with open(OUTPUT_FILE, "w") as f:
        json.dump({"batch": commands}, f, indent=2)
    print(f"Batch of {len(commands)} commands written to {OUTPUT_FILE}")

def parse(text):
    text = text.lower().strip()
    
    # Room templates
    if "create" in text or "make" in text or "build" in text:
        for room_name in ROOMS:
            if room_name in text:
                write_batch(ROOMS[room_name])
                print(f"Creating {room_name} room...")
                return
    
    # Clear/reset
    if "clear" in text or "reset" in text:
        write_command({"command": "clear_all"})
        print("Clearing all objects...")
        return
    
    # Setup camera: "setup camera" or "fix camera"
    if "setup camera" in text or "fix camera" in text:
        write_command({"command": "setup_camera"})
        print("Setting up camera view...")
        return
    
    # Lighting: "add light at 0 5 0" or "add spotlight red at 2 3 0"
    m = re.match(r"(?:add|create|spawn)\s+(?:a\s+)?(\w+)?\s*light\s+(?:at\s+)?(-?\d+\.?\d*)\s+(-?\d+\.?\d*)\s+(-?\d+\.?\d*)", text)
    if m:
        color = m.group(1) if m.group(1) else "white"
        write_command({
            "command": "light",
            "id": f"light_{abs(hash(text)) % 1000}",
            "lightType": "point",
            "position": {"x": float(m.group(2)), "y": float(m.group(3)), "z": float(m.group(4))},
            "color": color
        })
        print(f"Adding {color} light...")
        return
    
    # Rotation: "rotate sofa 45" or "rotate table 90 degrees"
    m = re.match(r"rotate\s+(\w+)\s+(-?\d+\.?\d*)\s*(?:degrees?)?", text)
    if m:
        write_command({
            "command": "rotate",
            "id": m.group(1),
            "rotation": {"x": 0, "y": float(m.group(2)), "z": 0}
        })
        print(f"Rotating {m.group(1)} by {m.group(2)} degrees...")
        return
    
    # Scale: "scale table 2" or "resize chair 0.5"
    m = re.match(r"(?:scale|resize)\s+(\w+)\s+(-?\d+\.?\d*)", text)
    if m:
        factor = float(m.group(2))
        write_command({
            "command": "scale",
            "id": m.group(1),
            "scale": {"x": factor, "y": factor, "z": factor}
        })
        print(f"Scaling {m.group(1)} by {factor}x...")
        return
    
    # Spawn: "spawn cube mybox at 1 2 3"
    m = re.match(r"spawn\s+(\w+)\s+(\w+)\s+at\s+(-?\d+\.?\d*)\s+(-?\d+\.?\d*)\s+(-?\d+\.?\d*)", text)
    if m:
        write_command({
            "command": "spawn",
            "type": m.group(1),
            "id": m.group(2),
            "position": {"x": float(m.group(3)), "y": float(m.group(4)), "z": float(m.group(5))}
        })
        print(f"Spawning {m.group(1)} as {m.group(2)}...")
        return
    
    # Move: "move mybox to 5 0 5"
    m = re.match(r"move\s+(\w+)\s+to\s+(-?\d+\.?\d*)\s+(-?\d+\.?\d*)\s+(-?\d+\.?\d*)", text)
    if m:
        write_command({
            "command": "move",
            "id": m.group(1),
            "position": {"x": float(m.group(2)), "y": float(m.group(3)), "z": float(m.group(4))}
        })
        print(f"Moving {m.group(1)}...")
        return
    
    # Set material: "set_material mybox red" or "color sofa blue"
    m = re.match(r"(?:set_material|color|paint)\s+(\w+)\s+(\w+)", text)
    if m:
        write_command({
            "command": "set_material",
            "id": m.group(1),
            "material": m.group(2)
        })
        print(f"Setting {m.group(1)} material to {m.group(2)}...")
        return
    
    # Delete: "delete mybox"
    m = re.match(r"(?:delete|remove)\s+(\w+)", text)
    if m:
        write_command({"command": "delete", "id": m.group(1)})
        print(f"Deleting {m.group(1)}...")
        return
    
    # Natural language: "put red sofa at left"
    m = re.match(r"(?:put|add|place)\s+(?:a\s+)?(\w+)?\s*(\w+)\s+(?:at|in|on)?\s*(.+)?", text)
    if m:
        color = m.group(1)
        obj_type = m.group(2)
        location = m.group(3) or "center"
        pos = {"x": 0, "y": 0.5, "z": 0}
        if "left" in location: pos["x"] = -3
        if "right" in location: pos["x"] = 3
        if "front" in location: pos["z"] = -3
        if "back" in location: pos["z"] = 3
        write_command({
            "command": "spawn",
            "type": "cube",
            "id": f"{obj_type}_{abs(hash(text)) % 1000}",
            "position": pos,
            "material": color
        })
        print(f"Placing {color or ''} {obj_type}...")
        return
    
    print("Unknown command. Try: create a bar, spawn cube mybox at 0 1 0, rotate sofa 45, add light at 0 5 0")

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Commands:")
        print("  Rooms:     create a bar | living | bedroom | kitchen | office | cafe")
        print("  Spawn:     spawn cube mybox at 0 1 0")
        print("  Move:      move mybox to 5 0 5")
        print("  Rotate:    rotate sofa 45")
        print("  Scale:     scale table 2")
        print("  Color:     color sofa red")
        print("  Light:     add light at 0 5 0")
        print("  Delete:    delete mybox")
        print("  Clear:     clear all")
        sys.exit(0)
    parse(sys.argv[1])
