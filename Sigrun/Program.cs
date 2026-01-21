using Sigrun.Game.Scenes;

namespace Sigrun;

public class Program
{
    public static void Main(string[] args)
    {
        var s = new DebugScene();
        var r = new RoomScene();
        
        Engine.Sigrun.AddScene(s);
        Engine.Sigrun.AddScene(r);
        
        Engine.Sigrun.Start();
        Engine.Sigrun.Dispose();
    }
}