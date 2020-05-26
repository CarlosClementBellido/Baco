using Microsoft.AspNetCore.Hosting;

namespace ApiBaco
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IWebHost host = new WebHostBuilder()
                .UseUrls("http://192.168.1.4:5000")
                .UseKestrel()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
