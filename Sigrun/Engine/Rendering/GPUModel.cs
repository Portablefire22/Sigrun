using System.Numerics;

namespace Sigrun.Engine.Rendering;

public struct GPUModel
{
    public Matrix4x4 ModelMatrix;
    
    public const int SizeInBytes = 160;
}