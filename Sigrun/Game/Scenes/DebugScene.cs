using System.Numerics;
using Sigrun.Engine.Entity;
using Sigrun.Engine.Entity.Components;
using Sigrun.Engine.Entity.Components.Physics;
using Sigrun.Engine.Entity.Components.Physics.Colliders;
using Sigrun.Engine.Rendering;
using Sigrun.Engine.Rendering.Primitives;
using Sigrun.Engine.Scenes;

namespace Sigrun.Game.Scenes;

public class DebugScene : Scene
{
    public DebugScene()
    {
        var player = new Player.Player();
        SpawnObject(player);

        // var obj1 = GameObject.FromModelFile("Assets/Models/4tunnels.rmesh", "4tunnel");
        // var obj2 = GameObject.FromModelFile("Assets/Models/173.rmesh", "173");
        // obj2.Scale = 0.005f;
        // obj1.Scale = 0.0005f;
        // obj2.Position -= Vector3.UnitY * 25f;
        //
        // var rigidbody = new Rigidbody(obj2) {Collider = new BoxCollider(obj2) };
        // obj2.Components.Add(rigidbody);
        
        var obj2 = new GameObject();
        
        var mod = new Model() { Meshes = [new CubeMesh(new Vector3(2))] };
        var rigidbody = new Rigidbody(obj2) {Collider = new BoxCollider(obj2) };
        var renderer = new Renderer(obj2, mod);
        obj2.Components.Add(renderer);
        obj2.Components.Add(rigidbody);
        obj2.Position += new Vector3(3, 0, 0);

        var obj3 = new GameObject();
        
        var mod2 = new Model() { Meshes = [new CubeMesh(new Vector3(2))] };
        var rigidbody2 = new Rigidbody(obj2) {Collider = new BoxCollider(obj3) };
        var renderer2 = new Renderer(obj3, mod2);
        obj3.Components.Add(renderer2);
        obj3.Components.Add(rigidbody2);

        rigidbody.Collider.Intersects(rigidbody2.Collider);
        
        SpawnObject(obj2);
        SpawnObject(obj3);
    }
}