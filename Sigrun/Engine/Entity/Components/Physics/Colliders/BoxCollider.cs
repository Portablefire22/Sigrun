using System.Numerics;

namespace Sigrun.Engine.Entity.Components.Physics.Colliders;

public class BoxCollider : Component, ICollider
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
    
    
    public BoxCollider(GameObject parent) : base(parent)
    {
        Centre = parent.Position;
        Dimensions = new Vector3(1);
    }

    private void DimensionsChanged()
    {
        _pointOne = Centre + Dimensions / 2;
        _pointTwo = Centre - Dimensions / 2;
    }
}