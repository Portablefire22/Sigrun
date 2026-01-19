using System.Numerics;
using Sigrun.Engine.Entity;
using Sigrun.Engine.Entity.Components;
using Sigrun.Player.Components;

namespace Sigrun.Player;

public class Player : GameObject
{
    public Camera Camera { get; set; }
    public PlayerController PlayerController { get; set; }
    
    private float _cameraOffset = 1.75f;
    
    public Player() : this(new Vector3(0), new Vector3(0)) { }
    
    public Player(Vector3 position, Vector3 rotation)
    {
        Position = position;
        Rotation = rotation;

        
        Camera = new Camera(this, _cameraOffset, new Vector3(0));
        PlayerController = new PlayerController(this, Camera);
        Components.AddRange(Camera, PlayerController);
    }

}