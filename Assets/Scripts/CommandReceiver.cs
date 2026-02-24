using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CommandReceiver : MonoBehaviour
{
    [Serializable]
    private class Vector3Data
    {
        public float x;
        public float y;
        public float z;
        public Vector3 ToVector3() { return new Vector3(x, y, z); }
    }

    [Serializable]
    private class CommandMessage
    {
        public string command;
        public string id;
        public string type;
        public Vector3Data position;
        public Vector3Data scale;
        public Vector3Data rotation;
        public string material;
        public string lightType;
        public string color;
    }

    [Serializable]
    private class BatchWrapper
    {
        public CommandMessage[] batch;
    }

    private Dictionary<string, GameObject> _entities = new Dictionary<string, GameObject>();
    private Dictionary<string, Material> _materials = new Dictionary<string, Material>();
    private Dictionary<string, Color> _colors = new Dictionary<string, Color>();
    private string _commandFilePath;
    private GameObject _worldRoot;

    private void Start()
    {
        _commandFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "VirtualWorldBridge",
            "command.json"
        );
        
        _worldRoot = GameObject.Find("WorldRoot");
        if (_worldRoot == null)
            _worldRoot = new GameObject("WorldRoot");
        
        CreateMaterials();
        Debug.Log("CommandReceiver started. Watching: " + _commandFilePath);
    }

    private void CreateMaterials()
    {
        CreateMat("red", new Color(0.8f, 0.2f, 0.2f));
        CreateMat("green", new Color(0.2f, 0.7f, 0.2f));
        CreateMat("blue", new Color(0.2f, 0.4f, 0.8f));
        CreateMat("yellow", new Color(0.9f, 0.85f, 0.2f));
        CreateMat("orange", new Color(0.9f, 0.5f, 0.1f));
        CreateMat("purple", new Color(0.6f, 0.2f, 0.8f));
        CreateMat("pink", new Color(0.95f, 0.5f, 0.7f));
        CreateMat("white", new Color(0.95f, 0.95f, 0.95f));
        CreateMat("black", new Color(0.1f, 0.1f, 0.1f));
        CreateMat("gray", new Color(0.5f, 0.5f, 0.5f));
        CreateMat("brown", new Color(0.55f, 0.35f, 0.2f));
        CreateMat("cream", new Color(0.96f, 0.94f, 0.88f));
        CreateMat("wood", new Color(0.6f, 0.4f, 0.25f));
        CreateMat("darkwood", new Color(0.35f, 0.2f, 0.1f));
        CreateMat("brick", new Color(0.6f, 0.25f, 0.2f));
        CreateMat("marble", new Color(0.92f, 0.9f, 0.88f));
        CreateMat("leather", new Color(0.45f, 0.28f, 0.18f));
        CreateMat("metal", new Color(0.7f, 0.7f, 0.75f));
        CreateMat("gold", new Color(0.85f, 0.7f, 0.2f));
        CreateMat("silver", new Color(0.8f, 0.8f, 0.85f));
    }

    private void CreateMat(string name, Color color)
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader == null)
            mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        _materials[name] = mat;
        _colors[name] = color;
    }

    private void Update()
    {
        if (!File.Exists(_commandFilePath)) return;

        string json;
        try
        {
            json = File.ReadAllText(_commandFilePath);
            if (string.IsNullOrWhiteSpace(json)) return;
        }
        catch (IOException) { return; }

        try
        {
            if (json.Contains("batch"))
            {
                var batch = JsonUtility.FromJson<BatchWrapper>(json);
                if (batch != null && batch.batch != null)
                {
                    foreach (var cmd in batch.batch)
                        ExecuteCommand(cmd);
                    Debug.Log("Executed batch of " + batch.batch.Length + " commands");
                }
            }
            else
            {
                var msg = JsonUtility.FromJson<CommandMessage>(json);
                if (msg != null) ExecuteCommand(msg);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Parse failed: " + ex.Message);
        }
        finally
        {
            try { File.Delete(_commandFilePath); } catch { }
        }
    }

    private void ExecuteCommand(CommandMessage msg)
    {
        Debug.Log("Executing: " + msg.command + " " + msg.id);
        
        switch (msg.command)
        {
            case "spawn": HandleSpawn(msg); break;
            case "move": HandleMove(msg); break;
            case "rotate": HandleRotate(msg); break;
            case "scale": HandleScale(msg); break;
            case "set_material": HandleSetMaterial(msg); break;
            case "delete": HandleDelete(msg); break;
            case "clear_all": HandleClearAll(); break;
            case "light": HandleLight(msg); break;
            case "setup_camera": HandleSetupCamera(); break;
        }
    }

    private void HandleSetupCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0, 8, -15);
            cam.transform.eulerAngles = new Vector3(25, 0, 0);
            Debug.Log("Camera positioned for viewing");
        }
    }

    private void HandleSpawn(CommandMessage msg)
    {
        GameObject obj = CreatePrimitive(msg.type);
        obj.name = msg.id;
        obj.transform.SetParent(_worldRoot.transform);
        
        if (msg.position != null)
            obj.transform.position = msg.position.ToVector3();
        
        if (msg.scale != null)
            obj.transform.localScale = msg.scale.ToVector3();
        
        if (!string.IsNullOrEmpty(msg.material))
            ApplyMaterial(obj, msg.material);
        
        _entities[msg.id] = obj;
    }

    private void HandleMove(CommandMessage msg)
    {
        if (_entities.TryGetValue(msg.id, out var obj) && msg.position != null)
            obj.transform.position = msg.position.ToVector3();
    }

    private void HandleRotate(CommandMessage msg)
    {
        if (_entities.TryGetValue(msg.id, out var obj) && msg.rotation != null)
            obj.transform.eulerAngles = msg.rotation.ToVector3();
    }

    private void HandleScale(CommandMessage msg)
    {
        if (_entities.TryGetValue(msg.id, out var obj) && msg.scale != null)
            obj.transform.localScale = msg.scale.ToVector3();
    }

    private void HandleSetMaterial(CommandMessage msg)
    {
        if (_entities.TryGetValue(msg.id, out var obj))
            ApplyMaterial(obj, msg.material);
    }

    private void HandleDelete(CommandMessage msg)
    {
        if (_entities.TryGetValue(msg.id, out var obj))
        {
            _entities.Remove(msg.id);
            Destroy(obj);
        }
    }

    private void HandleClearAll()
    {
        foreach (Transform child in _worldRoot.transform)
            Destroy(child.gameObject);
        _entities.Clear();
        Debug.Log("Cleared all objects");
    }

    private void HandleLight(CommandMessage msg)
    {
        GameObject lightObj = new GameObject(msg.id);
        lightObj.transform.SetParent(_worldRoot.transform);
        
        if (msg.position != null)
            lightObj.transform.position = msg.position.ToVector3();
        
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.intensity = 2f;
        light.range = 15f;
        
        if (!string.IsNullOrEmpty(msg.color) && _colors.TryGetValue(msg.color, out var c))
            light.color = c;
        else
            light.color = Color.white;
        
        _entities[msg.id] = lightObj;
        Debug.Log("Created light: " + msg.id);
    }

    private GameObject CreatePrimitive(string typeName)
    {
        if (typeName == "sphere") return GameObject.CreatePrimitive(PrimitiveType.Sphere);
        if (typeName == "cylinder") return GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        if (typeName == "plane") return GameObject.CreatePrimitive(PrimitiveType.Plane);
        if (typeName == "capsule") return GameObject.CreatePrimitive(PrimitiveType.Capsule);
        return GameObject.CreatePrimitive(PrimitiveType.Cube);
    }

    private void ApplyMaterial(GameObject obj, string matName)
    {
        if (string.IsNullOrEmpty(matName)) return;
        var rend = obj.GetComponent<Renderer>();
        if (rend != null && _materials.TryGetValue(matName, out var mat))
            rend.material = mat;
    }
}
