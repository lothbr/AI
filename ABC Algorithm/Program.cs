using ABC_Algorithm;
using Serilog;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Log.Logger = new LoggerConfiguration()
       .Enrich.FromLogContext()
       .MinimumLevel.Information()
       .WriteTo.File($"{builder.Configuration.GetSection("SerilogPath:logPath").Value}\\ArtificialBeeLogs.txt", shared: true, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, fileSizeLimitBytes: 60 * 1024 * 1024)
       .CreateLogger();

        var app = builder.Build();

        var AbcAlgorithm = new ArtificialBeeColony();

        AbcAlgorithm.Start();

        app.MapGet("/", () => "Welcome to my AI Algorithm Solution");

        app.Run();

        
    }
}