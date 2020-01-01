using System;
using System.Linq;
using System.Security.Cryptography;

namespace Kalmit
{
    public class CommonConversion
    {
        static public byte[] ByteArrayFromStringBase16(string base16) =>
            Enumerable.Range(0, base16.Length / 2)
            .Select(octetIndex => Convert.ToByte(base16.Substring(octetIndex * 2, 2), 16))
            .ToArray();

        static public string StringBase16FromByteArray(byte[] array) =>
            BitConverter.ToString(array).Replace("-", "").ToUpperInvariant();

        static public byte[] HashSHA256(byte[] input)
        {
            using (var hasher = new SHA256Managed())
                return hasher.ComputeHash(input);
        }

        static public byte[] DecompressGzip(byte[] compressed)
        {
            using (var decompressStream = new System.IO.Compression.GZipStream(
                new System.IO.MemoryStream(compressed), System.IO.Compression.CompressionMode.Decompress))
            {
                var decompressedStream = new System.IO.MemoryStream();
                decompressStream.CopyTo(decompressedStream);
                return decompressedStream.ToArray();
            }
        }
    }
}