using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TextToWorld.Core
{
    /// <summary>
    /// Parses text commands into structured command objects.
    /// Supports: spawn, move, rotate, scale, set_color, set_texture, delete, clear, export_xml, import_xml
    /// </summary>
    public static class CommandParser
    {
        /// <summary>
        /// Represents a parsed command ready for execution.
        /// </summary>
        public class ParsedCommand
        {
            public CommandType Type;
            public string TargetId;
            public string ObjectType;
            public Vector3 Position;
            public Vector3 Rotation;
            public Vector3 Scale;
            public string Color;
            public string Texture;
            public string FilePath;
            public bool IsValid;
            public string ErrorMessage;

            public static ParsedCommand Invalid(string error)
            {
                return new ParsedCommand { IsValid = false, ErrorMessage = error };
            }
        }

        public enum CommandType
        {
            None,
            Spawn,
            Move,
            Rotate,
            Scale,
            SetColor,
            SetTexture,
            Delete,
            Clear,
            ExportXml,
            ImportXml,
            Help
        }

        // Regex patterns for parsing
        private static readonly Regex SpawnPattern = new Regex(
            @"^spawn\s+(\w+)(?:\s+id=(\w+))?(?:\s+color=(\w+))?\s+at\s+(-?\d+\.?\d*),(-?\d+\.?\d*),(-?\d+\.?\d*)$",
            RegexOptions.IgnoreCase);
        
        private static readonly Regex MovePattern = new Regex(
            @"^move\s+(\w+)\s+to\s+(-?\d+\.?\d*),(-?\d+\.?\d*),(-?\d+\.?\d*)$",
            RegexOptions.IgnoreCase);
        
        private static readonly Regex RotatePattern = new Regex(
            @"^rotate\s+(\w+)\s+to\s+(-?\d+\.?\d*),(-?\d+\.?\d*),(-?\d+\.?\d*)$",
            RegexOptions.IgnoreCase);
        
        private static readonly Regex ScalePattern = new Regex(
            @"^scale\s+(\w+)\s+to\s+(-?\d+\.?\d*),(-?\d+\.?\d*),(-?\d+\.?\d*)$",
            RegexOptions.IgnoreCase);
        
        private static readonly Regex SetColorPattern = new Regex(
            @"^set_color\s+(\w+)\s+(\w+)$",
            RegexOptions.IgnoreCase);
        
        private static readonly Regex SetTexturePattern = new Regex(
            @"^set_texture\s+(\w+)\s+(\w+)$",
            RegexOptions.IgnoreCase);
        
        private static readonly Regex DeletePattern = new Regex(
            @"^delete\s+(\w+)$",
            RegexOptions.IgnoreCase);

        /// <summary>
        /// Parse a text command string into a structured command.
        /// </summary>
        public static ParsedCommand Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return ParsedCommand.Invalid("Empty command");

            input = input.Trim();
            string lower = input.ToLower();

            // Check simple commands first
            if (lower == "clear" || lower == "clear_all")
            {
                return new ParsedCommand { Type = CommandType.Clear, IsValid = true };
            }

            if (lower == "export_xml" || lower == "export")
            {
                return new ParsedCommand { Type = CommandType.ExportXml, IsValid = true };
            }

            if (lower == "import_xml" || lower == "import")
            {
                return new ParsedCommand { Type = CommandType.ImportXml, IsValid = true };
            }

            if (lower == "help" || lower == "?")
            {
                return new ParsedCommand { Type = CommandType.Help, IsValid = true };
            }

            // Try spawn pattern
            var match = SpawnPattern.Match(input);
            if (match.Success)
            {
                return new ParsedCommand
                {
                    Type = CommandType.Spawn,
                    ObjectType = match.Groups[1].Value.ToLower(),
                    TargetId = string.IsNullOrEmpty(match.Groups[2].Value) ? null : match.Groups[2].Value,
                    Color = string.IsNullOrEmpty(match.Groups[3].Value) ? "white" : match.Groups[3].Value.ToLower(),
                    Position = new Vector3(
                        float.Parse(match.Groups[4].Value),
                        float.Parse(match.Groups[5].Value),
                        float.Parse(match.Groups[6].Value)
                    ),
                    Scale = Vector3.one,
                    Rotation = Vector3.zero,
                    IsValid = true
                };
            }

            // Try move pattern
            match = MovePattern.Match(input);
            if (match.Success)
            {
                return new ParsedCommand
                {
                    Type = CommandType.Move,
                    TargetId = match.Groups[1].Value,
                    Position = new Vector3(
                        float.Parse(match.Groups[2].Value),
                        float.Parse(match.Groups[3].Value),
                        float.Parse(match.Groups[4].Value)
                    ),
                    IsValid = true
                };
            }

            // Try rotate pattern
            match = RotatePattern.Match(input);
            if (match.Success)
            {
                return new ParsedCommand
                {
                    Type = CommandType.Rotate,
                    TargetId = match.Groups[1].Value,
                    Rotation = new Vector3(
                        float.Parse(match.Groups[2].Value),
                        float.Parse(match.Groups[3].Value),
                        float.Parse(match.Groups[4].Value)
                    ),
                    IsValid = true
                };
            }

            // Try scale pattern
            match = ScalePattern.Match(input);
            if (match.Success)
            {
                return new ParsedCommand
                {
                    Type = CommandType.Scale,
                    TargetId = match.Groups[1].Value,
                    Scale = new Vector3(
                        float.Parse(match.Groups[2].Value),
                        float.Parse(match.Groups[3].Value),
                        float.Parse(match.Groups[4].Value)
                    ),
                    IsValid = true
                };
            }

            // Try set_color pattern
            match = SetColorPattern.Match(input);
            if (match.Success)
            {
                return new ParsedCommand
                {
                    Type = CommandType.SetColor,
                    TargetId = match.Groups[1].Value,
                    Color = match.Groups[2].Value.ToLower(),
                    IsValid = true
                };
            }

            // Try set_texture pattern
            match = SetTexturePattern.Match(input);
            if (match.Success)
            {
                return new ParsedCommand
                {
                    Type = CommandType.SetTexture,
                    TargetId = match.Groups[1].Value,
                    Texture = match.Groups[2].Value,
                    IsValid = true
                };
            }

            // Try delete pattern
            match = DeletePattern.Match(input);
            if (match.Success)
            {
                return new ParsedCommand
                {
                    Type = CommandType.Delete,
                    TargetId = match.Groups[1].Value,
                    IsValid = true
                };
            }

            return ParsedCommand.Invalid($"Unknown command: {input}");
        }

        /// <summary>
        /// Get help text for all available commands.
        /// </summary>
        public static string GetHelpText()
        {
            return @"
=== Text-to-World Commands ===

SPAWN:
  spawn <type> at x,y,z
  spawn <type> id=<name> at x,y,z
  spawn <type> id=<name> color=<color> at x,y,z
  Example: spawn cube id=box1 color=red at 0,1,0

MOVE:
  move <id> to x,y,z
  Example: move box1 to 5,1,3

ROTATE:
  rotate <id> to x,y,z
  Example: rotate box1 to 0,45,0

SCALE:
  scale <id> to x,y,z
  Example: scale box1 to 2,2,2

SET COLOR:
  set_color <id> <colorName>
  Example: set_color box1 blue

SET TEXTURE:
  set_texture <id> <textureName>
  Example: set_texture box1 wood

DELETE:
  delete <id>
  Example: delete box1

CLEAR:
  clear

EXPORT/IMPORT:
  export_xml
  import_xml

TYPES: cube, sphere, cylinder, capsule, plane, table, chair
COLORS: red, blue, green, yellow, orange, purple, pink, white, black, gray, brown, wood, metal, gold, silver
";
        }
    }
}
