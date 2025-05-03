using System.Diagnostics.CodeAnalysis;
using Results.Model;
using Microsoft.Extensions.Logging;

namespace Results.IofXml;

public sealed class IofXmlResultSource : IResultSource
{
    private IList<ParticipantResult> participantResults = [];

    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    private readonly Configuration configuration;

    private readonly FileListener fileListener;
    private readonly ILogger<IofXmlResultSource> logger;
    private readonly IIofXmlDeserializer deserializer;
    private int lastFileNumber;
    private TimeSpan currentTimeOfDay;

    public bool SupportsPreliminary => false;

    public TimeSpan CurrentTimeOfDay => currentTimeOfDay;

    public IofXmlResultSource(Configuration configuration, FileListener fileListener,
        ILogger<IofXmlResultSource> logger, IIofXmlDeserializer deserializer)
    {
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this.fileListener = fileListener ?? throw new ArgumentNullException(nameof(fileListener));
        this.logger = logger;
        this.deserializer = deserializer;
        fileListener.NewFile += FileListener_NewFile;
    }

    private void FileListener_NewFile(object? sender, NewFileEventArgs e)
    {
        try
        {
            participantResults = LoadResultFile(e.FullName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while loading result file");
        }
    }

    public IList<ParticipantResult> GetParticipantResults()
    {
        return participantResults;
    }

    public Task<string> NewResultPostAsync(Stream body, DateTime timestamp)
    {
        throw new NotImplementedException();
    }

    private IList<ParticipantResult> LoadResultFile(string path)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            return deserializer.Deserialize(stream, out currentTimeOfDay);
        }
        finally
        {
            RenameFile(path);
        }
    }

    private void RenameFile(string path)
    {
        string folder = Path.GetDirectoryName(path)!;
        string fileName = Path.GetFileName(path);

        for (var i = lastFileNumber; i < 10000; i++)
        {
            string newPath = Path.Combine(folder, $"{fileName}.{i:0000}");
            if (File.Exists(newPath)) continue;

            try
            {
                File.Move(path, newPath);
                lastFileNumber = (i + 1) % 10000;
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while renameing file");
            }
        }
    }

    public void Dispose()
    {
        fileListener.Dispose();
    }
}