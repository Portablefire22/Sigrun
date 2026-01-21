using System.Numerics;

namespace Sigrun.Engine.Rendering;

public class MeshVertex
{
    public Vector3 Position { get; set; }
    public Vector3 Normal { get; set; }
    public Vector2 Uv { get; set; }
    public int Index { get; set; }
    public Vector2 LightmapUv { get; set; }

    public float Alpha { get; set; } = 1f;
    
    public byte Red { get; set; }
    public byte Green { get; set; }
    public byte Blue { get; set; }
}