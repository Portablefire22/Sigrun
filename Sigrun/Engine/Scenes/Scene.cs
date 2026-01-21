using Sigrun.Engine.Entity;

namespace Sigrun.Engine.Scenes;

public class Scene
{
   public List<GameObject> Objects { get; private set; } = [];

   public Scene() { }
   
   public Scene(List<GameObject> objects)
   {
      Objects = objects;
   }

   public void SpawnObject(GameObject gameObject)
   {
      Objects.Add(gameObject);
   }
}