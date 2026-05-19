using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized material management system.
/// Creates and caches materials with different colors and properties.
/// Provides a clean interface for applying materials to objects.
/// </summary>
namespace TextToWorld.Materials
{
    public class MaterialManager : MonoBehaviour
    {
        // Singleton instance
        private static MaterialManager _instance;
        public static MaterialManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("MaterialManager");
                    _instance = go.AddComponent<MaterialManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // Material cache
        private Dictionary<string, Material> _materials = new Dictionary<string, Material>();
        private Dictionary<string, Color> _colors = new Dictionary<string, Color>();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeMaterials();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Initialize all predefined materials and colors.
        /// </summary>
        private void InitializeMaterials()
        {
            Debug.Log("MaterialManager: Initializing materials...");

            // Basic colors
            CreateMaterial("red", new Color(0.9f, 0.2f, 0.2f));
            CreateMaterial("green", new Color(0.2f, 0.8f, 0.2f));
            CreateMaterial("blue", new Color(0.2f, 0.4f, 0.9f));
            CreateMaterial("yellow", new Color(0.95f, 0.9f, 0.2f));
            CreateMaterial("orange", new Color(0.95f, 0.5f, 0.1f));
            CreateMaterial("purple", new Color(0.7f, 0.2f, 0.9f));
            CreateMaterial("pink", new Color(0.95f, 0.6f, 0.7f));
            CreateMaterial("cyan", new Color(0.2f, 0.9f, 0.9f));
            CreateMaterial("magenta", new Color(0.9f, 0.2f, 0.9f));
            CreateMaterial("white", new Color(0.95f, 0.95f, 0.95f));
            CreateMaterial("black", new Color(0.1f, 0.1f, 0.1f));
            CreateMaterial("gray", new Color(0.5f, 0.5f, 0.5f));
            CreateMaterial("brown", new Color(0.6f, 0.4f, 0.25f));

            // Natural materials
            CreateMaterial("wood", new Color(0.65f, 0.45f, 0.3f));
            CreateMaterial("darkwood", new Color(0.4f, 0.25f, 0.15f));
            CreateMaterial("stone", new Color(0.6f, 0.6f, 0.65f));
            CreateMaterial("grass", new Color(0.3f, 0.7f, 0.3f));
            CreateMaterial("sand", new Color(0.9f, 0.85f, 0.6f));
            CreateMaterial("water", new Color(0.2f, 0.5f, 0.8f), true);  // Transparent
            CreateMaterial("ice", new Color(0.7f, 0.9f, 1.0f), true);
            CreateMaterial("snow", new Color(0.95f, 0.95f, 1.0f));

            // Building materials
            CreateMaterial("brick", new Color(0.7f, 0.3f, 0.25f));
            CreateMaterial("concrete", new Color(0.65f, 0.65f, 0.65f));
            CreateMaterial("marble", new Color(0.92f, 0.92f, 0.9f));
            CreateMaterial("glass", new Color(0.8f, 0.9f, 0.95f), true);
            CreateMaterial("metal", new Color(0.75f, 0.75f, 0.8f));
            CreateMaterial("steel", new Color(0.7f, 0.7f, 0.75f));
            CreateMaterial("gold", new Color(0.9f, 0.75f, 0.25f));
            CreateMaterial("silver", new Color(0.85f, 0.85f, 0.9f));
            CreateMaterial("copper", new Color(0.85f, 0.5f, 0.3f));

            // Fabric/texture materials
            CreateMaterial("leather", new Color(0.5f, 0.3f, 0.2f));
            CreateMaterial("carpet", new Color(0.6f, 0.5f, 0.45f));
            CreateMaterial("fabric", new Color(0.7f, 0.65f, 0.6f));
            CreateMaterial("rubber", new Color(0.15f, 0.15f, 0.15f));
            CreateMaterial("plastic", new Color(0.8f, 0.8f, 0.8f));

            // Road/pavement
            CreateMaterial("asphalt", new Color(0.2f, 0.2f, 0.22f));
            CreateMaterial("road", new Color(0.25f, 0.25f, 0.27f));

            // Cream/neutral tones
            CreateMaterial("cream", new Color(0.96f, 0.94f, 0.88f));
            CreateMaterial("beige", new Color(0.9f, 0.85f, 0.75f));
            CreateMaterial("tan", new Color(0.85f, 0.75f, 0.6f));

            // Default fallback
            CreateMaterial("default", new Color(0.7f, 0.7f, 0.7f));

            Debug.Log($"MaterialManager: Created {_materials.Count} materials");
        }

        /// <summary>
        /// Create a material with the given name and color.
        /// </summary>
        private void CreateMaterial(string name, Color color, bool isTransparent = false)
        {
            // Try URP first, fallback to Standard
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat.shader == null)
            {
                mat = new Material(Shader.Find("Standard"));
            }

            mat.color = color;
            mat.name = name;

            // Handle transparency
            if (isTransparent)
            {
                // URP transparency
                mat.SetFloat("_Surface", 1); // Transparent
                mat.SetFloat("_Blend", 0);   // Alpha
                
                // Standard shader transparency fallback
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;

                // Make it semi-transparent
                color.a = 0.6f;
                mat.color = color;
            }

            _materials[name] = mat;
            _colors[name] = color;
        }

        /// <summary>
        /// Get a material by name. Returns default if not found.
        /// </summary>
        public Material GetMaterial(string materialName)
        {
            if (string.IsNullOrEmpty(materialName))
                return _materials["default"];

            string key = materialName.ToLower();
            if (_materials.ContainsKey(key))
                return _materials[key];

            Debug.LogWarning($"MaterialManager: Material '{materialName}' not found, using default");
            return _materials["default"];
        }

        /// <summary>
        /// Get a color by name. Returns gray if not found.
        /// </summary>
        public Color GetColor(string colorName)
        {
            if (string.IsNullOrEmpty(colorName))
                return Color.gray;

            string key = colorName.ToLower();
            if (_colors.ContainsKey(key))
                return _colors[key];

            return Color.gray;
        }

        /// <summary>
        /// Apply a material to a GameObject's renderer.
        /// </summary>
        public void ApplyMaterial(GameObject obj, string materialName)
        {
            if (obj == null) return;

            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = GetMaterial(materialName);
            }
            else
            {
                Debug.LogWarning($"MaterialManager: No renderer found on {obj.name}");
            }
        }

        /// <summary>
        /// Create a custom material on the fly from a hex color.
        /// </summary>
        public Material CreateMaterialFromHex(string hexColor)
        {
            Color color;
            if (ColorUtility.TryParseHtmlString(hexColor, out color))
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                if (mat.shader == null)
                    mat = new Material(Shader.Find("Standard"));
                
                mat.color = color;
                return mat;
            }

            Debug.LogWarning($"MaterialManager: Failed to parse hex color '{hexColor}'");
            return GetMaterial("default");
        }

        /// <summary>
        /// Get all available material names.
        /// </summary>
        public List<string> GetAllMaterialNames()
        {
            return new List<string>(_materials.Keys);
        }

        /// <summary>
        /// Check if a material exists.
        /// </summary>
        public bool HasMaterial(string materialName)
        {
            return _materials.ContainsKey(materialName.ToLower());
        }
    }
}
