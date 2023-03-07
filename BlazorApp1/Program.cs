using BlazorApp1.Data;
using Microsoft.AspNetCore.Server.HttpSys;
using Org.BouncyCastle.Asn1.Ocsp;
using Results.Contract;
using Results.Meos;
using Results.Model;
using Results.Simulator;
using System;

var builder = WebApplication.CreateBuilder(args);

var resultsConfiguration = new Results.Configuration
{
    ResultSource = ResultSource.Meos
};

// Logging https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-7.0
builder.Host.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddSingleton(resultsConfiguration);
builder.Services.AddSingleton<ResultService>();
builder.Services.AddSingleton<IResultService, Results.ResultService>();

builder.Services.AddSingleton<IResultSource>();

####
switch (resultsConfiguration.ResultSource)
{
    case ResultSource.Meos:
        builder.Services.AddSingleton<IResultSource, MeosResultSource>();
        break;
    case ResultSource.OlaDatabase:
        builder.Services.AddSingleton<IResultSource, MeosResultSource>();
        break;
    case ResultSource.Simulator:
        builder.Services.AddSingleton<IResultSource,SimulatorResultSource>();
        break;
    default: throw new NotImplementedException(resultsConfiguration.ResultSource.ToString());
}

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
} );

app.Run();
