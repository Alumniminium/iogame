using System;
using System.Net;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace server;

public sealed class Program
{
    public static void Main()
    {
        // hook up global unhandled exception handler
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            Console.WriteLine("Unhandled exception: " + e.ExceptionObject);
            Thread.Sleep(Timeout.Infinite);
        };



        var host = new WebHostBuilder()
            .UseKestrel((o) =>
            {
                o.Listen(IPAddress.Parse("0.0.0.0"), 5000, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
                });
            })
            .UseStartup<Startup>()
            .Build();

        host.Run();
    }
}