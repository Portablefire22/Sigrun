using System.Numerics;
using Sigrun.Engine.Entity.Components;
using Sigrun.Engine.Entity.Components.Physics.Colliders;
using Sigrun.Engine.Rendering;
using Sigrun.Engine.Rendering.Loader;

namespace Sigrun.Engine.Entity;

/// <summary>
/// Base element of all non-engine functionality. Non-functional without accompanying
/// components.
/// </summary>
public class GameObject
{
    public List<Component> Components { get; set; } = [];
    
    public Vector3 Position { get; set; }
    public Rotation Rotation { get; set; } = new Rotation();
    public Vector3 Scale { get; set; } = new Vector3(1.0f);

    public List<string> Tags { get; private set; } = [];

    public static GameObject FromRMeshFile(string path, string name)
    {
        var loader = new RMeshLoader2();
        var model = loader.LoadFromFile(path, name);


        var obj = new GameObject()
        {
            Components = []
        };
        var renderer = new Renderer(obj, model)
        {
            Model = model
        };
        obj.Components.Add(renderer);
        return obj;
    }

    public Bounds? GetBounds()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer != null) return Bounds.FromModel(renderer.Model);
        var coll = GetComponent<Collider>();
        if (coll != null) return Bounds.FromMesh(coll.Mesh);
        return null;
    }
    
    public T? GetComponent<T>()
    {
        foreach (var component in Components)
        {
            if (component is T output) return output;
        }
        return default;
    }
}