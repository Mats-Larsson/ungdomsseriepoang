using System.Text;

namespace Usp;

/// <summary>
/// Replays the MeOS posts recorded at the final 2026-10-04 (see <see cref="TestData.Access"/>) by posting
/// them to /meos, waiting the original time between posts divided by <paramref name="speed"/>.
/// The time of the application (<see cref="TimeProvider"/>) is set from the file name of each post.
/// </summary>
internal sealed class MeosSimulator(
    IHttpClientFactory httpClientFactory,
    IHostApplicationLifetime lifetime,
    MeosSimTimeProvider timeProvider,
    ILogger<MeosSimulator> logger,
    int listenerPort,
    int speed) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!await WaitForStartAsync(stoppingToken).ConfigureAwait(false)) return;

        var url = new Uri($"http://localhost:{listenerPort}/meos");
        logger.LogInformation("MeOS simulator posting to {Url} at speed {Speed}", url, speed);

        using var httpClient = httpClientFactory.CreateClient();
        int count = 0;
        foreach ((DateTime timestamp, string content, TimeSpan? timeToNext) in TestData.Access.ReadFiles(TestData.Access.Final2026ZipPath))
        {
            timeProvider.SetLocalTime(timestamp);
            try
            {
                using var body = new StringContent(content, Encoding.UTF8, "application/xml");
                using var response = await httpClient.PostAsync(url, body, stoppingToken).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    logger.LogWarning("MeOS simulator post {Count} responded {StatusCode}", count, response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning(ex, "MeOS simulator post {Count} failed", count);
            }
            count++;

            if (timeToNext is { } delay)
                await Task.Delay(delay / speed, stoppingToken).ConfigureAwait(false);
        }

        logger.LogInformation("MeOS simulator done, {Count} posts sent", count);
    }

    /// <summary>Waits until the web server accepts requests. Returns false if stopped before that.</summary>
    private async Task<bool> WaitForStartAsync(CancellationToken stoppingToken)
    {
        var started = new TaskCompletionSource();
        await using var startedRegistration = lifetime.ApplicationStarted.Register(() => started.TrySetResult()).ConfigureAwait(false);
        await using var stoppingRegistration = stoppingToken.Register(() => started.TrySetCanceled()).ConfigureAwait(false);
        try
        {
            await started.Task.ConfigureAwait(false);
            return true;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
    }
}
