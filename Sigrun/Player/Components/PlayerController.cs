using System.Numerics;
using Sigrun.Player.Components;
using Sigrun.Time;
using Veldrid;

namespace Sigrun.Engine.Entity.Components;

public class PlayerController : Component
{
    private readonly Camera _camera;
    
    public float Speed { get; set; } = 25f;
    public float Sensitivity { get; set; } = 0.1f;
    public float Zoom { get; set; } = 45f ;
    
    private Vector2 _lastMouse;
    
    public PlayerController(GameObject parent, Camera camera) : base(parent)
    {
        _camera = camera;
    }
    
    public override void Update()
    {
        ProcessMovement();
        ProcessMouse();
    }
    
    public void ProcessMouse()
    {
        var mousePosition = InputState.MousePosition;
        var mouseMovement = mousePosition - _lastMouse;
        _lastMouse = mousePosition; 
        
        var xOffset = mouseMovement.X * Sensitivity;
        var yOffset = mouseMovement.Y * -Sensitivity;

        _camera.Yaw += xOffset;
        _camera.Pitch += yOffset;

        _camera.Pitch = Math.Clamp(_camera.Pitch, -89f, 89f);

        _camera.UpdateCameraVectors();
    }

    public void ProcessMovement()
    {
        var velocity = Speed * TimeHandler.DeltaTime;
        foreach (var key in InputState.PressedKeys)
        {
            switch (key)
            {
                case Key.W:
                    Parent.Position += _camera.Front * velocity;
                    break;
                case Key.S:
                    Parent.Position -= _camera.Front * velocity;
                    break;
                case Key.D:
                    Parent.Position += _camera.Right * velocity;
                    break;
                case Key.A:
                    Parent.Position -= _camera.Right * velocity;
                    break;
                case Key.Space:
                    Parent.Position += _camera.Up * velocity;
                    break;
                case Key.ShiftLeft:
                case Key.ShiftRight:
                    Parent.Position -= _camera.Up * velocity;
                    break;
            }
        }
    }
}