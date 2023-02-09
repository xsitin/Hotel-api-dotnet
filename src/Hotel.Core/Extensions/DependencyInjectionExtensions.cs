using Hotel.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel.Core.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddCoreComponents(this IServiceCollection services)
        {
            //services.AddTransient<IEmployeeRepository, EmployeeRepository>();
           // services.AddScoped<ICarService, CarService>();
           services.AddScoped<IRoomService, RoomService>();
           services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}
