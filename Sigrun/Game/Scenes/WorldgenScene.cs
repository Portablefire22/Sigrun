using Sigrun.Engine.Entity;
using Sigrun.Engine.Scenes;
using Sigrun.Game.World;

namespace Sigrun.Game.Scenes;

public class WorldgenScene : Scene
{
    public WorldgenScene()
    {
        var empty = new GameObject();
        var gen = new MapGenerator(empty);
        empty.Components.Add(gen);
        // SpawnObject(empty);
    }
}