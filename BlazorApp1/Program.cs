using BlazorApp1.Data;
using Microsoft.AspNetCore.Server.HttpSys;
using Org.BouncyCastle.Asn1.Ocsp;
using Results.Contract;
using Results.Meos;
using Results.Model;
using Results.Simulator;
using System;
using Results.Ola;

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

builder.Services.AddSingleton<MeosResultSource>();
builder.Services.AddSingleton<OlaResultSource>();
builder.Services.AddSingleton<SimulatorResultSource>();

builder.Services.AddSingleton<IResultSource>(provider => resultsConfiguration.ResultSource switch
{
    ResultSource.Meos => provider.GetService<MeosResultSource>()! as IResultSource,
    ResultSource.OlaDatabase => provider.GetService<OlaResultSource>()! as IResultSource,
    ResultSource.Simulator => provider.GetService<SimulatorResultSource>()! as IResultSource,
    _ => throw new NotImplementedException(resultsConfiguration.ResultSource.ToString())
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

app.Run();