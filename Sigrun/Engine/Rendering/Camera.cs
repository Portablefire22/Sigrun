using System.Numerics;

namespace Sigrun.Engine.Rendering;

public interface ICamera
{
    public Vector3 Position { get; set; } 
    public float Yaw { get; set; }
    public float Pitch { get; set; }

    public float ZFar { get; set; }
    public float ZNear { get; set; } 
    public float Fov { get; set; }
    
    public Vector3 Front { get; protected set; }
    public Vector3 Up { get; protected set; }
    public Vector3 Right { get; protected set; }
    public Vector3 WorldUp { get; }
    
    public Matrix4x4 ViewMatrix { get; }
    public Matrix4x4 GetProjectionMatrix(float aspectRatio);
}