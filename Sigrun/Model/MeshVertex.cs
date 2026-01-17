using System.Numerics;

namespace Sigrun.Model;

public class MeshVertex
{
    public Vector3 Position { get; set; }
    public Vector2 Uv { get; set; }
    public Vector2 LightmapUv { get; set; }
    public uint Texture { get; set; }
}