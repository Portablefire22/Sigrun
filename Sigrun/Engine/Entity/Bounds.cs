using System.Numerics;
using Sigrun.Engine.Rendering;

namespace Sigrun.Engine.Entity;

public class Bounds
{
    public Vector3 Maxs { get; private set; }
    public Vector3 Mins { get; private set; }

    public static Bounds FromModel(Model model)
    {
        var min = new Vector3(float.NegativeInfinity);
        var max = new Vector3(float.PositiveInfinity);
        foreach (var mesh in model.Meshes)
        {
            foreach (var verts in mesh.Vertices)
            {
                min = Vector3.Min(verts.Position, min);
                max = Vector3.Max(verts.Position, max);
            }
        }

        return new Bounds()
        {
            Maxs = max,
            Mins = min,
        };
    }

    public static Bounds FromMesh(Mesh mesh)
    {
        var min = new Vector3(float.NegativeInfinity);
        var max = new Vector3(float.PositiveInfinity);
        foreach (var verts in mesh.Vertices)
        {
            min = Vector3.Min(verts.Position, min);
            max = Vector3.Max(verts.Position, max);
        }

        return new Bounds()
        {
            Maxs = max,
            Mins = min,
        };
    }
}