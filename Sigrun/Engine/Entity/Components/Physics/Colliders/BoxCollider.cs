using System.Numerics;
using Microsoft.Extensions.Logging;
using Sigrun.Engine.Rendering;
using Sigrun.Engine.Rendering.Primitives;

namespace Sigrun.Engine.Entity.Components.Physics.Colliders;

public class BoxCollider : Collider
{
    
    public Vector3 Centre
    {
        get;
        set;
    }

    public Vector3 Dimensions
    {
        get;
        set;
    }


    public BoxCollider(GameObject parent, Mesh mesh, Vector3 dimensions) : base(parent, mesh)
    {
        Centre = parent.Position;
        Dimensions = dimensions;
    }

    public BoxCollider(GameObject parent) : this(parent, new CubeMesh(new Vector3(1)), new Vector3(1)) { }

    public override bool Intersects(Collider other)
    {
        switch (other)
        {
            case BoxCollider boxCollider:
                return Intersects(boxCollider);
        }

        return false;
    }

    public bool Intersects(BoxCollider other)
    {
        
        return false;
    }
}