﻿using Results.Contract;
using Results.Model;

namespace Results.Meos
{
    internal record MeosParticipantResult : ParticipantResult
    {
        public MeosParticipantResult(string @class, string name, string club, TimeSpan? startTime, TimeSpan? time, ParticipantStatus status) 
            : base(@class, name, club, startTime, time, status)
        {
        }
    }
}
