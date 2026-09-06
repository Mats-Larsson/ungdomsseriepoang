using Microsoft.Extensions.DependencyInjection;

namespace Usp;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUspServices(this IServiceCollection services)
    {
        services.AddSingleton<Data.ResultService>();
        services.AddSingleton<Endpoints>();

        return services;
    }
}
