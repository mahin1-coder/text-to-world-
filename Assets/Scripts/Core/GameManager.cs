using UnityEngine;
using TextToWorld.Core;
using TextToWorld.UI;

namespace TextToWorld
{
    /// <summary>
    /// Main entry point for the Text-to-World system.
    /// Add this to an empty GameObject in your scene to bootstrap all systems.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Auto-Create Systems")]
        [SerializeField] private bool autoCreateSceneController = true;
        [SerializeField] private bool autoCreateConsoleUI = true;
        [SerializeField] private bool autoCreatePlayer = true;

        [Header("References (Auto-populated if null)")]
        [SerializeField] private SceneController sceneController;
        [SerializeField] private CommandConsoleUI consoleUI;

        [Header("Settings")]
        [SerializeField] private bool runDemoOnStart = false;

        private void Awake()
        {
            // Ensure WorldState exists
            var _ = WorldState.Instance;

            // Create or find SceneController
            if (sceneController == null && autoCreateSceneController)
            {
                sceneController = FindFirstObjectByType<SceneController>();
                if (sceneController == null)
                {
                    var go = new GameObject("SceneController");
                    sceneController = go.AddComponent<SceneController>();
                }
            }

            // Create or find Console UI
            if (consoleUI == null && autoCreateConsoleUI)
            {
                consoleUI = FindFirstObjectByType<CommandConsoleUI>();
                if (consoleUI == null)
                {
                    var go = new GameObject("CommandConsoleUI");
                    consoleUI = go.AddComponent<CommandConsoleUI>();
                }
            }
        }

        private void Start()
        {
            Debug.Log("=== Text-to-World System Initialized ===");
            Debug.Log("Press TAB to open command console");
            Debug.Log("Use WASD to move, mouse to look around");
            Debug.Log("Press ESC to unlock cursor");

            if (runDemoOnStart)
            {
                RunDemo();
            }
        }

        /// <summary>
        /// Run a demo scene with some objects.
        /// </summary>
        public void RunDemo()
        {
            if (sceneController == null) return;

            // Clear and create demo scene
            sceneController.ExecuteCommand("clear");
            
            // Floor
            sceneController.ExecuteCommand("spawn plane id=floor at 0,0,0");
            sceneController.ExecuteCommand("scale floor to 5,1,5");
            sceneController.ExecuteCommand("set_color floor gray");

            // Table
            sceneController.ExecuteCommand("spawn table id=table1 color=wood at 0,0,2");

            // Chairs
            sceneController.ExecuteCommand("spawn chair id=chair1 color=brown at -1,0,2");
            sceneController.ExecuteCommand("spawn chair id=chair2 color=brown at 1,0,2");
            sceneController.ExecuteCommand("rotate chair2 to 0,180,0");

            // Some cubes
            sceneController.ExecuteCommand("spawn cube id=box1 color=red at -3,0.5,0");
            sceneController.ExecuteCommand("spawn sphere id=ball1 color=blue at 3,0.5,0");

            Debug.Log("Demo scene created!");
        }

        #region Public API

        /// <summary>
        /// Execute a text command.
        /// </summary>
        public void ExecuteCommand(string command)
        {
            if (sceneController != null)
            {
                sceneController.ExecuteCommand(command);
            }
        }

        /// <summary>
        /// Export current scene to XML.
        /// </summary>
        public void ExportScene(string path = null)
        {
            XmlExporter.Export(WorldState.Instance, path);
        }

        /// <summary>
        /// Import scene from XML.
        /// </summary>
        public void ImportScene(string path = null)
        {
            if (sceneController != null)
            {
                XmlImporter.Import(sceneController, path);
            }
        }

        /// <summary>
        /// Clear all objects.
        /// </summary>
        public void ClearScene()
        {
            ExecuteCommand("clear");
        }

        /// <summary>
        /// Get the SceneController reference.
        /// </summary>
        public SceneController SceneController => sceneController;

        #endregion
    }
}
