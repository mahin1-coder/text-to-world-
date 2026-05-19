# 🏗️ Text-to-World Architecture

**Technical Documentation for Developers**

This document provides an in-depth look at the system architecture, design decisions, and implementation details.

---

## Table of Contents

1. [System Overview](#system-overview)
2. [Architecture Diagram](#architecture-diagram)
3. [Component Details](#component-details)
4. [Data Flow](#data-flow)
5. [SceneData Schema](#scenedata-schema)
6. [Parser Algorithm](#parser-algorithm)
7. [Placement Algorithms](#placement-algorithms)
8. [Extension Points](#extension-points)
9. [Performance Considerations](#performance-considerations)
10. [Testing Strategy](#testing-strategy)

---

## System Overview

The Text-to-World system follows a **pipeline architecture** with clear separation of concerns:

1. **Input Layer** - Text prompt from user
2. **Parsing Layer** - Natural language → Structured data
3. **Data Layer** - SceneData schema
4. **Generation Layer** - World construction
5. **Rendering Layer** - Unity visualization

### Design Principles

- **Modularity** - Each component has a single responsibility
- **Extensibility** - Easy to add new features without breaking existing code
- **Testability** - Components can be tested independently
- **Readability** - Clean code with extensive comments for student developers
- **Performance** - Coroutines and frame-spreading to avoid freezing

---

## Architecture Diagram

```
┌───────────────────────────────────────────────────────────────┐
│                         USER LAYER                             │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐           │
│  │ TextInputUI │  │ Unity Editor│  │ External API│           │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘           │
└─────────┼─────────────────┼─────────────────┼─────────────────┘
          │                 │                 │
          └─────────────────┴─────────────────┘
                            ↓
┌───────────────────────────────────────────────────────────────┐
│                      CONTROL LAYER                             │
│                  ┌──────────────────┐                          │
│                  │ WorldGenerator   │  ← Main Orchestrator     │
│                  └────────┬─────────┘                          │
└───────────────────────────┼─────────────────────────────────────┘
                            ↓
┌───────────────────────────────────────────────────────────────┐
│                      PARSING LAYER                             │
│                  ┌──────────────────┐                          │
│                  │  PromptParser    │  ← Text → SceneData      │
│                  └────────┬─────────┘                          │
└───────────────────────────┼─────────────────────────────────────┘
                            ↓
┌───────────────────────────────────────────────────────────────┐
│                       DATA LAYER                               │
│                  ┌──────────────────┐                          │
│                  │    SceneData     │  ← Structured Schema     │
│                  │  - Environment   │                          │
│                  │  - Objects[]     │                          │
│                  │  - Lighting      │                          │
│                  └────────┬─────────┘                          │
└───────────────────────────┼─────────────────────────────────────┘
                            ↓
┌───────────────────────────────────────────────────────────────┐
│                   GENERATION LAYER                             │
│  ┌─────────────────┐ ┌──────────────┐ ┌──────────────────┐   │
│  │EnvironmentMgr   │ │ObjectSpawner │ │ MaterialManager  │   │
│  │- Terrain        │ │- Placement   │ │- Material Cache  │   │
│  │- Sky/Lighting   │ │- Instantiate │ │- Color Mapping   │   │
│  │- Atmosphere     │ │- Algorithms  │ │                  │   │
│  └─────────────────┘ └──────────────┘ └──────────────────┘   │
└───────────────────────────────────────────────────────────────┘
                            ↓
┌───────────────────────────────────────────────────────────────┐
│                    RENDERING LAYER                             │
│                    Unity Scene Graph                           │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐         │
│  │ Terrain │  │ Objects │  │ Lights  │  │  Camera │         │
│  └─────────┘  └─────────┘  └─────────┘  └─────────┘         │
└───────────────────────────────────────────────────────────────┘
```

---

## Component Details

### 1. SceneData.cs

**Purpose:** Define the structured schema for representing a virtual world.

**Key Classes:**
```csharp
SceneData           // Root container
WorldObject         // Individual object definition
LightingData        // Lighting configuration
PointLight          // Point light data
SceneDataHelper     // Validation utilities
```

**Responsibilities:**
- Data structure for world representation
- JSON serialization/deserialization
- Default value handling
- Validation logic

**Design Pattern:** Data Transfer Object (DTO)

---

### 2. PromptParser.cs

**Purpose:** Convert natural language text into structured SceneData.

**Key Methods:**
```csharp
ParsePrompt(string)              // Main entry point
DetectEnvironment(string)        // Extract environment type
DetectSkyState(string)           // Extract time of day
ExtractObjects(string)           // Find objects in prompt
DetectObjectCount(string, type)  // Parse quantities
GenerateRandomPositions()        // Random placement
```

**Algorithm:**
1. Convert prompt to lowercase
2. Scan for environment keywords (forest, city, desert, etc.)
3. Scan for sky keywords (day, night, sunset, etc.)
4. Extract object types using dictionary mapping
5. Detect modifiers (colors, sizes, counts)
6. Infer placement strategies
7. Build SceneData structure

**Limitations (Current Implementation):**
- Uses keyword matching (not semantic understanding)
- Limited to predefined object types
- No context awareness
- No pronoun resolution

**Future Improvements:**
- Replace with LLM API calls (GPT-4, Claude)
- Add context tracking
- Support complex relationships ("tree next to river")
- Handle negations ("no rocks")

---

### 3. MaterialManager.cs

**Purpose:** Centralized material creation and caching.

**Key Features:**
- Singleton pattern for global access
- 40+ predefined materials
- Material caching (create once, reuse many)
- URP and Standard shader support
- Transparency handling

**Materials Included:**
- **Colors:** red, blue, green, yellow, etc.
- **Natural:** wood, stone, grass, sand, water, snow
- **Building:** brick, concrete, marble, glass, metal
- **Fabric:** leather, carpet, fabric, rubber
- **Special:** neon, gold, silver, copper

**API:**
```csharp
GetMaterial(string name)           // Get cached material
ApplyMaterial(GameObject, string)  // Apply to object
CreateMaterialFromHex(string)      // Custom color
HasMaterial(string)                // Check existence
```

**Design Pattern:** Singleton, Factory

---

### 4. EnvironmentManager.cs

**Purpose:** Configure terrain, sky, lighting, and atmosphere.

**Responsibilities:**
- Create and style terrain plane
- Configure directional light (sun/moon)
- Set ambient lighting colors
- Configure fog and atmosphere
- Support multiple environment types

**Key Methods:**
```csharp
SetupEnvironment(SceneData)     // Full environment setup
SetupTerrain(string)            // Create terrain
SetupLighting(LightingData)     // Configure lights
SetupSky(string, LightingData)  // Configure ambient
SetupAtmosphere(string)         // Add effects
```

**Lighting Configurations:**
- **Daytime:** Bright, blue sky, soft fog
- **Night:** Dark, low intensity, dark fog
- **Sunset/Sunrise:** Orange/pink tones, medium intensity
- **Cloudy:** Gray tones, dense fog

---

### 5. ObjectSpawner.cs

**Purpose:** Instantiate objects with smart placement algorithms.

**Placement Strategies:**

#### Random
```csharp
GenerateRandomPositions(WorldObject)
```
- Completely random positions within spawn area
- May overlap
- Fast, simple

#### Scattered
```csharp
GenerateScatteredPositions(WorldObject)
```
- Random positions with minimum distance
- No overlaps
- Natural appearance
- Uses rejection sampling

#### Grid
```csharp
GenerateGridPositions(WorldObject)
```
- Organized rows and columns
- Perfect for classrooms, offices
- Calculates grid size automatically

#### Line/Row
```csharp
GenerateLinePositions(WorldObject)
```
- Single row arrangement
- Equal spacing
- Good for fences, roads

#### Circle
```csharp
GenerateCirclePositions(WorldObject)
```
- Circular pattern
- Radius scales with count
- Good for decorative arrangements

#### Center
```csharp
GenerateCenterPosition(WorldObject)
```
- Single object at center
- Good for focal points

**Key Methods:**
```csharp
SpawnObject(WorldObject)        // Spawn all instances
CreateInstance(WorldObject, Vector3, int)  // Single instance
GeneratePositions(WorldObject)  // Get positions list
ClearAllObjects()              // Cleanup
```

---

### 6. WorldGenerator.cs

**Purpose:** Main orchestrator coordinating all subsystems.

**Generation Pipeline:**
```csharp
1. Parse prompt → SceneData
2. Clear existing world
3. Setup environment (terrain, sky, lights)
4. Spawn all objects (spread across frames)
5. Finalize and log results
```

**Key Features:**
- Coroutine-based generation (non-blocking)
- Frame spreading for performance
- Debug logging with timestamps
- Context menu test functions
- Public API for external systems

**API Methods:**
```csharp
GenerateWorldFromPrompt(string)      // Main entry
GenerateWorldFromSceneData(SceneData) // Skip parsing
ClearWorld()                         // Reset scene
RegenerateCurrentWorld()             // Randomize again
IsGenerating()                       // Check status
GetCurrentScene()                    // Get SceneData
```

**Design Pattern:** Facade, Coordinator

---

### 7. TextInputUI.cs

**Purpose:** Unity UI controller for text input and generation.

**Features:**
- InputField for text prompts
- Generate button
- Clear button
- Example prompt buttons
- Status text display
- Enter key submit

**Setup Requirements:**
1. Canvas
2. TMP_InputField
3. Buttons
4. Status TextMeshProUGUI
5. WorldGenerator reference

**Event Handling:**
```csharp
OnGenerateButtonClicked()   // Button click
OnPromptSubmit(string)      // Enter key
OnClearButtonClicked()      // Clear button
LoadExample(string)         // Example button
```

---

### 8. ExamplePrompts.cs

**Purpose:** Collection of test prompts for quick testing.

**Categories:**
- Nature (forest, desert, beach, mountain)
- Urban (city, street, park)
- Indoor (classroom, office, living room)
- Fantasy (space, medieval, cave)
- Test (simple geometry tests)
- Complex (multi-element scenes)

**API:**
```csharp
GetDefaultExamples()          // All prompts
GetPromptsByCategory(string)  // Filter by category
GetRandomPrompt()             // Random selection
GetCategories()               // All category names
```

---

## Data Flow

### Complete Generation Flow

```
1. User Input
   "Create a forest with trees and rocks"
           ↓
2. TextInputUI captures input
           ↓
3. WorldGenerator.GenerateWorldFromPrompt(prompt)
           ↓
4. PromptParser.ParsePrompt(prompt)
   - Detect environment: "forest"
   - Detect sky: "daytime" (default)
   - Extract objects: [tree, rock]
   - Detect counts: [20 trees, 10 rocks]
   - Detect placement: ["scattered", "random"]
           ↓
5. Create SceneData
   {
     environment: "forest",
     sky: "daytime",
     terrain: "grass",
     objects: [
       { type: "tree", count: 20, ... },
       { type: "rock", count: 10, ... }
     ]
   }
           ↓
6. EnvironmentManager.SetupEnvironment(sceneData)
   - Create grass terrain plane
   - Configure daytime lighting
   - Set blue sky ambient
   - Enable fog
           ↓
7. ObjectSpawner.SpawnObject(tree WorldObject)
   - Generate 20 scattered positions
   - Create 20 cylinder primitives
   - Apply wood material
   - Parent to WorldRoot
           ↓
8. ObjectSpawner.SpawnObject(rock WorldObject)
   - Generate 10 random positions
   - Create 10 sphere primitives
   - Apply stone material
   - Parent to WorldRoot
           ↓
9. Complete!
   - Log generation stats
   - Display "Generation complete"
```

---

## SceneData Schema

### JSON Example

```json
{
  "environment": "forest",
  "sky": "sunset",
  "terrain": "grass",
  "objects": [
    {
      "type": "tree",
      "primitiveShape": "cylinder",
      "count": 20,
      "size": "medium",
      "customSize": { "x": 2, "y": 2, "z": 2 },
      "material": "wood",
      "color": "default",
      "placement": "scattered",
      "position": { "x": 0, "y": 0, "z": 0 },
      "rotation": { "x": 0, "y": 0, "z": 0 },
      "shape": "default",
      "relationship": "",
      "metadata": {}
    },
    {
      "type": "river",
      "primitiveShape": "plane",
      "count": 1,
      "size": "large",
      "material": "water",
      "placement": "center"
    }
  ],
  "lighting": {
    "ambientMode": "gradient",
    "skyColor": "#FF6B35",
    "horizonColor": "#F7931E",
    "groundColor": "#4A4A4A",
    "directionalIntensity": 0.7,
    "directionalRotation": { "x": 10, "y": -30, "z": 0 },
    "additionalLights": []
  }
}
```

---

## Parser Algorithm

### Keyword Detection

The current parser uses **keyword matching** with dictionaries:

```csharp
// Environment detection
foreach (keyword in environmentKeywords)
    if (prompt.Contains(keyword))
        return environmentType

// Object detection
foreach (word in prompt.Split())
    if (objectMappings.ContainsKey(word))
        foundObjects.Add(CreateWorldObject(word))
```

### Count Extraction

Uses regex patterns:
```csharp
pattern = @"(\d+|one|two|three|many)\s+{objectType}"
match = Regex.Match(prompt, pattern)
if (match) return ParseNumber(match)
```

### Size Detection

Similar regex approach:
```csharp
pattern = @"(tiny|small|medium|large|huge)\s+{objectType}"
```

### Future: LLM Integration

Replace keyword matching with AI:

```csharp
public static async Task<SceneData> ParsePromptWithAI(string prompt)
{
    // 1. Send prompt to GPT-4 or Claude
    var aiResponse = await OpenAI.Complete(
        systemPrompt: "Convert natural language to JSON scene description",
        userPrompt: prompt
    );
    
    // 2. Parse JSON response
    var sceneData = JsonUtility.FromJson<SceneData>(aiResponse);
    
    // 3. Validate and return
    return sceneData;
}
```

---

## Placement Algorithms

### Scattered Placement (Rejection Sampling)

```
Input: count, spawnArea, minDistance
Output: positions[]

attempts = 0
while positions.Count < count and attempts < maxAttempts:
    candidatePos = Random.Range(spawnArea.min, spawnArea.max)
    
    valid = true
    foreach existingPos in positions:
        if Distance(candidatePos, existingPos) < minDistance:
            valid = false
            break
    
    if valid:
        positions.Add(candidatePos)
    
    attempts++

return positions
```

### Grid Placement

```
Input: count
Output: positions[]

gridSize = ceil(sqrt(count))
spacing = max(minDistance, objectSize * 1.5)
startPos = -((gridSize-1) * spacing) / 2

for row in 0..gridSize:
    for col in 0..gridSize:
        if positions.Count >= count:
            break
        pos = startPos + (col * spacing, 0, row * spacing)
        positions.Add(pos)

return positions
```

---

## Extension Points

### 1. Add New Object Types

**In PromptParser.cs:**
```csharp
private static Dictionary<string, (string, string)> objectMappings = {
    { "newobject", ("cube", "metal") },  // Add here
    // ...
};
```

### 2. Add New Materials

**In MaterialManager.cs:**
```csharp
private void InitializeMaterials()
{
    CreateMaterial("newmaterial", new Color(0.5f, 0.5f, 0.5f));
}
```

### 3. Add New Environment Types

**In PromptParser.cs:**
```csharp
private static Dictionary<string, string> environmentKeywords = {
    { "newenv", "newenv" },  // Add here
};
```

**In EnvironmentManager.cs:**
```csharp
private string GetTerrainMaterial(string terrainType)
{
    switch (terrainType.ToLower())
    {
        case "newenv": return "custommaterial";
        // ...
    }
}
```

### 4. Add New Placement Algorithm

**In ObjectSpawner.cs:**
```csharp
private List<Vector3> GeneratePositions(WorldObject obj)
{
    switch (obj.placement.ToLower())
    {
        case "newpattern":
            return GenerateNewPattern(obj);
        // ...
    }
}

private List<Vector3> GenerateNewPattern(WorldObject obj)
{
    // Your algorithm here
}
```

### 5. Integrate LLM API

**Create new file: AIPromptParser.cs**
```csharp
using OpenAI;  // Or Anthropic, etc.

public class AIPromptParser
{
    public static async Task<SceneData> ParseWithAI(string prompt)
    {
        // API call
        // JSON parsing
        // Return SceneData
    }
}
```

**Update WorldGenerator.cs:**
```csharp
// Option to use AI parser
public bool useAIParser = false;

if (useAIParser)
    _currentScene = await AIPromptParser.ParseWithAI(prompt);
else
    _currentScene = PromptParser.ParsePrompt(prompt);
```

---

## Performance Considerations

### Frame Spreading

Objects are spawned across multiple frames to avoid freezing:

```csharp
foreach (var worldObj in _currentScene.objects)
{
    objectSpawner.SpawnObject(worldObj);
    yield return null;  // Next frame
}
```

### Object Pooling (Future)

For frequent regeneration:
```csharp
public class ObjectPool
{
    Dictionary<PrimitiveType, Queue<GameObject>> pools;
    
    public GameObject Get(PrimitiveType type)
    {
        if (pools[type].Count > 0)
            return pools[type].Dequeue();
        return CreateNew(type);
    }
    
    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        pools[obj.type].Enqueue(obj);
    }
}
```

### Material Caching

MaterialManager caches all materials:
- Created once on startup
- Reused for all objects
- No runtime material creation

### Spawn Area Limits

Default: 80x80 area, max 100 objects per type
- Prevents infinite loops in scattered placement
- Configurable per scene

---

## Testing Strategy

### Unit Testing (Recommended)

```csharp
[Test]
public void TestParseSimplePrompt()
{
    var scene = PromptParser.ParsePrompt("create a forest");
    Assert.AreEqual("forest", scene.environment);
    Assert.IsTrue(scene.objects.Count > 0);
}

[Test]
public void TestObjectCountDetection()
{
    var scene = PromptParser.ParsePrompt("create 10 trees");
    var trees = scene.objects.Find(o => o.type == "tree");
    Assert.AreEqual(10, trees.count);
}

[Test]
public void TestMaterialManager()
{
    var mat = MaterialManager.Instance.GetMaterial("wood");
    Assert.IsNotNull(mat);
    Assert.AreEqual("wood", mat.name);
}
```

### Integration Testing

Use context menu functions:
1. `Test: Generate Forest`
2. `Test: Generate City`
3. Verify objects spawn correctly
4. Verify materials applied
5. Verify lighting configured

### Manual Testing

Example prompts in `ExamplePrompts.cs`:
- Test each category
- Verify edge cases
- Test malformed prompts

---

## Error Handling

### Validation Layers

1. **Input Validation** (TextInputUI)
   - Check for empty prompts
   - Display error messages

2. **Parse Validation** (PromptParser)
   - Fallback to defaults if parsing fails
   - Log warnings for unknown keywords

3. **Data Validation** (SceneDataHelper)
   - Check required fields
   - Validate counts and ranges

4. **Generation Validation** (WorldGenerator)
   - Check for null components
   - Verify spawn area bounds
   - Limit object counts

### Error Messages

```csharp
// Clear, actionable messages
Debug.LogError("WorldGenerator: No camera found. Please add a camera to the scene.");
Debug.LogWarning("PromptParser: Unknown object 'xyz', skipping.");
Debug.Log("ObjectSpawner: Could only place 15/20 trees (spawn area full).");
```

---

## Best Practices

### For Developers

1. **Always check for null** before using references
2. **Use coroutines** for multi-frame operations
3. **Log important steps** with clear prefixes
4. **Cache expensive operations** (materials, transforms)
5. **Validate input data** at every layer
6. **Use meaningful variable names** (no single letters)
7. **Comment complex logic** for student developers
8. **Keep functions small** (< 30 lines ideally)

### For Extension

1. **Add to dictionaries** rather than modifying core logic
2. **Create new files** for new major features
3. **Follow namespace conventions** (TextToWorld.*)
4. **Document public APIs** with XML comments
5. **Test with example prompts** before committing

---

## Glossary

- **SceneData** - Structured JSON representation of a world
- **WorldObject** - Definition of an object type to spawn
- **Primitive** - Basic Unity shapes (cube, sphere, etc.)
- **Placement Strategy** - Algorithm for positioning objects
- **Material** - Visual appearance (color, texture, shader)
- **Environment** - Overall world type (forest, city, etc.)
- **Orchestrator** - Component that coordinates other components

---

## References

- Unity Documentation: https://docs.unity3d.com/
- JSON Serialization: https://docs.unity3d.com/Manual/JSONSerialization.html
- Coroutines: https://docs.unity3d.com/Manual/Coroutines.html
- TextMeshPro: https://docs.unity3d.com/Packages/com.unity.textmeshpro@latest

---

**Ready to dive deeper? Start exploring the code! 🚀**
