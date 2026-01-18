using System.Numerics;
using Sigrun.Model.Entities;

namespace Sigrun.Model;

public class Model
{
    public Mesh Mesh;
    public RoomMeshEntity[] Entities;
    public List<string> Textures = new List<string>();
    public Vector3? Position;
    public float Scale;
    public Vector3 Rotation;
}