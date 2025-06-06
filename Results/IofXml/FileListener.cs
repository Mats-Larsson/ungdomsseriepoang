﻿using System.Diagnostics.CodeAnalysis;

namespace Results.IofXml;

public sealed class FileListener : IDisposable
{
    private readonly FileSystemWatcher watcher;
    private readonly Lock lockObj = new();
    
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")] 
    public DirectoryInfo Directory { get; }

    public FileListener(Configuration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
        if (configuration.IofXmlInputFolder == null) throw new InvalidOperationException("IofXmlInputFolder nust not be null");
        Directory = new DirectoryInfo(configuration.IofXmlInputFolder);
        if (!Directory.Exists) throw new InvalidOperationException($"Directory not found: {Directory.FullName}");
        watcher = new FileSystemWatcher(Directory.FullName);

        watcher.Created += Watcher_Created;
        watcher.Changed += Watcher_Changed;
        watcher.Renamed += Watcher_Renamed;
        watcher.Filter = "*.xml";
        watcher.EnableRaisingEvents = true;
        Process();
    }

    public event EventHandler<NewFileEventArgs>? NewFile;

    private void Watcher_Renamed(object sender, RenamedEventArgs e)
    {
        Process();
    }

    private void Watcher_Changed(object sender, FileSystemEventArgs e)
    {
        Process();
    }

    private void Watcher_Created(object sender, FileSystemEventArgs e)
    {
        Process();
    }

    private void Process()
    {
        lock (lockObj)
        {
            var files = Directory.GetFiles("*.xml", new EnumerationOptions { RecurseSubdirectories = false, MatchType = MatchType.Simple });
            foreach (var file in files)
            {
                if (IsFileFree(file))
                {
                    NewFile?.Invoke(this, new NewFileEventArgs(file.FullName));
                }
            }
        }
    }

    private static bool IsFileFree(FileInfo file)
    {
        var isFree = false;
        for (int i = 0; i < 3; i++)
        {
            try
            {
                using var fileStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
                isFree = true;
                break;
            }
            catch (FileNotFoundException)
            {
                break;
            }
            catch (IOException) { }
            Task.Delay(10).Wait();
        }

        return isFree;
    }

    public void Dispose()
    {
        watcher.Dispose();
    }
}

public class NewFileEventArgs(string fullName) : EventArgs
{
    public string FullName { get; } = fullName;
}