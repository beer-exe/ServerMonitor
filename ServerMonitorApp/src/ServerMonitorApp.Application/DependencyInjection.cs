using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServerMonitorApp.Application.Behaviours;
using System.Reflection;

namespace ServerMonitorApp.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.AddMediatR(options => {
                options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                options.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            });

            return services;
        }
    }
}
