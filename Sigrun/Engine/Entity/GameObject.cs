using System.Numerics;
using Sigrun.Engine.Entity.Components;
using Sigrun.Rendering.Loader;

namespace Sigrun.Engine.Entity;

/// <summary>
/// Base element of all non-engine functionality. Non-functional without accompanying
/// components.
/// </summary>
public class GameObject
{
    public List<Component> Components { get; set; } = [];
    
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public float Scale { get; set; } = 1.0f;

    public static GameObject FromModelFile(string path, string name)
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
}