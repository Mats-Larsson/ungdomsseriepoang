namespace Results
{
    public class TimeService : ITimeService
    {
        public TimeSpan CurrentTimeOfDay => DateTime.Now - DateTime.Now.Date;
    }
}
