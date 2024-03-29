using Microsoft.AspNetCore;

namespace Project_OdataToEntity
{
    class Program
    {
        static void Main(String[] args)
        {
            var host = WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .Build();

            host.Run();
        }
    }
}
