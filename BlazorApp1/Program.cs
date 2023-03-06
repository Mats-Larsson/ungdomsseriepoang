using BlazorApp1.Data;
using Microsoft.AspNetCore.Server.HttpSys;
using Org.BouncyCastle.Asn1.Ocsp;
using Results.Contract;
using System;

var builder = WebApplication.CreateBuilder(args);

var resultsConfiguration = new Results.Configuration
{
    ResultSource = ResultSource.Meos
};

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddSingleton(resultsConfiguration);
builder.Services.AddSingleton<ResultService>();
builder.Services.AddSingleton<IResultService, Results.ResultService>();

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
