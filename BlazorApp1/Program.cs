using BlazorApp1.Data;
using Results.Contract;

var builder = WebApplication.CreateBuilder(args);

var resultsConfiguration = new Results.Configuration();

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

app.Run();
