using System.Numerics;
using Sigrun.Engine.Entity.Components.Physics.Colliders;

namespace Sigrun.Engine.Entity.Components.Physics;

public class Rigidbody : Component
{
    public Collider Collider { get; set; }
    public bool Static { get; set; }
    public Vector3 Velocity { get; set; }

    
    
    public Rigidbody(GameObject parent) : base(parent) { }

    public override void FixedUpdate()
    {
        // Self-Movement
        if (Static) return;

        if (Collider.IsTouching) return;
        
        Parent.Position += Velocity;
    }

    public void AddForce(Vector3 force)
    {
        Velocity += force;
    }
}