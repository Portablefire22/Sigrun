using System.Numerics;

namespace Sigrun.Engine.Rendering.Entities;

public class SpotlightEntity : LightEntity
{
    public string Angles { get; set; }
    public int InnerConeAngle { get; set; }
    public int OuterConeAngle { get; set; }
    
    
    public SpotlightEntity(Vector3 position, float range, string color, float intensity, string angles, int innerConeAngle, int outerConeAngle) : base(position, range, color, intensity)
    {
        Angles = angles;
        InnerConeAngle = innerConeAngle;
        OuterConeAngle = outerConeAngle;
    }
}