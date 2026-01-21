using System.Numerics;

namespace Sigrun.Engine.Rendering;

public class Mesh
{
   public MeshVertex[] Vertices;
   public ushort[] Indices;
   public string Texture = "";
   public string Name = "";
   public bool Alpha;

   public MeshVertex[] GetVerticesInWorld(Vector3 origin)
   {
      var tmp = new MeshVertex[Vertices.Length];
      Array.Copy(Vertices, tmp, Vertices.Length);
      foreach (var vert in tmp)
      {
         vert.Position += origin;
      }
      return tmp;
   }
}