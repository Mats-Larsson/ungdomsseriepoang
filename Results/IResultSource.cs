﻿using Results.Model;

namespace Results
{
    internal interface IResultSource
    {
        IList<ParticipantResult> GetParticipantResults();
    }
}