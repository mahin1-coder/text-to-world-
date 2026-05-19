using UnityEngine;
using System.Collections.Generic;
using TextToWorld.Data;
using TextToWorld.Materials;

/// <summary>
/// Handles spawning of objects in the virtual world.
/// Supports various placement strategies: random, scattered, grid, line, circle, etc.
/// </summary>
namespace TextToWorld.Spawning
{
    public class ObjectSpawner : MonoBehaviour
    {
        [Header("Spawn Area Configuration")]
        public Vector3 spawnAreaMin = new Vector3(-40f, 0f, -40f);
        public Vector3 spawnAreaMax = new Vector3(40f, 10f, 40f);
        public float minDistanceBetweenObjects = 2f;

        [Header("Parent Container")]
        public Transform worldRoot;

        // Track spawned objects
        private List<GameObject> _spawnedObjects = new List<GameObject>();

        private void Awake()
        {
            // Ensure we have a world root
            if (worldRoot == null)
            {
                var rootObj = GameObject.Find("WorldRoot");
                if (rootObj == null)
                {
                    rootObj = new GameObject("WorldRoot");
                }
                worldRoot = rootObj.transform;
            }
        }

        /// <summary>
        /// Spawn a single WorldObject with all its instances.
        /// </summary>
        public void SpawnObject(WorldObject worldObj)
        {
            if (worldObj == null)
            {
                Debug.LogError("ObjectSpawner: Null WorldObject provided");
                return;
            }

            Debug.Log($"ObjectSpawner: Spawning {worldObj.count}x {worldObj.type} ({worldObj.placement})");

            // Get positions based on placement strategy
            List<Vector3> positions = GeneratePositions(worldObj);

            // Create instances
            for (int i = 0; i < worldObj.count && i < positions.Count; i++)
            {
                CreateInstance(worldObj, positions[i], i);
            }
        }

        /// <summary>
        /// Create a single instance of a WorldObject at the given position.
        /// </summary>
        private void CreateInstance(WorldObject worldObj, Vector3 position, int index)
        {
            // Create primitive shape
            GameObject obj = GameObject.CreatePrimitive(worldObj.GetPrimitiveType());
            obj.name = $"{worldObj.type}_{index}";
            obj.transform.position = position;
            obj.transform.localScale = worldObj.GetScale();
            obj.transform.eulerAngles = worldObj.rotation;

            // Parent to world root
            obj.transform.SetParent(worldRoot);

            // Apply material
            string materialName = !string.IsNullOrEmpty(worldObj.color) ? worldObj.color : worldObj.material;
            MaterialManager.Instance.ApplyMaterial(obj, materialName);

            // Add to tracking list
            _spawnedObjects.Add(obj);
        }

        /// <summary>
        /// Generate positions based on placement strategy.
        /// </summary>
        private List<Vector3> GeneratePositions(WorldObject worldObj)
        {
            switch (worldObj.placement.ToLower())
            {
                case "center":
                    return GenerateCenterPosition(worldObj);
                
                case "random":
                    return GenerateRandomPositions(worldObj);
                
                case "scattered":
                    return GenerateScatteredPositions(worldObj);
                
                case "grid":
                    return GenerateGridPositions(worldObj);
                
                case "line":
                case "row":
                    return GenerateLinePositions(worldObj);
                
                case "circle":
                    return GenerateCirclePositions(worldObj);
                
                default:
                    Debug.LogWarning($"ObjectSpawner: Unknown placement '{worldObj.placement}', using random");
                    return GenerateRandomPositions(worldObj);
            }
        }

        /// <summary>
        /// Single position at the center.
        /// </summary>
        private List<Vector3> GenerateCenterPosition(WorldObject worldObj)
        {
            var positions = new List<Vector3>();
            
            // If explicit position is set, use it
            if (worldObj.position != Vector3.zero)
            {
                positions.Add(worldObj.position);
            }
            else
            {
                // Otherwise use center of spawn area
                Vector3 center = (spawnAreaMin + spawnAreaMax) / 2f;
                center.y = worldObj.GetScale().y / 2f;  // Place on ground
                positions.Add(center);
            }
            
            return positions;
        }

        /// <summary>
        /// Completely random positions (may overlap).
        /// </summary>
        private List<Vector3> GenerateRandomPositions(WorldObject worldObj)
        {
            var positions = new List<Vector3>();

            for (int i = 0; i < worldObj.count; i++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                    worldObj.GetScale().y / 2f,  // Ground level
                    Random.Range(spawnAreaMin.z, spawnAreaMax.z)
                );
                positions.Add(pos);
            }

            return positions;
        }

        /// <summary>
        /// Scattered positions with minimum distance between objects (no overlap).
        /// </summary>
        private List<Vector3> GenerateScatteredPositions(WorldObject worldObj)
        {
            var positions = new List<Vector3>();
            int maxAttempts = worldObj.count * 20;  // Prevent infinite loops
            int attempts = 0;

            while (positions.Count < worldObj.count && attempts < maxAttempts)
            {
                attempts++;

                Vector3 candidatePos = new Vector3(
                    Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                    worldObj.GetScale().y / 2f,
                    Random.Range(spawnAreaMin.z, spawnAreaMax.z)
                );

                // Check if far enough from other positions
                bool valid = true;
                foreach (var existingPos in positions)
                {
                    if (Vector3.Distance(candidatePos, existingPos) < minDistanceBetweenObjects)
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                {
                    positions.Add(candidatePos);
                }
            }

            if (positions.Count < worldObj.count)
            {
                Debug.LogWarning($"ObjectSpawner: Could only place {positions.Count}/{worldObj.count} {worldObj.type}s with scattered placement");
            }

            return positions;
        }

        /// <summary>
        /// Grid layout positions.
        /// </summary>
        private List<Vector3> GenerateGridPositions(WorldObject worldObj)
        {
            var positions = new List<Vector3>();

            // Calculate grid dimensions
            int gridSize = Mathf.CeilToInt(Mathf.Sqrt(worldObj.count));
            float spacing = Mathf.Max(minDistanceBetweenObjects, worldObj.GetScale().x * 1.5f);

            // Center the grid
            float gridWidth = (gridSize - 1) * spacing;
            float gridDepth = (gridSize - 1) * spacing;
            Vector3 startPos = new Vector3(-gridWidth / 2f, worldObj.GetScale().y / 2f, -gridDepth / 2f);

            int count = 0;
            for (int row = 0; row < gridSize && count < worldObj.count; row++)
            {
                for (int col = 0; col < gridSize && count < worldObj.count; col++)
                {
                    Vector3 pos = startPos + new Vector3(col * spacing, 0, row * spacing);
                    positions.Add(pos);
                    count++;
                }
            }

            return positions;
        }

        /// <summary>
        /// Line/row layout positions.
        /// </summary>
        private List<Vector3> GenerateLinePositions(WorldObject worldObj)
        {
            var positions = new List<Vector3>();
            float spacing = Mathf.Max(minDistanceBetweenObjects, worldObj.GetScale().x * 1.5f);
            float totalLength = (worldObj.count - 1) * spacing;
            float startX = -totalLength / 2f;

            for (int i = 0; i < worldObj.count; i++)
            {
                Vector3 pos = new Vector3(
                    startX + i * spacing,
                    worldObj.GetScale().y / 2f,
                    0
                );
                positions.Add(pos);
            }

            return positions;
        }

        /// <summary>
        /// Circle layout positions.
        /// </summary>
        private List<Vector3> GenerateCirclePositions(WorldObject worldObj)
        {
            var positions = new List<Vector3>();
            float radius = Mathf.Max(10f, worldObj.count * 2f);  // Scale radius with count
            float angleStep = 360f / worldObj.count;

            for (int i = 0; i < worldObj.count; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * radius,
                    worldObj.GetScale().y / 2f,
                    Mathf.Sin(angle) * radius
                );
                positions.Add(pos);
            }

            return positions;
        }

        /// <summary>
        /// Clear all spawned objects.
        /// </summary>
        public void ClearAllObjects()
        {
            Debug.Log($"ObjectSpawner: Clearing {_spawnedObjects.Count} objects");

            foreach (var obj in _spawnedObjects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }

            _spawnedObjects.Clear();

            // Also clear any children of WorldRoot
            if (worldRoot != null)
            {
                foreach (Transform child in worldRoot)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Get count of currently spawned objects.
        /// </summary>
        public int GetSpawnedObjectCount()
        {
            return _spawnedObjects.Count;
        }

        /// <summary>
        /// Set custom spawn area bounds.
        /// </summary>
        public void SetSpawnArea(Vector3 min, Vector3 max)
        {
            spawnAreaMin = min;
            spawnAreaMax = max;
            Debug.Log($"ObjectSpawner: Spawn area set to {min} - {max}");
        }
    }
}
