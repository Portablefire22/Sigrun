using Sigrun.Engine.Entity.Components.Physics.Colliders;

namespace Sigrun.Engine.Entity.Components.Physics;

public class Rigidbody : Component
{
    public ICollider Collider;
    
    public Rigidbody(GameObject parent) : base(parent)
    {
    }
}