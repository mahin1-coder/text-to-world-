using System;
using System.Collections.Generic;
using UnityEngine;

namespace TextToWorld.Core
{
    /// <summary>
    /// Main controller that manages the 3D scene based on WorldState.
    /// Handles spawning, updating, and destroying GameObjects.
    /// </summary>
    public class SceneController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Transform worldRoot;
        [SerializeField] private bool autoSpawnPlayer = true;
        [SerializeField] private GameObject playerPrefab;

        [Header("Debug")]
        [SerializeField] private bool logCommands = true;

        // Material cache
        private Dictionary<string, Material> _materials = new Dictionary<string, Material>();
        private Dictionary<string, Color> _colorMap = new Dictionary<string, Color>();
        
        // Texture cache
        private Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();

        // Reference to world state
        private WorldState _worldState;

        // Player reference
        private GameObject _playerObject;

        private void Awake()
        {
            // Create world root if not assigned
            if (worldRoot == null)
            {
                var rootGo = GameObject.Find("WorldRoot");
                if (rootGo == null)
                {
                    rootGo = new GameObject("WorldRoot");
                }
                worldRoot = rootGo.transform;
            }

            InitializeMaterials();
            InitializeTextures();
        }

        private void Start()
        {
            _worldState = WorldState.Instance;

            // Subscribe to world state events
            _worldState.OnObjectAdded += HandleObjectAdded;
            _worldState.OnObjectUpdated += HandleObjectUpdated;
            _worldState.OnObjectRemoved += HandleObjectRemoved;
            _worldState.OnWorldCleared += HandleWorldCleared;

            // Spawn player if needed
            if (autoSpawnPlayer)
            {
                SpawnPlayer();
            }

            Log("SceneController initialized");
        }

        private void OnDestroy()
        {
            if (_worldState != null)
            {
                _worldState.OnObjectAdded -= HandleObjectAdded;
                _worldState.OnObjectUpdated -= HandleObjectUpdated;
                _worldState.OnObjectRemoved -= HandleObjectRemoved;
                _worldState.OnWorldCleared -= HandleWorldCleared;
            }
        }

        #region Command Execution

        /// <summary>
        /// Execute a parsed command.
        /// </summary>
        public void ExecuteCommand(CommandParser.ParsedCommand cmd)
        {
            if (!cmd.IsValid)
            {
                Log($"Invalid command: {cmd.ErrorMessage}", LogType.Warning);
                return;
            }

            switch (cmd.Type)
            {
                case CommandParser.CommandType.Spawn:
                    SpawnObject(cmd);
                    break;
                case CommandParser.CommandType.Move:
                    MoveObject(cmd.TargetId, cmd.Position);
                    break;
                case CommandParser.CommandType.Rotate:
                    RotateObject(cmd.TargetId, cmd.Rotation);
                    break;
                case CommandParser.CommandType.Scale:
                    ScaleObject(cmd.TargetId, cmd.Scale);
                    break;
                case CommandParser.CommandType.SetColor:
                    SetObjectColor(cmd.TargetId, cmd.Color);
                    break;
                case CommandParser.CommandType.SetTexture:
                    SetObjectTexture(cmd.TargetId, cmd.Texture);
                    break;
                case CommandParser.CommandType.Delete:
                    DeleteObject(cmd.TargetId);
                    break;
                case CommandParser.CommandType.Clear:
                    ClearAll();
                    break;
                case CommandParser.CommandType.ExportXml:
                    XmlExporter.Export(_worldState);
                    break;
                case CommandParser.CommandType.ImportXml:
                    XmlImporter.Import(this);
                    break;
                case CommandParser.CommandType.Help:
                    Debug.Log(CommandParser.GetHelpText());
                    break;
            }
        }

        /// <summary>
        /// Execute a raw text command.
        /// </summary>
        public void ExecuteCommand(string textCommand)
        {
            var parsed = CommandParser.Parse(textCommand);
            ExecuteCommand(parsed);
        }

        #endregion

        #region Object Operations

        private void SpawnObject(CommandParser.ParsedCommand cmd)
        {
            // Generate ID if not provided
            string id = cmd.TargetId ?? _worldState.GenerateId(cmd.ObjectType);

            // Check for duplicate ID
            if (_worldState.HasObject(id))
            {
                Log($"Object '{id}' already exists", LogType.Warning);
                return;
            }

            // Create object data
            var data = new SceneObjectData
            {
                id = id,
                type = cmd.ObjectType,
                position = cmd.Position,
                rotation = cmd.Rotation,
                scale = cmd.Scale,
                color = cmd.Color,
                texture = cmd.Texture
            };

            // Add to world state (this triggers HandleObjectAdded)
            _worldState.AddObject(data);
        }

        private void MoveObject(string id, Vector3 position)
        {
            var data = _worldState.GetObject(id);
            if (data == null)
            {
                Log($"Object '{id}' not found", LogType.Warning);
                return;
            }

            data.position = position;
            _worldState.UpdateObject(data);
        }

        private void RotateObject(string id, Vector3 rotation)
        {
            var data = _worldState.GetObject(id);
            if (data == null)
            {
                Log($"Object '{id}' not found", LogType.Warning);
                return;
            }

            data.rotation = rotation;
            _worldState.UpdateObject(data);
        }

        private void ScaleObject(string id, Vector3 scale)
        {
            var data = _worldState.GetObject(id);
            if (data == null)
            {
                Log($"Object '{id}' not found", LogType.Warning);
                return;
            }

            data.scale = scale;
            _worldState.UpdateObject(data);
        }

        private void SetObjectColor(string id, string color)
        {
            var data = _worldState.GetObject(id);
            if (data == null)
            {
                Log($"Object '{id}' not found", LogType.Warning);
                return;
            }

            data.color = color;
            _worldState.UpdateObject(data);
            
            // Also apply immediately to GameObject
            if (data.gameObject != null)
            {
                ApplyMaterial(data.gameObject, color);
            }
        }

        private void SetObjectTexture(string id, string texture)
        {
            var data = _worldState.GetObject(id);
            if (data == null)
            {
                Log($"Object '{id}' not found", LogType.Warning);
                return;
            }

            data.texture = texture;
            _worldState.UpdateObject(data);
            
            // Also apply immediately to GameObject
            if (data.gameObject != null)
            {
                ApplyTexture(data.gameObject, texture);
            }
        }

        private void DeleteObject(string id)
        {
            var data = _worldState.GetObject(id);
            if (data?.gameObject != null)
            {
                Destroy(data.gameObject);
            }
            _worldState.RemoveObject(id);
        }

        private void ClearAll()
        {
            // Destroy all GameObjects under world root
            foreach (Transform child in worldRoot)
            {
                Destroy(child.gameObject);
            }
            _worldState.ClearAll();
            Log("Scene cleared");
        }

        #endregion

        #region World State Event Handlers

        private void HandleObjectAdded(SceneObjectData data)
        {
            // Create the actual GameObject
            GameObject obj = CreateGameObject(data.type);
            obj.name = data.id;
            obj.transform.SetParent(worldRoot);
            
            // Apply transform
            obj.transform.position = data.position;
            obj.transform.eulerAngles = data.rotation;
            obj.transform.localScale = data.scale;

            // Apply appearance
            if (!string.IsNullOrEmpty(data.color))
            {
                ApplyMaterial(obj, data.color);
            }
            if (!string.IsNullOrEmpty(data.texture))
            {
                ApplyTexture(obj, data.texture);
            }

            // Ensure collider exists
            if (obj.GetComponent<Collider>() == null)
            {
                obj.AddComponent<BoxCollider>();
            }

            // Store reference
            data.gameObject = obj;

            Log($"Spawned: {data}");
        }

        private void HandleObjectUpdated(SceneObjectData data)
        {
            if (data.gameObject == null) return;
            data.ApplyToGameObject();
        }

        private void HandleObjectRemoved(string id)
        {
            Log($"Removed: {id}");
        }

        private void HandleWorldCleared()
        {
            Log("World cleared");
        }

        #endregion

        #region GameObject Creation

        private GameObject CreateGameObject(string typeName)
        {
            typeName = typeName.ToLower();

            // Check for primitives
            switch (typeName)
            {
                case "cube":
                case "box":
                    return GameObject.CreatePrimitive(PrimitiveType.Cube);
                case "sphere":
                case "ball":
                    return GameObject.CreatePrimitive(PrimitiveType.Sphere);
                case "cylinder":
                    return GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                case "capsule":
                    return GameObject.CreatePrimitive(PrimitiveType.Capsule);
                case "plane":
                case "floor":
                    return GameObject.CreatePrimitive(PrimitiveType.Plane);
                case "quad":
                    return GameObject.CreatePrimitive(PrimitiveType.Quad);
            }

            // For furniture types, create composite primitives
            switch (typeName)
            {
                case "table":
                    return CreateTable();
                case "chair":
                    return CreateChair();
                default:
                    // Default to cube for unknown types
                    Log($"Unknown type '{typeName}', using cube", LogType.Warning);
                    return GameObject.CreatePrimitive(PrimitiveType.Cube);
            }
        }

        private GameObject CreateTable()
        {
            var table = new GameObject("Table");
            
            // Table top
            var top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.transform.SetParent(table.transform);
            top.transform.localPosition = new Vector3(0, 0.4f, 0);
            top.transform.localScale = new Vector3(1.5f, 0.1f, 1f);

            // Legs
            for (int i = 0; i < 4; i++)
            {
                var leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leg.transform.SetParent(table.transform);
                leg.transform.localScale = new Vector3(0.1f, 0.4f, 0.1f);
                float x = (i % 2 == 0) ? 0.65f : -0.65f;
                float z = (i < 2) ? 0.4f : -0.4f;
                leg.transform.localPosition = new Vector3(x, 0.2f, z);
            }

            // Add single collider
            var col = table.AddComponent<BoxCollider>();
            col.size = new Vector3(1.5f, 0.5f, 1f);
            col.center = new Vector3(0, 0.25f, 0);

            return table;
        }

        private GameObject CreateChair()
        {
            var chair = new GameObject("Chair");
            
            // Seat
            var seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.transform.SetParent(chair.transform);
            seat.transform.localPosition = new Vector3(0, 0.25f, 0);
            seat.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);

            // Back
            var back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.transform.SetParent(chair.transform);
            back.transform.localPosition = new Vector3(0, 0.55f, -0.2f);
            back.transform.localScale = new Vector3(0.5f, 0.5f, 0.1f);

            // Legs
            for (int i = 0; i < 4; i++)
            {
                var leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leg.transform.SetParent(chair.transform);
                leg.transform.localScale = new Vector3(0.08f, 0.25f, 0.08f);
                float x = (i % 2 == 0) ? 0.18f : -0.18f;
                float z = (i < 2) ? 0.18f : -0.18f;
                leg.transform.localPosition = new Vector3(x, 0.125f, z);
            }

            // Add collider
            var col = chair.AddComponent<BoxCollider>();
            col.size = new Vector3(0.5f, 0.8f, 0.5f);
            col.center = new Vector3(0, 0.4f, 0);

            return chair;
        }

        #endregion

        #region Player

        private void SpawnPlayer()
        {
            if (_playerObject != null) return;

            if (playerPrefab != null)
            {
                _playerObject = Instantiate(playerPrefab, new Vector3(0, 1, -5), Quaternion.identity);
            }
            else
            {
                // Create default FPS player
                _playerObject = new GameObject("FPSPlayer");
                
                // Add character controller
                var cc = _playerObject.AddComponent<CharacterController>();
                cc.height = 1.8f;
                cc.radius = 0.3f;
                cc.center = new Vector3(0, 0.9f, 0);

                // Add camera
                var camObj = new GameObject("PlayerCamera");
                camObj.transform.SetParent(_playerObject.transform);
                camObj.transform.localPosition = new Vector3(0, 1.6f, 0);
                var cam = camObj.AddComponent<Camera>();
                cam.nearClipPlane = 0.1f;
                camObj.AddComponent<AudioListener>();

                // Add FPS controller
                _playerObject.AddComponent<Player.FPSController>();

                _playerObject.transform.position = new Vector3(0, 0, -5);
            }

            // Register actor
            var actor = new ActorData
            {
                id = "player_1",
                type = "fps",
                gameObject = _playerObject
            };
            _worldState.RegisterActor(actor);

            Log("Player spawned");
        }

        #endregion

        #region Materials & Textures

        private void InitializeMaterials()
        {
            // Define color palette
            _colorMap["red"] = new Color(0.8f, 0.2f, 0.2f);
            _colorMap["green"] = new Color(0.2f, 0.7f, 0.2f);
            _colorMap["blue"] = new Color(0.2f, 0.4f, 0.8f);
            _colorMap["yellow"] = new Color(0.9f, 0.85f, 0.2f);
            _colorMap["orange"] = new Color(0.9f, 0.5f, 0.1f);
            _colorMap["purple"] = new Color(0.6f, 0.2f, 0.8f);
            _colorMap["pink"] = new Color(0.95f, 0.5f, 0.7f);
            _colorMap["white"] = new Color(0.95f, 0.95f, 0.95f);
            _colorMap["black"] = new Color(0.1f, 0.1f, 0.1f);
            _colorMap["gray"] = new Color(0.5f, 0.5f, 0.5f);
            _colorMap["grey"] = new Color(0.5f, 0.5f, 0.5f);
            _colorMap["brown"] = new Color(0.55f, 0.35f, 0.2f);
            _colorMap["wood"] = new Color(0.6f, 0.4f, 0.25f);
            _colorMap["darkwood"] = new Color(0.35f, 0.2f, 0.1f);
            _colorMap["brick"] = new Color(0.6f, 0.25f, 0.2f);
            _colorMap["marble"] = new Color(0.92f, 0.9f, 0.88f);
            _colorMap["metal"] = new Color(0.7f, 0.7f, 0.75f);
            _colorMap["gold"] = new Color(0.85f, 0.7f, 0.2f);
            _colorMap["silver"] = new Color(0.8f, 0.8f, 0.85f);
            _colorMap["cream"] = new Color(0.96f, 0.94f, 0.88f);
            _colorMap["leather"] = new Color(0.45f, 0.28f, 0.18f);

            // Create materials for each color
            foreach (var kvp in _colorMap)
            {
                CreateMaterial(kvp.Key, kvp.Value);
            }
        }

        private void CreateMaterial(string name, Color color)
        {
            // Try URP first, fall back to Standard
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material mat = new Material(shader);
            mat.color = color;
            _materials[name] = mat;
        }

        private void ApplyMaterial(GameObject obj, string colorName)
        {
            if (string.IsNullOrEmpty(colorName)) return;
            
            colorName = colorName.ToLower();
            if (!_materials.TryGetValue(colorName, out var mat))
            {
                Log($"Unknown color: {colorName}", LogType.Warning);
                return;
            }

            // Apply to all renderers (for composite objects)
            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var rend in renderers)
            {
                rend.material = mat;
            }
        }

        private void InitializeTextures()
        {
            // Placeholder - textures would be loaded from Resources folder
            // _textures["wood"] = Resources.Load<Texture2D>("Textures/wood");
        }

        private void ApplyTexture(GameObject obj, string textureName)
        {
            if (string.IsNullOrEmpty(textureName)) return;
            
            if (!_textures.TryGetValue(textureName, out var tex))
            {
                Log($"Unknown texture: {textureName}", LogType.Warning);
                return;
            }

            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var rend in renderers)
            {
                rend.material.mainTexture = tex;
            }
        }

        #endregion

        #region Utility

        private void Log(string message, LogType type = LogType.Log)
        {
            if (!logCommands && type == LogType.Log) return;

            switch (type)
            {
                case LogType.Warning:
                    Debug.LogWarning($"[SceneController] {message}");
                    break;
                case LogType.Error:
                    Debug.LogError($"[SceneController] {message}");
                    break;
                default:
                    Debug.Log($"[SceneController] {message}");
                    break;
            }
        }

        #endregion

        #region Public API for Import

        /// <summary>
        /// Spawn an object directly from SceneObjectData (used by importer).
        /// </summary>
        public void SpawnFromData(SceneObjectData data)
        {
            if (_worldState.HasObject(data.id))
            {
                Log($"Object '{data.id}' already exists during import, skipping", LogType.Warning);
                return;
            }
            _worldState.AddObject(data);
        }

        #endregion
    }
}
