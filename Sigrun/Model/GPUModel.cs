using System.Numerics;

namespace Sigrun.Model;

public struct GPUModel
{
    public Matrix4x4 ModelMatrix;
    public Vector4 ModelColour;
    
    public const int SizeInBytes = 160;
}