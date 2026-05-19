using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TextToWorld.Core;

/// <summary>
/// Unity UI interface for text-to-world input.
/// Provides a text field for users to enter prompts and buttons to generate/clear worlds.
/// 
/// Setup Instructions:
/// 1. Create a Canvas in your scene (UI → Canvas)
/// 2. Add an InputField (TextMeshPro) for text input
/// 3. Add Buttons for Generate, Clear, and example prompts
/// 4. Attach this script to an empty GameObject
/// 5. Assign references in the inspector
/// </summary>
namespace TextToWorld.UI
{
    public class TextInputUI : MonoBehaviour
    {
        [Header("UI References")]
        public TMP_InputField promptInputField;  // TextMeshPro InputField
        public Button generateButton;
        public Button clearButton;
        public TextMeshProUGUI statusText;

        [Header("World Generator")]
        public WorldGenerator worldGenerator;

        [Header("Example Prompt Buttons (Optional)")]
        public Button exampleButton1;
        public Button exampleButton2;
        public Button exampleButton3;
        public Button exampleButton4;

        [Header("Example Prompts")]
        public string example1 = "Create a forest with trees, rocks, and a river";
        public string example2 = "Generate a futuristic city at night with neon lights";
        public string example3 = "Make a desert with sand dunes and cactus at sunset";
        public string example4 = "Create a classroom with desks, chairs, and a whiteboard";

        private void Start()
        {
            // Find WorldGenerator if not assigned
            if (worldGenerator == null)
            {
                worldGenerator = FindObjectOfType<WorldGenerator>();
                if (worldGenerator == null)
                {
                    Debug.LogError("TextInputUI: WorldGenerator not found! Please assign or create one.");
                }
            }

            // Setup button listeners
            if (generateButton != null)
            {
                generateButton.onClick.AddListener(OnGenerateButtonClicked);
            }
            else
            {
                Debug.LogWarning("TextInputUI: Generate button not assigned");
            }

            if (clearButton != null)
            {
                clearButton.onClick.AddListener(OnClearButtonClicked);
            }

            // Setup example buttons
            if (exampleButton1 != null) exampleButton1.onClick.AddListener(() => LoadExample(example1));
            if (exampleButton2 != null) exampleButton2.onClick.AddListener(() => LoadExample(example2));
            if (exampleButton3 != null) exampleButton3.onClick.AddListener(() => LoadExample(example3));
            if (exampleButton4 != null) exampleButton4.onClick.AddListener(() => LoadExample(example4));

            // Setup Enter key submit
            if (promptInputField != null)
            {
                promptInputField.onSubmit.AddListener(OnPromptSubmit);
            }

            UpdateStatus("Ready. Enter a prompt to generate a world.");
        }

        /// <summary>
        /// Called when Generate button is clicked.
        /// </summary>
        private void OnGenerateButtonClicked()
        {
            if (promptInputField == null || string.IsNullOrWhiteSpace(promptInputField.text))
            {
                UpdateStatus("Error: Please enter a prompt first!");
                return;
            }

            string prompt = promptInputField.text;
            GenerateWorld(prompt);
        }

        /// <summary>
        /// Called when Enter key is pressed in the input field.
        /// </summary>
        private void OnPromptSubmit(string prompt)
        {
            if (!string.IsNullOrWhiteSpace(prompt))
            {
                GenerateWorld(prompt);
            }
        }

        /// <summary>
        /// Called when Clear button is clicked.
        /// </summary>
        private void OnClearButtonClicked()
        {
            if (worldGenerator != null)
            {
                worldGenerator.ClearWorld();
                UpdateStatus("World cleared.");
            }
        }

        /// <summary>
        /// Generate a world from the given prompt.
        /// </summary>
        private void GenerateWorld(string prompt)
        {
            if (worldGenerator == null)
            {
                UpdateStatus("Error: WorldGenerator not found!");
                Debug.LogError("TextInputUI: WorldGenerator is null");
                return;
            }

            UpdateStatus($"Generating: \"{prompt}\"...");
            Debug.Log($"TextInputUI: Requesting world generation - {prompt}");

            // Call the generator
            worldGenerator.GenerateWorldFromPrompt(prompt);

            // Update status after a brief delay
            Invoke(nameof(UpdateStatusAfterGeneration), 1f);
        }

        /// <summary>
        /// Load an example prompt into the input field.
        /// </summary>
        private void LoadExample(string examplePrompt)
        {
            if (promptInputField != null)
            {
                promptInputField.text = examplePrompt;
                UpdateStatus($"Example loaded. Click Generate or press Enter.");
            }
        }

        /// <summary>
        /// Update status text after generation completes.
        /// </summary>
        private void UpdateStatusAfterGeneration()
        {
            UpdateStatus("Generation complete! Enter another prompt or try an example.");
        }

        /// <summary>
        /// Update the status text display.
        /// </summary>
        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
            Debug.Log($"TextInputUI Status: {message}");
        }

        /// <summary>
        /// Public method to set the prompt programmatically.
        /// </summary>
        public void SetPrompt(string prompt)
        {
            if (promptInputField != null)
            {
                promptInputField.text = prompt;
            }
        }

        /// <summary>
        /// Public method to trigger generation programmatically.
        /// </summary>
        public void GenerateFromCurrentPrompt()
        {
            OnGenerateButtonClicked();
        }

        private void OnDestroy()
        {
            // Cleanup listeners
            if (generateButton != null)
                generateButton.onClick.RemoveListener(OnGenerateButtonClicked);
            
            if (clearButton != null)
                clearButton.onClick.RemoveListener(OnClearButtonClicked);

            if (promptInputField != null)
                promptInputField.onSubmit.RemoveListener(OnPromptSubmit);
        }
    }
}
