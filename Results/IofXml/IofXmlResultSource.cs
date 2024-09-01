using Results.Model;
using Results.Ola.IofXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Results.IofXml;

public sealed class IofXmlResultSource : IResultSource
{
    private ResultList? result;
    private readonly Configuration configuration;

    public bool SupportsPreliminary => false;

    public TimeSpan CurrentTimeOfDay => result?.createTime.TimeOfDay ?? TimeSpan.Zero;

    public IofXmlResultSource(Configuration configuration)
    {
        this.configuration = configuration;
    }

    public IList<ParticipantResult> GetParticipantResults()
    {
        throw new NotImplementedException();
    }

    public Task<string> NewResultPostAsync(Stream body, DateTime timestamp)
    {
        throw new NotImplementedException();
    }

    public static ResultList LoadResultFile(String path) {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);

        XmlAttributeOverrides overrides = new();
        XmlAttributes ignore = new() { XmlIgnore = true };
        overrides.Add(typeof(PersonRaceResult), "SplitTime", ignore);
        XmlSerializer xml = new(typeof(ResultList), overrides);
        ResultList resultList = (ResultList)xml.Deserialize(stream);

        return resultList;
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
