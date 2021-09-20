using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using iogame.Net;
using iogame.Simulation;
using iogame.Simulation.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

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
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}