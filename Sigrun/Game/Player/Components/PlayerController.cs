using Sigrun.Engine;
using Sigrun.Engine.Entity;
using Sigrun.Engine.Entity.Components;
using Sigrun.Engine.Time;
using Veldrid;

namespace Sigrun.Game.Player.Components;

public class PlayerController : Component
{
    private readonly CameraComponent _cameraComponent;
    
    public float Speed { get; set; } = 25f;
    public float Sensitivity { get; set; } = 0.1f;
    
    
    public PlayerController(GameObject parent, CameraComponent cameraComponent) : base(parent)
    {
        _cameraComponent = cameraComponent;
    }

    public override void Startup()
    {
        Engine.Sigrun.SetMainCamera(_cameraComponent);
    }

    public override void Update()
    {
        ProcessMovement();
        ProcessMouse();
    }
    
    public void ProcessMouse()
    {
        var xOffset = InputState.Delta.X * Sensitivity;
        var yOffset = InputState.Delta.Y * -Sensitivity;

        _cameraComponent.Yaw += xOffset;
        _cameraComponent.Pitch += yOffset;

        _cameraComponent.Pitch = Math.Clamp(_cameraComponent.Pitch, -89f, 89f);

        _cameraComponent.UpdateCameraVectors();
    }

    public void ProcessMovement()
    {
        var velocity = Speed * TimeHandler.DeltaTime;
        foreach (var key in InputState.PressedKeys)
        {
            switch (key)
            {
                case Key.W:
                    Parent.Position += _cameraComponent.Front * velocity;
                    break;
                case Key.S:
                    Parent.Position -= _cameraComponent.Front * velocity;
                    break;
                case Key.D:
                    Parent.Position += _cameraComponent.Right * velocity;
                    break;
                case Key.A:
                    Parent.Position -= _cameraComponent.Right * velocity;
                    break;
                case Key.Space:
                    Parent.Position += _cameraComponent.Up * velocity;
                    break;
                case Key.ShiftLeft:
                case Key.ShiftRight:
                    Parent.Position -= _cameraComponent.Up * velocity;
                    break;
            }
        }
    }
}