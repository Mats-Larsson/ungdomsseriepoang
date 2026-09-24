namespace Usp;

public static class ServiceCollectionExtensions
{
    public static void AddUspServices(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddSingleton<Data.ResultService>();
        services.AddSingleton<Endpoints>();
    }
}
