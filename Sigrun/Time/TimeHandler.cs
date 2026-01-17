namespace Sigrun.Time;

public static class TimeHandler
{
    
    static DateTime time1 = DateTime.Now;
    static DateTime time2 = DateTime.Now;

    public static float DeltaTime { get; private set; }
    
    public static void UpdateDeltaTime()
    {
        time2 = DateTime.Now;
        DeltaTime = (time2.Ticks - time1.Ticks) / 10000000f;
        time1 = time2;
    }
}