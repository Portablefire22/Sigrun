using System.Numerics;
using Microsoft.Extensions.Logging;
using Sigrun.Engine.Logging;
using Sigrun.Engine.Rendering;
using Sigrun.Engine.Rendering.Primitives;

namespace Sigrun.Engine.Entity.Components.Physics.Colliders;

public abstract class Collider : Component
{
    public List<Collider> Touching { get; protected set; }
    
    public Mesh Mesh { get; protected set; }

    public MeshVertex[] Vertices => Mesh.Vertices;
    
    public bool IsTouching => Touching.Count > 0;
    
    public Triangle[] Triangles { get; protected set; }
    
    protected Collider(GameObject parent, Mesh mesh) : base(parent)
    {
        Touching = [];
        Mesh = mesh;
        
        Triangles = new Triangle[mesh.Indices.Length / 3];
        int j = 0;
        for (int i = 0; i < mesh.Indices.Length; i += 3)
        {
            var i1 = mesh.Indices[i];
            var i2 = mesh.Indices[i + 1];
            var i3 = mesh.Indices[i + 2];

            var p1 = mesh.Vertices[i1].Position + Parent.Position;
            var p2 = mesh.Vertices[i2].Position + Parent.Position;
            var p3 = mesh.Vertices[i3].Position + Parent.Position;
            
            Triangles[j] = new Triangle()
            {
                Points = new []{p1,p2,p3}
            };
            j++;
        }
        Sigrun.AddCollider(this);
    }

    public Vector3 GetClosestPoint(Vector3 other)
    {
        var logger = LoggingProvider.NewLogger<BoxCollider>();
        var verts = Mesh.GetVerticesInWorld(Parent.Position);
        var closest = verts[0].Position;
        var dist = float.PositiveInfinity;
        for (int i = 1; i < verts.Length; i++)
        {
            var vert = verts[i].Position;
            var tmp = Math.Abs(Vector3.Distance(vert, other));
            if (tmp < dist)
            {
                logger.LogDebug($"{tmp} | {vert}");
                dist = tmp;
                closest = vert;
            }
        }
        return closest;
    }
    
    
    public abstract bool Intersects(Collider other);
}