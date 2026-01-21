using System.Numerics;

namespace Sigrun.Engine.Rendering.Entities;

public class ScreenEntity : RoomMeshEntity
{
    public string ImagePath { get; set; }
    
    public ScreenEntity(Vector3 position, string imagePath) : base(position)
    {
        ImagePath = imagePath;
    }
}