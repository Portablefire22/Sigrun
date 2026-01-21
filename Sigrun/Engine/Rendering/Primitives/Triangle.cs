using System.Numerics;

namespace Sigrun.Engine.Rendering.Primitives;

public class Triangle
{
    public Vector3[] Points = new Vector3[3];
    public Vector3 Normal
    {
        get
        {
            var a = Points[0];
            var b = Points[1];
            var c = Points[2];
            return Vector3.Normalize(Vector3.Cross(b - a, c - a));
        }
    }
}