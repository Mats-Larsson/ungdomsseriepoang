using Results.Contract;
using Results.Meos;
using Results.Model;
using Results.Simulator;
using Results.Ola;
using Usp;
using Results;
using Results.Eventor;
using Results.IofXml;
using Results.Liveresultat;
using Scalar.AspNetCore;

var options = Options.Parse(args);
if (options == null)
{
    Console.Error.WriteLine(Options.HelpText?.ToString());
    Console.Error.Flush();
    Environment.ExitCode = 1;

    return;
}

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Path.GetFullPath(Path.Combine(typeof(Program).Assembly.Location, "..")),
    WebRootPath = Path.GetFullPath(Path.Combine(typeof(Program).Assembly.Location, "..", "wwwroot"))
});

var resultsConfiguration = Options.CreateConfiguration(options);

// Logging https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-7.0
builder.Logging.ClearProviders().AddConsole();

builder.WebHost.ConfigureKestrel(opt => opt.ListenAnyIP(options.ListenerPort));

// Add services to the container.
RegisterServices(builder, resultsConfiguration, options);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // OpenAPI & Scalar
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.Use(async (context, next) =>
{
    app.Logger.LogInformation(
        "Port={Port}, Path={Path}",
        options.ListenerPort,
        context.Request.Path);

    await next().ConfigureAwait(false);
});
app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapPost("/meos", (Endpoints endpoints, HttpRequest request) => endpoints.NewResultPostAsync(request));
app.MapGet("/teams", (Endpoints endpoints, HttpContext context) => endpoints.GetTeamsResultAsync(context));
app.MapGet("/participants", (Endpoints endpoints, HttpContext context) => endpoints.GetParticipantsResultAsync(context));

Configuration configuration = app.Services.GetRequiredService<Configuration>();
app.Logger.LogInformation("{}", configuration.ToString());

app.MapGet("/debug-webroot", (IWebHostEnvironment env) => Microsoft.AspNetCore.Http.Results.Ok(new
{
    env.WebRootPath,
    Exists = Directory.Exists(env.WebRootPath)
}));

app.Run();
return;

void RegisterServices(WebApplicationBuilder webApplicationBuilder, Configuration resultsConfiguration1, Options options1)
{
    webApplicationBuilder.Services.AddRazorPages();
    webApplicationBuilder.Services.AddServerSideBlazor();

    webApplicationBuilder.Services.AddSingleton(resultsConfiguration1);
    webApplicationBuilder.Services.AddSingleton<Usp.Data.ResultService>();
    webApplicationBuilder.Services.AddSingleton<IResultService, ResultService>();
    if (options1.Source == Source.Simulator)
        webApplicationBuilder.Services.AddSingleton<ITeamService, SimulatorTeamService>();
    else
        webApplicationBuilder.Services.AddSingleton<ITeamService, TeamService>();

    webApplicationBuilder.Services.AddSingleton<MeosResultSource>();
    webApplicationBuilder.Services.AddSingleton<OlaResultSource>();
    webApplicationBuilder.Services.AddSingleton<SimulatorResultSource>();
    webApplicationBuilder.Services.AddSingleton<LiveresultatResultSource>();
    webApplicationBuilder.Services.AddSingleton<IofXmlResultSource>();
    webApplicationBuilder.Services.AddSingleton<EventorResultSource>();

    webApplicationBuilder.Services.AddSingleton<LiveresultatFacade>();
    webApplicationBuilder.Services.AddSingleton<IEventorFacade,EventorFacade>();
    webApplicationBuilder.Services.AddSingleton<IIofXmlDeserializer, IofXmlDeserializer>();

    webApplicationBuilder.Services.AddSingleton<ClassFilter>();
    webApplicationBuilder.Services.AddSingleton<FileListener>();

    webApplicationBuilder.Services.AddSingleton<Endpoints>();

    webApplicationBuilder.Services.AddSingleton<IResultSource>(provider =>
    {
        return options1.Source switch
        {
            Source.Simulator => provider.GetRequiredService<SimulatorResultSource>(),
            Source.Meos => provider.GetRequiredService<MeosResultSource>(),
            Source.Ola => provider.GetRequiredService<OlaResultSource>(),
            Source.Liveresultat => provider.GetRequiredService<LiveresultatResultSource>(),
            Source.IofXml => provider.GetRequiredService<IofXmlResultSource>(),
            Source.Eventor => provider.GetRequiredService<EventorResultSource>(),
            _ => throw new InvalidOperationException()
        };
    });
}