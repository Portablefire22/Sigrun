using System.Numerics;

namespace Sigrun.Model.Entities;

public class PlayerStartEntity : RoomMeshEntity
{
    public string StartAngles { get; set; }
    
    public PlayerStartEntity(Vector3 position,string startAngles) : base(position)
    {
        StartAngles = startAngles;
    }
}