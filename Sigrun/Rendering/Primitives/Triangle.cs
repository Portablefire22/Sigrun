using System.Numerics;
using System.Runtime.InteropServices.ComTypes;

namespace Sigrun.Rendering.Primitives;

public class Triangle
{
    public Vector3[] Points = new Vector3[3];
    public Vector3 Normal
    {
        get
        {
            var a = Points[0];
            var b = Points[0];
            var c = Points[0];

            // var aB = new Vector3()
            // {
            //     X = a.Y * b.Z - a.Z * b.Y,
            //     Y = a.Z * b.X - a.X * b.Z,
            //     Z = a.X * b.Y - a.Y * b.X
            // }; 
            // var aC = new Vector3()
            // {
            //     X = a.Y * c.Z - a.Z * c.Y,
            //     Y = a.Z * c.X - a.X * c.Z,
            //     Z = a.X * c.Y - a.Y * c.X
            // };

            return Vector3.Normalize(Vector3.Cross(b - a, c - a));
        }
    }
}