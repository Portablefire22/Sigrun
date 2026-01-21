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
      var player = new Player.Player();
      SpawnObject(player);
      
      var obj1 = GameObject.FromModelFile("Assets/Models/4tunnels.rmesh", "4tunnel");
      var obj2 = GameObject.FromModelFile("Assets/Models/173.rmesh", "173");
      obj2.Scale = 0.005f;
      obj1.Scale = 0.0005f;
      obj2.Position -= Vector3.UnitY * 25f;
      
      var rigidbody = new Rigidbody(obj2) {Collider = new BoxCollider(obj2) };
      obj2.Components.Add(rigidbody);
      
      SpawnObject(obj1);
      SpawnObject(obj2);
   }
}