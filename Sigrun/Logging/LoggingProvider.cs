using Microsoft.Extensions.Logging;

namespace Sigrun.Logging;

public class LoggingProvider
{
    private static ILoggerFactory _factory;
    
    static LoggingProvider()
    {
        _factory = LoggerFactory.Create(builder => builder
            #if DEBUG
            .AddFilter("Sigrun", LogLevel.Debug)
            #endif
            .AddConsole());
    }
    
    public static ILogger<T> NewLogger<T>()
    {
        return _factory.CreateLogger<T>();
    }
    
    public static ILogger NewLogger(string name)
    {
        return _factory.CreateLogger(name);
    } 
}