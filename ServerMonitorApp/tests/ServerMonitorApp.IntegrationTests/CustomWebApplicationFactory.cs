using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServerMonitorApp.API;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                ServiceDescriptor? descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IntegrationTestDb");
                });

                ServiceProvider? sp = services.BuildServiceProvider();
                using (IServiceScope? scope = sp.CreateScope())
                {
                    IServiceProvider? scopedServices = scope.ServiceProvider;
                    ApplicationDbContext? db = scopedServices.GetRequiredService<ApplicationDbContext>();

                    db.Database.EnsureCreated();

                    if (!db.Users.Any(u => u.Username == "integrationuser"))
                    {
                        db.Users.Add(new User
                        {
                            Id = Guid.NewGuid(),
                            Username = "integrationuser",
                            Email = "integration@test.com",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                            Role = "Admin"
                        });
                        db.SaveChanges();
                    }
                }
            });
        }
    }
}