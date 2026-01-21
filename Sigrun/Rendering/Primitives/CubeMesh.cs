using System.Numerics;

namespace Sigrun.Rendering.Primitives;

public class CubeMesh : Mesh
{
    public CubeMesh(Vector3 dimensions)
    {
        var hX = dimensions.X / 2;
        var hY = dimensions.Y / 2;
        var hZ = dimensions.Z / 2;
        Vertices = new[]
        {
            new MeshVertex()
            {
                Position = new Vector3(hX, -hY, -hZ),
                Uv = new Vector2(1,0),
                Normal = new Vector3(1,0,0)
            },
            new MeshVertex()
            {
                Position = new Vector3(hX, -hY, hZ),
                Uv = new Vector2(1,1),
                Normal = new Vector3(1,0,1)
            },
            new MeshVertex()
            {
                Position = new Vector3(-hX,-hY, hZ),
                Uv = new Vector2(0,1),
                Normal = new Vector3(0,0,1)
            },
            new MeshVertex()
            {
                Position = new Vector3(-hX,-hY, -hZ),
                Uv = new Vector2(0,0),
                Normal = new Vector3(0,0,0)
            }, 
            new MeshVertex()
            {
                Position = new Vector3(hX,hY, -hZ),
                Uv = new Vector2(1,1),
                Normal = new Vector3(1,1,0)
            },  
            new MeshVertex()
            {
                Position = new Vector3(hX,hY,hZ),
                Uv = new Vector2(1,1),
                Normal = new Vector3(1,1,1)
            }, 
            new MeshVertex()
            {
                Position = new Vector3(-hX,hY, hZ),
                Uv = new Vector2(0,1),
                Normal = new Vector3(0,0,1)
            }, 
            new MeshVertex()
            {
                Position = new Vector3(-hX,hY, -hZ),
                Uv = new Vector2(0,1),
                Normal = new Vector3(0,1,0)
            },
        };
    
        Indices = new ushort[]
        {  1, 0, 3,
            1, 3, 2,
           5, 1, 2, 
           5, 2, 6,
           4, 5, 6, 
           4, 6, 7,
           0, 4, 7, 
           0, 7, 3,
           2, 3, 7, 
           2, 7, 6,
           5, 4, 0,
           5, 0, 1
        };
        Texture = "missingTexture.jpg";
    }
}