using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace read_memory_64_bit;

class Program
{
    static string AppVersionId => "2024-01-03";

    static int Main(string[] args)
    {
        var app = new CommandLineApplication
        {
            Name = "read-memory-64-bit",
            Description = "Welcome to the Sanderling memory reading command-line interface. This tool helps you read objects from the memory of a 64-bit EVE Online client process and save it to a file. In addition to that, you have the option to save the entire memory contents of a game client process to a file.\nTo get help or report an issue, see the project website at https://github.com/Arcitectus/Sanderling",
        };

        app.HelpOption(inherited: true);

        app.VersionOption(template: "-v|--version", shortFormVersion: "version " + AppVersionId);

        app.Command("save-process-sample", saveProcessSampleCmd =>
        {
            saveProcessSampleCmd.Description = "Save a sample from a live process to a file. Use the '--pid' parameter to specify the process id.";

            var processIdParam =
            saveProcessSampleCmd.Option("--pid", "[Required] Id of the Windows process to read from.", CommandOptionType.SingleValue).IsRequired(errorMessage: "From which process should I read?");

            var delaySecondsParam =
            saveProcessSampleCmd.Option("--delay", "Timespan to wait before starting the collection of the sample, in seconds.", CommandOptionType.SingleValue);

            saveProcessSampleCmd.OnExecute(() =>
            {
                var processIdArgument = processIdParam.Value();

                var delayMilliSeconds =
                delaySecondsParam.HasValue() ?
                (int)(double.Parse(delaySecondsParam.Value()) * 1000) :
                0;

                var processId = int.Parse(processIdArgument);

                if (0 < delayMilliSeconds)
                {
                    Console.WriteLine("Delaying for " + delayMilliSeconds + " milliseconds.");
                    Task.Delay(TimeSpan.FromMilliseconds(delayMilliSeconds)).Wait();
                }

                Console.WriteLine("Starting to collect the sample...");

                var processSampleFile = GetProcessSampleFileFromProcessId(processId);

                Console.WriteLine("Completed collecting the sample.");

                var processSampleId = Pine.CommonConversion.StringBase16FromByteArray(
                    Pine.CommonConversion.HashSHA256(processSampleFile));

                var fileName = "process-sample-" + processSampleId[..10] + ".zip";

                System.IO.File.WriteAllBytes(fileName, processSampleFile);

                Console.WriteLine("Saved sample {0} to file '{1}'.", processSampleId, fileName);
            });
        });

        app.Command("read-memory-eve-online", readMemoryEveOnlineCmd =>
        {
            readMemoryEveOnlineCmd.Description = "Read the memory of an 64 bit EVE Online client process. You can use a live process ('--pid') or a process sample file ('--source-file') as the source.";

            var processIdParam = readMemoryEveOnlineCmd.Option("--pid", "Id of the Windows process to read from.", CommandOptionType.SingleValue);
            var rootAddressParam = readMemoryEveOnlineCmd.Option("--root-address", "Address of the UI root. If the address is not specified, the program searches the whole process memory for UI roots.", CommandOptionType.SingleValue);
            var sourceFileParam = readMemoryEveOnlineCmd.Option("--source-file", "Process sample file to read from.", CommandOptionType.SingleValue);
            var outputFileParam = readMemoryEveOnlineCmd.Option("--output-file", "File to save the memory reading result to.", CommandOptionType.SingleValue);
            var removeOtherDictEntriesParam = readMemoryEveOnlineCmd.Option("--remove-other-dict-entries", "Use this to remove the other dict entries from the UI nodes in the resulting JSON representation.", CommandOptionType.NoValue);
            var warmupIterationsParam = readMemoryEveOnlineCmd.Option("--warmup-iterations", "Only to measure execution time: Use this to perform additional warmup runs before measuring execution time.", CommandOptionType.SingleValue);

            readMemoryEveOnlineCmd.OnExecute(() =>
            {
                var processIdArgument = processIdParam.Value();
                var rootAddressArgument = rootAddressParam.Value();
                var sourceFileArgument = sourceFileParam.Value();
                var outputFileArgument = outputFileParam.Value();
                var removeOtherDictEntriesArgument = removeOtherDictEntriesParam.HasValue();
                var warmupIterationsArgument = warmupIterationsParam.Value();

                var processId =
                    0 < processIdArgument?.Length
                    ?
                    (int?)int.Parse(processIdArgument)
                    :
                    null;

                (IMemoryReader, IImmutableList<ulong>) GetMemoryReaderAndRootAddressesFromProcessSampleFile(byte[] processSampleFile)
                {
                    var processSampleId = Pine.CommonConversion.StringBase16FromByteArray(
                        Pine.CommonConversion.HashSHA256(processSampleFile));

                    Console.WriteLine($"Reading from process sample {processSampleId}.");

                    var processSampleUnpacked = ProcessSample.ProcessSampleFromZipArchive(processSampleFile);

                    var memoryReader = new MemoryReaderFromProcessSample(processSampleUnpacked.memoryRegions);

                    var searchUIRootsStopwatch = System.Diagnostics.Stopwatch.StartNew();

                    var memoryRegions =
                        processSampleUnpacked.memoryRegions
                        .Select(memoryRegion => (memoryRegion.baseAddress, length: memoryRegion.content.Value.Length))
                        .ToImmutableList();

                    var uiRootCandidatesAddresses =
                        EveOnline64.EnumeratePossibleAddressesForUIRootObjects(memoryRegions, memoryReader)
                        .ToImmutableList();

                    searchUIRootsStopwatch.Stop();

                    Console.WriteLine($"Found {uiRootCandidatesAddresses.Count} candidates for UIRoot in {(int)searchUIRootsStopwatch.Elapsed.TotalSeconds} seconds: " + string.Join(",", uiRootCandidatesAddresses.Select(address => $"0x{address:X}")));

                    return (memoryReader, uiRootCandidatesAddresses);
                }

                (IMemoryReader, IImmutableList<ulong>) GetMemoryReaderAndWithSpecifiedRootFromProcessSampleFile(byte[] processSampleFile, ulong rootAddress)
                {
                    var processSampleId = Pine.CommonConversion.StringBase16FromByteArray(
                        Pine.CommonConversion.HashSHA256(processSampleFile));

                    Console.WriteLine($"Reading from process sample {processSampleId}.");

                    var processSampleUnpacked = ProcessSample.ProcessSampleFromZipArchive(processSampleFile);

                    var memoryReader = new MemoryReaderFromProcessSample(processSampleUnpacked.memoryRegions);

                    Console.WriteLine($"Reading UIRoot from specified address: {rootAddress}");

                    return (memoryReader, ImmutableList<ulong>.Empty.Add(rootAddress));
                }

                (IMemoryReader, IImmutableList<ulong>) GetMemoryReaderAndRootAddresses()
                {
                    if (processId.HasValue)
                    {
                        if (0 < rootAddressArgument?.Length)
                        {
                            return (new MemoryReaderFromLiveProcess(processId.Value), ImmutableList.Create(ParseULong(rootAddressArgument)));
                        }

                        return GetMemoryReaderAndRootAddressesFromProcessSampleFile(GetProcessSampleFileFromProcessId(processId.Value));
                    }

                    if (!(0 < sourceFileArgument?.Length))
                    {
                        throw new Exception("Where should I read from?");
                    }

                    if (0 < rootAddressArgument?.Length)
                    {
                        return GetMemoryReaderAndWithSpecifiedRootFromProcessSampleFile(System.IO.File.ReadAllBytes(sourceFileArgument), ParseULong(rootAddressArgument));
                    }

                    return GetMemoryReaderAndRootAddressesFromProcessSampleFile(System.IO.File.ReadAllBytes(sourceFileArgument));
                }

                var (memoryReader, uiRootCandidatesAddresses) = GetMemoryReaderAndRootAddresses();

                IImmutableList<UITreeNode> ReadUITrees() =>
                    uiRootCandidatesAddresses
                    .Select(uiTreeRoot => EveOnline64.ReadUITreeFromAddress(uiTreeRoot, memoryReader, 99))
                    .Where(uiTree => uiTree != null)
                    .ToImmutableList();

                if (warmupIterationsArgument != null)
                {
                    var iterations = int.Parse(warmupIterationsArgument);

                    Console.WriteLine("Performing " + iterations + " warmup iterations...");

                    for (var i = 0; i < iterations; i++)
                    {
                        ReadUITrees().ToList();
                        System.Threading.Thread.Sleep(1111);
                    }
                }

                var readUiTreesStopwatch = System.Diagnostics.Stopwatch.StartNew();

                var uiTrees = ReadUITrees();

                readUiTreesStopwatch.Stop();

                var uiTreesWithStats =
                    uiTrees
                    .Select(uiTree =>
                    new
                    {
                        uiTree = uiTree,
                        nodeCount = uiTree.EnumerateSelfAndDescendants().Count()
                    })
                    .OrderByDescending(uiTreeWithStats => uiTreeWithStats.nodeCount)
                    .ToImmutableList();

                var uiTreesReport =
                    uiTreesWithStats
                    .Select(uiTreeWithStats => $"\n0x{uiTreeWithStats.uiTree.pythonObjectAddress:X}: {uiTreeWithStats.nodeCount} nodes.")
                    .ToImmutableList();

                Console.WriteLine($"Read {uiTrees.Count} UI trees in {(int)readUiTreesStopwatch.Elapsed.TotalMilliseconds} milliseconds:" + string.Join("", uiTreesReport));

                var largestUiTree =
                    uiTreesWithStats
                    .OrderByDescending(uiTreeWithStats => uiTreeWithStats.nodeCount)
                    .FirstOrDefault().uiTree;

                if (largestUiTree != null)
                {
                    var uiTreePreparedForFile = largestUiTree;

                    if (removeOtherDictEntriesArgument)
                    {
                        uiTreePreparedForFile = uiTreePreparedForFile.WithOtherDictEntriesRemoved();
                    }

                    var serializeStopwatch = System.Diagnostics.Stopwatch.StartNew();

                    var uiTreeAsJson = EveOnline64.SerializeMemoryReadingNodeToJson(uiTreePreparedForFile);

                    serializeStopwatch.Stop();

                    Console.WriteLine(
                        "Serialized largest tree to " + uiTreeAsJson.Length + " characters of JSON in " +
                        serializeStopwatch.ElapsedMilliseconds + " milliseconds.");

                    var fileContent = System.Text.Encoding.UTF8.GetBytes(uiTreeAsJson);

                    var sampleId = Pine.CommonConversion.StringBase16FromByteArray(
                        Pine.CommonConversion.HashSHA256(fileContent));

                    var outputFilePath = outputFileArgument;

                    if (!(0 < outputFileArgument?.Length))
                    {
                        var outputFileName = "eve-online-memory-reading-" + sampleId[..10] + ".json";

                        outputFilePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), outputFileName);

                        Console.WriteLine(
                            "I found no configuration of an output file path, so I use '" +
                            outputFilePath + "' as the default.");
                    }

                    System.IO.File.WriteAllBytes(outputFilePath, fileContent);

                    Console.WriteLine($"I saved memory reading {sampleId} from address 0x{largestUiTree.pythonObjectAddress:X} to file '{outputFilePath}'.");
                }
                else
                {
                    Console.WriteLine("No largest UI tree.");
                }
            });
        });

        app.OnExecute(() =>
        {
            Console.WriteLine("Please specify a subcommand.");
            app.ShowHelp();

            return 1;
        });

        return app.Execute(args);
    }

    static byte[] GetProcessSampleFileFromProcessId(int processId)
    {
        var process = System.Diagnostics.Process.GetProcessById(processId);

        var beginMainWindowClientAreaScreenshotBmp = BMPFileFromBitmap(GetScreenshotOfWindowClientAreaAsBitmap(process.MainWindowHandle));

        var (committedRegions, logEntries) = EveOnline64.ReadCommittedMemoryRegionsWithContentFromProcessId(processId);

        var endMainWindowClientAreaScreenshotBmp = BMPFileFromBitmap(GetScreenshotOfWindowClientAreaAsBitmap(process.MainWindowHandle));

        return ProcessSample.ZipArchiveFromProcessSample(
            committedRegions,
            logEntries,
            beginMainWindowClientAreaScreenshotBmp: beginMainWindowClientAreaScreenshotBmp,
            endMainWindowClientAreaScreenshotBmp: endMainWindowClientAreaScreenshotBmp);
    }

    //  Screenshot implementation found at https://github.com/Viir/bots/blob/225c680115328d9ba0223760cec85d56f2ea9a87/implement/templates/locate-object-in-window/src/BotEngine/VolatileHostWindowsApi.elm#L479-L557

    static public byte[] BMPFileFromBitmap(System.Drawing.Bitmap bitmap)
    {
        using var stream = new System.IO.MemoryStream();

        bitmap.Save(stream, format: System.Drawing.Imaging.ImageFormat.Bmp);
        return stream.ToArray();
    }

    static public int[][] GetScreenshotOfWindowAsPixelsValuesR8G8B8(IntPtr windowHandle)
    {
        var screenshotAsBitmap = GetScreenshotOfWindowAsBitmap(windowHandle);
        if (screenshotAsBitmap == null)
            return null;
        var bitmapData = screenshotAsBitmap.LockBits(
            new System.Drawing.Rectangle(0, 0, screenshotAsBitmap.Width, screenshotAsBitmap.Height),
            System.Drawing.Imaging.ImageLockMode.ReadOnly,
            System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        int byteCount = bitmapData.Stride * screenshotAsBitmap.Height;
        byte[] pixelsArray = new byte[byteCount];
        IntPtr ptrFirstPixel = bitmapData.Scan0;
        Marshal.Copy(ptrFirstPixel, pixelsArray, 0, pixelsArray.Length);
        screenshotAsBitmap.UnlockBits(bitmapData);
        var pixels = new int[screenshotAsBitmap.Height][];
        for (var rowIndex = 0; rowIndex < screenshotAsBitmap.Height; ++rowIndex)
        {
            var rowPixelValues = new int[screenshotAsBitmap.Width];
            for (var columnIndex = 0; columnIndex < screenshotAsBitmap.Width; ++columnIndex)
            {
                var pixelBeginInArray = bitmapData.Stride * rowIndex + columnIndex * 3;
                var red = pixelsArray[pixelBeginInArray + 2];
                var green = pixelsArray[pixelBeginInArray + 1];
                var blue = pixelsArray[pixelBeginInArray + 0];
                rowPixelValues[columnIndex] = (red << 16) | (green << 8) | blue;
            }
            pixels[rowIndex] = rowPixelValues;
        }
        return pixels;
    }

    //  https://github.com/Viir/bots/blob/225c680115328d9ba0223760cec85d56f2ea9a87/implement/templates/locate-object-in-window/src/BotEngine/VolatileHostWindowsApi.elm#L535-L557
    static public System.Drawing.Bitmap GetScreenshotOfWindowAsBitmap(IntPtr windowHandle)
    {
        SetProcessDPIAware();
        var windowRect = new WinApi.Rect();
        if (WinApi.GetWindowRect(windowHandle, ref windowRect) == IntPtr.Zero)
            return null;
        int width = windowRect.right - windowRect.left;
        int height = windowRect.bottom - windowRect.top;
        var asBitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        System.Drawing.Graphics.FromImage(asBitmap).CopyFromScreen(
            windowRect.left,
            windowRect.top,
            0,
            0,
            new System.Drawing.Size(width, height),
            System.Drawing.CopyPixelOperation.SourceCopy);
        return asBitmap;
    }

    static public System.Drawing.Bitmap GetScreenshotOfWindowClientAreaAsBitmap(IntPtr windowHandle)
    {
        SetProcessDPIAware();

        var clientRect = new WinApi.Rect();

        if (WinApi.GetClientRect(windowHandle, ref clientRect) == IntPtr.Zero)
            return null;

        var clientRectLeftTop = new WinApi.Point { x = clientRect.left, y = clientRect.top };
        var clientRectRightBottom = new WinApi.Point { x = clientRect.right, y = clientRect.bottom };

        WinApi.ClientToScreen(windowHandle, ref clientRectLeftTop);
        WinApi.ClientToScreen(windowHandle, ref clientRectRightBottom);

        clientRect = new WinApi.Rect
        {
            left = clientRectLeftTop.x,
            top = clientRectLeftTop.y,
            right = clientRectRightBottom.x,
            bottom = clientRectRightBottom.y
        };

        int width = clientRect.right - clientRect.left;
        int height = clientRect.bottom - clientRect.top;
        var asBitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        System.Drawing.Graphics.FromImage(asBitmap).CopyFromScreen(
            clientRect.left,
            clientRect.top,
            0,
            0,
            new System.Drawing.Size(width, height),
            System.Drawing.CopyPixelOperation.SourceCopy);
        return asBitmap;
    }

    static void SetProcessDPIAware()
    {
        //  https://www.google.com/search?q=GetWindowRect+dpi
        //  https://github.com/dotnet/wpf/issues/859
        //  https://github.com/dotnet/winforms/issues/135
        WinApi.SetProcessDPIAware();
    }

    static ulong ParseULong(string asString)
    {
        if (asString.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
            return ulong.Parse(asString[2..], System.Globalization.NumberStyles.HexNumber);

        return ulong.Parse(asString);
    }
}

public class EveOnline64
{
    static public IImmutableList<ulong> EnumeratePossibleAddressesForUIRootObjectsFromProcessId(int processId)
    {
        var memoryReader = new MemoryReaderFromLiveProcess(processId);

        var (committedMemoryRegions, _) = ReadCommittedMemoryRegionsWithoutContentFromProcessId(processId);

        return EnumeratePossibleAddressesForUIRootObjects(committedMemoryRegions, memoryReader);
    }

    static public (IImmutableList<SampleMemoryRegion> memoryRegions, IImmutableList<string> logEntries) ReadCommittedMemoryRegionsWithContentFromProcessId(int processId)
    {
        var genericResult = ReadCommittedMemoryRegionsFromProcessId(processId, readContent: true);

        return genericResult;
    }

    static public (IImmutableList<(ulong baseAddress, int length)> memoryRegions, IImmutableList<string> logEntries) ReadCommittedMemoryRegionsWithoutContentFromProcessId(int processId)
    {
        var genericResult = ReadCommittedMemoryRegionsFromProcessId(processId, readContent: false);

        var memoryRegions =
            genericResult.memoryRegions
            .Select(memoryRegion => (baseAddress: memoryRegion.baseAddress, length: (int)memoryRegion.length))
            .ToImmutableList();

        return (memoryRegions, genericResult.logEntries);
    }

    static public (IImmutableList<SampleMemoryRegion> memoryRegions, IImmutableList<string> logEntries) ReadCommittedMemoryRegionsFromProcessId(
        int processId,
        bool readContent)
    {
        var logEntries = new List<string>();

        void logLine(string lineText)
        {
            logEntries.Add(lineText);
            //  Console.WriteLine(lineText);
        }

        logLine("Reading from process " + processId + ".");

        var processHandle = WinApi.OpenProcess(
            (int)(WinApi.ProcessAccessFlags.QueryInformation | WinApi.ProcessAccessFlags.VirtualMemoryRead), false, processId);

        long address = 0;

        var committedRegions = new List<SampleMemoryRegion>();

        do
        {
            int result = WinApi.VirtualQueryEx(
                processHandle,
                (IntPtr)address,
                out WinApi.MEMORY_BASIC_INFORMATION64 m,
                (uint)Marshal.SizeOf(typeof(WinApi.MEMORY_BASIC_INFORMATION64)));

            var regionProtection = (WinApi.MemoryInformationProtection)m.Protect;

            logLine($"{m.BaseAddress}-{(uint)m.BaseAddress + (uint)m.RegionSize - 1} : {m.RegionSize} bytes result={result}, state={(WinApi.MemoryInformationState)m.State}, type={(WinApi.MemoryInformationType)m.Type}, protection={regionProtection}");

            if (address == (long)m.BaseAddress + (long)m.RegionSize)
                break;

            address = (long)m.BaseAddress + (long)m.RegionSize;

            if (m.State != (int)WinApi.MemoryInformationState.MEM_COMMIT)
                continue;

            var protectionFlagsToSkip = WinApi.MemoryInformationProtection.PAGE_GUARD | WinApi.MemoryInformationProtection.PAGE_NOACCESS;

            var matchingFlagsToSkip = protectionFlagsToSkip & regionProtection;

            if (matchingFlagsToSkip != 0)
            {
                logLine($"Skipping region beginning at {m.BaseAddress:X} as it has flags {matchingFlagsToSkip}.");
                continue;
            }

            var regionBaseAddress = m.BaseAddress;

            byte[] regionContent = null;

            if (readContent)
            {
                UIntPtr bytesRead = UIntPtr.Zero;
                var regionContentBuffer = new byte[(long)m.RegionSize];

                WinApi.ReadProcessMemory(processHandle, regionBaseAddress, regionContentBuffer, (UIntPtr)regionContentBuffer.LongLength, ref bytesRead);

                if (bytesRead.ToUInt64() != (ulong)regionContentBuffer.LongLength)
                    throw new Exception($"Failed to ReadProcessMemory at 0x{regionBaseAddress:X}: Only read " + bytesRead + " bytes.");

                regionContent = regionContentBuffer;
            }

            committedRegions.Add(new SampleMemoryRegion(
                baseAddress: regionBaseAddress,
                length: m.RegionSize,
                content: regionContent));

        } while (true);

        logLine($"Found {committedRegions.Count} committed regions with a total size of {committedRegions.Select(region => (long)region.length).Sum()}.");

        return (committedRegions.ToImmutableList(), logEntries.ToImmutableList());
    }

    static public IImmutableList<ulong> EnumeratePossibleAddressesForUIRootObjects(
        IEnumerable<(ulong baseAddress, int length)> memoryRegions,
        IMemoryReader memoryReader)
    {
        var memoryRegionsOrderedByAddress =
            memoryRegions
            .OrderBy(memoryRegion => memoryRegion.baseAddress)
            .ToImmutableArray();

        string ReadNullTerminatedAsciiStringFromAddressUpTo255(ulong address)
        {
            var asMemory = memoryReader.ReadBytes(address, 0x100);

            if (asMemory == null)
                return null;

            var asSpan = asMemory.Value.Span;

            var length = 0;

            for (var i = 0; i < asSpan.Length; ++i)
            {
                length = i;

                if (asSpan[i] == 0)
                    break;
            }

            return System.Text.Encoding.ASCII.GetString(asSpan[..length]);
        }

        ReadOnlyMemory<ulong>? ReadMemoryRegionContentAsULongArray((ulong baseAddress, int length) memoryRegion)
        {
            var asByteArray = memoryReader.ReadBytes(memoryRegion.baseAddress, memoryRegion.length);

            if (asByteArray == null)
                return null;

            return TransformMemoryContent.AsULongMemory(asByteArray.Value);
        }

        IEnumerable<ulong> EnumerateCandidatesForPythonTypeObjectType()
        {
            IEnumerable<ulong> EnumerateCandidatesForPythonTypeObjectTypeInMemoryRegion((ulong baseAddress, int length) memoryRegion)
            {
                var memoryRegionContentAsULongArray = ReadMemoryRegionContentAsULongArray(memoryRegion);

                if (memoryRegionContentAsULongArray == null)
                    yield break;

                for (var candidateAddressIndex = 0; candidateAddressIndex < memoryRegionContentAsULongArray.Value.Length - 4; ++candidateAddressIndex)
                {
                    var candidateAddressInProcess = memoryRegion.baseAddress + (ulong)candidateAddressIndex * 8;

                    var candidate_ob_type = memoryRegionContentAsULongArray.Value.Span[candidateAddressIndex + 1];

                    if (candidate_ob_type != candidateAddressInProcess)
                        continue;

                    var candidate_tp_name =
                        ReadNullTerminatedAsciiStringFromAddressUpTo255(
                            memoryRegionContentAsULongArray.Value.Span[candidateAddressIndex + 3]);

                    if (candidate_tp_name != "type")
                        continue;

                    yield return candidateAddressInProcess;
                }
            }

            return
                memoryRegionsOrderedByAddress
                .AsParallel()
                .WithDegreeOfParallelism(2)
                .SelectMany(EnumerateCandidatesForPythonTypeObjectTypeInMemoryRegion)
                .ToImmutableArray();
        }

        IEnumerable<(ulong address, string tp_name)> EnumerateCandidatesForPythonTypeObjects(
            IImmutableList<ulong> typeObjectCandidatesAddresses)
        {
            if (typeObjectCandidatesAddresses.Count < 1)
                yield break;

            var typeAddressMin = typeObjectCandidatesAddresses.Min();
            var typeAddressMax = typeObjectCandidatesAddresses.Max();

            foreach (var memoryRegion in memoryRegionsOrderedByAddress)
            {
                var memoryRegionContentAsULongArray = ReadMemoryRegionContentAsULongArray(memoryRegion);

                if (memoryRegionContentAsULongArray == null)
                    continue;

                for (var candidateAddressIndex = 0; candidateAddressIndex < memoryRegionContentAsULongArray.Value.Length - 4; ++candidateAddressIndex)
                {
                    var candidateAddressInProcess = memoryRegion.baseAddress + (ulong)candidateAddressIndex * 8;

                    var candidate_ob_type = memoryRegionContentAsULongArray.Value.Span[candidateAddressIndex + 1];

                    {
                        //  This check is redundant with the following one. It just implements a specialization to optimize runtime expenses.
                        if (candidate_ob_type < typeAddressMin || typeAddressMax < candidate_ob_type)
                            continue;
                    }

                    if (!typeObjectCandidatesAddresses.Contains(candidate_ob_type))
                        continue;

                    var candidate_tp_name =
                        ReadNullTerminatedAsciiStringFromAddressUpTo255(
                            memoryRegionContentAsULongArray.Value.Span[candidateAddressIndex + 3]);

                    if (candidate_tp_name == null)
                        continue;

                    yield return (candidateAddressInProcess, candidate_tp_name);
                }
            }
        }

        IEnumerable<ulong> EnumerateCandidatesForInstancesOfPythonType(
            IImmutableList<ulong> typeObjectCandidatesAddresses)
        {
            if (typeObjectCandidatesAddresses.Count < 1)
                yield break;

            var typeAddressMin = typeObjectCandidatesAddresses.Min();
            var typeAddressMax = typeObjectCandidatesAddresses.Max();

            foreach (var memoryRegion in memoryRegionsOrderedByAddress)
            {
                var memoryRegionContentAsULongArray = ReadMemoryRegionContentAsULongArray(memoryRegion);

                if (memoryRegionContentAsULongArray == null)
                    continue;

                for (var candidateAddressIndex = 0; candidateAddressIndex < memoryRegionContentAsULongArray.Value.Length - 4; ++candidateAddressIndex)
                {
                    var candidateAddressInProcess = memoryRegion.baseAddress + (ulong)candidateAddressIndex * 8;

                    var candidate_ob_type = memoryRegionContentAsULongArray.Value.Span[candidateAddressIndex + 1];

                    {
                        //  This check is redundant with the following one. It just implements a specialization to reduce processing time.
                        if (candidate_ob_type < typeAddressMin || typeAddressMax < candidate_ob_type)
                            continue;
                    }

                    if (!typeObjectCandidatesAddresses.Contains(candidate_ob_type))
                        continue;

                    yield return candidateAddressInProcess;
                }
            }
        }

        var uiRootTypeObjectCandidatesAddresses =
            EnumerateCandidatesForPythonTypeObjects(EnumerateCandidatesForPythonTypeObjectType().ToImmutableList())
            .Where(typeObject => typeObject.tp_name == "UIRoot")
            .Select(typeObject => typeObject.address)
            .ToImmutableList();

        return
            EnumerateCandidatesForInstancesOfPythonType(uiRootTypeObjectCandidatesAddresses)
            .ToImmutableList();
    }

    struct PyDictEntry
    {
        public ulong hash;
        public ulong key;
        public ulong value;
    }

    static readonly IImmutableSet<string> DictEntriesOfInterestKeys = ImmutableHashSet.Create(
        "_top", "_left", "_width", "_height", "_displayX", "_displayY",
        "_displayHeight", "_displayWidth",
        "_name", "_text", "_setText",
        "children",
        "texturePath", "_bgTexturePath",
        "_hint", "_display",

        //  HPGauges
        "lastShield", "lastArmor", "lastStructure",

        //  Found in "ShipHudSpriteGauge"
        "_lastValue",

        //  Found in "ModuleButton"
        "ramp_active",

        //  Found in the Transforms contained in "ShipModuleButtonRamps"
        "_rotation",

        //  Found under OverviewEntry in Sprite named "iconSprite"
        "_color",

        //  Found in "SE_TextlineCore"
        "_sr",

        //  Found in "_sr" Bunch
        "htmlstr",

        // 2023-01-03 Sample with PhotonUI: process-sample-ebdfff96e7.zip
        "_texturePath", "_opacity", "_bgColor", "isExpanded"
    );

    struct LocalMemoryReadingTools
    {
        public IMemoryReader memoryReader;

        public Func<ulong, IImmutableDictionary<string, ulong>> getDictionaryEntriesWithStringKeys;

        public Func<ulong, string> GetPythonTypeNameFromPythonObjectAddress;

        public Func<ulong, object> GetDictEntryValueRepresentation;
    }

    static readonly IImmutableDictionary<string, Func<ulong, LocalMemoryReadingTools, object>> specializedReadingFromPythonType =
        ImmutableDictionary<string, Func<ulong, LocalMemoryReadingTools, object>>.Empty
        .Add("str", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_str))
        .Add("unicode", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_unicode))
        .Add("int", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_int))
        .Add("bool", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_bool))
        .Add("float", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_float))
        .Add("PyColor", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_PyColor))
        .Add("Bunch", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_Bunch));

    static object ReadingFromPythonType_str(ulong address, LocalMemoryReadingTools memoryReadingTools)
    {
        return ReadPythonStringValue(address, memoryReadingTools.memoryReader, 0x1000);
    }

    static object ReadingFromPythonType_unicode(ulong address, LocalMemoryReadingTools memoryReadingTools)
    {
        var pythonObjectMemory = memoryReadingTools.memoryReader.ReadBytes(address, 0x20);

        if (!(pythonObjectMemory?.Length == 0x20))
            return "Failed to read python object memory.";

        var unicode_string_length = BitConverter.ToUInt64(pythonObjectMemory.Value.Span[0x10..]);

        if (0x1000 < unicode_string_length)
            return "String too long.";

        var stringBytesCount = (int)unicode_string_length * 2;

        var stringBytes = memoryReadingTools.memoryReader.ReadBytes(
            BitConverter.ToUInt64(pythonObjectMemory.Value.Span[0x18..]), stringBytesCount);

        if (!(stringBytes?.Length == stringBytesCount))
            return "Failed to read string bytes.";

        return System.Text.Encoding.Unicode.GetString(stringBytes.Value.Span);
    }

    static object ReadingFromPythonType_int(ulong address, LocalMemoryReadingTools memoryReadingTools)
    {
        var intObjectMemory = memoryReadingTools.memoryReader.ReadBytes(address, 0x18);

        if (!(intObjectMemory?.Length == 0x18))
            return "Failed to read int object memory.";

        var value = BitConverter.ToInt64(intObjectMemory.Value.Span[0x10..]);

        var asInt32 = (int)value;

        if (asInt32 == value)
            return asInt32;

        return new
        {
            @int = value,
            int_low32 = asInt32,
        };
    }

    static object ReadingFromPythonType_bool(ulong address, LocalMemoryReadingTools memoryReadingTools)
    {
        var pythonObjectMemory = memoryReadingTools.memoryReader.ReadBytes(address, 0x18);

        if (!(pythonObjectMemory?.Length == 0x18))
            return "Failed to read python object memory.";

        return BitConverter.ToInt64(pythonObjectMemory.Value.Span[0x10..]) != 0;
    }

    static object ReadingFromPythonType_float(ulong address, LocalMemoryReadingTools memoryReadingTools)
    {
        return ReadPythonFloatObjectValue(address, memoryReadingTools.memoryReader);
    }

    static object ReadingFromPythonType_PyColor(ulong address, LocalMemoryReadingTools memoryReadingTools)
    {
        var pyColorObjectMemory = memoryReadingTools.memoryReader.ReadBytes(address, 0x18);

        if (!(pyColorObjectMemory?.Length == 0x18))
            return "Failed to read pyColorObjectMemory.";

        var dictionaryAddress = BitConverter.ToUInt64(pyColorObjectMemory.Value.Span[0x10..]);

        var dictionaryEntries = memoryReadingTools.getDictionaryEntriesWithStringKeys(dictionaryAddress);

        if (dictionaryEntries == null)
            return "Failed to read dictionary entries.";

        int? readValuePercentFromDictEntryKey(string dictEntryKey)
        {
            if (!dictionaryEntries.TryGetValue(dictEntryKey, out var valueAddress))
                return null;

            var valueAsFloat = ReadPythonFloatObjectValue(valueAddress, memoryReadingTools.memoryReader);

            if (!valueAsFloat.HasValue)
                return null;

            return (int)(valueAsFloat.Value * 100);
        }

        return new
        {
            aPercent = readValuePercentFromDictEntryKey("_a"),
            rPercent = readValuePercentFromDictEntryKey("_r"),
            gPercent = readValuePercentFromDictEntryKey("_g"),
            bPercent = readValuePercentFromDictEntryKey("_b"),
        };
    }

    static object ReadingFromPythonType_Bunch(ulong address, LocalMemoryReadingTools memoryReadingTools)
    {
        var dictionaryEntries = memoryReadingTools.getDictionaryEntriesWithStringKeys(address);

        if (dictionaryEntries == null)
            return "Failed to read dictionary entries.";

        var entriesOfInterest = new List<UITreeNode.DictEntry>();

        foreach (var entry in dictionaryEntries)
        {
            if (!DictEntriesOfInterestKeys.Contains(entry.Key))
            {
                continue;
            }

            entriesOfInterest.Add(new UITreeNode.DictEntry
            (
                key: entry.Key,
                value: memoryReadingTools.GetDictEntryValueRepresentation(entry.Value)
            ));
        }

        var entriesOfInterestJObject =
            new System.Text.Json.Nodes.JsonObject(
                entriesOfInterest.Select(dictEntry =>
                new KeyValuePair<string, System.Text.Json.Nodes.JsonNode?>
                    (dictEntry.key,
                    System.Text.Json.Nodes.JsonNode.Parse(SerializeMemoryReadingNodeToJson(dictEntry.value)))));

        return new UITreeNode.Bunch
        (
            entriesOfInterest: entriesOfInterestJObject
        );
    }


    class MemoryReadingCache
    {
        IDictionary<ulong, string> PythonTypeNameFromPythonObjectAddress;

        IDictionary<ulong, string> PythonStringValueMaxLength4000;

        IDictionary<ulong, object> DictEntryValueRepresentation;

        public MemoryReadingCache()
        {
            PythonTypeNameFromPythonObjectAddress = new Dictionary<ulong, string>();
            PythonStringValueMaxLength4000 = new Dictionary<ulong, string>();
            DictEntryValueRepresentation = new Dictionary<ulong, object>();
        }

        public string GetPythonTypeNameFromPythonObjectAddress(ulong address, Func<ulong, string> getFresh) =>
            GetFromCacheOrUpdate(PythonTypeNameFromPythonObjectAddress, address, getFresh);

        public string GetPythonStringValueMaxLength4000(ulong address, Func<ulong, string> getFresh) =>
            GetFromCacheOrUpdate(PythonStringValueMaxLength4000, address, getFresh);

        public object GetDictEntryValueRepresentation(ulong address, Func<ulong, object> getFresh) =>
            GetFromCacheOrUpdate(DictEntryValueRepresentation, address, getFresh);

        static TValue GetFromCacheOrUpdate<TKey, TValue>(IDictionary<TKey, TValue> cache, TKey key, Func<TKey, TValue> getFresh)
        {
            if (cache.TryGetValue(key, out var fromCache))
                return fromCache;

            var fresh = getFresh(key);

            cache[key] = fresh;
            return fresh;
        }
    }

    static public UITreeNode ReadUITreeFromAddress(ulong nodeAddress, IMemoryReader memoryReader, int maxDepth) =>
        ReadUITreeFromAddress(nodeAddress, memoryReader, maxDepth, null);

    static UITreeNode ReadUITreeFromAddress(ulong nodeAddress, IMemoryReader memoryReader, int maxDepth, MemoryReadingCache cache)
    {
        cache ??= new MemoryReadingCache();

        var uiNodeObjectMemory = memoryReader.ReadBytes(nodeAddress, 0x30);

        if (!(0x30 == uiNodeObjectMemory?.Length))
            return null;

        string getPythonTypeNameFromPythonTypeObjectAddress(ulong typeObjectAddress)
        {
            var typeObjectMemory = memoryReader.ReadBytes(typeObjectAddress, 0x20);

            if (!(typeObjectMemory?.Length == 0x20))
                return null;

            var tp_name = BitConverter.ToUInt64(typeObjectMemory.Value.Span[0x18..]);

            var nameBytes = memoryReader.ReadBytes(tp_name, 100)?.ToArray();

            if (!(nameBytes?.Contains((byte)0) ?? false))
                return null;

            return System.Text.Encoding.ASCII.GetString(nameBytes.TakeWhile(character => character != 0).ToArray());
        }

        string getPythonTypeNameFromPythonObjectAddress(ulong objectAddress)
        {
            return cache.GetPythonTypeNameFromPythonObjectAddress(objectAddress, objectAddress =>
            {
                var objectMemory = memoryReader.ReadBytes(objectAddress, 0x10);

                if (!(objectMemory?.Length == 0x10))
                    return null;

                return getPythonTypeNameFromPythonTypeObjectAddress(BitConverter.ToUInt64(objectMemory.Value.Span[8..]));
            });
        }

        string readPythonStringValueMaxLength4000(ulong strObjectAddress)
        {
            return cache.GetPythonStringValueMaxLength4000(
                strObjectAddress,
                strObjectAddress => ReadPythonStringValue(strObjectAddress, memoryReader, 4000));
        }

        PyDictEntry[] ReadActiveDictionaryEntriesFromDictionaryAddress(ulong dictionaryAddress)
        {
            /*
            Sources:
            https://github.com/python/cpython/blob/362ede2232107fc54d406bb9de7711ff7574e1d4/Include/dictobject.h
            https://github.com/python/cpython/blob/362ede2232107fc54d406bb9de7711ff7574e1d4/Objects/dictobject.c
            */

            var dictMemory = memoryReader.ReadBytes(dictionaryAddress, 0x30);

            //  Console.WriteLine($"dictMemory is {(dictMemory == null ? "not " : "")}ok for 0x{dictionaryAddress:X}");

            if (!(dictMemory?.Length == 0x30))
                return null;

            var dictMemoryAsLongMemory = TransformMemoryContent.AsULongMemory(dictMemory.Value);

            //  var dictTypeName = getPythonTypeNameFromObjectAddress(dictionaryAddress);

            //  Console.WriteLine($"Type name for dictionary 0x{dictionaryAddress:X} is '{dictTypeName}'.");

            //  https://github.com/python/cpython/blob/362ede2232107fc54d406bb9de7711ff7574e1d4/Include/dictobject.h#L60-L89

            var ma_fill = dictMemoryAsLongMemory.Span[2];
            var ma_used = dictMemoryAsLongMemory.Span[3];
            var ma_mask = dictMemoryAsLongMemory.Span[4];
            var ma_table = dictMemoryAsLongMemory.Span[5];

            //  Console.WriteLine($"Details for dictionary 0x{dictionaryAddress:X}: type_name = '{dictTypeName}' ma_mask = 0x{ma_mask:X}, ma_table = 0x{ma_table:X}.");

            var numberOfSlots = (int)ma_mask + 1;

            if (numberOfSlots < 0 || 10_000 < numberOfSlots)
            {
                //  Avoid stalling the whole reading process when a single dictionary contains garbage.
                return null;
            }

            var slotsMemorySize = numberOfSlots * 8 * 3;

            var slotsMemory = memoryReader.ReadBytes(ma_table, slotsMemorySize);

            //  Console.WriteLine($"slotsMemory (0x{ma_table:X}) has length of {slotsMemory?.Length} and is {(slotsMemory?.Length == slotsMemorySize ? "" : "not ")}ok for 0x{dictionaryAddress:X}");

            if (!(slotsMemory?.Length == slotsMemorySize))
                return null;

            var slotsMemoryAsLongMemory = TransformMemoryContent.AsULongMemory(slotsMemory.Value);

            var entries = new List<PyDictEntry>();

            for (var slotIndex = 0; slotIndex < numberOfSlots; ++slotIndex)
            {
                var hash = slotsMemoryAsLongMemory.Span[slotIndex * 3];
                var key = slotsMemoryAsLongMemory.Span[slotIndex * 3 + 1];
                var value = slotsMemoryAsLongMemory.Span[slotIndex * 3 + 2];

                if (key == 0 || value == 0)
                    continue;

                entries.Add(new PyDictEntry { hash = hash, key = key, value = value });
            }

            return [.. entries];
        }

        IImmutableDictionary<string, ulong> GetDictionaryEntriesWithStringKeys(ulong dictionaryObjectAddress)
        {
            var dictionaryEntries = ReadActiveDictionaryEntriesFromDictionaryAddress(dictionaryObjectAddress);

            if (dictionaryEntries == null)
                return null;

            return
                dictionaryEntries
                .Select(entry => new { key = readPythonStringValueMaxLength4000(entry.key), value = entry.value })
                .Aggregate(
                    seed: ImmutableDictionary<string, ulong>.Empty,
                    func: (dict, entry) => dict.SetItem(entry.key, entry.value));
        }

        var localMemoryReadingTools = new LocalMemoryReadingTools
        {
            memoryReader = memoryReader,
            getDictionaryEntriesWithStringKeys = GetDictionaryEntriesWithStringKeys,
            GetPythonTypeNameFromPythonObjectAddress = getPythonTypeNameFromPythonObjectAddress,
        };

        var pythonObjectTypeName = getPythonTypeNameFromPythonObjectAddress(nodeAddress);

        if (!(0 < pythonObjectTypeName?.Length))
            return null;

        var dictAddress = BitConverter.ToUInt64(uiNodeObjectMemory.Value.Span[0x10..]);

        var dictionaryEntries = ReadActiveDictionaryEntriesFromDictionaryAddress(dictAddress);

        if (dictionaryEntries == null)
            return null;

        var dictEntriesOfInterest = new List<UITreeNode.DictEntry>();

        var otherDictEntriesKeys = new List<string>();

        object GetDictEntryValueRepresentation(ulong valueOjectAddress)
        {
            return cache.GetDictEntryValueRepresentation(valueOjectAddress, valueOjectAddress =>
            {
                var value_pythonTypeName = getPythonTypeNameFromPythonObjectAddress(valueOjectAddress);

                var genericRepresentation = new UITreeNode.DictEntryValueGenericRepresentation
                (
                    address: valueOjectAddress,
                    pythonObjectTypeName: value_pythonTypeName
                );

                if (value_pythonTypeName == null)
                    return genericRepresentation;

                specializedReadingFromPythonType.TryGetValue(value_pythonTypeName, out var specializedRepresentation);

                if (specializedRepresentation == null)
                    return genericRepresentation;

                return specializedRepresentation(genericRepresentation.address, localMemoryReadingTools);
            });
        }

        localMemoryReadingTools.GetDictEntryValueRepresentation = GetDictEntryValueRepresentation;

        foreach (var dictionaryEntry in dictionaryEntries)
        {
            var keyObject_type_name = getPythonTypeNameFromPythonObjectAddress(dictionaryEntry.key);

            //  Console.WriteLine($"Dict entry type name is '{keyObject_type_name}'");

            if (keyObject_type_name != "str")
                continue;

            var keyString = readPythonStringValueMaxLength4000(dictionaryEntry.key);

            if (!DictEntriesOfInterestKeys.Contains(keyString))
            {
                otherDictEntriesKeys.Add(keyString);
                continue;
            }

            dictEntriesOfInterest.Add(new UITreeNode.DictEntry
            (
                key: keyString,
                value: GetDictEntryValueRepresentation(dictionaryEntry.value)
            ));
        }

        {
            var _displayDictEntry = dictEntriesOfInterest.FirstOrDefault(entry => entry.key == "_display");

            if (_displayDictEntry != null && (_displayDictEntry.value is bool displayAsBool))
                if (!displayAsBool)
                    return null;
        }

        UITreeNode[] ReadChildren()
        {
            if (maxDepth < 1)
                return null;

            //  https://github.com/Arcitectus/Sanderling/blob/b07769fb4283e401836d050870121780f5f37910/guide/image/2015-01.eve-online-python-ui-tree-structure.png

            var childrenDictEntry = dictEntriesOfInterest.FirstOrDefault(entry => entry.key == "children");

            if (childrenDictEntry == null)
                return null;

            var childrenEntryObjectAddress =
                ((UITreeNode.DictEntryValueGenericRepresentation)childrenDictEntry.value).address;

            //  Console.WriteLine($"'children' dict entry of 0x{nodeAddress:X} points to 0x{childrenEntryObjectAddress:X}.");

            var pyChildrenListMemory = memoryReader.ReadBytes(childrenEntryObjectAddress, 0x18);

            if (!(pyChildrenListMemory?.Length == 0x18))
                return null;

            var pyChildrenDictAddress = BitConverter.ToUInt64(pyChildrenListMemory.Value.Span[0x10..]);

            var pyChildrenDictEntries = ReadActiveDictionaryEntriesFromDictionaryAddress(pyChildrenDictAddress);

            //  Console.WriteLine($"Found {(pyChildrenDictEntries == null ? "no" : "some")} children dictionary entries for 0x{nodeAddress:X}");

            if (pyChildrenDictEntries == null)
                return null;

            var childrenEntry =
                pyChildrenDictEntries
                .FirstOrDefault(dictionaryEntry =>
                {
                    if (getPythonTypeNameFromPythonObjectAddress(dictionaryEntry.key) != "str")
                        return false;

                    var keyString = readPythonStringValueMaxLength4000(dictionaryEntry.key);

                    return keyString == "_childrenObjects";
                });

            //  Console.WriteLine($"Found {(childrenEntry.value == 0 ? "no" : "a")} dictionary entry for children of 0x{nodeAddress:X}");

            if (childrenEntry.value == 0)
                return null;

            var pythonListObjectMemory = memoryReader.ReadBytes(childrenEntry.value, 0x20);

            if (!(pythonListObjectMemory?.Length == 0x20))
                return null;

            //  https://github.com/python/cpython/blob/362ede2232107fc54d406bb9de7711ff7574e1d4/Include/listobject.h

            var list_ob_size = BitConverter.ToUInt64(pythonListObjectMemory.Value.Span[0x10..]);

            if (4000 < list_ob_size)
                return null;

            var listEntriesSize = (int)list_ob_size * 8;

            var list_ob_item = BitConverter.ToUInt64(pythonListObjectMemory.Value.Span[0x18..]);

            var listEntriesMemory = memoryReader.ReadBytes(list_ob_item, listEntriesSize);

            if (!(listEntriesMemory?.Length == listEntriesSize))
                return null;

            var listEntries = TransformMemoryContent.AsULongMemory(listEntriesMemory.Value);

            //  Console.WriteLine($"Found {listEntries.Length} children entries for 0x{nodeAddress:X}: " + String.Join(", ", listEntries.Select(childAddress => $"0x{childAddress:X}").ToArray()));

            return
                 listEntries
                 .ToArray()
                 .Select(childAddress => ReadUITreeFromAddress(childAddress, memoryReader, maxDepth - 1, cache))
                 .ToArray();
        }

        var dictEntriesOfInterestLessNoneType =
            dictEntriesOfInterest
            .Where(entry => !((entry.value as UITreeNode.DictEntryValueGenericRepresentation)?.pythonObjectTypeName == "NoneType"))
            .ToArray();

        var dictEntriesOfInterestDict =
            dictEntriesOfInterestLessNoneType.Aggregate(
                seed: ImmutableDictionary<string, object>.Empty,
                func: (dict, entry) => dict.SetItem(entry.key, entry.value));

        return new UITreeNode
        (
            pythonObjectAddress: nodeAddress,
            pythonObjectTypeName: pythonObjectTypeName,
            dictEntriesOfInterest: dictEntriesOfInterestDict,
            otherDictEntriesKeys: [.. otherDictEntriesKeys],
            children: ReadChildren()?.Where(child => child != null)?.ToArray()
        );
    }

    static string ReadPythonStringValue(ulong stringObjectAddress, IMemoryReader memoryReader, int maxLength)
    {
        //  https://github.com/python/cpython/blob/362ede2232107fc54d406bb9de7711ff7574e1d4/Include/stringobject.h

        var stringObjectMemory = memoryReader.ReadBytes(stringObjectAddress, 0x20);

        if (!(stringObjectMemory?.Length == 0x20))
            return "Failed to read string object memory.";

        var stringObject_ob_size = BitConverter.ToUInt64(stringObjectMemory.Value.Span[0x10..]);

        if (0 < maxLength && maxLength < (int)stringObject_ob_size || int.MaxValue < stringObject_ob_size)
            return "String too long.";

        var stringBytes = memoryReader.ReadBytes(stringObjectAddress + 8 * 4, (int)stringObject_ob_size);

        if (!(stringBytes?.Length == (int)stringObject_ob_size))
            return "Failed to read string bytes.";

        return System.Text.Encoding.ASCII.GetString(stringBytes.Value.Span);
    }

    static double? ReadPythonFloatObjectValue(ulong floatObjectAddress, IMemoryReader memoryReader)
    {
        //  https://github.com/python/cpython/blob/362ede2232107fc54d406bb9de7711ff7574e1d4/Include/floatobject.h

        var pythonObjectMemory = memoryReader.ReadBytes(floatObjectAddress, 0x20);

        if (!(pythonObjectMemory?.Length == 0x20))
            return null;

        return BitConverter.ToDouble(pythonObjectMemory.Value.Span[0x10..]);
    }

    static public string SerializeMemoryReadingNodeToJson(object obj) =>
        System.Text.Json.JsonSerializer.Serialize(obj, MemoryReadingJsonSerializerOptions);

    static public System.Text.Json.JsonSerializerOptions MemoryReadingJsonSerializerOptions =>
        new()
        {
            Converters =
            {
                //  Support common JSON parsers: Wrap large integers in a string to work around limitations there. (https://discourse.elm-lang.org/t/how-to-parse-a-json-object/4977)
                new Int64JsonConverter(),
                new UInt64JsonConverter()
            }
        };
}


public interface IMemoryReader
{
    ReadOnlyMemory<byte>? ReadBytes(ulong startAddress, int length);
}

public class MemoryReaderFromProcessSample : IMemoryReader
{
    readonly IImmutableList<SampleMemoryRegion> memoryRegionsOrderedByAddress;

    public MemoryReaderFromProcessSample(IImmutableList<SampleMemoryRegion> memoryRegions)
    {
        memoryRegionsOrderedByAddress =
            memoryRegions
            .OrderBy(memoryRegion => memoryRegion.baseAddress)
            .ToImmutableList();
    }

    public ReadOnlyMemory<byte>? ReadBytes(ulong startAddress, int length)
    {
        var memoryRegion =
            memoryRegionsOrderedByAddress
            .Where(region => region.baseAddress <= startAddress)
            .LastOrDefault();

        if (memoryRegion?.content == null)
            return null;

        var start = startAddress - memoryRegion.baseAddress;

        if ((int)start < 0)
            return null;

        if (memoryRegion.content.Value.Length <= (int)start)
            return null;

        return
            memoryRegion?.content?.Slice((int)start, Math.Min(length, memoryRegion.content.Value.Length - (int)start));
    }
}


public class MemoryReaderFromLiveProcess : IMemoryReader, IDisposable
{
    readonly IntPtr processHandle;

    public MemoryReaderFromLiveProcess(int processId)
    {
        processHandle = WinApi.OpenProcess(
            (int)(WinApi.ProcessAccessFlags.QueryInformation | WinApi.ProcessAccessFlags.VirtualMemoryRead), false, processId);
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

        if (numberOfBytesRead == 0)
            return null;

        if (int.MaxValue < numberOfBytesRead)
            return null;

        if (numberOfBytesRead == (ulong)buffer.LongLength)
            return buffer;

        return buffer;
    }
}

static public class WinApi
{
    [DllImport("kernel32.dll")]
    static public extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    static public extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION64 lpBuffer, uint dwLength);

    [DllImport("kernel32.dll")]
    static public extern bool ReadProcessMemory(IntPtr hProcess, ulong lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, ref UIntPtr lpNumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    static public extern bool CloseHandle(IntPtr hHandle);

    [DllImport("user32.dll", SetLastError = true)]
    static public extern bool SetProcessDPIAware();

    [DllImport("user32.dll")]
    static public extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

    [DllImport("user32.dll")]
    static public extern IntPtr GetClientRect(IntPtr hWnd, ref Rect rect);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static public extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public int x;

        public int y;
    }

    //  http://www.pinvoke.net/default.aspx/kernel32.virtualqueryex
    //  https://docs.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-memory_basic_information
    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORY_BASIC_INFORMATION64
    {
        public ulong BaseAddress;
        public ulong AllocationBase;
        public int AllocationProtect;
        public int __alignment1;
        public ulong RegionSize;
        public int State;
        public int Protect;
        public int Type;
        public int __alignment2;
    }

    public enum AllocationProtect : uint
    {
        PAGE_EXECUTE = 0x00000010,
        PAGE_EXECUTE_READ = 0x00000020,
        PAGE_EXECUTE_READWRITE = 0x00000040,
        PAGE_EXECUTE_WRITECOPY = 0x00000080,
        PAGE_NOACCESS = 0x00000001,
        PAGE_READONLY = 0x00000002,
        PAGE_READWRITE = 0x00000004,
        PAGE_WRITECOPY = 0x00000008,
        PAGE_GUARD = 0x00000100,
        PAGE_NOCACHE = 0x00000200,
        PAGE_WRITECOMBINE = 0x00000400
    }

    [Flags]
    public enum ProcessAccessFlags : uint
    {
        All = 0x001F0FFF,
        Terminate = 0x00000001,
        CreateThread = 0x00000002,
        VirtualMemoryOperation = 0x00000008,
        VirtualMemoryRead = 0x00000010,
        VirtualMemoryWrite = 0x00000020,
        DuplicateHandle = 0x00000040,
        CreateProcess = 0x000000080,
        SetQuota = 0x00000100,
        SetInformation = 0x00000200,
        QueryInformation = 0x00000400,
        QueryLimitedInformation = 0x00001000,
        Synchronize = 0x00100000
    }

    //  https://docs.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-memory_basic_information
    public enum MemoryInformationState : int
    {
        MEM_COMMIT = 0x1000,
        MEM_FREE = 0x10000,
        MEM_RESERVE = 0x2000,
    }

    //  https://docs.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-memory_basic_information
    public enum MemoryInformationType : int
    {
        MEM_IMAGE = 0x1000000,
        MEM_MAPPED = 0x40000,
        MEM_PRIVATE = 0x20000,
    }

    //  https://docs.microsoft.com/en-au/windows/win32/memory/memory-protection-constants
    [Flags]
    public enum MemoryInformationProtection : int
    {
        PAGE_EXECUTE = 0x10,
        PAGE_EXECUTE_READ = 0x20,
        PAGE_EXECUTE_READWRITE = 0x40,
        PAGE_EXECUTE_WRITECOPY = 0x80,
        PAGE_NOACCESS = 0x01,
        PAGE_READONLY = 0x02,
        PAGE_READWRITE = 0x04,
        PAGE_WRITECOPY = 0x08,
        PAGE_TARGETS_INVALID = 0x40000000,
        PAGE_TARGETS_NO_UPDATE = 0x40000000,

        PAGE_GUARD = 0x100,
        PAGE_NOCACHE = 0x200,
        PAGE_WRITECOMBINE = 0x400,
    }
}

/// <summary>
/// Offsets from https://docs.python.org/2/c-api/structures.html
/// </summary>
public class PyObject
{
    public const int Offset_ob_refcnt = 0;
    public const int Offset_ob_type = 8;
}

public record UITreeNode(
    ulong pythonObjectAddress,
    string pythonObjectTypeName,
    IReadOnlyDictionary<string, object> dictEntriesOfInterest,
    string[] otherDictEntriesKeys,
    IReadOnlyList<UITreeNode> children)
{
    public record DictEntryValueGenericRepresentation(
        ulong address,
        string pythonObjectTypeName);

    public record DictEntry(
        string key,
        object value);

    public record Bunch(
        System.Text.Json.Nodes.JsonObject entriesOfInterest);

    public IEnumerable<UITreeNode> EnumerateSelfAndDescendants() =>
        new[] { this }
        .Concat((children ?? Array.Empty<UITreeNode>()).SelectMany(child => child?.EnumerateSelfAndDescendants() ?? ImmutableList<UITreeNode>.Empty));

    public UITreeNode WithOtherDictEntriesRemoved()
    {
        return new UITreeNode
        (
            pythonObjectAddress: pythonObjectAddress,
            pythonObjectTypeName: pythonObjectTypeName,
            dictEntriesOfInterest: dictEntriesOfInterest,
            otherDictEntriesKeys: null,
            children: children?.Select(child => child?.WithOtherDictEntriesRemoved()).ToArray()
        );
    }
}

static class TransformMemoryContent
{
    static public ReadOnlyMemory<ulong> AsULongMemory(ReadOnlyMemory<byte> byteMemory) =>
        MemoryMarshal.Cast<byte, ulong>(byteMemory.Span).ToArray();
}

class ProcessSample
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
                var fullPath = fileFullPathAndContent.name.Split(new[] { '/', '\\' });

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

public class Int64JsonConverter : System.Text.Json.Serialization.JsonConverter<long>
{
    public override long Read(
        ref System.Text.Json.Utf8JsonReader reader,
        Type typeToConvert,
        System.Text.Json.JsonSerializerOptions options) =>
            long.Parse(reader.GetString()!);

    public override void Write(
        System.Text.Json.Utf8JsonWriter writer,
        long integer,
        System.Text.Json.JsonSerializerOptions options) =>
            writer.WriteStringValue(integer.ToString());
}

public class UInt64JsonConverter : System.Text.Json.Serialization.JsonConverter<ulong>
{
    public override ulong Read(
        ref System.Text.Json.Utf8JsonReader reader,
        Type typeToConvert,
        System.Text.Json.JsonSerializerOptions options) =>
            ulong.Parse(reader.GetString()!);

    public override void Write(
        System.Text.Json.Utf8JsonWriter writer,
        ulong integer,
        System.Text.Json.JsonSerializerOptions options) =>
            writer.WriteStringValue(integer.ToString());
}
