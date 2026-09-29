using Microsoft.Extensions.DependencyInjection;
using ResultCalc.Contract;
using ResultCalc.Eventor;
using ResultCalc.IofXml;
using ResultCalc.Liveresultat;
using ResultCalc.Meos;
using ResultCalc.Model;
using ResultCalc.Ola;
using ResultCalc.Simulator;

namespace ResultCalc;

public static class ServiceCollectionExtensions
{
    public static void AddResultsServices(this IServiceCollection services, Configuration configuration, Source source)
    {
        services.AddSingleton(configuration);
        services.AddSingleton<IResultService, ResultService>();
        if (source == Source.Simulator)
            services.AddSingleton<ITeamService, SimulatorTeamService>();
        else
            services.AddSingleton<ITeamService, TeamService>();

        services.AddSingleton<MeosResultSource>();
        services.AddSingleton<OlaResultSource>();
        services.AddSingleton<SimulatorResultSource>();
        services.AddSingleton<LiveresultatResultSource>();
        services.AddSingleton<IofXmlResultSource>();
        services.AddSingleton<EventorResultSource>();

        services.AddSingleton<LiveresultatFacade>();
        services.AddSingleton<IEventorFacade, EventorFacade>();
        services.AddSingleton<IIofXmlDeserializer, IofXmlDeserializer>();

        services.AddSingleton<ClassFilter>();
        services.AddSingleton<FileListener>();

        services.AddSingleton<IResultSource>(provider => source switch
        {
            Source.Simulator => provider.GetRequiredService<SimulatorResultSource>(),
            Source.Meos => provider.GetRequiredService<MeosResultSource>(),
            Source.Ola => provider.GetRequiredService<OlaResultSource>(),
            Source.Liveresultat => provider.GetRequiredService<LiveresultatResultSource>(),
            Source.IofXml => provider.GetRequiredService<IofXmlResultSource>(),
            Source.Eventor => provider.GetRequiredService<EventorResultSource>(),
            _ => throw new InvalidOperationException()
        });
    }
}
