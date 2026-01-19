using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace Sigrun.Engine;

public static class InputState
{
    public static List<Key> PressedKeys { get; private set; } = new ();

    public static Vector2 MousePosition
    {
        get; private set; 
    }

    public static Vector2 Delta { get; set; }

    public static void OnMouseMove(MouseMoveEventArgs obj)
    {
        MousePosition = obj.MousePosition;
    }
    
    public static void OnKeyDown(KeyEvent keyEvent)
    {
        var key = keyEvent.Key;
        if (!PressedKeys.Contains(key))
        {
            PressedKeys.Add(key);
        }
    }

    public static void OnKeyUp(KeyEvent keyEvent)
    {
        PressedKeys.Remove(keyEvent.Key);
    }
}