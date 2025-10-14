using System;
using System.Collections.Immutable;
using System.Linq;

namespace read_memory_64_bit;


public interface IMemoryReader
{
    ReadOnlyMemory<byte>? ReadBytes(ulong startAddress, int length);
}

public class MemoryReaderFromProcessSample(
    IImmutableList<SampleMemoryRegion> memoryRegions)
    : IMemoryReader
{
    readonly IImmutableList<SampleMemoryRegion> memoryRegionsOrderedByAddress =
            memoryRegions
            .OrderBy(memoryRegion => memoryRegion.baseAddress)
            .ToImmutableList();

    public ReadOnlyMemory<byte>? ReadBytes(ulong startAddress, int length)
    {
        var memoryRegion =
            memoryRegionsOrderedByAddress
            .Where(region => region.baseAddress <= startAddress)
            .LastOrDefault();

        if (memoryRegion?.content is not { } memoryRegionContent)
            return null;

        var start =
            startAddress - memoryRegion.baseAddress;

        if ((int)start < 0)
            return null;

        if (memoryRegionContent.Length <= (int)start)
            return null;

        var sliceLengthBound =
            memoryRegionContent.Length - (int)start;

        var sliceLength =
            length < sliceLengthBound ? length : sliceLengthBound;

        return
            memoryRegionContent.Slice(
                start: (int)start,
                length: sliceLength);
    }
}


public class MemoryReaderFromLiveProcess : IMemoryReader, IDisposable
{
    readonly IntPtr processHandle;

    public MemoryReaderFromLiveProcess(int processId)
    {
        processHandle =
            WinApi.OpenProcess(
                (int)(WinApi.ProcessAccessFlags.QueryInformation | WinApi.ProcessAccessFlags.VirtualMemoryRead),
                false,
                dwProcessId: processId);
    }

    public void Dispose()
    {
        if (processHandle != IntPtr.Zero)
            WinApi.CloseHandle(processHandle);
    }

    public ReadOnlyMemory<byte>? ReadBytes(ulong startAddress, int length)
    {
        var buffer = new byte[length];

        UIntPtr numberOfBytesReadAsPtr = UIntPtr.Zero;

        if (!WinApi.ReadProcessMemory(processHandle, startAddress, buffer, (UIntPtr)buffer.LongLength, ref numberOfBytesReadAsPtr))
            return null;

        var numberOfBytesRead = numberOfBytesReadAsPtr.ToUInt64();

        if (numberOfBytesRead is 0)
            return null;

        if (int.MaxValue < numberOfBytesRead)
            return null;

        if (numberOfBytesRead == (ulong)buffer.LongLength)
            return buffer;

        return buffer;
    }
}
