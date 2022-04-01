using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Pine;

/*
2020-07-16 Discovered: The roundtrip over `ZipArchiveFromEntries` and `EntriesFromZipArchive` changed the order of entries!
*/
static public class ZipArchive
{
    /// <summary>
    /// https://github.com/dotnet/corefx/blob/a10890f4ffe0fadf090c922578ba0e606ebdd16c/src/System.IO.Compression/src/System/IO/Compression/ZipArchiveEntry.cs#L206-L234
    /// </summary>
    static public DateTimeOffset EntryLastWriteTimeDefault => new DateTimeOffset(1980, 1, 1, 0, 0, 0, TimeSpan.Zero);

    static public byte[] ZipArchiveFromEntries(
        IEnumerable<(string name, byte[] content)> entries,
        System.IO.Compression.CompressionLevel compressionLevel = System.IO.Compression.CompressionLevel.Optimal) =>
        ZipArchiveFromEntries(
            entries.Select(entry => (entry.name, entry.content, EntryLastWriteTimeDefault)).ToImmutableList(),
            compressionLevel);

    static public byte[] ZipArchiveFromEntries(
        IReadOnlyDictionary<IImmutableList<string>, byte[]> entries,
        System.IO.Compression.CompressionLevel compressionLevel = System.IO.Compression.CompressionLevel.Optimal) =>
        ZipArchiveFromEntries(
            entries.Select(entry => (name: String.Join("/", entry.Key), content: entry.Value)),
            compressionLevel);

    static public byte[] ZipArchiveFromEntries(
        IReadOnlyDictionary<IImmutableList<string>, IReadOnlyList<byte>> entries,
        System.IO.Compression.CompressionLevel compressionLevel = System.IO.Compression.CompressionLevel.Optimal) =>
        ZipArchiveFromEntries(
            entries.ToImmutableDictionary(entry => entry.Key, entry => entry.Value.ToArray()),
            compressionLevel);

    static public byte[] ZipArchiveFromEntries(
        IEnumerable<(string name, byte[] content, DateTimeOffset lastWriteTime)> entries,
        System.IO.Compression.CompressionLevel compressionLevel = System.IO.Compression.CompressionLevel.Optimal)
    {
        var stream = new MemoryStream();

        using (var fclZipArchive = new System.IO.Compression.ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Create, true))
        {
            foreach (var (entryName, entryContent, lastWriteTime) in entries)
            {
                var entry = fclZipArchive.CreateEntry(entryName, compressionLevel);
                entry.LastWriteTime = lastWriteTime;

                using var entryStream = entry.Open();

                entryStream.Write(entryContent, 0, entryContent.Length);
            }
        }

        stream.Seek(0, SeekOrigin.Begin);

        var zipArchive = new byte[stream.Length];
        stream.Read(zipArchive, 0, (int)stream.Length);
        stream.Dispose();
        return zipArchive;
    }

    static public IEnumerable<(string name, byte[] content)> FileEntriesFromZipArchive(byte[] zipArchive) =>
        EntriesFromZipArchive(
            zipArchive: zipArchive,
            includeEntry: entry => !entry.FullName.Replace('\\', '/').EndsWith('/'));

    static public IEnumerable<(string name, byte[] content)> EntriesFromZipArchive(byte[] zipArchive) =>
        EntriesFromZipArchive(zipArchive: zipArchive, includeEntry: _ => true);

    static public IEnumerable<(string name, byte[] content)> EntriesFromZipArchive(
        byte[] zipArchive,
        Func<System.IO.Compression.ZipArchiveEntry, bool> includeEntry)
    {
        using var fclZipArchive = new System.IO.Compression.ZipArchive(new MemoryStream(zipArchive), System.IO.Compression.ZipArchiveMode.Read);

        foreach (var entry in fclZipArchive.Entries)
        {
            if (!includeEntry(entry))
                continue;

            using var entryStream = entry.Open();
            using var memoryStream = new MemoryStream();

            entryStream.CopyTo(memoryStream);

            var entryContent = memoryStream.ToArray();

            if (entryContent.Length != entry.Length)
                throw new Exception("Error trying to read entry '" + entry.FullName + "': got " + entryContent.Length + " bytes from entry instead of " + entry.Length);

            yield return (entry.FullName, entryContent);
        }
    }
}
