using UnityEngine;
using TextToWorld.Data;
using TextToWorld.Materials;

/// <summary>
/// Manages environment settings: sky, terrain, lighting, and atmosphere.
/// Configures the scene's visual ambiance based on SceneData.
/// </summary>
namespace TextToWorld.Environment
{
    public class EnvironmentManager : MonoBehaviour
    {
        // References
        private GameObject _terrain;
        private Light _directionalLight;
        
        // Terrain settings
        [Header("Terrain Configuration")]
        public Vector3 terrainSize = new Vector3(100f, 1f, 100f);
        public Vector3 terrainPosition = new Vector3(0, 0, 0);

        /// <summary>
        /// Setup the complete environment from SceneData.
        /// </summary>
        public void SetupEnvironment(SceneData sceneData)
        {
            Debug.Log("EnvironmentManager: Setting up environment...");

            // 1. Create/update terrain
            SetupTerrain(sceneData.terrain);

            // 2. Configure lighting
            SetupLighting(sceneData.lighting);

            // 3. Configure sky/ambient
            SetupSky(sceneData.sky, sceneData.lighting);

            // 4. Add atmospheric effects
            SetupAtmosphere(sceneData.environment);

            Debug.Log("EnvironmentManager: Environment setup complete");
        }

        /// <summary>
        /// Create or update the terrain based on terrain type.
        /// </summary>
        private void SetupTerrain(string terrainType)
        {
            Debug.Log($"EnvironmentManager: Creating terrain - {terrainType}");

            // Remove old terrain if exists
            if (_terrain != null)
            {
                Destroy(_terrain);
            }

            // Create ground plane
            _terrain = GameObject.CreatePrimitive(PrimitiveType.Plane);
            _terrain.name = "Terrain";
            _terrain.transform.position = terrainPosition;
            _terrain.transform.localScale = terrainSize;

            // Apply appropriate material
            string materialName = GetTerrainMaterial(terrainType);
            MaterialManager.Instance.ApplyMaterial(_terrain, materialName);

            // Disable collider if needed (optional)
            // var collider = _terrain.GetComponent<Collider>();
            // if (collider != null) collider.enabled = true;
        }

        /// <summary>
        /// Map terrain type to material name.
        /// </summary>
        private string GetTerrainMaterial(string terrainType)
        {
            switch (terrainType.ToLower())
            {
                case "grass": return "grass";
                case "sand": return "sand";
                case "concrete": return "concrete";
                case "asphalt": return "asphalt";
                case "wood": return "wood";
                case "stone": return "stone";
                case "marble": return "marble";
                case "snow": return "snow";
                case "ice": return "ice";
                case "water": return "water";
                case "metal": return "metal";
                case "carpet": return "carpet";
                default: return "grass";
            }
        }

        /// <summary>
        /// Setup directional light (sun/moon) based on lighting data.
        /// </summary>
        private void SetupLighting(LightingData lightingData)
        {
            Debug.Log("EnvironmentManager: Configuring lighting...");

            // Find or create directional light
            _directionalLight = FindObjectOfType<Light>();
            if (_directionalLight == null || _directionalLight.type != LightType.Directional)
            {
                var lightObj = new GameObject("Directional Light");
                _directionalLight = lightObj.AddComponent<Light>();
                _directionalLight.type = LightType.Directional;
            }

            // Configure light properties
            _directionalLight.intensity = lightingData.directionalIntensity;
            _directionalLight.transform.eulerAngles = lightingData.directionalRotation;
            _directionalLight.color = Color.white;

            // Add shadows
            _directionalLight.shadows = LightShadows.Soft;
            _directionalLight.shadowStrength = 0.8f;

            Debug.Log($"  Light intensity: {lightingData.directionalIntensity}");
            Debug.Log($"  Light rotation: {lightingData.directionalRotation}");
        }

        /// <summary>
        /// Configure sky and ambient lighting.
        /// </summary>
        private void SetupSky(string skyState, LightingData lightingData)
        {
            Debug.Log($"EnvironmentManager: Setting up sky - {skyState}");

            // Configure ambient lighting mode
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;

            // Parse hex colors
            Color skyColor = ParseHexColor(lightingData.skyColor);
            Color horizonColor = ParseHexColor(lightingData.horizonColor);
            Color groundColor = ParseHexColor(lightingData.groundColor);

            // Apply ambient colors
            RenderSettings.ambientSkyColor = skyColor;
            RenderSettings.ambientEquatorColor = horizonColor;
            RenderSettings.ambientGroundColor = groundColor;

            // Set fog for atmosphere
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            
            switch (skyState.ToLower())
            {
                case "night":
                    RenderSettings.fogColor = new Color(0.05f, 0.05f, 0.15f);
                    RenderSettings.fogDensity = 0.01f;
                    break;
                case "sunset":
                case "sunrise":
                    RenderSettings.fogColor = new Color(0.9f, 0.6f, 0.4f);
                    RenderSettings.fogDensity = 0.008f;
                    break;
                case "cloudy":
                    RenderSettings.fogColor = new Color(0.6f, 0.6f, 0.6f);
                    RenderSettings.fogDensity = 0.012f;
                    break;
                default: // daytime
                    RenderSettings.fogColor = new Color(0.7f, 0.8f, 0.9f);
                    RenderSettings.fogDensity = 0.005f;
                    break;
            }

            Debug.Log($"  Sky color: {skyColor}");
            Debug.Log($"  Fog color: {RenderSettings.fogColor}");
        }

        /// <summary>
        /// Add atmospheric elements based on environment type.
        /// </summary>
        private void SetupAtmosphere(string environmentType)
        {
            Debug.Log($"EnvironmentManager: Adding atmosphere for {environmentType}");

            // TODO: Add environment-specific effects
            // - Particle systems (rain, snow, leaves, dust)
            // - Skybox customization
            // - Post-processing effects
            
            switch (environmentType.ToLower())
            {
                case "desert":
                    // Could add dust particles, heat haze effect
                    break;
                case "forest":
                    // Could add falling leaves, mist
                    break;
                case "city":
                    // Could add smog, neon glow
                    break;
                case "space":
                    // Dark skybox, stars
                    RenderSettings.fog = false;
                    RenderSettings.ambientLight = new Color(0.1f, 0.1f, 0.15f);
                    break;
            }
        }

        /// <summary>
        /// Parse hex color string to Unity Color.
        /// </summary>
        private Color ParseHexColor(string hexColor)
        {
            Color color;
            if (ColorUtility.TryParseHtmlString(hexColor, out color))
            {
                return color;
            }
            
            Debug.LogWarning($"EnvironmentManager: Failed to parse color '{hexColor}', using default");
            return Color.gray;
        }

        /// <summary>
        /// Clear the current environment.
        /// </summary>
        public void ClearEnvironment()
        {
            if (_terrain != null)
            {
                Destroy(_terrain);
                _terrain = null;
            }

            // Reset to defaults
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.5f, 0.5f, 0.5f);
            RenderSettings.fog = false;
        }

        /// <summary>
        /// Update just the lighting without rebuilding everything.
        /// </summary>
        public void UpdateLighting(LightingData lightingData)
        {
            SetupLighting(lightingData);
            SetupSky("daytime", lightingData);
        }
    }
}
