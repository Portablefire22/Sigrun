using System.Numerics;
using Sigrun.Engine.Entity;
using Sigrun.Engine.Entity.Components.Physics;
using Sigrun.Engine.Entity.Components.Physics.Colliders;
using Sigrun.Engine.Scenes;

namespace Sigrun.Game.Scenes;

public class RoomScene : Scene
{

   public RoomScene()
   {
      var obj1 = GameObject.FromRMeshFile("Assets/Models/4tunnels.rmesh", "4tunnel");
      var obj2 = GameObject.FromRMeshFile("Assets/Models/173.rmesh", "173");
      obj2.Scale = new Vector3(0.005f);
      obj1.Scale = new Vector3(0.005f);
      var rotator = new Rotator(obj1);
      obj1.Components.Add(rotator);
      var rotator2 = new Rotator(obj2);
      obj2.Components.Add(rotator2);
      obj2.Position -= Vector3.UnitY * 25f;
      
      var rigidbody = new Rigidbody(obj2) {Collider = new BoxCollider(obj2) };
      obj2.Components.Add(rigidbody);
      
      SpawnObject(obj1);
      SpawnObject(obj2);
   }
}