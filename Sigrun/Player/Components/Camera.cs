using System.Numerics;
using Microsoft.Extensions.Logging;
using Sigrun.Engine.Entity;
using Sigrun.Engine.Entity.Components;
using Sigrun.Logging;

namespace Sigrun.Player.Components;

public class Camera : Component
{
    public Vector3 Position => Parent.Position + Vector3.UnitY * Offset;
    public Vector3 Target;
    public float Offset;
    
    public Camera(GameObject parent, float offset, Vector3 target) : base(parent)
    {
        Offset = offset;
        Target = target;

        WorldUp = new Vector3(0, 1,0);
        
        
        _logger = LoggingProvider.NewLogger<Camera>();
    }



    public float Yaw { get; set; } = -90f;
    public float Pitch { get; set; } = 0;


    private ILogger _logger;

    
    public Vector3 Front { get; private set; }
    public Vector3 Up { get; private set; }
    public Vector3 Right { get; private set; }
    public Vector3 WorldUp { get; private set; }
    
    public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(Position, Position + Front, Up);
    public Matrix4x4 GetProjectionMatrix(float aspectRatio)
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(
                1, 
                aspectRatio,
                0.001f,
                1000000000f);
    }

    public void UpdateCameraVectors()
    {
        var front = new Vector3()
        {
            X = (float)(Math.Cos(ToRadians(Yaw)) * Math.Cos(ToRadians(Pitch))),
            Y = (float)Math.Sin(ToRadians(Pitch)),
            Z = (float)(Math.Sin(ToRadians(Yaw)) * Math.Cos(ToRadians(Pitch))),
        };
        Front = Vector3.Normalize(front);

        Right = Vector3.Normalize(Vector3.Cross(Front, WorldUp));
        Up = Vector3.Normalize(Vector3.Cross(Right, Front));
    }

    private double ToRadians(float val)
    {
        return (Math.PI / 180) * val;
    }
}

enum Movement  
{
    FORWARD,
    BACKWARD,
    LEFT,
    RIGHT
}