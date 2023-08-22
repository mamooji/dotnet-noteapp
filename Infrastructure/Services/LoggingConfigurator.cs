using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Infrastructure.Services;

public static class LoggingConfigurator
{
    public static LoggerConfiguration CreateLoggerConfiguration()
    {
        return new LoggerConfiguration();
    }

    public static void BootstrapLogger(LoggerConfiguration loggerConfiguration)
    {
        Log.Logger = loggerConfiguration.CreateBootstrapLogger();
    }

    public static LoggerConfiguration AddAllRequiredSinks(this LoggerConfiguration loggerConfiguration,
        bool useHumanReadableLogs = false)
    {
        var logPath = Path.Combine(".", "logs", "stdout.log");
        var jsonFormatter = new CompactJsonFormatter();

        var config = loggerConfiguration.Enrich.FromLogContext()
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .WriteTo.File(jsonFormatter, logPath, rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7);

        if (useHumanReadableLogs)
            config.WriteTo.Console();
        else
            config.WriteTo.Console(jsonFormatter);

        return loggerConfiguration;
    }
}