using System;
using UnityEngine;

namespace TextToWorld.Core
{
    /// <summary>
    /// Data model representing a single object in the virtual world.
    /// Used for state management and XML serialization.
    /// </summary>
    [Serializable]
    public class SceneObjectData
    {
        // Unique identifier for this object
        public string id;
        
        // Object type (cube, sphere, table, chair, etc.)
        public string type;
        
        // Transform data
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        
        // Appearance
        public string color;
        public string texture;
        
        // Optional prefab reference (for custom models)
        public string prefabName;
        
        // Reference to the actual GameObject (not serialized)
        [NonSerialized]
        public GameObject gameObject;

        /// <summary>
        /// Create a new SceneObjectData with default values.
        /// </summary>
        public SceneObjectData()
        {
            id = "";
            type = "cube";
            position = Vector3.zero;
            rotation = Vector3.zero;
            scale = Vector3.one;
            color = "white";
            texture = "";
            prefabName = "";
        }

        /// <summary>
        /// Create a SceneObjectData with specified id and type.
        /// </summary>
        public SceneObjectData(string id, string type)
        {
            this.id = id;
            this.type = type;
            position = Vector3.zero;
            rotation = Vector3.zero;
            scale = Vector3.one;
            color = "white";
            texture = "";
            prefabName = "";
        }

        /// <summary>
        /// Sync data from the actual GameObject transform.
        /// </summary>
        public void SyncFromGameObject()
        {
            if (gameObject == null) return;
            
            position = gameObject.transform.position;
            rotation = gameObject.transform.eulerAngles;
            scale = gameObject.transform.localScale;
        }

        /// <summary>
        /// Apply this data to the GameObject transform.
        /// </summary>
        public void ApplyToGameObject()
        {
            if (gameObject == null) return;
            
            gameObject.transform.position = position;
            gameObject.transform.eulerAngles = rotation;
            gameObject.transform.localScale = scale;
        }

        /// <summary>
        /// Create a deep copy of this object data.
        /// </summary>
        public SceneObjectData Clone()
        {
            return new SceneObjectData
            {
                id = this.id,
                type = this.type,
                position = this.position,
                rotation = this.rotation,
                scale = this.scale,
                color = this.color,
                texture = this.texture,
                prefabName = this.prefabName
            };
        }

        public override string ToString()
        {
            return $"[{id}] {type} at {position} color={color}";
        }
    }
}
