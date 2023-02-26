namespace Results
{
    public class TimeService : ITimeService
    {
        public TimeSpan TimeOfDay => DateTime.Now - DateTime.Now.Date;
    }
}
