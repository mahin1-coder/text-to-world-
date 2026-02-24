using System;
using System.IO;
using System.Xml;
using UnityEngine;

namespace TextToWorld.Core
{
    /// <summary>
    /// Exports the current WorldState to XML format.
    /// </summary>
    public static class XmlExporter
    {
        private static string DefaultExportPath => Path.Combine(Application.persistentDataPath, "scene_export.xml");

        /// <summary>
        /// Export world state to XML file.
        /// </summary>
        public static void Export(WorldState worldState, string filePath = null)
        {
            if (worldState == null)
            {
                Debug.LogError("XmlExporter: WorldState is null");
                return;
            }

            filePath = filePath ?? DefaultExportPath;

            // Sync all object transforms before export
            worldState.SyncAllFromScene();

            try
            {
                using (var writer = XmlWriter.Create(filePath, new XmlWriterSettings 
                { 
                    Indent = true, 
                    IndentChars = "  " 
                }))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Scene");
                    writer.WriteAttributeString("version", "1.0");
                    writer.WriteAttributeString("exportTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    // Write Objects section
                    writer.WriteStartElement("Objects");
                    foreach (var obj in worldState.GetAllObjects())
                    {
                        WriteObject(writer, obj);
                    }
                    writer.WriteEndElement(); // Objects

                    // Write Actors section
                    writer.WriteStartElement("Actors");
                    foreach (var actor in worldState.GetAllActors())
                    {
                        WriteActor(writer, actor);
                    }
                    writer.WriteEndElement(); // Actors

                    writer.WriteEndElement(); // Scene
                    writer.WriteEndDocument();
                }

                Debug.Log($"XmlExporter: Scene exported to {filePath}");
                Debug.Log($"XmlExporter: Exported {worldState.ObjectCount} objects");
            }
            catch (Exception ex)
            {
                Debug.LogError($"XmlExporter: Export failed - {ex.Message}");
            }
        }

        private static void WriteObject(XmlWriter writer, SceneObjectData obj)
        {
            writer.WriteStartElement("Object");
            writer.WriteAttributeString("id", obj.id);
            writer.WriteAttributeString("type", obj.type);

            // Position
            writer.WriteStartElement("Position");
            writer.WriteAttributeString("x", obj.position.x.ToString("F3"));
            writer.WriteAttributeString("y", obj.position.y.ToString("F3"));
            writer.WriteAttributeString("z", obj.position.z.ToString("F3"));
            writer.WriteEndElement();

            // Rotation
            writer.WriteStartElement("Rotation");
            writer.WriteAttributeString("x", obj.rotation.x.ToString("F3"));
            writer.WriteAttributeString("y", obj.rotation.y.ToString("F3"));
            writer.WriteAttributeString("z", obj.rotation.z.ToString("F3"));
            writer.WriteEndElement();

            // Scale
            writer.WriteStartElement("Scale");
            writer.WriteAttributeString("x", obj.scale.x.ToString("F3"));
            writer.WriteAttributeString("y", obj.scale.y.ToString("F3"));
            writer.WriteAttributeString("z", obj.scale.z.ToString("F3"));
            writer.WriteEndElement();

            // Appearance
            writer.WriteStartElement("Appearance");
            writer.WriteAttributeString("color", obj.color ?? "white");
            writer.WriteAttributeString("texture", obj.texture ?? "");
            writer.WriteEndElement();

            // Prefab (if any)
            if (!string.IsNullOrEmpty(obj.prefabName))
            {
                writer.WriteElementString("PrefabName", obj.prefabName);
            }

            writer.WriteEndElement(); // Object
        }

        private static void WriteActor(XmlWriter writer, ActorData actor)
        {
            // Sync position from GameObject
            actor.SyncFromGameObject();

            writer.WriteStartElement("Actor");
            writer.WriteAttributeString("id", actor.id);
            writer.WriteAttributeString("type", actor.type);

            // Position
            writer.WriteStartElement("Position");
            writer.WriteAttributeString("x", actor.position.x.ToString("F3"));
            writer.WriteAttributeString("y", actor.position.y.ToString("F3"));
            writer.WriteAttributeString("z", actor.position.z.ToString("F3"));
            writer.WriteEndElement();

            // Rotation
            writer.WriteStartElement("Rotation");
            writer.WriteAttributeString("x", actor.rotation.x.ToString("F3"));
            writer.WriteAttributeString("y", actor.rotation.y.ToString("F3"));
            writer.WriteAttributeString("z", actor.rotation.z.ToString("F3"));
            writer.WriteEndElement();

            writer.WriteEndElement(); // Actor
        }

        /// <summary>
        /// Get the default export path.
        /// </summary>
        public static string GetDefaultPath()
        {
            return DefaultExportPath;
        }
    }
}
