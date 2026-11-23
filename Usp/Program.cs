using Common;
using Usp;
using ResultCalc;
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

// Bootstrap logger so that errors during startup are logged before the configured logger is in place
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        Args = args,
        ContentRootPath = AppContext.BaseDirectory,
        WebRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot")
    });

    var resultsConfiguration = Options.CreateConfiguration(options);

    // Eventor api key: from -a/--apikey, otherwise from configuration "Eventor:ApiKey"
    // (user secrets when running locally in Development, or environment variable Eventor__ApiKey). Never checked in.
    if (string.IsNullOrEmpty(resultsConfiguration.ApiKey))
        resultsConfiguration = resultsConfiguration with { ApiKey = builder.Configuration["Eventor:ApiKey"] };

    // Logging via Serilog, configured from the "Serilog" section in appsettings.json.
    // Also logs to usp-yyyyMMdd.log in --logdir, default next to usp.exe. A new file is started at midnight.
    // shared: several instances may run at the same time.
    var logDir = Path.GetFullPath(string.IsNullOrEmpty(options.LogDir) ? AppContext.BaseDirectory : options.LogDir);
    builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.File(
            Path.Combine(logDir, "usp-.log"),
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}",
            shared: true));

    builder.WebHost.ConfigureKestrel(opt => opt.ListenAnyIP(options.ListenerPort));

    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor();

    // Add services to the container.
    builder.Services.AddResultsServices(resultsConfiguration, options.Source);
    builder.Services.AddUspServices();
    if (options is { MeosSim: true, Source: Source.Meos })
        builder.Services.AddMeosSimulator(options.ListenerPort, options.MeosSimSpeed);

    builder.Services.AddOpenApi();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        // OpenAPI & Scalar
        app.MapOpenApi();
        app.MapScalarApiReference();

        // Shows where static files are served from, for troubleshooting. Not exposed in production since it reveals the install path.
        app.MapGet("/debug-webroot", (IWebHostEnvironment env) => Results.Ok(new
        {
            env.WebRootPath,
            Exists = Directory.Exists(env.WebRootPath)
        }));
    }
    else
    {
        app.UseExceptionHandler("/Error");
    }

    app.UseStaticFiles();

    // One log line per request: "HTTP GET /teams responded 200 in 12.3456 ms".
    // Placed after UseStaticFiles so css/js requests are not logged; Blazor's own traffic is logged at Debug.
    app.UseSerilogRequestLogging(o =>
    {
        o.GetLevel = (context, _, ex) =>
            ex != null || context.Response.StatusCode >= 500 ? Serilog.Events.LogEventLevel.Error
                : context.Request.Path.StartsWithSegments("/_blazor", StringComparison.OrdinalIgnoreCase)
                  || context.Request.Path.StartsWithSegments("/_framework", StringComparison.OrdinalIgnoreCase)
                    ? Serilog.Events.LogEventLevel.Debug
                : Serilog.Events.LogEventLevel.Information;
    });

    app.UseRouting();

    app.MapBlazorHub();
    app.MapFallbackToPage("/_Host");
    app.MapPost("/meos", (Endpoints endpoints, HttpRequest request) => endpoints.NewResultPostAsync(request))
        .Produces<string>(StatusCodes.Status200OK, "application/xml")
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);
    app.MapGet("/teams", (Endpoints endpoints) => endpoints.GetTeamsResultAsync())
        .Produces<string>(StatusCodes.Status200OK, "text/csv");
    app.MapGet("/participants", (Endpoints endpoints) => endpoints.GetParticipantsResultAsync())
        .Produces<string>(StatusCodes.Status200OK, "text/csv");

    var uspConfiguration = app.Services.GetRequiredService<Configuration>();
    app.Logger.LogInformation("Version {Version}, listening on port {Port}", Helper.AppVersion, options.ListenerPort);
    app.Logger.LogInformation("{Configuration}", uspConfiguration.ToLogString());

    await app.RunAsync().ConfigureAwait(false);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    Environment.ExitCode = 1;
}
finally
{
    await Log.CloseAndFlushAsync().ConfigureAwait(false);
}
