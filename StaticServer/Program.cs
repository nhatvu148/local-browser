namespace StaticServer
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Reflection;

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    int port = 3089;
                    bool isPortInUse = true;
                    while (isPortInUse)
                    {

                        isPortInUse = IsPortOpen("localhost", port, TimeSpan.FromMilliseconds(500));
                        if (isPortInUse)
                        {
                            Console.WriteLine($"Port {port++} is being used by another process");
                        }
                        else
                        {
                            Console.WriteLine($"Port {port} is available");
                        }
                    }

                    webBuilder
                    .UseStartup<Startup>()
                    .UseUrls($"http://localhost:{port}/");

                    string pathDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    File.WriteAllText($"{pathDirectory}/../Logs.txt", port.ToString());
                });

        public static bool IsPortOpen(string host, int port, TimeSpan timeout)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(timeout);
                    client.EndConnect(result);
                    return success;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
