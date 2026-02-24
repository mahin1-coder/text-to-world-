using UnityEngine;
using TextToWorld.Core;

namespace TextToWorld.UI
{
    /// <summary>
    /// In-game console UI for typing text commands.
    /// Press backtick (`) or Tab to toggle console.
    /// </summary>
    public class CommandConsoleUI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
        [SerializeField] private KeyCode alternateToggleKey = KeyCode.BackQuote;

        [Header("Appearance")]
        [SerializeField] private int fontSize = 16;
        [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.85f);
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color inputBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        // State
        private bool _isVisible = false;
        private string _inputText = "";
        private string _outputLog = "";
        private Vector2 _scrollPosition;
        private SceneController _sceneController;

        // GUI styles
        private GUIStyle _boxStyle;
        private GUIStyle _inputStyle;
        private GUIStyle _outputStyle;
        private GUIStyle _labelStyle;
        private bool _stylesInitialized = false;

        // Command history
        private System.Collections.Generic.List<string> _commandHistory = new System.Collections.Generic.List<string>();
        private int _historyIndex = -1;
        private const int MaxHistorySize = 50;
        private const int MaxLogLines = 100;

        private void Start()
        {
            _sceneController = FindFirstObjectByType<SceneController>();
            
            if (_sceneController == null)
            {
                Debug.LogWarning("CommandConsoleUI: SceneController not found in scene");
            }

            // Listen to Unity's log messages
            Application.logMessageReceived += HandleLog;

            AppendOutput("=== Text-to-World Console ===");
            AppendOutput("Type 'help' for available commands");
            AppendOutput("");
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void Update()
        {
            // Toggle console visibility
            if (Input.GetKeyDown(toggleKey) || Input.GetKeyDown(alternateToggleKey))
            {
                _isVisible = !_isVisible;
                
                if (_isVisible)
                {
                    // When opening console, unlock cursor
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }

            // Handle history navigation when console is visible
            if (_isVisible)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    NavigateHistory(-1);
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    NavigateHistory(1);
                }
            }
        }

        private void OnGUI()
        {
            if (!_isVisible) return;

            InitializeStyles();

            float consoleWidth = Screen.width * 0.6f;
            float consoleHeight = Screen.height * 0.4f;
            float padding = 10f;

            // Console window position (bottom-left)
            Rect windowRect = new Rect(
                padding,
                Screen.height - consoleHeight - padding,
                consoleWidth,
                consoleHeight
            );

            // Background
            GUI.Box(windowRect, "", _boxStyle);

            GUILayout.BeginArea(new Rect(
                windowRect.x + padding,
                windowRect.y + padding,
                windowRect.width - padding * 2,
                windowRect.height - padding * 2
            ));

            // Title
            GUILayout.Label("Command Console (Tab to close)", _labelStyle);

            // Output log (scrollable)
            float outputHeight = windowRect.height - 80;
            _scrollPosition = GUILayout.BeginScrollView(
                _scrollPosition,
                GUILayout.Height(outputHeight)
            );
            GUILayout.Label(_outputLog, _outputStyle);
            GUILayout.EndScrollView();

            // Input field
            GUILayout.BeginHorizontal();
            GUILayout.Label(">", _labelStyle, GUILayout.Width(15));

            GUI.SetNextControlName("CommandInput");
            _inputText = GUILayout.TextField(_inputText, _inputStyle);

            // Focus input field
            GUI.FocusControl("CommandInput");

            GUILayout.EndHorizontal();

            GUILayout.EndArea();

            // Handle Enter key
            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Return && !string.IsNullOrWhiteSpace(_inputText))
            {
                ExecuteInput();
                e.Use();
            }
        }

        private void InitializeStyles()
        {
            if (_stylesInitialized) return;

            _boxStyle = new GUIStyle(GUI.skin.box);
            _boxStyle.normal.background = MakeTexture(2, 2, backgroundColor);

            _inputStyle = new GUIStyle(GUI.skin.textField);
            _inputStyle.fontSize = fontSize;
            _inputStyle.normal.textColor = textColor;
            _inputStyle.normal.background = MakeTexture(2, 2, inputBackgroundColor);
            _inputStyle.focused.background = MakeTexture(2, 2, inputBackgroundColor);
            _inputStyle.focused.textColor = textColor;

            _outputStyle = new GUIStyle(GUI.skin.label);
            _outputStyle.fontSize = fontSize - 2;
            _outputStyle.normal.textColor = textColor;
            _outputStyle.wordWrap = true;
            _outputStyle.richText = true;

            _labelStyle = new GUIStyle(GUI.skin.label);
            _labelStyle.fontSize = fontSize;
            _labelStyle.normal.textColor = textColor;
            _labelStyle.fontStyle = FontStyle.Bold;

            _stylesInitialized = true;
        }

        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            Texture2D tex = new Texture2D(width, height);
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private void ExecuteInput()
        {
            string command = _inputText.Trim();
            _inputText = "";

            if (string.IsNullOrEmpty(command)) return;

            // Add to history
            AddToHistory(command);

            // Echo command
            AppendOutput($"> {command}");

            // Execute
            if (_sceneController != null)
            {
                _sceneController.ExecuteCommand(command);
            }
            else
            {
                AppendOutput("<color=red>Error: SceneController not found</color>");
            }

            // Auto-scroll to bottom
            _scrollPosition.y = float.MaxValue;
        }

        private void AddToHistory(string command)
        {
            // Don't add duplicates of the last command
            if (_commandHistory.Count > 0 && _commandHistory[_commandHistory.Count - 1] == command)
                return;

            _commandHistory.Add(command);

            // Trim history if too long
            while (_commandHistory.Count > MaxHistorySize)
            {
                _commandHistory.RemoveAt(0);
            }

            _historyIndex = _commandHistory.Count;
        }

        private void NavigateHistory(int direction)
        {
            if (_commandHistory.Count == 0) return;

            _historyIndex += direction;
            _historyIndex = Mathf.Clamp(_historyIndex, 0, _commandHistory.Count);

            if (_historyIndex < _commandHistory.Count)
            {
                _inputText = _commandHistory[_historyIndex];
            }
            else
            {
                _inputText = "";
            }
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            // Only capture logs from our system
            if (!logString.Contains("[SceneController]") && 
                !logString.Contains("WorldState") &&
                !logString.Contains("XmlExporter") &&
                !logString.Contains("XmlImporter") &&
                type != LogType.Error && 
                type != LogType.Warning)
            {
                return;
            }

            string colorTag = type switch
            {
                LogType.Error => "<color=red>",
                LogType.Warning => "<color=yellow>",
                _ => "<color=lime>"
            };

            AppendOutput($"{colorTag}{logString}</color>");
        }

        private void AppendOutput(string message)
        {
            _outputLog += message + "\n";

            // Trim log if too long
            string[] lines = _outputLog.Split('\n');
            if (lines.Length > MaxLogLines)
            {
                _outputLog = string.Join("\n", lines, lines.Length - MaxLogLines, MaxLogLines);
            }
        }

        /// <summary>
        /// Show or hide the console programmatically.
        /// </summary>
        public void SetVisible(bool visible)
        {
            _isVisible = visible;
        }

        /// <summary>
        /// Check if console is currently visible.
        /// </summary>
        public bool IsVisible => _isVisible;
    }
}
