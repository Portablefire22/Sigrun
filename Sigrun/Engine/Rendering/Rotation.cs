using System.Numerics;
using System.Runtime.CompilerServices;
using Sigrun.Engine.Maths;

namespace Sigrun.Engine.Rendering;

public class Rotation
{
    public Quaternion Quaternion { get; private set; } = new Quaternion(0,0,0,1);

    public double Yaw => Math.Atan2(2.0 * (Quaternion.Y * Quaternion.Z + Quaternion.W * Quaternion.X),
        Quaternion.W * Quaternion.W - Quaternion.X * Quaternion.X - Quaternion.Y * Quaternion.Y +
        Quaternion.Z * Quaternion.Z);

    public double Pitch => Math.Asin(-2.0 * (Quaternion.X * Quaternion.Z - Quaternion.W * Quaternion.Y));

    public double Roll => Math.Atan2(2.0 * (Quaternion.X * Quaternion.Y + Quaternion.W * Quaternion.Z),
        Quaternion.W * Quaternion.W + Quaternion.X * Quaternion.X - Quaternion.Y * Quaternion.Y -
        Quaternion.Z * Quaternion.Z);

    public Rotation(Vector3 angles) : this(angles.X, angles.Y, angles.Z) {}

    public Rotation() { }

    public Rotation(float x, float y, float z)
    {
        Quaternion = Quaternion.CreateFromYawPitchRoll(
            MathsUtility.ToRadians(y), 
            MathsUtility.ToRadians(x), 
            MathsUtility.ToRadians(z));
    }

    public Rotation(Quaternion quaternion)
    {
        Quaternion = quaternion;
    }
    
    public static Rotation From(Vector3 angles)
    {
        return new Rotation(angles);
    }

    public static Rotation From(float x, float y, float z)
    {
        return From(new Vector3(x, y, z));
    }

    public static Rotation operator *(Rotation r1, Rotation r2)
    {
        return new Rotation(r1.Quaternion * r2.Quaternion);
    }
   
    public static Rotation operator +(Rotation r1, Rotation r2) 
    {
        return new Rotation(r1.Quaternion * r2.Quaternion);
    }
}