﻿using System.Text.Json.Serialization;

namespace Results.Liveresultat;

public record LastPassingList : StatusBase
{
    [JsonPropertyName("passings")]
    public IList<Passing>? Passings { get; init; }
}