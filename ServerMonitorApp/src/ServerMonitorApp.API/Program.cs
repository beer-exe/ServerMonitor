using ServerMonitorApp.Infrastructure.Persistence;
using ServerMonitorApp.Application.Common.Interfaces;

namespace ServerMonitorApp.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IHost? host = CreateHostBuilder(args).Build();

            if (args.Length > 0 && args[0].ToLower() == "/seed")
            {
                using (IServiceScope? scope = host.Services.CreateScope())
                {
                    IServiceProvider? services = scope.ServiceProvider;
                    ILogger<Program>? logger = services.GetRequiredService<ILogger<Program>>();

                    try
                    {
                        ApplicationDbContext? context = services.GetRequiredService<ApplicationDbContext>();
                        IPasswordHasher? passwordHasher = services.GetRequiredService<IPasswordHasher>();

                        await ApplicationDbContextSeed.SeedSampleDataAsync(context, passwordHasher, logger);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Đã xảy ra lỗi trong quá trình Seeding Database.");
                    }
                }

                Console.WriteLine("Quá trình Seed Data đã hoàn tất. Ứng dụng sẽ thoát.");
                return;
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }

    }
}