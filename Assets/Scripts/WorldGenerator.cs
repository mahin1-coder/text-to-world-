using UnityEngine;
using System.Collections;
using TextToWorld.Data;
using TextToWorld.Parsing;
using TextToWorld.Environment;
using TextToWorld.Spawning;
using TextToWorld.Materials;

/// <summary>
/// Main orchestrator for the Text-to-World system.
/// Coordinates all subsystems: parsing, environment, materials, and object spawning.
/// This is the primary entry point for generating worlds from text prompts.
/// </summary>
namespace TextToWorld.Core
{
    public class WorldGenerator : MonoBehaviour
    {
        [Header("Component References")]
        public EnvironmentManager environmentManager;
        public ObjectSpawner objectSpawner;

        [Header("Generation Settings")]
        public bool autoSetupCamera = true;
        public Vector3 defaultCameraPosition = new Vector3(0, 15, -30);
        public Vector3 defaultCameraRotation = new Vector3(30, 0, 0);

        [Header("Debug")]
        public bool logSceneJson = true;
        public bool logGenerationSteps = true;

        // Current scene data
        private SceneData _currentScene;
        private bool _isGenerating = false;

        private void Awake()
        {
            // Auto-create components if not assigned
            if (environmentManager == null)
            {
                environmentManager = gameObject.AddComponent<EnvironmentManager>();
            }
            if (objectSpawner == null)
            {
                objectSpawner = gameObject.AddComponent<ObjectSpawner>();
            }

            // Initialize MaterialManager (singleton)
            _ = MaterialManager.Instance;
        }

        private void Start()
        {
            if (autoSetupCamera)
            {
                SetupCamera();
            }

            Debug.Log("WorldGenerator: Ready to generate worlds from text prompts!");
            Debug.Log("Call GenerateWorldFromPrompt(\"your prompt here\") to start.");
        }

        /// <summary>
        /// Main method: Generate a complete world from a text prompt.
        /// This is the primary API for external systems.
        /// </summary>
        public void GenerateWorldFromPrompt(string prompt)
        {
            if (_isGenerating)
            {
                Debug.LogWarning("WorldGenerator: Already generating a world. Please wait...");
                return;
            }

            if (string.IsNullOrWhiteSpace(prompt))
            {
                Debug.LogError("WorldGenerator: Empty prompt provided");
                return;
            }

            StartCoroutine(GenerateWorldCoroutine(prompt));
        }

        /// <summary>
        /// Generate world from structured SceneData (skip parsing).
        /// </summary>
        public void GenerateWorldFromSceneData(SceneData sceneData)
        {
            if (_isGenerating)
            {
                Debug.LogWarning("WorldGenerator: Already generating a world. Please wait...");
                return;
            }

            if (sceneData == null || !SceneDataHelper.IsValid(sceneData))
            {
                Debug.LogError("WorldGenerator: Invalid SceneData provided");
                return;
            }

            StartCoroutine(GenerateFromSceneDataCoroutine(sceneData));
        }

        /// <summary>
        /// Coroutine that handles the complete generation pipeline.
        /// </summary>
        private IEnumerator GenerateWorldCoroutine(string prompt)
        {
            _isGenerating = true;
            float startTime = Time.realtimeSinceStartup;

            Debug.Log("═══════════════════════════════════════════════════════");
            Debug.Log($"WorldGenerator: Starting world generation from prompt:");
            Debug.Log($"  \"{prompt}\"");
            Debug.Log("═══════════════════════════════════════════════════════");

            // STEP 1: Parse the prompt into SceneData
            if (logGenerationSteps) Debug.Log("STEP 1: Parsing prompt...");
            _currentScene = PromptParser.ParsePrompt(prompt);
            
            if (_currentScene == null)
            {
                Debug.LogError("WorldGenerator: Failed to parse prompt");
                _isGenerating = false;
                yield break;
            }

            // Log the generated scene JSON
            if (logSceneJson)
            {
                Debug.Log("Generated Scene JSON:");
                Debug.Log(_currentScene.ToJson());
            }

            yield return null;  // Give frame to process

            // STEP 2: Clear existing world
            if (logGenerationSteps) Debug.Log("STEP 2: Clearing existing world...");
            ClearWorld();
            yield return null;

            // STEP 3: Setup environment (terrain, sky, lighting)
            if (logGenerationSteps) Debug.Log("STEP 3: Setting up environment...");
            environmentManager.SetupEnvironment(_currentScene);
            yield return null;

            // STEP 4: Spawn all objects
            if (logGenerationSteps) Debug.Log("STEP 4: Spawning objects...");
            foreach (var worldObj in _currentScene.objects)
            {
                objectSpawner.SpawnObject(worldObj);
                yield return null;  // Spread spawning across frames
            }

            // STEP 5: Finalize
            float elapsed = Time.realtimeSinceStartup - startTime;
            Debug.Log("═══════════════════════════════════════════════════════");
            Debug.Log($"WorldGenerator: World generation complete!");
            Debug.Log($"  Environment: {_currentScene.environment}");
            Debug.Log($"  Sky: {_currentScene.sky}");
            Debug.Log($"  Objects: {_currentScene.objects.Count} types");
            Debug.Log($"  Total instances: {objectSpawner.GetSpawnedObjectCount()}");
            Debug.Log($"  Generation time: {elapsed:F2}s");
            Debug.Log("═══════════════════════════════════════════════════════");

            _isGenerating = false;
        }

        /// <summary>
        /// Generate from pre-parsed SceneData.
        /// </summary>
        private IEnumerator GenerateFromSceneDataCoroutine(SceneData sceneData)
        {
            _isGenerating = true;
            float startTime = Time.realtimeSinceStartup;

            Debug.Log("WorldGenerator: Generating world from SceneData...");
            _currentScene = sceneData;

            // Clear existing
            ClearWorld();
            yield return null;

            // Setup environment
            environmentManager.SetupEnvironment(_currentScene);
            yield return null;

            // Spawn objects
            foreach (var worldObj in _currentScene.objects)
            {
                objectSpawner.SpawnObject(worldObj);
                yield return null;
            }

            float elapsed = Time.realtimeSinceStartup - startTime;
            Debug.Log($"WorldGenerator: World generated in {elapsed:F2}s");

            _isGenerating = false;
        }

        /// <summary>
        /// Clear the entire world (objects + environment).
        /// </summary>
        public void ClearWorld()
        {
            Debug.Log("WorldGenerator: Clearing world...");
            
            if (objectSpawner != null)
            {
                objectSpawner.ClearAllObjects();
            }

            if (environmentManager != null)
            {
                environmentManager.ClearEnvironment();
            }

            _currentScene = null;
        }

        /// <summary>
        /// Setup camera for viewing the generated world.
        /// </summary>
        private void SetupCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.transform.position = defaultCameraPosition;
                mainCamera.transform.eulerAngles = defaultCameraRotation;
                Debug.Log("WorldGenerator: Camera positioned for viewing");
            }
            else
            {
                Debug.LogWarning("WorldGenerator: No main camera found");
            }
        }

        /// <summary>
        /// Get the current scene data (for inspection/debugging).
        /// </summary>
        public SceneData GetCurrentScene()
        {
            return _currentScene;
        }

        /// <summary>
        /// Check if currently generating.
        /// </summary>
        public bool IsGenerating()
        {
            return _isGenerating;
        }

        /// <summary>
        /// Regenerate the current world (useful for randomization).
        /// </summary>
        public void RegenerateCurrentWorld()
        {
            if (_currentScene != null)
            {
                GenerateWorldFromSceneData(_currentScene);
            }
            else
            {
                Debug.LogWarning("WorldGenerator: No current scene to regenerate");
            }
        }

        // ===== PUBLIC API FOR EXTERNAL SYSTEMS =====

        /// <summary>
        /// Quick test method for the inspector or console.
        /// </summary>
        [ContextMenu("Test: Generate Forest")]
        public void TestGenerateForest()
        {
            GenerateWorldFromPrompt("Create a forest with trees, rocks, and a river");
        }

        [ContextMenu("Test: Generate City")]
        public void TestGenerateCity()
        {
            GenerateWorldFromPrompt("Generate a futuristic city at night with neon lights and tall buildings");
        }

        [ContextMenu("Test: Generate Desert")]
        public void TestGenerateDesert()
        {
            GenerateWorldFromPrompt("Create a desert world with sand dunes and cactus at sunset");
        }

        [ContextMenu("Test: Generate Classroom")]
        public void TestGenerateClassroom()
        {
            GenerateWorldFromPrompt("Generate a classroom with desks, chairs, and a whiteboard");
        }

        [ContextMenu("Clear World")]
        public void TestClearWorld()
        {
            ClearWorld();
        }
    }
}
