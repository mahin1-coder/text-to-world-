using System;
using System.Collections.Generic;
using UnityEngine;

namespace TextToWorld.Core
{
    /// <summary>
    /// Singleton that maintains the authoritative state of all objects in the virtual world.
    /// Acts as the single source of truth for scene data.
    /// </summary>
    public class WorldState : MonoBehaviour
    {
        // Singleton instance
        private static WorldState _instance;
        public static WorldState Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<WorldState>();
                    if (_instance == null)
                    {
                        var go = new GameObject("WorldState");
                        _instance = go.AddComponent<WorldState>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        // All scene objects indexed by ID
        private Dictionary<string, SceneObjectData> _objects = new Dictionary<string, SceneObjectData>();
        
        // Player/Actor data
        private Dictionary<string, ActorData> _actors = new Dictionary<string, ActorData>();

        // Events for state changes
        public event Action<SceneObjectData> OnObjectAdded;
        public event Action<SceneObjectData> OnObjectUpdated;
        public event Action<string> OnObjectRemoved;
        public event Action OnWorldCleared;

        // Auto-increment counter for generating unique IDs
        private int _idCounter = 0;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #region Object Management

        /// <summary>
        /// Generate a unique ID for a new object.
        /// </summary>
        public string GenerateId(string prefix = "obj")
        {
            _idCounter++;
            return $"{prefix}_{_idCounter}";
        }

        /// <summary>
        /// Check if an object with the given ID exists.
        /// </summary>
        public bool HasObject(string id)
        {
            return _objects.ContainsKey(id);
        }

        /// <summary>
        /// Add a new object to the world state.
        /// Returns false if ID already exists.
        /// </summary>
        public bool AddObject(SceneObjectData data)
        {
            if (string.IsNullOrEmpty(data.id))
            {
                data.id = GenerateId(data.type);
            }

            if (_objects.ContainsKey(data.id))
            {
                Debug.LogWarning($"WorldState: Object with ID '{data.id}' already exists. Use UpdateObject instead.");
                return false;
            }

            _objects[data.id] = data;
            OnObjectAdded?.Invoke(data);
            Debug.Log($"WorldState: Added {data}");
            return true;
        }

        /// <summary>
        /// Update an existing object in the world state.
        /// </summary>
        public bool UpdateObject(SceneObjectData data)
        {
            if (!_objects.ContainsKey(data.id))
            {
                Debug.LogWarning($"WorldState: Object '{data.id}' not found for update.");
                return false;
            }

            _objects[data.id] = data;
            OnObjectUpdated?.Invoke(data);
            return true;
        }

        /// <summary>
        /// Get object data by ID.
        /// </summary>
        public SceneObjectData GetObject(string id)
        {
            return _objects.TryGetValue(id, out var data) ? data : null;
        }

        /// <summary>
        /// Remove an object from the world state.
        /// </summary>
        public bool RemoveObject(string id)
        {
            if (_objects.Remove(id))
            {
                OnObjectRemoved?.Invoke(id);
                Debug.Log($"WorldState: Removed object '{id}'");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get all objects in the world.
        /// </summary>
        public IEnumerable<SceneObjectData> GetAllObjects()
        {
            return _objects.Values;
        }

        /// <summary>
        /// Get count of objects.
        /// </summary>
        public int ObjectCount => _objects.Count;

        /// <summary>
        /// Clear all objects from the world state.
        /// </summary>
        public void ClearAll()
        {
            _objects.Clear();
            _actors.Clear();
            _idCounter = 0;
            OnWorldCleared?.Invoke();
            Debug.Log("WorldState: Cleared all objects");
        }

        #endregion

        #region Actor Management

        /// <summary>
        /// Register an actor (player/NPC) in the world.
        /// </summary>
        public void RegisterActor(ActorData actor)
        {
            _actors[actor.id] = actor;
        }

        /// <summary>
        /// Get actor by ID.
        /// </summary>
        public ActorData GetActor(string id)
        {
            return _actors.TryGetValue(id, out var actor) ? actor : null;
        }

        /// <summary>
        /// Get all actors.
        /// </summary>
        public IEnumerable<ActorData> GetAllActors()
        {
            return _actors.Values;
        }

        #endregion

        #region Sync Methods

        /// <summary>
        /// Sync all object data from their GameObjects (call before export).
        /// </summary>
        public void SyncAllFromScene()
        {
            foreach (var obj in _objects.Values)
            {
                obj.SyncFromGameObject();
            }
        }

        #endregion
    }

    /// <summary>
    /// Data for actors (players, NPCs) in the scene.
    /// </summary>
    [Serializable]
    public class ActorData
    {
        public string id;
        public string type; // "fps", "npc", etc.
        public Vector3 position;
        public Vector3 rotation;

        [NonSerialized]
        public GameObject gameObject;

        public ActorData()
        {
            id = "player_1";
            type = "fps";
            position = new Vector3(0, 1.8f, 0);
            rotation = Vector3.zero;
        }

        public void SyncFromGameObject()
        {
            if (gameObject == null) return;
            position = gameObject.transform.position;
            rotation = gameObject.transform.eulerAngles;
        }
    }
}
