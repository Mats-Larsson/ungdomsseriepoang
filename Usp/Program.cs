using Results.Contract;
using Results.Meos;
using Results.Model;
using Results.Simulator;
using Results.Ola;
using Usp;
using Results;
using Results.Liveresultat;

var options = Options.Parse(args);
if (options == null)
{
    Console.Error.WriteLine(Options.HelpText?.ToString());
    Console.Error.Flush();
    // TODO: Set exit code != 0

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


var url = $"http://*:{options.ListenerPort}";
builder.WebHost.UseUrls(url);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddSingleton(resultsConfiguration);
builder.Services.AddSingleton<Usp.Data.ResultService>();
builder.Services.AddSingleton<IResultService, ResultService>();
if (options.Source == Source.Simulator)
    builder.Services.AddSingleton<ITeamService, SimulatorTeamService>();
else
    builder.Services.AddSingleton<ITeamService, TeamService>();

builder.Services.AddSingleton<MeosResultSource>();
builder.Services.AddSingleton<OlaResultSource>();
builder.Services.AddSingleton<SimulatorResultSource>();
builder.Services.AddSingleton<LiveresultatResultSource>();
builder.Services.AddSingleton<LiveresultatFacade>();

builder.Services.AddSingleton<IResultSource>(provider =>
{
    return options.Source switch
    {
        Source.Simulator => provider.GetService<SimulatorResultSource>()!,
        Source.Meos => provider.GetService<MeosResultSource>()!,
        Source.Ola => provider.GetService<OlaResultSource>()!,
        Source.Liveresultat => provider.GetService<LiveresultatResultSource>()!,
        _ => throw new InvalidOperationException()
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapPost("/meos", async (HttpRequest request) =>
{
    var resultService = app.Services.GetService<IResultService>()!;
    return await resultService.NewResultPostAsync(request.Body, DateTime.Now).ConfigureAwait(false);
});
app.MapGet("/teams", context =>
{
    var resultService = app.Services.GetService<IResultService>()!;
    var teamResults = resultService.GetScoreBoard().TeamResults;

    return Microsoft.AspNetCore.Http.Results.Content(Helper.ToCsvText(teamResults), contentType: "text/csv")
        .ExecuteAsync(context);
});
app.MapGet("/participants", context =>
{
    var resultService = app.Services.GetService<IResultService>()!;
    var participantPointsList = resultService.GetParticipantPointsList();

    return Microsoft.AspNetCore.Http.Results.Content(Helper.ToCsvText(participantPointsList), contentType: "text/csv")
        .ExecuteAsync(context);
});

Configuration configuration = app.Services.GetService<Configuration>()!;
app.Logger.LogInformation("{}", configuration.ToString());

app.Run();