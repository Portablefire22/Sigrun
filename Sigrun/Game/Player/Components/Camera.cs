using System.Numerics;
using Sigrun.Engine.Entity;
using Sigrun.Engine.Entity.Components;

namespace Sigrun.Game.Player.Components;

public class Camera : Component
{
    public Vector3 Position => Parent.Position + Vector3.UnitY * Offset;
    public float Offset;
    
    public Camera(GameObject parent, float offset) : base(parent)
    {
        Offset = offset;
        WorldUp = new Vector3(0, 1,0);
    }

    public float Yaw { get; set; } = -90f;
    public float Pitch { get; set; } = 0;

    public float ZFar { get; set; } = 1000f;
    public float ZNear { get; set; } = 0.1f;
    public float Fov { get; set; } = 100;
    
    public Vector3 Front { get; private set; }
    public Vector3 Up { get; private set; }
    public Vector3 Right { get; private set; }
    public Vector3 WorldUp { get; }
    
    public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(Position, Position + Front, Up);
    public Matrix4x4 GetProjectionMatrix(float aspectRatio)
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(
                (float)ToRadians(Fov), 
                aspectRatio,
                ZNear,
                ZFar);
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