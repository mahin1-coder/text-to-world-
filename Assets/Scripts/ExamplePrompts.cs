using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Collection of example prompts for testing the Text-to-World system.
/// Use these to quickly test different scenarios and environments.
/// </summary>
namespace TextToWorld.Examples
{
    [CreateAssetMenu(fileName = "ExamplePrompts", menuName = "TextToWorld/Example Prompts")]
    public class ExamplePrompts : ScriptableObject
    {
        [System.Serializable]
        public class PromptExample
        {
            public string name;
            [TextArea(2, 5)]
            public string prompt;
            public string category;
        }

        [Header("Example Prompt Categories")]
        public List<PromptExample> prompts = new List<PromptExample>();

        // ===== BUILT-IN EXAMPLES =====
        
        public static List<PromptExample> GetDefaultExamples()
        {
            return new List<PromptExample>
            {
                // NATURE ENVIRONMENTS
                new PromptExample
                {
                    name = "Forest Scene",
                    prompt = "Create a forest with 20 trees, rocks scattered around, and a river running through the center",
                    category = "Nature"
                },
                new PromptExample
                {
                    name = "Desert Landscape",
                    prompt = "Create a desert world with sand dunes, cactus plants, rocks, and a sunset sky",
                    category = "Nature"
                },
                new PromptExample
                {
                    name = "Beach Scene",
                    prompt = "Make a beach with sand, water, rocks, and palm trees",
                    category = "Nature"
                },
                new PromptExample
                {
                    name = "Mountain Area",
                    prompt = "Generate a mountain environment with rocks, boulders, and snow at night",
                    category = "Nature"
                },

                // URBAN ENVIRONMENTS
                new PromptExample
                {
                    name = "Futuristic City",
                    prompt = "Generate a futuristic city at night with neon lights, tall buildings, roads, and flying cars",
                    category = "Urban"
                },
                new PromptExample
                {
                    name = "City Street",
                    prompt = "Create a city street with buildings, cars, roads, and street lights",
                    category = "Urban"
                },
                new PromptExample
                {
                    name = "Urban Park",
                    prompt = "Make an urban park with grass, trees, benches, and a fountain in the center",
                    category = "Urban"
                },

                // INDOOR ENVIRONMENTS
                new PromptExample
                {
                    name = "Classroom",
                    prompt = "Generate a classroom with 20 desks arranged in a grid, 20 chairs, a whiteboard, and windows",
                    category = "Indoor"
                },
                new PromptExample
                {
                    name = "Office Space",
                    prompt = "Create an office with desks, chairs, computers, and a whiteboard",
                    category = "Indoor"
                },
                new PromptExample
                {
                    name = "Living Room",
                    prompt = "Make a living room with a sofa, coffee table, and a TV",
                    category = "Indoor"
                },
                new PromptExample
                {
                    name = "Library",
                    prompt = "Generate a library with many desks, chairs, and bookshelves arranged in rows",
                    category = "Indoor"
                },

                // FANTASY/CREATIVE
                new PromptExample
                {
                    name = "Space Station",
                    prompt = "Create a space environment with metallic structures and floating objects in the dark",
                    category = "Fantasy"
                },
                new PromptExample
                {
                    name = "Medieval Village",
                    prompt = "Make a medieval village with stone buildings, wooden fences, and a town square",
                    category = "Fantasy"
                },
                new PromptExample
                {
                    name = "Crystal Cave",
                    prompt = "Generate a cave with colorful crystal rocks scattered everywhere",
                    category = "Fantasy"
                },

                // SIMPLE TESTS
                new PromptExample
                {
                    name = "Simple Scene",
                    prompt = "Create a scene with 10 red cubes and 5 blue spheres",
                    category = "Test"
                },
                new PromptExample
                {
                    name = "Grid Test",
                    prompt = "Make a grid of 25 desks",
                    category = "Test"
                },
                new PromptExample
                {
                    name = "Circle Pattern",
                    prompt = "Create 12 trees arranged in a circle",
                    category = "Test"
                },

                // COMPLEX SCENES
                new PromptExample
                {
                    name = "Forest with Bridge",
                    prompt = "Create a forest with trees, rocks, a river running through it, and a wooden bridge crossing the river",
                    category = "Complex"
                },
                new PromptExample
                {
                    name = "City Intersection",
                    prompt = "Generate a city intersection with roads, 10 buildings around the edges, cars, and street lights at night",
                    category = "Complex"
                },
                new PromptExample
                {
                    name = "Desert Oasis",
                    prompt = "Make a desert with sand dunes, a small water pond in the center, palm trees near the water, and rocks scattered around at sunset",
                    category = "Complex"
                }
            };
        }

        // ===== HELPER METHODS =====

        /// <summary>
        /// Get all prompts in a specific category.
        /// </summary>
        public static List<PromptExample> GetPromptsByCategory(string category)
        {
            var allPrompts = GetDefaultExamples();
            return allPrompts.FindAll(p => p.category == category);
        }

        /// <summary>
        /// Get a random prompt.
        /// </summary>
        public static PromptExample GetRandomPrompt()
        {
            var allPrompts = GetDefaultExamples();
            return allPrompts[Random.Range(0, allPrompts.Count)];
        }

        /// <summary>
        /// Get all category names.
        /// </summary>
        public static List<string> GetCategories()
        {
            return new List<string>
            {
                "Nature",
                "Urban",
                "Indoor",
                "Fantasy",
                "Test",
                "Complex"
            };
        }

        /// <summary>
        /// Print all examples to console (for debugging).
        /// </summary>
        [ContextMenu("Print All Examples")]
        public void PrintAllExamples()
        {
            var examples = GetDefaultExamples();
            Debug.Log($"=== {examples.Count} Example Prompts ===");
            
            foreach (var example in examples)
            {
                Debug.Log($"[{example.category}] {example.name}");
                Debug.Log($"  → \"{example.prompt}\"");
            }
        }
    }
}
