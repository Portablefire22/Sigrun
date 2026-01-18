using System.Numerics;

namespace Sigrun.Model.Entities;

public class ModelEntity : RoomMeshEntity
{
    public string Name { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Scale { get; set; }
    public Mesh Mesh { get; set; }
    public ModelEntity(string name, Vector3 position, Vector3 rotation, Vector3 scale) : base(position)
    {
        Name = name;
        Rotation = rotation;
        Scale = scale;
    } 
}