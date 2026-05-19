using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data models for representing a virtual world scene in a structured format.
/// These classes define the schema for scene JSON that can be generated from user prompts.
/// </summary>
namespace TextToWorld.Data
{
    /// <summary>
    /// Root scene data structure containing environment and objects.
    /// Example JSON format that this class represents:
    /// {
    ///   "environment": "forest",
    ///   "sky": "daytime",
    ///   "terrain": "grass",
    ///   "objects": [...]
    /// }
    /// </summary>
    [Serializable]
    public class SceneData
    {
        public string environment = "default";      // Environment type: forest, city, desert, classroom, etc.
        public string sky = "daytime";              // Sky state: daytime, night, sunset, sunrise, cloudy
        public string terrain = "grass";            // Terrain type: grass, sand, concrete, wood, snow
        public List<WorldObject> objects = new List<WorldObject>();  // All objects in the scene
        public LightingData lighting = new LightingData();           // Lighting configuration

        /// <summary>
        /// Creates a default empty scene.
        /// </summary>
        public SceneData()
        {
            objects = new List<WorldObject>();
            lighting = new LightingData();
        }

        /// <summary>
        /// Converts this scene data to a formatted JSON string.
        /// </summary>
        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }

        /// <summary>
        /// Creates a SceneData object from a JSON string.
        /// </summary>
        public static SceneData FromJson(string json)
        {
            return JsonUtility.FromJson<SceneData>(json);
        }
    }

    /// <summary>
    /// Represents a single object/entity in the virtual world.
    /// Includes type, count, size, material, placement, and other properties.
    /// </summary>
    [Serializable]
    public class WorldObject
    {
        public string type;                  // Object type: tree, building, car, desk, etc.
        public string primitiveShape = "cube";  // Primitive shape: cube, sphere, cylinder, capsule, plane
        public int count = 1;                // Number of instances to create
        public string size = "medium";       // Size: small, medium, large, or custom
        public Vector3 customSize = Vector3.one;  // Custom size (if not using preset)
        public string material = "default";  // Material name: wood, metal, glass, brick, etc.
        public string color = "default";     // Color override
        public string placement = "random";  // Placement strategy: random, scattered, grid, line, circle, center
        public Vector3 position = Vector3.zero;  // Specific position (if not random)
        public Vector3 rotation = Vector3.zero;  // Rotation in degrees
        public string shape = "default";     // Shape modifier: curved, straight, angular, etc.
        public string relationship = "";     // Relationship to other objects: "near river", "on table", etc.
        public Dictionary<string, string> metadata = new Dictionary<string, string>();  // Additional properties

        /// <summary>
        /// Gets the actual Unity Vector3 scale based on size preset or custom size.
        /// </summary>
        public Vector3 GetScale()
        {
            if (customSize != Vector3.one) return customSize;

            // Convert size string to scale
            switch (size.ToLower())
            {
                case "tiny": return new Vector3(0.5f, 0.5f, 0.5f);
                case "small": return new Vector3(1f, 1f, 1f);
                case "medium": return new Vector3(2f, 2f, 2f);
                case "large": return new Vector3(4f, 4f, 4f);
                case "huge": return new Vector3(8f, 8f, 8f);
                default: return new Vector3(2f, 2f, 2f);  // medium as default
            }
        }

        /// <summary>
        /// Gets the Unity PrimitiveType from the primitiveShape string.
        /// </summary>
        public PrimitiveType GetPrimitiveType()
        {
            switch (primitiveShape.ToLower())
            {
                case "sphere": return PrimitiveType.Sphere;
                case "cylinder": return PrimitiveType.Cylinder;
                case "capsule": return PrimitiveType.Capsule;
                case "plane": return PrimitiveType.Plane;
                case "cube":
                default: return PrimitiveType.Cube;
            }
        }
    }

    /// <summary>
    /// Lighting configuration for the scene.
    /// Controls ambient light, directional light, and additional lights.
    /// </summary>
    [Serializable]
    public class LightingData
    {
        public string ambientMode = "gradient";  // Ambient lighting: gradient, flat, skybox
        public string skyColor = "#87CEEB";      // Sky color (hex)
        public string horizonColor = "#FFA500";  // Horizon color (hex)
        public string groundColor = "#228B22";   // Ground color (hex)
        public float directionalIntensity = 1.0f;  // Main directional light intensity
        public Vector3 directionalRotation = new Vector3(50, -30, 0);  // Sun/moon direction
        public List<PointLight> additionalLights = new List<PointLight>();  // Extra lights

        public LightingData()
        {
            additionalLights = new List<PointLight>();
        }
    }

    /// <summary>
    /// Represents a point light in the scene.
    /// </summary>
    [Serializable]
    public class PointLight
    {
        public Vector3 position = Vector3.zero;
        public string color = "white";
        public float intensity = 1.0f;
        public float range = 10.0f;
    }

    /// <summary>
    /// Helper class for parsing and validating scene data.
    /// </summary>
    public static class SceneDataHelper
    {
        /// <summary>
        /// Validates that a SceneData object has required fields.
        /// </summary>
        public static bool IsValid(SceneData sceneData)
        {
            if (sceneData == null) return false;
            if (string.IsNullOrEmpty(sceneData.environment)) return false;
            if (sceneData.objects == null) return false;
            return true;
        }

        /// <summary>
        /// Creates a sample scene for testing.
        /// </summary>
        public static SceneData CreateSampleScene()
        {
            var scene = new SceneData
            {
                environment = "forest",
                sky = "daytime",
                terrain = "grass"
            };

            scene.objects.Add(new WorldObject
            {
                type = "tree",
                primitiveShape = "cylinder",
                count = 10,
                size = "medium",
                material = "wood",
                placement = "scattered"
            });

            scene.objects.Add(new WorldObject
            {
                type = "rock",
                primitiveShape = "sphere",
                count = 15,
                size = "small",
                material = "stone",
                placement = "random"
            });

            return scene;
        }
    }
}
