using ABC_Algorithm;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        var AbcAlgorithm = new ArtificialBeeColony();

        AbcAlgorithm.Start();

        app.MapGet("/", () => "Welcome to my AI Algorithm Solution");

        app.Run();

        
    }
}