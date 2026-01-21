using Sigrun.Engine.Entity;
using Sigrun.Game.Player;

namespace Sigrun.Engine.Scenes;

public class Scene
{
   public List<GameObject> Objects { get; private set; } = [];

   public Scene()
   {
      var player = new Player();
      SpawnObject(player);
   }
   
   public Scene(List<GameObject> objects)
   {
      Objects = objects;
   }

   public void SpawnObject(GameObject gameObject)
   {
      Objects.Add(gameObject);
   }
}