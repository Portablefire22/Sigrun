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