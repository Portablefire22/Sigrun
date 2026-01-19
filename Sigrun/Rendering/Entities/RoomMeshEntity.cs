using System.Numerics;

namespace Sigrun.Rendering.Entities;

public class RoomMeshEntity
{
   public Vector3 Position { get; set; }
   
   public RoomMeshEntity(Vector3 position)
   {
      Position = position;
   } 
}