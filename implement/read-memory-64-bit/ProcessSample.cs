using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace read_memory_64_bit;


public class ProcessSample
{
    static public byte[] ZipArchiveFromProcessSample(
        IImmutableList<SampleMemoryRegion> memoryRegions,
        IImmutableList<string> logEntries,
        byte[] beginMainWindowClientAreaScreenshotBmp,
        byte[] endMainWindowClientAreaScreenshotBmp)
    {
        var screenshotEntriesCandidates = new[]
        {
            (filePath: ImmutableList.Create("begin-main-window-client-area.bmp"), content: beginMainWindowClientAreaScreenshotBmp),
            (filePath: ImmutableList.Create("end-main-window-client-area.bmp"), content: endMainWindowClientAreaScreenshotBmp),
        };

        var screenshotEntries =
            screenshotEntriesCandidates
            .Where(filePathAndContent => filePathAndContent.content != null)
            .Select(filePathAndContent => new KeyValuePair<IImmutableList<string>, byte[]>(
                filePathAndContent.filePath, filePathAndContent.content))
            .ToArray();

        var zipArchiveEntries =
            memoryRegions.ToImmutableDictionary(
                region => (IImmutableList<string>)(["Process", "Memory", $"0x{region.baseAddress:X}"]),
                region => region.content.Value.ToArray())
            .Add(new[] { "copy-memory-log" }.ToImmutableList(), System.Text.Encoding.UTF8.GetBytes(String.Join("\n", logEntries)))
            .AddRange(screenshotEntries);

        return Pine.ZipArchive.ZipArchiveFromEntries(zipArchiveEntries);
    }

    static public (IImmutableList<SampleMemoryRegion> memoryRegions, IImmutableList<string> copyMemoryLog) ProcessSampleFromZipArchive(byte[] sampleFile)
    {
        var files =
            Pine.ZipArchive.EntriesFromZipArchive(sampleFile);

        IEnumerable<(IImmutableList<string> filePath, byte[] fileContent)> GetFilesInDirectory(IImmutableList<string> directory)
        {
            foreach (var fileFullPathAndContent in files)
            {
                var fullPath = fileFullPathAndContent.name.Split(['/', '\\']);

                if (!fullPath.Take(directory.Count).SequenceEqual(directory))
                    continue;

                yield return (fullPath.Skip(directory.Count).ToImmutableList(), fileFullPathAndContent.content);
            }
        }

        var memoryRegions =
            GetFilesInDirectory(ImmutableList.Create("Process", "Memory"))
            .Where(fileSubpathAndContent => fileSubpathAndContent.filePath.Count == 1)
            .Select(fileSubpathAndContent =>
            {
                var baseAddressBase16 = System.Text.RegularExpressions.Regex.Match(fileSubpathAndContent.filePath.Single(), @"0x(.+)").Groups[1].Value;

                var baseAddress = ulong.Parse(baseAddressBase16, System.Globalization.NumberStyles.HexNumber);

                return new SampleMemoryRegion(
                    baseAddress,
                    length: (ulong)fileSubpathAndContent.fileContent.LongLength,
                    content: fileSubpathAndContent.fileContent);
            }).ToImmutableList();

        return (memoryRegions, null);
    }
}

public record SampleMemoryRegion(
    ulong baseAddress,
    ulong length,
    ReadOnlyMemory<byte>? content);

