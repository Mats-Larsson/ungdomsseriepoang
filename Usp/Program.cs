using Common;
using Usp;
using Results;
using Scalar.AspNetCore;
using Serilog;

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

// Logging via Serilog, configured from the "Serilog" section in appsettings.json
builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.WebHost.ConfigureKestrel(opt => opt.ListenAnyIP(options.ListenerPort));

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add services to the container.
builder.Services.AddResultsServices(resultsConfiguration, options.Source);
builder.Services.AddUspServices();

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
app.Logger.LogInformation("{Configuration}", configuration.ToString());

app.MapGet("/debug-webroot", (IWebHostEnvironment env) => Microsoft.AspNetCore.Http.Results.Ok(new
{
    env.WebRootPath,
    Exists = Directory.Exists(env.WebRootPath)
}));

app.Run();