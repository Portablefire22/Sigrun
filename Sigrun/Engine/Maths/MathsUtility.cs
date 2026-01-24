namespace Sigrun.Engine.Maths;

public static class MathsUtility
{
    public static bool AlmostEqual(float val1, float val2, float tolerance)
    {
        var diff = Math.Abs(val1 - val2);
        return (diff <= tolerance);
    }
    
    public static double ToRadians(double val)
    {
        return (Math.PI / 180) * val;
    }
    public static float ToRadians(float val)
    {
        return (float)((Math.PI / 180) * val);
    }
}