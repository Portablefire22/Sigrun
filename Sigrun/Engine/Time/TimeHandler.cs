namespace Sigrun.Engine.Time;

public static class TimeHandler
{
    
    static DateTime time1 = DateTime.Now;
    static DateTime time2 = DateTime.Now;

    private static DateTime lastFrameTime = DateTime.Now;
    
    public static float FramesPerSecond { get; private set; }
    public static double FrameTime { get; private set; }

    private static uint _frameCount;
    public static float DeltaTime { get; private set; }
    
    public static float FixedUpdateMillis { get; private set; }
    
    public static void UpdateDeltaTime()
    {
        time2 = DateTime.Now;
        DeltaTime = (time2.Ticks - time1.Ticks) / 10000000f;
        time1 = time2;
        _frameCount++;
        if ((time2 - lastFrameTime).TotalSeconds >= 1.0)
        {
            lastFrameTime = DateTime.Now;
            FramesPerSecond = _frameCount;
            FrameTime = 1000.0 / _frameCount;
            _frameCount = 0;
        }
    }
}