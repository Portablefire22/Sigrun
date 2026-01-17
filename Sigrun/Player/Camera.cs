using System.Numerics;
using Microsoft.Extensions.Logging;
using Sigrun.Logging;
using Sigrun.Time;
using Veldrid;
using Veldrid.Sdl2;

namespace Sigrun.Player;

public class Camera
{
    public Vector3 Position;
    public Vector3 Target;

    public Camera(Vector3 position, Vector3 target)
    {
        Position = position;
        Target = target;

        _worldUp = new Vector3(0, 1,0);
        
        
        _logger = LoggingProvider.NewLogger<Camera>();
    }

    public float Yaw { get; set; } = -90f;
    public float Pitch { get; set; } = 0;
    public float Speed { get; set; } = 250f;
    public float Sensitivity { get; set; } = 0.1f;
    public float Zoom { get; set; } = 45f ;

    private ILogger _logger;

    private Vector2 _lastMouse;
    
    private Vector3 _front;
    private Vector3 _up;
    private Vector3 _right;
    private Vector3 _worldUp;
    
    public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(Position, Position + _front, _up);
    public Matrix4x4 GetProjectionMatrix(float aspectRatio)
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(
                1, 
                aspectRatio,
                0.001f,
                1000000000f);
    }

    private void UpdateCameraVectors()
    {
        var front = new Vector3()
        {
            X = (float)(Math.Cos(ToRadians(Yaw)) * Math.Cos(ToRadians(Pitch))),
            Y = (float)Math.Sin(ToRadians(Pitch)),
            Z = (float)(Math.Sin(ToRadians(Yaw)) * Math.Cos(ToRadians(Pitch))),
        };
        _front = Vector3.Normalize(front);

        _right = Vector3.Normalize(Vector3.Cross(_front, _worldUp));
        _up = Vector3.Normalize(Vector3.Cross(_right, _front));
    }

    private double ToRadians(float val)
    {
        return (Math.PI / 180) * val;
    }

    public void OnMouseMove(MouseMoveEventArgs obj)
    {
        var mouseMovement = obj.MousePosition - _lastMouse;
        _lastMouse = obj.MousePosition; 
        
        var xOffset = mouseMovement.X * Sensitivity;
        var yOffset = mouseMovement.Y * -Sensitivity;

        Yaw += xOffset;
        Pitch += yOffset;

        Pitch = Math.Clamp(Pitch, -89f, 89f);

        UpdateCameraVectors();
    }

    public void OnKeyDown(KeyEvent obj)
    {
        // throw new NotImplementedException();
        var velocity = Speed * TimeHandler.DeltaTime;
        switch (obj.Key)
        {
            case Key.W:
                Position += _front * velocity;
                break;
            case Key.S:
                Position -= _front * velocity;
                break;
            case Key.D:
                Position += _right * velocity;
                break;
            case Key.A:
                Position -= _right * velocity;
               break;
            case Key.Space :
                Position += _up * velocity;
                break;
            case Key.ShiftLeft:
            case Key.ShiftRight:
                Position -= _up * velocity;
                break;
        }
        _logger.LogDebug($"Pos: {Position.X} {Position.Y} {Position.Z}");
    }
}

enum Movement  
{
    FORWARD,
    BACKWARD,
    LEFT,
    RIGHT
}