namespace Sigrun.Game.Blitz;

public static class BlitzRng
{
    private static int RndA = 48271;
    private static int RndM = 214783647;
    private static int RndQ = 44488;
    private static int RndR = 3399;

    public static int RndState { get; private set; }

    public static void BlitzSeedRnd( int seed )
    {
        seed &= 0x7fffffff;
        RndState = (seed != 0) ? seed : 1;
    }

    public static int BlitzRand( int from, int to )
    {
        if (to < from)
        {
            int a = to;
            from = to;
            to = a;
        }
        var x = (int)(Random() * (to - from + 1)) + from;
        return x;
    }

    public static double BlitzRand( float min, float max )
    {
        return Random() * (max - min) + min;
    }

    private static double Random()
    {
        RndState = RndA * (RndState % RndQ) - RndR * (RndState / RndQ);
        if ( RndState < 0 ) RndState += RndM;
        var d = (double)(RndState & 65535) / 65536.0f + (0.5f / 65536.0f);
        return d;
    }
	
	
    public static int generateSeedNumber(char[] seed) {
        int tmp = 0;
        int shift = 0;
        foreach (char c in seed) {
            tmp = tmp ^ (c << shift);
            shift = (shift + 1) % 24;
        }
        return tmp;
    }	 
}