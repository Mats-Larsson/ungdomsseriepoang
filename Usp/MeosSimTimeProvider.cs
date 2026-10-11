namespace Usp;

/// <summary>
/// Time used while <see cref="MeosSimulator"/> replays recorded posts. Before each post the simulator sets the time
/// from the file name of the post. Between posts the time runs <c>speed</c> times faster than real time.
/// Until the first post it is the real time.
/// </summary>
internal sealed class MeosSimTimeProvider(int speed) : TimeProvider
{
    private sealed record Anchor(DateTimeOffset Time, long Timestamp);

    private volatile Anchor anchor = new(System.GetUtcNow(), System.GetTimestamp());

    public void SetLocalTime(DateTime localTime)
    {
        anchor = new Anchor(new DateTimeOffset(localTime, LocalTimeZone.GetUtcOffset(localTime)), System.GetTimestamp());
    }

    public override DateTimeOffset GetUtcNow()
    {
        var current = anchor;
        return (current.Time + System.GetElapsedTime(current.Timestamp) * speed).ToUniversalTime();
    }
}
