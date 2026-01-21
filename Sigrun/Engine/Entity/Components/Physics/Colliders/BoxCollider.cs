using System.Numerics;
using Microsoft.Extensions.Logging;
using Sigrun.Logging;
using Sigrun.Rendering;
using Sigrun.Rendering.Primitives;

namespace Sigrun.Engine.Entity.Components.Physics.Colliders;

public class BoxCollider : Collider
{
    
    public Vector3 Centre
    {
        get;
        set
        {
            field = value;
            DimensionsChanged();
        }
    }

    public Vector3 Dimensions
    {
        get;
        set
        {
            field = value;
            DimensionsChanged();
        }
    }

    private Vector3 _pointOne;
    private Vector3 _pointTwo;

    public BoxCollider(GameObject parent, Mesh mesh, Vector3 dimensions) : base(parent, mesh)
    {
        Centre = parent.Position;
        Dimensions = dimensions;
    }

    public BoxCollider(GameObject parent) : this(parent, new CubeMesh(new Vector3(1)), new Vector3(1)) { }

    private void DimensionsChanged()
    {
        _pointOne = Centre + Dimensions / 2;
        _pointTwo = Centre - Dimensions / 2;
    }

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
        var logger = LoggingProvider.NewLogger<BoxCollider>();
        
        logger.LogError($"{Triangles[2].Normal}");
        
        
        return false;
    }
}