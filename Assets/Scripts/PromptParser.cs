using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using TextToWorld.Data;

/// <summary>
/// Parses natural language text prompts into structured SceneData.
/// This is the core intelligence layer that converts user input into world descriptions.
/// 
/// TODO: This uses basic keyword matching and regex patterns.
/// Future improvement: Integrate with AI/LLM APIs (OpenAI, Claude, etc.) for better understanding.
/// </summary>
namespace TextToWorld.Parsing
{
    public class PromptParser : MonoBehaviour
    {
        // Environment keywords mapping
        private static Dictionary<string, string> environmentKeywords = new Dictionary<string, string>
        {
            { "forest", "forest" },
            { "woods", "forest" },
            { "jungle", "forest" },
            { "city", "city" },
            { "urban", "city" },
            { "town", "city" },
            { "desert", "desert" },
            { "sand", "desert" },
            { "classroom", "classroom" },
            { "school", "classroom" },
            { "office", "office" },
            { "workspace", "office" },
            { "beach", "beach" },
            { "ocean", "beach" },
            { "mountain", "mountain" },
            { "space", "space" },
            { "futuristic", "futuristic" },
            { "medieval", "medieval" },
            { "dungeon", "dungeon" }
        };

        // Object type mappings to primitives and materials
        private static Dictionary<string, (string primitive, string material)> objectMappings = new Dictionary<string, (string, string)>
        {
            // Natural objects
            { "tree", ("cylinder", "wood") },
            { "trees", ("cylinder", "wood") },
            { "rock", ("sphere", "stone") },
            { "rocks", ("sphere", "stone") },
            { "stone", ("cube", "stone") },
            { "boulder", ("sphere", "stone") },
            { "river", ("plane", "water") },
            { "water", ("plane", "water") },
            { "grass", ("plane", "grass") },
            { "bush", ("sphere", "green") },
            { "flower", ("sphere", "red") },
            
            // Structures
            { "bridge", ("cube", "wood") },
            { "building", ("cube", "concrete") },
            { "buildings", ("cube", "concrete") },
            { "house", ("cube", "brick") },
            { "tower", ("cylinder", "stone") },
            { "wall", ("cube", "brick") },
            { "fence", ("cube", "wood") },
            
            // Furniture
            { "desk", ("cube", "wood") },
            { "table", ("cube", "wood") },
            { "chair", ("cube", "leather") },
            { "sofa", ("cube", "brown") },
            { "bed", ("cube", "white") },
            
            // Classroom items
            { "whiteboard", ("plane", "white") },
            { "blackboard", ("plane", "black") },
            { "board", ("plane", "white") },
            
            // City/futuristic
            { "car", ("cube", "metal") },
            { "cars", ("cube", "metal") },
            { "vehicle", ("cube", "metal") },
            { "road", ("plane", "asphalt") },
            { "street", ("plane", "asphalt") },
            { "skyscraper", ("cube", "glass") },
            { "neon", ("cube", "cyan") },
            
            // Desert
            { "cactus", ("cylinder", "green") },
            { "cacti", ("cylinder", "green") },
            { "dune", ("sphere", "sand") },
            { "sand", ("plane", "sand") }
        };

        // Sky time keywords
        private static Dictionary<string, string> skyKeywords = new Dictionary<string, string>
        {
            { "day", "daytime" },
            { "daytime", "daytime" },
            { "sunny", "daytime" },
            { "night", "night" },
            { "nighttime", "night" },
            { "dark", "night" },
            { "sunset", "sunset" },
            { "dusk", "sunset" },
            { "sunrise", "sunrise" },
            { "dawn", "sunrise" },
            { "cloudy", "cloudy" },
            { "overcast", "cloudy" }
        };

        // Color keywords
        private static string[] colorKeywords = new string[]
        {
            "red", "blue", "green", "yellow", "orange", "purple", "pink",
            "white", "black", "gray", "brown", "cyan", "magenta"
        };

        /// <summary>
        /// Main method: Parse a natural language prompt into structured SceneData.
        /// </summary>
        public static SceneData ParsePrompt(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                Debug.LogWarning("PromptParser: Empty prompt received");
                return CreateDefaultScene();
            }

            Debug.Log($"PromptParser: Parsing prompt - '{prompt}'");

            var sceneData = new SceneData();
            string lowerPrompt = prompt.ToLower();

            // 1. Detect environment type
            sceneData.environment = DetectEnvironment(lowerPrompt);
            Debug.Log($"  Environment detected: {sceneData.environment}");

            // 2. Detect sky/time of day
            sceneData.sky = DetectSkyState(lowerPrompt);
            Debug.Log($"  Sky state detected: {sceneData.sky}");

            // 3. Detect terrain type
            sceneData.terrain = DetectTerrain(lowerPrompt, sceneData.environment);
            Debug.Log($"  Terrain detected: {sceneData.terrain}");

            // 4. Extract objects from the prompt
            sceneData.objects = ExtractObjects(lowerPrompt);
            Debug.Log($"  Objects extracted: {sceneData.objects.Count}");

            // 5. Configure lighting based on sky state
            sceneData.lighting = ConfigureLighting(sceneData.sky);

            return sceneData;
        }

        /// <summary>
        /// Detect the environment type from the prompt.
        /// </summary>
        private static string DetectEnvironment(string prompt)
        {
            foreach (var kvp in environmentKeywords)
            {
                if (prompt.Contains(kvp.Key))
                {
                    return kvp.Value;
                }
            }
            return "default";
        }

        /// <summary>
        /// Detect the sky/lighting state from the prompt.
        /// </summary>
        private static string DetectSkyState(string prompt)
        {
            foreach (var kvp in skyKeywords)
            {
                if (prompt.Contains(kvp.Key))
                {
                    return kvp.Value;
                }
            }
            return "daytime";  // Default to daytime
        }

        /// <summary>
        /// Detect terrain type based on environment and keywords.
        /// </summary>
        private static string DetectTerrain(string prompt, string environment)
        {
            // Explicit terrain keywords
            if (prompt.Contains("grass")) return "grass";
            if (prompt.Contains("sand")) return "sand";
            if (prompt.Contains("concrete")) return "concrete";
            if (prompt.Contains("wood")) return "wood";
            if (prompt.Contains("stone")) return "stone";
            if (prompt.Contains("snow")) return "snow";
            if (prompt.Contains("ice")) return "ice";

            // Infer from environment
            switch (environment)
            {
                case "forest": return "grass";
                case "desert": return "sand";
                case "city": return "concrete";
                case "classroom": return "wood";
                case "office": return "carpet";
                case "beach": return "sand";
                case "mountain": return "stone";
                case "space": return "metal";
                default: return "grass";
            }
        }

        /// <summary>
        /// Extract objects mentioned in the prompt and create WorldObject instances.
        /// </summary>
        private static List<WorldObject> ExtractObjects(string prompt)
        {
            var objects = new List<WorldObject>();
            var words = prompt.Split(new char[] { ' ', ',', '.', '!', '?', ';' }, StringSplitOptions.RemoveEmptyEntries);

            // Track which object types have been found
            var foundObjects = new Dictionary<string, WorldObject>();

            // Scan for object keywords
            foreach (var word in words)
            {
                string lowerWord = word.ToLower();
                
                if (objectMappings.ContainsKey(lowerWord) && !foundObjects.ContainsKey(lowerWord))
                {
                    var (primitive, material) = objectMappings[lowerWord];
                    
                    var obj = new WorldObject
                    {
                        type = lowerWord,
                        primitiveShape = primitive,
                        material = material,
                        count = DetectObjectCount(prompt, lowerWord),
                        size = DetectObjectSize(prompt, lowerWord),
                        placement = DetectPlacement(prompt, lowerWord)
                    };

                    // Check for color modifiers
                    string detectedColor = DetectColor(prompt, lowerWord);
                    if (!string.IsNullOrEmpty(detectedColor))
                    {
                        obj.color = detectedColor;
                        obj.material = detectedColor;
                    }

                    foundObjects[lowerWord] = obj;
                    objects.Add(obj);
                }
            }

            // If no objects found, add some default objects based on environment
            if (objects.Count == 0)
            {
                Debug.LogWarning("PromptParser: No specific objects detected, using defaults");
                objects = GetDefaultObjectsForEnvironment(DetectEnvironment(prompt));
            }

            return objects;
        }

        /// <summary>
        /// Try to detect how many instances of an object should be created.
        /// </summary>
        private static int DetectObjectCount(string prompt, string objectType)
        {
            // Look for patterns like "5 trees", "ten rocks", "many buildings"
            var numberPattern = $@"(\d+|one|two|three|four|five|six|seven|eight|nine|ten|many|few|several)\s+{objectType}";
            var match = Regex.Match(prompt, numberPattern, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                string numStr = match.Groups[1].Value.ToLower();
                
                // Parse numeric or word numbers
                switch (numStr)
                {
                    case "one": return 1;
                    case "two": return 2;
                    case "three": return 3;
                    case "four": return 4;
                    case "five": return 5;
                    case "six": return 6;
                    case "seven": return 7;
                    case "eight": return 8;
                    case "nine": return 9;
                    case "ten": return 10;
                    case "many": return 20;
                    case "few": return 3;
                    case "several": return 5;
                    default:
                        if (int.TryParse(numStr, out int count))
                            return Mathf.Clamp(count, 1, 100);  // Limit to max 100
                        break;
                }
            }

            // Default counts based on object type
            if (objectType.Contains("tree")) return 15;
            if (objectType.Contains("rock")) return 10;
            if (objectType.Contains("building")) return 5;
            if (objectType.Contains("car")) return 8;
            
            return 1;  // Default single instance
        }

        /// <summary>
        /// Detect size modifiers (small, medium, large).
        /// </summary>
        private static string DetectObjectSize(string prompt, string objectType)
        {
            var sizePattern = $@"(tiny|small|medium|large|huge|big)\s+{objectType}";
            var match = Regex.Match(prompt, sizePattern, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                return match.Groups[1].Value.ToLower();
            }

            return "medium";  // Default size
        }

        /// <summary>
        /// Detect placement strategy from prompt.
        /// </summary>
        private static string DetectPlacement(string prompt, string objectType)
        {
            if (prompt.Contains("scattered")) return "scattered";
            if (prompt.Contains("random")) return "random";
            if (prompt.Contains("grid")) return "grid";
            if (prompt.Contains("line") || prompt.Contains("row")) return "line";
            if (prompt.Contains("circle")) return "circle";
            if (prompt.Contains("center")) return "center";

            return "random";  // Default to random placement
        }

        /// <summary>
        /// Detect color modifiers near an object.
        /// </summary>
        private static string DetectColor(string prompt, string objectType)
        {
            foreach (var color in colorKeywords)
            {
                // Check for patterns like "red car" or "car is red"
                if (prompt.Contains($"{color} {objectType}") || prompt.Contains($"{objectType} {color}"))
                {
                    return color;
                }
            }
            return null;
        }

        /// <summary>
        /// Configure lighting based on sky state.
        /// </summary>
        private static LightingData ConfigureLighting(string skyState)
        {
            var lighting = new LightingData();

            switch (skyState)
            {
                case "daytime":
                    lighting.skyColor = "#87CEEB";      // Sky blue
                    lighting.horizonColor = "#FFA500";  // Orange
                    lighting.groundColor = "#228B22";   // Forest green
                    lighting.directionalIntensity = 1.0f;
                    lighting.directionalRotation = new Vector3(50, -30, 0);
                    break;

                case "night":
                    lighting.skyColor = "#0C1445";      // Dark blue
                    lighting.horizonColor = "#1A1A2E";  // Dark purple
                    lighting.groundColor = "#0F0F0F";   // Almost black
                    lighting.directionalIntensity = 0.3f;
                    lighting.directionalRotation = new Vector3(-50, -30, 0);
                    break;

                case "sunset":
                    lighting.skyColor = "#FF6B35";      // Orange
                    lighting.horizonColor = "#F7931E";  // Yellow-orange
                    lighting.groundColor = "#4A4A4A";   // Gray
                    lighting.directionalIntensity = 0.7f;
                    lighting.directionalRotation = new Vector3(10, -30, 0);
                    break;

                case "sunrise":
                    lighting.skyColor = "#FFB6C1";      // Light pink
                    lighting.horizonColor = "#FFA500";  // Orange
                    lighting.groundColor = "#5A5A5A";   // Gray
                    lighting.directionalIntensity = 0.6f;
                    lighting.directionalRotation = new Vector3(10, 30, 0);
                    break;

                case "cloudy":
                    lighting.skyColor = "#808080";      // Gray
                    lighting.horizonColor = "#A9A9A9";  // Dark gray
                    lighting.groundColor = "#696969";   // Dim gray
                    lighting.directionalIntensity = 0.5f;
                    lighting.directionalRotation = new Vector3(50, -30, 0);
                    break;
            }

            return lighting;
        }

        /// <summary>
        /// Get default objects for an environment if none are specified.
        /// </summary>
        private static List<WorldObject> GetDefaultObjectsForEnvironment(string environment)
        {
            var objects = new List<WorldObject>();

            switch (environment)
            {
                case "forest":
                    objects.Add(new WorldObject { type = "tree", primitiveShape = "cylinder", count = 20, material = "wood", placement = "scattered" });
                    objects.Add(new WorldObject { type = "rock", primitiveShape = "sphere", count = 10, material = "stone", placement = "random", size = "small" });
                    break;

                case "desert":
                    objects.Add(new WorldObject { type = "dune", primitiveShape = "sphere", count = 8, material = "sand", placement = "scattered", size = "large" });
                    objects.Add(new WorldObject { type = "cactus", primitiveShape = "cylinder", count = 12, material = "green", placement = "random" });
                    break;

                case "city":
                    objects.Add(new WorldObject { type = "building", primitiveShape = "cube", count = 15, material = "concrete", placement = "grid", size = "large" });
                    objects.Add(new WorldObject { type = "car", primitiveShape = "cube", count = 10, material = "metal", placement = "random", size = "small" });
                    break;

                case "classroom":
                    objects.Add(new WorldObject { type = "desk", primitiveShape = "cube", count = 20, material = "wood", placement = "grid", size = "small" });
                    objects.Add(new WorldObject { type = "chair", primitiveShape = "cube", count = 20, material = "brown", placement = "grid", size = "small" });
                    objects.Add(new WorldObject { type = "whiteboard", primitiveShape = "plane", count = 1, material = "white", placement = "center" });
                    break;

                default:
                    // Generic default
                    objects.Add(new WorldObject { type = "cube", primitiveShape = "cube", count = 5, material = "gray", placement = "random" });
                    break;
            }

            return objects;
        }

        /// <summary>
        /// Create a minimal default scene when parsing fails.
        /// </summary>
        private static SceneData CreateDefaultScene()
        {
            return new SceneData
            {
                environment = "default",
                sky = "daytime",
                terrain = "grass",
                objects = new List<WorldObject>
                {
                    new WorldObject { type = "placeholder", primitiveShape = "cube", count = 1, material = "gray", placement = "center" }
                }
            };
        }
    }
}
