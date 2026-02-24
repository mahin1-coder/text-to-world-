using System;
using System.IO;
using System.Xml;
using UnityEngine;

namespace TextToWorld.Core
{
    /// <summary>
    /// Imports scene data from XML and rebuilds the scene.
    /// </summary>
    public static class XmlImporter
    {
        private static string DefaultImportPath => Path.Combine(Application.persistentDataPath, "scene_export.xml");

        /// <summary>
        /// Import scene from XML file.
        /// </summary>
        public static void Import(SceneController sceneController, string filePath = null)
        {
            filePath = filePath ?? DefaultImportPath;

            if (!File.Exists(filePath))
            {
                Debug.LogError($"XmlImporter: File not found - {filePath}");
                return;
            }

            try
            {
                // Clear existing scene first
                WorldState.Instance.ClearAll();

                var doc = new XmlDocument();
                doc.Load(filePath);

                // Read Objects
                var objectNodes = doc.SelectNodes("//Scene/Objects/Object");
                int objectCount = 0;

                if (objectNodes != null)
                {
                    foreach (XmlNode node in objectNodes)
                    {
                        var data = ParseObjectNode(node);
                        if (data != null)
                        {
                            sceneController.SpawnFromData(data);
                            objectCount++;
                        }
                    }
                }

                // Read Actors (for future use - currently just logs)
                var actorNodes = doc.SelectNodes("//Scene/Actors/Actor");
                int actorCount = 0;

                if (actorNodes != null)
                {
                    foreach (XmlNode node in actorNodes)
                    {
                        var actor = ParseActorNode(node);
                        if (actor != null)
                        {
                            // For now, just register the data
                            // Player repositioning could be added here
                            actorCount++;
                        }
                    }
                }

                Debug.Log($"XmlImporter: Imported {objectCount} objects, {actorCount} actors from {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"XmlImporter: Import failed - {ex.Message}");
            }
        }

        private static SceneObjectData ParseObjectNode(XmlNode node)
        {
            try
            {
                var data = new SceneObjectData();

                // Read attributes
                data.id = node.Attributes?["id"]?.Value ?? "";
                data.type = node.Attributes?["type"]?.Value ?? "cube";

                // Read Position
                var posNode = node.SelectSingleNode("Position");
                if (posNode != null)
                {
                    data.position = new Vector3(
                        ParseFloat(posNode.Attributes?["x"]?.Value),
                        ParseFloat(posNode.Attributes?["y"]?.Value),
                        ParseFloat(posNode.Attributes?["z"]?.Value)
                    );
                }

                // Read Rotation
                var rotNode = node.SelectSingleNode("Rotation");
                if (rotNode != null)
                {
                    data.rotation = new Vector3(
                        ParseFloat(rotNode.Attributes?["x"]?.Value),
                        ParseFloat(rotNode.Attributes?["y"]?.Value),
                        ParseFloat(rotNode.Attributes?["z"]?.Value)
                    );
                }

                // Read Scale
                var scaleNode = node.SelectSingleNode("Scale");
                if (scaleNode != null)
                {
                    data.scale = new Vector3(
                        ParseFloat(scaleNode.Attributes?["x"]?.Value, 1f),
                        ParseFloat(scaleNode.Attributes?["y"]?.Value, 1f),
                        ParseFloat(scaleNode.Attributes?["z"]?.Value, 1f)
                    );
                }

                // Read Appearance
                var appearanceNode = node.SelectSingleNode("Appearance");
                if (appearanceNode != null)
                {
                    data.color = appearanceNode.Attributes?["color"]?.Value ?? "white";
                    data.texture = appearanceNode.Attributes?["texture"]?.Value ?? "";
                }

                // Read PrefabName
                var prefabNode = node.SelectSingleNode("PrefabName");
                if (prefabNode != null)
                {
                    data.prefabName = prefabNode.InnerText;
                }

                return data;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"XmlImporter: Failed to parse object - {ex.Message}");
                return null;
            }
        }

        private static ActorData ParseActorNode(XmlNode node)
        {
            try
            {
                var actor = new ActorData();

                actor.id = node.Attributes?["id"]?.Value ?? "player_1";
                actor.type = node.Attributes?["type"]?.Value ?? "fps";

                // Read Position
                var posNode = node.SelectSingleNode("Position");
                if (posNode != null)
                {
                    actor.position = new Vector3(
                        ParseFloat(posNode.Attributes?["x"]?.Value),
                        ParseFloat(posNode.Attributes?["y"]?.Value, 1.8f),
                        ParseFloat(posNode.Attributes?["z"]?.Value)
                    );
                }

                // Read Rotation
                var rotNode = node.SelectSingleNode("Rotation");
                if (rotNode != null)
                {
                    actor.rotation = new Vector3(
                        ParseFloat(rotNode.Attributes?["x"]?.Value),
                        ParseFloat(rotNode.Attributes?["y"]?.Value),
                        ParseFloat(rotNode.Attributes?["z"]?.Value)
                    );
                }

                return actor;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"XmlImporter: Failed to parse actor - {ex.Message}");
                return null;
            }
        }

        private static float ParseFloat(string value, float defaultValue = 0f)
        {
            if (string.IsNullOrEmpty(value)) return defaultValue;
            return float.TryParse(value, out float result) ? result : defaultValue;
        }

        /// <summary>
        /// Get the default import path.
        /// </summary>
        public static string GetDefaultPath()
        {
            return DefaultImportPath;
        }
    }
}
