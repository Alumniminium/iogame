using System.Net;

namespace iogame
{
    public class Program
    {

        public static void Main(string[] args)
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