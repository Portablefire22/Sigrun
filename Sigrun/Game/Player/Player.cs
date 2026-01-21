using System.Numerics;
using Sigrun.Engine.Entity;
using Sigrun.Engine.Rendering;
using Sigrun.Game.Player.Components;

namespace Sigrun.Game.Player;

public class Player : GameObject
{
    public CameraComponent CameraComponent { get; set; }
    public PlayerController PlayerController { get; set; }
    
    private float _cameraOffset = 1.75f;
    
    public Player() : this(new Vector3(0), new Vector3(0)) { }
    
    public Player(Vector3 position, Vector3 rotation)
    {
        Position = position;
        Rotation = new Rotation(rotation);

        
        CameraComponent = new CameraComponent(this, _cameraOffset);
        PlayerController = new PlayerController(this, CameraComponent);
        Components.AddRange(CameraComponent, PlayerController);
    }

}