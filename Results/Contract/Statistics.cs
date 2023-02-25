using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Results.Contract
{
    public class Statistics
    {
        public int NumNotActivated { get; private set; }
        public int NumActivated { get; private set; }
        public int NumStarted { get; private set; }
        public int NumPassed { get; private set; }
        public int NumNotValid { get; private set; }
        public int NumNotStarted { get; private set; }

        public Statistics() { }        
        public Statistics(int numNotActivated, int numActivated, int numStarted, int numPassed, int numNotValid)
        {
            NumNotActivated = numNotActivated;
            NumActivated = numActivated;
            NumStarted = numStarted;
            NumPassed = numPassed;
            NumNotValid = numNotValid;
        }

        internal void IncNumNotActivated() { NumNotActivated++; }
        internal void IncNumActivated() { NumActivated++;}
        internal void IncNumStarted() { NumStarted++; }
        internal void IncNumPassed() { NumPassed++;}
        internal void IncNumNotValid() { NumNotValid++; }
        internal void IncNumNotStarted() { NumNotStarted++; }
    }
}
