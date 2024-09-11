using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace RealmAspNet
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //webBuilder.UseUrls("http://*:5000", "http://*:5001");
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}