using System.Numerics;

namespace Sigrun.Engine.Rendering.Entities;

public class LightEntity : RoomMeshEntity
{
    public float Range { get; set; }
    public string Color { get; set; }
    public float Intensity { get; set; }
    public LightEntity(Vector3 position, float range, string color, float intensity) : base(position)
    {
        Range = range;
        Color = color;
        Intensity = intensity;
    } 
}