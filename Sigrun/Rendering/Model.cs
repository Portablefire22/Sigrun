using System.Numerics;
using Sigrun.Rendering.Entities;

namespace Sigrun.Rendering;

public class Model
{
    public Mesh Mesh;
    public RoomMeshEntity[] Entities;
    public List<string> Textures = new List<string>();
}