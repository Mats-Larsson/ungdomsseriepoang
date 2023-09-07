using System.Text;
using Results.Contract;
using Results.Meos;
using Results.Model;
using Results.Simulator;
using Results.Ola;
using Usp;
using Results;

var options = Options.Parse(args);
if (options == null)
{
    Console.Error.WriteLine(Options.HelpText?.ToString());
    // TODO: Set exit code != 0
    return;
}

var builder = WebApplication.CreateBuilder(args);

var resultsConfiguration = options.CreateConfiguration();

// Logging https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-7.0
builder.Host.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

var url = $"http://localhost:{options.ListenerPort}";
builder.WebHost.UseUrls(url);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddSingleton(resultsConfiguration);
builder.Services.AddSingleton<Usp.Data.ResultService>();
builder.Services.AddSingleton<IResultService, ResultService>();
builder.Services.AddSingleton<IBasePointsService, BasePointsService>();

builder.Services.AddSingleton<MeosResultSource>();
builder.Services.AddSingleton<OlaResultSource>();
builder.Services.AddSingleton<SimulatorResultSource>();

builder.Services.AddSingleton<IResultSource>(provider =>
{
    if (options.UseMeos) return provider.GetService<MeosResultSource>()!;
    if (options.UseOla) return provider.GetService<OlaResultSource>()!;
    if (options.UseSimulator) return provider.GetService<SimulatorResultSource>()!;
    throw new NotImplementedException();
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

app.Run();