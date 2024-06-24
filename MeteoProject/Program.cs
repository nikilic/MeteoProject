using Projekat3;

internal class Program
{
    private static readonly string _urlServer="http://localhost:5000/";
    private static async Task Main(string[] args)
    {
        WebServer server = new WebServer(_urlServer);
        await server.Run();
        Console.ReadKey();
    }
}