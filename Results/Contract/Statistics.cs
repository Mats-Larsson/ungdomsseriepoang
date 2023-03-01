namespace Results.Contract
{
    public class Statistics
    {
        public TimeSpan CurrentTimeOfDay { get; internal set; }
        public int NumNotActivated { get; private set; }
        public int NumActivated { get; private set; }
        public int NumStarted { get; private set; }
        public int NumPreliminary { get; private set; }
        public int NumPassed { get; private set; }
        public int NumNotValid { get; private set; }
        public int NumNotStarted { get; private set; }

        internal void IncNumNotActivated() { NumNotActivated++; }
        internal void IncNumActivated() { NumActivated++;}
        internal void IncNumStarted() { NumStarted++; }
        internal void IncNumPreliminary() { NumPreliminary++; }
        internal void IncNumPassed() { NumPassed++;}
        internal void IncNumNotValid() { NumNotValid++; }
        internal void IncNumNotStarted() { NumNotStarted++; }
    }
}
