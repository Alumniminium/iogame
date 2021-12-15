using System.Net;

namespace server
{
    public class Program
    {
        public static void Main()
        {
            var host = new WebHostBuilder()
                .UseKestrel((o) => {
                    o.Listen(IPAddress.Parse("0.0.0.0"),5000);
                })
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}