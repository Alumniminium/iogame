namespace iogame
{
    public class Program
    {

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel((o) => {
                    o.ListenAnyIP(5000);
                })
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}