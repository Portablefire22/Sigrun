using Sigrun.Game.Scenes;

namespace Sigrun;

public class Program
{
    public static void Main(string[] args)
    {
        var s = new DebugScene();
        var r = new RoomScene();
        var wg = new WorldgenScene();
        
        Engine.Sigrun.AddScenes(r,s); 
        
        Engine.Sigrun.Start();
        Engine.Sigrun.Dispose();
    }
}