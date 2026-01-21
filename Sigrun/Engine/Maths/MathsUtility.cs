namespace Sigrun.Engine.Maths;

public static class MathsUtility
{
    public static bool AlmostEqual(float val1, float val2, float tolerance)
    {
        var diff = Math.Abs(val1 - val2);
        return (diff <= tolerance);
    }
}