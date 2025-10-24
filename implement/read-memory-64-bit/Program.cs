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
    static string AppVersionId => "2025-10-24";

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

                var processSampleId =
                Convert.ToHexStringLower(
                    System.Security.Cryptography.SHA256.HashData(processSampleFile));

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
                    var processSampleId =
                    Convert.ToHexStringLower(
                        System.Security.Cryptography.SHA256.HashData(processSampleFile));

                    Console.WriteLine($"Reading from process sample {processSampleId}.");

                    var processSampleUnpacked = ProcessSample.ProcessSampleFromZipArchive(processSampleFile);

                    var memoryReader = new MemoryReaderFromProcessSample(processSampleUnpacked.memoryRegions);

                    var searchUIRootsStopwatch = System.Diagnostics.Stopwatch.StartNew();

                    var memoryRegions =
                        processSampleUnpacked.memoryRegions
                        .Select(memoryRegion => (memoryRegion.baseAddress, length: (ulong)memoryRegion.content.Value.Length))
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
                    var processSampleId =
                    Convert.ToHexStringLower(
                        System.Security.Cryptography.SHA256.HashData(processSampleFile));

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
                        var possibleRootAddresses =
                        0 < rootAddressArgument?.Length
                        ?
                        ImmutableList.Create(ParseULong(rootAddressArgument))
                        :
                        EveOnline64.EnumeratePossibleAddressesForUIRootObjectsFromProcessId(processId.Value);

                        return (new MemoryReaderFromLiveProcess(processId.Value), possibleRootAddresses);
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
                    .Where(uiTree => uiTree is not null)
                    .ToImmutableList();

                if (warmupIterationsArgument is not null)
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

                if (largestUiTree is not null)
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

                    var sampleId = Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(fileContent));

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

        var beginMainWindowClientAreaScreenshotBmp =
            BMPFileFromBitmap(GetScreenshotOfWindowClientAreaAsBitmap(process.MainWindowHandle));

        var (committedRegions, logEntries) =
            EveOnline64.ReadCommittedMemoryRegionsWithContentFromProcessId(processId);

        var endMainWindowClientAreaScreenshotBmp =
            BMPFileFromBitmap(GetScreenshotOfWindowClientAreaAsBitmap(process.MainWindowHandle));

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
        var screenshotAsBitmap =
            GetScreenshotOfWindowAsBitmap(windowHandle);

        if (screenshotAsBitmap is null)
            return null;

        var bitmapData =
            screenshotAsBitmap.LockBits(
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
        .Concat((children ?? []).SelectMany(child => child?.EnumerateSelfAndDescendants() ?? []));

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
