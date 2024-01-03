using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Pine;

public class CommonConversion
{
    static public byte[] ByteArrayFromStringBase16(string base16) =>
        Enumerable.Range(0, base16.Length / 2)
        .Select(octetIndex => Convert.ToByte(base16.Substring(octetIndex * 2, 2), 16))
        .ToArray();

    static public string StringBase16FromByteArray(IReadOnlyList<byte> bytes) =>
        BitConverter.ToString(bytes as byte[] ?? [.. bytes]).Replace("-", "").ToLowerInvariant();

    static public byte[] HashSHA256(byte[] input) => SHA256.HashData(input);

    static public byte[] CompressGzip(byte[] original)
    {
        using var compressedStream = new System.IO.MemoryStream();

        using var compressStream = new System.IO.Compression.GZipStream(
            compressedStream, System.IO.Compression.CompressionMode.Compress);

        compressStream.Write(original);
        compressStream.Flush();
        return compressedStream.ToArray();
    }

    static public byte[] DecompressGzip(byte[] compressed)
    {
        using var decompressStream = new System.IO.Compression.GZipStream(
            new System.IO.MemoryStream(compressed), System.IO.Compression.CompressionMode.Decompress);

        var decompressedStream = new System.IO.MemoryStream();
        decompressStream.CopyTo(decompressedStream);
        return decompressedStream.ToArray();
    }

    static public byte[] Deflate(byte[] input)
    {
        using var deflatedStream = new System.IO.MemoryStream();

        using var compressor = new System.IO.Compression.DeflateStream(
            deflatedStream, System.IO.Compression.CompressionMode.Compress);

        compressor.Write(input, 0, input.Length);
        compressor.Close();
        return deflatedStream.ToArray();
    }

    static public byte[] Inflate(IReadOnlyList<byte> input)
    {
        using var inflatedStream = new System.IO.MemoryStream();

        using var deflateStream = new System.IO.Compression.DeflateStream(
            new System.IO.MemoryStream(input as byte[] ?? [.. input]), System.IO.Compression.CompressionMode.Decompress);

        deflateStream.CopyTo(inflatedStream);

        return inflatedStream.ToArray();
    }

    static public string TimeStringViewForReport(DateTimeOffset time) =>
        time.ToString("yyyy-MM-ddTHH-mm-ss");
}
