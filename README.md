# 🌍 Text-to-World System

**Transform natural language into interactive 3D virtual worlds in Unity**

A complete system that converts user text prompts into fully-realized virtual environments with objects, terrain, lighting, and atmosphere.

---

## 🎯 Overview

The **Text-to-World** system enables users to generate virtual worlds simply by describing them in natural language.

**Example Inputs:**
- _"Create a forest with trees, rocks, and a river"_
- _"Generate a futuristic city at night with neon lights and tall buildings"_
- _"Make a classroom with desks, chairs, and a whiteboard"_
- _"Create a desert with sand dunes and cactus at sunset"_

**System Pipeline:**
```
User Prompt → Parser → Scene Data → World Generator → Rendered 3D World
```

---

## ✨ Features

✅ **Natural Language Parsing** - Understands objects, environments, colors, sizes, and placement  
✅ **Modular Architecture** - Clean separation of concerns with reusable components  
✅ **Environment System** - Dynamic sky, terrain, lighting, and atmosphere  
✅ **Material Management** - 40+ predefined materials (wood, metal, glass, etc.)  
✅ **Smart Object Placement** - Random, scattered, grid, line, and circle patterns  
✅ **Unity UI Integration** - Simple text input interface for in-game use  
✅ **Example Prompts** - 20+ pre-built test scenarios  
✅ **Extensible Design** - Ready for AI/LLM integration (GPT, Claude, etc.)  

---

## 🚀 Quick Start

### Prerequisites
- Unity 2021.3+ (URP or Built-in Render Pipeline)
- TextMeshPro package (usually pre-installed)

### Setup (5 minutes)

1. **Clone the repository:**
   ```bash
   git clone https://github.com/mahin1-coder/text-to-world-.git
   cd text-to-world-
   ```

2. **Open in Unity:**
   - Open Unity Hub
   - Add project → Select the `text-to-world-` folder
   - Open the project

3. **Create a new scene:**
   - Create a new Scene (`File → New Scene`)
   - Save as `TextToWorldScene`

4. **Setup the World Generator:**
   - Create an empty GameObject: `GameObject → Create Empty`
   - Rename it to `WorldGenerator`
   - Add the `WorldGenerator` component (`Add Component → WorldGenerator`)
   - The system will auto-create required sub-components

5. **Setup the UI (Optional but recommended):**
   - Create a Canvas: `UI → Canvas`
   - Create InputField: `Right-click Canvas → UI → Input Field - TextMeshPro`
   - Create Buttons: `Right-click Canvas → UI → Button - TextMeshPro` (x2)
   - Create Status Text: `Right-click Canvas → UI → Text - TextMeshPro`
   - Create an empty GameObject named `UIController`
   - Add `TextInputUI` component
   - Drag UI elements into the inspector fields

6. **Run it:**
   - Press Play
   - Enter a prompt like: `"Create a forest with trees and rocks"`
   - Click Generate or press Enter
   - Watch your world appear!

---

## 📖 Usage

### Method 1: Unity UI (Recommended)

1. Enter Play Mode
2. Type a prompt in the text field
3. Click "Generate" or press Enter
4. Your world appears instantly!

### Method 2: Code/Script

```csharp
using TextToWorld.Core;

public class MyScript : MonoBehaviour
{
    public WorldGenerator generator;
    
    void Start()
    {
        generator.GenerateWorldFromPrompt("Create a desert with cactus");
    }
}
```

### Method 3: Unity Inspector (Testing)

1. Select the `WorldGenerator` GameObject
2. Right-click the component in Inspector
3. Choose from context menu:
   - `Test: Generate Forest`
   - `Test: Generate City`
   - `Test: Generate Desert`
   - `Test: Generate Classroom`

### Method 4: Python CLI (Legacy)

```bash
python3 Tools/virtual_world_cli.py "create a bar"
python3 Tools/virtual_world_cli.py "spawn cube mybox at 0 1 0"
```

---

## 📝 Example Prompts

### Nature Environments
- `"Create a forest with 20 trees, rocks, and a river"`
- `"Make a desert with sand dunes, cactus, and rocks at sunset"`
- `"Generate a beach with sand, water, and palm trees"`
- `"Create a mountain area with snow and boulders at night"`

### Urban Environments
- `"Generate a futuristic city at night with neon lights and tall buildings"`
- `"Create a city street with buildings, cars, and roads"`
- `"Make an urban park with grass, trees, and benches"`

### Indoor Environments
- `"Generate a classroom with 20 desks in a grid, chairs, and a whiteboard"`
- `"Create an office with desks, chairs, and computers"`
- `"Make a living room with a sofa, coffee table, and TV"`

### Creative/Fantasy
- `"Create a space station with metallic structures in the dark"`
- `"Generate a medieval village with stone buildings and wooden fences"`
- `"Make a cave with colorful crystal rocks"`

### Simple Tests
- `"Create 10 red cubes and 5 blue spheres"`
- `"Make a grid of 25 desks"`
- `"Create 12 trees arranged in a circle"`

---

## 🏗️ Architecture

The system uses a clean, modular architecture:

```
┌─────────────────┐
│  User Input     │  ← TextInputUI.cs
└────────┬────────┘
         ↓
┌─────────────────┐
│ PromptParser    │  ← Converts text → SceneData
└────────┬────────┘
         ↓
┌─────────────────┐
│ SceneData       │  ← Structured JSON representation
└────────┬────────┘
         ↓
┌─────────────────┐
│ WorldGenerator  │  ← Main orchestrator
└─────┬───┬───┬───┘
      │   │   │
      ↓   ↓   ↓
  ┌───┐ ┌───┐ ┌───┐
  │Env│ │Obj│ │Mat│  ← EnvironmentManager, ObjectSpawner, MaterialManager
  └───┘ └───┘ └───┘
         ↓
┌─────────────────┐
│ 3D Virtual World│  ← Rendered scene
└─────────────────┘
```

**Core Components:**
- **SceneData.cs** - Data models and schema
- **PromptParser.cs** - Natural language → structured data
- **WorldGenerator.cs** - Main orchestrator
- **EnvironmentManager.cs** - Terrain, sky, lighting
- **ObjectSpawner.cs** - Object instantiation with placement algorithms
- **MaterialManager.cs** - Material creation and caching
- **TextInputUI.cs** - Unity UI interface
- **ExamplePrompts.cs** - Pre-built test prompts

See [ARCHITECTURE.md](ARCHITECTURE.md) for detailed technical documentation.

---

## 🎨 Supported Features

### Environment Types
Forest, Desert, City, Classroom, Office, Beach, Mountain, Space, Medieval, etc.

### Sky States
Daytime, Night, Sunset, Sunrise, Cloudy

### Terrain Types
Grass, Sand, Concrete, Wood, Stone, Marble, Snow, Ice, Water, Metal

### Object Types
Trees, Rocks, Buildings, Cars, Desks, Chairs, Tables, etc. (40+ types)

### Materials
Wood, Metal, Glass, Brick, Stone, Leather, Concrete, etc. (40+ materials)

### Colors
Red, Blue, Green, Yellow, Orange, Purple, Pink, Cyan, etc. (20+ colors)

### Placement Strategies
- **Random** - Completely random positions
- **Scattered** - Random with minimum distance
- **Grid** - Organized grid layout
- **Line/Row** - Linear arrangement
- **Circle** - Circular pattern
- **Center** - Single centered object

---

## 🔧 Configuration

### Spawn Area
Adjust in `ObjectSpawner` component:
```csharp
public Vector3 spawnAreaMin = new Vector3(-40, 0, -40);
public Vector3 spawnAreaMax = new Vector3(40, 10, 40);
```

### Camera Position
Adjust in `WorldGenerator` component:
```csharp
public Vector3 defaultCameraPosition = new Vector3(0, 15, -30);
public Vector3 defaultCameraRotation = new Vector3(30, 0, 0);
```

### Debug Logging
Toggle in `WorldGenerator`:
```csharp
public bool logSceneJson = true;
public bool logGenerationSteps = true;
```

---

## 🚧 Future Improvements

### Planned Features
- [ ] **AI/LLM Integration** - Connect to GPT-4, Claude, or other LLMs for advanced parsing
- [ ] **Asset Library** - Replace primitives with real 3D models
- [ ] **Physics & Animation** - Add realistic physics and animated objects
- [ ] **Procedural Generation** - Algorithmic terrain and structure generation
- [ ] **Save/Load Worlds** - Export and import generated scenes
- [ ] **Multiplayer Support** - Share worlds across network
- [ ] **VR/AR Support** - Immersive world exploration
- [ ] **Voice Input** - Speak your prompts
- [ ] **Real-time Editing** - Modify worlds while exploring

### AI Integration TODO
The system is designed for easy LLM integration. Add your API in `PromptParser.cs`:

```csharp
// TODO: Replace keyword matching with LLM API call
public static SceneData ParsePromptWithAI(string prompt)
{
    // Call OpenAI GPT-4 or Anthropic Claude
    // Parse response into SceneData
    // Return structured data
}
```

---

## 📂 Project Structure

```
text-to-world/
├── Assets/
│   ├── Scripts/
│   │   ├── SceneData.cs              ← Data models
│   │   ├── PromptParser.cs           ← NLP parser
│   │   ├── WorldGenerator.cs         ← Main orchestrator
│   │   ├── EnvironmentManager.cs     ← Terrain/sky/lighting
│   │   ├── ObjectSpawner.cs          ← Object placement
│   │   ├── MaterialManager.cs        ← Material system
│   │   ├── TextInputUI.cs            ← UI controller
│   │   ├── ExamplePrompts.cs         ← Test prompts
│   │   ├── CommandReceiver.cs        ← Legacy Python bridge
│   │   └── SurfaceMaterial.cs        ← Material properties
│   ├── Scenes/                       ← Your Unity scenes
│   └── ...
├── Tools/
│   └── virtual_world_cli.py          ← Legacy Python CLI
├── README.md                         ← This file
├── ARCHITECTURE.md                   ← Technical documentation
└── ...
```

---

## 🤝 Contributing

Contributions are welcome! Areas to improve:
1. Better natural language understanding
2. More object types and mappings
3. Advanced placement algorithms
4. Performance optimizations
5. UI/UX enhancements

---

## 📄 License

MIT License - Feel free to use in your projects!

---

## 👤 Author

**Mahin** - [GitHub](https://github.com/mahin1-coder)

---

## 🙏 Acknowledgments

Built with Unity and passion for creating accessible world-building tools.

---

**Ready to build worlds? Fire up Unity and start creating! 🌍✨**
