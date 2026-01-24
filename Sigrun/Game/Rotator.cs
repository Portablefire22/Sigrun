using Sigrun.Engine.Entity;
using Sigrun.Engine.Entity.Components;
using Sigrun.Engine.Maths;
using Sigrun.Engine.Rendering;
using Sigrun.Engine.Time;

namespace Sigrun.Game;

public class Rotator : Component
{
    public Rotator(GameObject parent) : base(parent)
    {
    }

    public override void FixedUpdate()
    {
        Parent.Rotation += new Rotation(MathsUtility.ToRadians(0) * TimeHandler.DeltaTime*100, 
        MathsUtility.ToRadians(500) * TimeHandler.DeltaTime * 100, 
        MathsUtility.ToRadians(0) * TimeHandler.DeltaTime * 100);
    }
    
}