using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using McMaster.Extensions.CommandLineUtils;

namespace read_memory_64_bit
{
    class Program
    {
        static string AppVersionId => "2020-01-11";

        static int Main(string[] args)
        {
            (bool isPresent, string argumentValue) argumentFromParameterName(string parameterName)
            {
                var match =
                    args
                    .Select(arg => Regex.Match(arg, parameterName + "(=(.*)|)", RegexOptions.IgnoreCase))
                    .FirstOrDefault(matchCandidate => matchCandidate.Success);

                if (match == null)
                    return (false, null);

                if (match.Groups[1].Length < 1)
                    return (true, null);

                return (true, match?.Groups[2].Value);
            }

            var app = new CommandLineApplication
            {
                Name = "read-memory-64-bit",
                Description = "Read memory from 64 bit EVE Online client processes.",
            };

            app.HelpOption(inherited: true);

            app.VersionOption(template: "-v|--version", shortFormVersion: "version " + AppVersionId);

            app.Command("save-process-sample", saveProcessSampleCmd =>
            {
                saveProcessSampleCmd.Description = "Save a sample from a live process to a file. Use the '--pid' parameter to specify the process id.";
                saveProcessSampleCmd.ThrowOnUnexpectedArgument = false;

                saveProcessSampleCmd.OnExecute(() =>
                {
                    var processIdArgument = argumentFromParameterName("--pid");

                    if (!processIdArgument.isPresent)
                        throw new Exception("Missing argument --pid for process ID.");

                    var processId = int.Parse(processIdArgument.argumentValue);

                    var processSampleFile = GetProcessSampleFileFromProcessId(processId);

                    var processSampleId = Kalmit.CommonConversion.StringBase16FromByteArray(Kalmit.CommonConversion.HashSHA256(processSampleFile));

                    var fileName = "process-sample-" + processSampleId.Substring(0, 10) + ".zip";

                    System.IO.File.WriteAllBytes(fileName, processSampleFile);

                    Console.WriteLine("Saved sample {0} to file '{1}'.", processSampleId, fileName);
                });
            });

            app.Command("read-memory-eve-online", readMemoryEveOnlineCmd =>
            {
                readMemoryEveOnlineCmd.Description = "Read the memory of an 64 bit EVE Online client process. You can use a live process ('--pid') or a process sample file ('--source-file') as the source.";
                readMemoryEveOnlineCmd.ThrowOnUnexpectedArgument = false;

                readMemoryEveOnlineCmd.OnExecute(() =>
                {
                    var processIdArgument = argumentFromParameterName("--pid");
                    var rootAddressArgument = argumentFromParameterName("--root-address");
                    var sourceFileArgument = argumentFromParameterName("--source-file");
                    var outputFileArgument = argumentFromParameterName("--output-file");
                    var removeOtherDictEntriesArgument = argumentFromParameterName("--remove-other-dict-entries");
                    var warmupFirstArgument = argumentFromParameterName("--warmup-first");

                    if (!processIdArgument.isPresent && !sourceFileArgument.isPresent)
                        throw new Exception("Where should I read from?");

                    (IImmutableList<ulong>, IMemoryReader) GetRootAddressesAndMemoryReader()
                    {
                        if (rootAddressArgument.isPresent)
                        {
                            if (processIdArgument.isPresent)
                            {
                                var processId = int.Parse(processIdArgument.argumentValue);

                                return (ImmutableList.Create(ParseULong(rootAddressArgument.argumentValue)), new MemoryReaderFromLiveProcess(processId));
                            }

                            throw new NotSupportedException("Not supported combination: '--root-address' without '--pid'.");
                        }

                        byte[] processSampleFile = null;

                        if (processIdArgument.isPresent)
                        {
                            var processId = int.Parse(processIdArgument.argumentValue);

                            processSampleFile = GetProcessSampleFileFromProcessId(processId);
                        }

                        if (sourceFileArgument.isPresent)
                        {
                            processSampleFile = System.IO.File.ReadAllBytes(sourceFileArgument.argumentValue);
                        }

                        var processSampleId = Kalmit.CommonConversion.StringBase16FromByteArray(Kalmit.CommonConversion.HashSHA256(processSampleFile));

                        Console.WriteLine($"Reading from process sample {processSampleId}.");

                        var processSampleUnpacked = ProcessSample.ProcessSampleFromZipArchive(processSampleFile);

                        var searchUIRootsStopwatch = System.Diagnostics.Stopwatch.StartNew();

                        var uiRootCandidatesAddresses =
                            EveOnline64.EnumeratePossibleAddressesForUIRootObjects(processSampleUnpacked.memoryRegions)
                            .ToImmutableList();

                        searchUIRootsStopwatch.Stop();

                        Console.WriteLine($"Found {uiRootCandidatesAddresses.Count} candidates for UIRoot in {(int)searchUIRootsStopwatch.Elapsed.TotalSeconds} seconds: " + string.Join(",", uiRootCandidatesAddresses.Select(address => $"0x{address:X}")));

                        var memoryReader = new MemoryReaderFromProcessSample(processSampleUnpacked.memoryRegions);

                        return (uiRootCandidatesAddresses, memoryReader);
                    }

                    var (uiRootCandidatesAddresses, memoryReader) = GetRootAddressesAndMemoryReader();

                    IImmutableList<UITreeNode> ReadUITrees() =>
                            uiRootCandidatesAddresses
                            .Select(uiTreeRoot => EveOnline64.ReadUITreeFromAddress(uiTreeRoot, memoryReader, 99))
                            .Where(uiTree => uiTree != null)
                            .ToImmutableList();

                    if (warmupFirstArgument.isPresent)
                    {
                        Console.WriteLine("Performing a warmup run first.");
                        ReadUITrees().ToList();
                        System.Threading.Thread.Sleep(1111);
                        ReadUITrees().ToList();
                        System.Threading.Thread.Sleep(1111);
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
                        if (outputFileArgument.isPresent)
                        {
                            var uiTreePreparedForFile = largestUiTree;

                            if (removeOtherDictEntriesArgument.isPresent)
                            {
                                uiTreePreparedForFile = uiTreePreparedForFile.WithOtherDictEntriesRemoved();
                            }

                            var uiTreeAsJson = Newtonsoft.Json.JsonConvert.SerializeObject(
                                uiTreePreparedForFile,
                                //  Support popular JSON parsers: Wrap large integers in a string to work around limitations there. (https://discourse.elm-lang.org/t/how-to-parse-a-json-object/4977)
                                new IntegersToStringJsonConverter()
                                );

                            var outputFilePath = outputFileArgument.argumentValue;

                            System.IO.File.WriteAllText(outputFilePath, uiTreeAsJson, System.Text.Encoding.UTF8);

                            Console.WriteLine($"I saved ui tree {largestUiTree.pythonObjectAddress:X} to file '{outputFilePath}'.");
                        }
                        else
                        {
                            Console.WriteLine("I found no configuration of an output file path.");
                        }
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

            var (committedRegions, logEntries) = EveOnline64.ReadCommittedMemoryRegionsFromProcessId(processId);

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
            using (var stream = new System.IO.MemoryStream())
            {
                bitmap.Save(stream, format: System.Drawing.Imaging.ImageFormat.Bmp);
                return stream.ToArray();
            }
        }

        public int[][] GetScreenshotOfWindowAsPixelsValuesR8G8B8(IntPtr windowHandle)
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
                return ulong.Parse(asString.Substring(2), System.Globalization.NumberStyles.HexNumber);

            return ulong.Parse(asString);
        }
    }

    public class EveOnline64
    {
        static public IImmutableList<ulong> EnumeratePossibleAddressesForUIRootObjectsFromProcessId(int processId)
        {
            var (committedMemoryRegions, _) = ReadCommittedMemoryRegionsFromProcessId(processId);

            return EnumeratePossibleAddressesForUIRootObjects(committedMemoryRegions);
        }

        static public (IImmutableList<(ulong baseAddress, byte[] content)> memoryRegions, IImmutableList<string> logEntries) ReadCommittedMemoryRegionsFromProcessId(int processId)
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

            var committedRegions = new List<(ulong baseAddress, byte[] content)>();

            do
            {
                WinApi.MEMORY_BASIC_INFORMATION64 m;
                int result = WinApi.VirtualQueryEx(processHandle, (IntPtr)address, out m, (uint)Marshal.SizeOf(typeof(WinApi.MEMORY_BASIC_INFORMATION64)));

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

                int bytesRead = 0;
                byte[] regionContentBuffer = new byte[(long)m.RegionSize];

                WinApi.ReadProcessMemory(processHandle, regionBaseAddress, regionContentBuffer, regionContentBuffer.Length, ref bytesRead);

                if (bytesRead != regionContentBuffer.Length)
                    throw new Exception($"Failed to ReadProcessMemory at 0x{regionBaseAddress:X}: Only read " + bytesRead + " bytes.");

                committedRegions.Add((regionBaseAddress, regionContentBuffer));

            } while (true);

            logLine($"Found {committedRegions.Count} committed regions with a total size of {committedRegions.Select(region => region.content.Length).Sum()}.");

            return (committedRegions.ToImmutableList(), logEntries.ToImmutableList());
        }

        static public IImmutableList<ulong> EnumeratePossibleAddressesForUIRootObjects(IEnumerable<(ulong baseAddress, byte[] content)> memoryRegions)
        {
            var memoryRegionsOrderedByAddress =
                memoryRegions
                .OrderBy(memoryRegion => memoryRegion.baseAddress)
                .Select(memoryRegion =>
                new
                {
                    baseAddress = memoryRegion.baseAddress,
                    content = memoryRegion.content,
                    contentAsULongArray = TransformMemoryContent.AsULongArray(memoryRegion.content)
                })
                .ToImmutableList();

            string ReadNullTerminatedAsciiStringFromAddress(ulong address)
            {
                var memoryRegion =
                    memoryRegions
                    .Where(c => c.baseAddress <= address)
                    .OrderBy(c => c.baseAddress)
                    .LastOrDefault();

                if (memoryRegion.content == null)
                    return null;

                var bytes =
                    memoryRegion.content
                    .Skip((int)(address - memoryRegion.baseAddress))
                    .TakeWhile(character => 0 < character)
                    .ToArray();

                return System.Text.Encoding.ASCII.GetString(bytes);
            }

            IEnumerable<ulong> EnumerateCandidatesForPythonTypeObjectType()
            {
                foreach (var memoryRegion in memoryRegionsOrderedByAddress)
                {
                    for (var candidateAddressIndex = 0; candidateAddressIndex < memoryRegion.contentAsULongArray.Length - 4; ++candidateAddressIndex)
                    {
                        var candidateAddressInProcess = memoryRegion.baseAddress + (ulong)candidateAddressIndex * 8;

                        var candidate_ob_type = memoryRegion.contentAsULongArray[candidateAddressIndex + 1];

                        if (candidate_ob_type != candidateAddressInProcess)
                            continue;

                        var candidate_tp_name =
                            ReadNullTerminatedAsciiStringFromAddress(memoryRegion.contentAsULongArray[candidateAddressIndex + 3]);

                        if (candidate_tp_name != "type")
                            continue;

                        yield return candidateAddressInProcess;
                    }
                }
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
                    for (var candidateAddressIndex = 0; candidateAddressIndex < memoryRegion.contentAsULongArray.Length - 4; ++candidateAddressIndex)
                    {
                        var candidateAddressInProcess = memoryRegion.baseAddress + (ulong)candidateAddressIndex * 8;

                        var candidate_ob_type = memoryRegion.contentAsULongArray[candidateAddressIndex + 1];

                        {
                            //  This check is redundant with the following one. It just implements a specialization to optimize runtime expenses.
                            if (candidate_ob_type < typeAddressMin || typeAddressMax < candidate_ob_type)
                                continue;
                        }

                        if (!typeObjectCandidatesAddresses.Contains(candidate_ob_type))
                            continue;

                        var candidate_tp_name =
                            ReadNullTerminatedAsciiStringFromAddress(memoryRegion.contentAsULongArray[candidateAddressIndex + 3]);

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
                    for (var candidateAddressIndex = 0; candidateAddressIndex < memoryRegion.contentAsULongArray.Length - 4; ++candidateAddressIndex)
                    {
                        var candidateAddressInProcess = memoryRegion.baseAddress + (ulong)candidateAddressIndex * 8;

                        var candidate_ob_type = memoryRegion.contentAsULongArray[candidateAddressIndex + 1];

                        {
                            //  This check is redundant with the following one. It just implements a specialization to optimize runtime expenses.
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
            "ramp_active"
        );

        static readonly IImmutableDictionary<string, Func<ulong, IMemoryReader, object>> specializedReadingFromPythonType =
            ImmutableDictionary<string, Func<ulong, IMemoryReader, object>>.Empty
            .Add("str", new Func<ulong, IMemoryReader, object>((address, memoryReader) =>
            {
                return ReadPythonStringValue(address, memoryReader, 0x1000);
            }))
            .Add("unicode", new Func<ulong, IMemoryReader, object>((address, memoryReader) =>
            {
                var pythonObjectMemory = memoryReader.ReadBytes(address, 0x20);

                if (!(pythonObjectMemory?.Length == 0x20))
                    return "Failed to read python object memory.";

                var unicode_string_length = BitConverter.ToUInt64(pythonObjectMemory, 0x10);

                if (0x1000 < unicode_string_length)
                    return "String too long.";

                var stringBytesCount = (int)unicode_string_length * 2;

                var stringBytes = memoryReader.ReadBytes(BitConverter.ToUInt64(pythonObjectMemory, 0x18), stringBytesCount);

                if (!(stringBytes?.Length == (int)stringBytesCount))
                    return "Failed to read string bytes.";

                return System.Text.Encoding.Unicode.GetString(stringBytes, 0, stringBytes.Length);
            }))
            .Add("int", new Func<ulong, IMemoryReader, object>((address, memoryReader) =>
            {
                var intObjectMemory = memoryReader.ReadBytes(address, 0x18);

                if (!(intObjectMemory?.Length == 0x18))
                    return "Failed to read int object memory.";

                return BitConverter.ToInt64(intObjectMemory, 0x10);
            }))
            .Add("bool", new Func<ulong, IMemoryReader, object>((address, memoryReader) =>
            {
                var pythonObjectMemory = memoryReader.ReadBytes(address, 0x18);

                if (!(pythonObjectMemory?.Length == 0x18))
                    return "Failed to read python object memory.";

                return BitConverter.ToInt64(pythonObjectMemory, 0x10) != 0;
            }))
            .Add("float", new Func<ulong, IMemoryReader, object>((address, memoryReader) =>
            {
                return ReadPythonFloatObjectValue(address, memoryReader);
            }));

        static public UITreeNode ReadUITreeFromAddress(ulong nodeAddress, IMemoryReader memoryReader, int maxDepth)
        {
            var uiNodeObjectMemory = memoryReader.ReadBytes(nodeAddress, 0x30);

            if (!(0x30 == uiNodeObjectMemory?.Length))
                return null;

            string getPythonTypeNameFromPythonTypeObjectAddress(ulong typeObjectAddress)
            {
                var typeObjectMemory = memoryReader.ReadBytes(typeObjectAddress, 0x20);

                if (!(typeObjectMemory?.Length == 0x20))
                    return null;

                var tp_name = BitConverter.ToUInt64(typeObjectMemory, 0x18);

                var nameBytes = memoryReader.ReadBytes(tp_name, 100);

                if (!(nameBytes?.Contains((byte)0) ?? false))
                    return null;

                return System.Text.Encoding.ASCII.GetString(nameBytes.TakeWhile(character => character != 0).ToArray());
            }

            string getPythonTypeNameFromPythonObjectAddress(ulong objectAddress)
            {
                var objectMemory = memoryReader.ReadBytes(objectAddress, 0x10);

                if (!(objectMemory?.Length == 0x10))
                    return null;

                return getPythonTypeNameFromPythonTypeObjectAddress(BitConverter.ToUInt64(objectMemory, 8));
            }

            string readPythonStringValueMaxLength4000(ulong strObjectAddress)
            {
                return ReadPythonStringValue(strObjectAddress, memoryReader, 4000);
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

                var dictMemoryAsLongArray = TransformMemoryContent.AsULongArray(dictMemory);

                //  var dictTypeName = getPythonTypeNameFromObjectAddress(dictionaryAddress);

                //  Console.WriteLine($"Type name for dictionary 0x{dictionaryAddress:X} is '{dictTypeName}'.");

                //  https://github.com/python/cpython/blob/362ede2232107fc54d406bb9de7711ff7574e1d4/Include/dictobject.h#L60-L89

                var ma_fill = dictMemoryAsLongArray[2];
                var ma_used = dictMemoryAsLongArray[3];
                var ma_mask = dictMemoryAsLongArray[4];
                var ma_table = dictMemoryAsLongArray[5];

                //  Console.WriteLine($"Details for dictionary 0x{dictionaryAddress:X}: type_name = '{dictTypeName}' ma_mask = 0x{ma_mask:X}, ma_table = 0x{ma_table:X}.");

                var numberOfSlots = (int)ma_mask + 1;

                var slotsMemorySize = numberOfSlots * 8 * 3;

                var slotsMemory = memoryReader.ReadBytes(ma_table, slotsMemorySize);

                //  Console.WriteLine($"slotsMemory (0x{ma_table:X}) has length of {slotsMemory?.Length} and is {(slotsMemory?.Length == slotsMemorySize ? "" : "not ")}ok for 0x{dictionaryAddress:X}");

                if (!(slotsMemory?.Length == slotsMemorySize))
                    return null;

                var slotsMemoryAsLongArray = TransformMemoryContent.AsULongArray(slotsMemory);

                var entries = new List<PyDictEntry>();

                for (var slotIndex = 0; slotIndex < numberOfSlots; ++slotIndex)
                {
                    var hash = slotsMemoryAsLongArray[slotIndex * 3];
                    var key = slotsMemoryAsLongArray[slotIndex * 3 + 1];
                    var value = slotsMemoryAsLongArray[slotIndex * 3 + 2];

                    if (key == 0 || value == 0)
                        continue;

                    entries.Add(new PyDictEntry { hash = hash, key = key, value = value });
                }

                return entries.ToArray();
            }

            var pythonObjectTypeName = getPythonTypeNameFromPythonObjectAddress(nodeAddress);

            if (!(0 < pythonObjectTypeName?.Length))
                return null;

            var dictAddress = BitConverter.ToUInt64(uiNodeObjectMemory, 0x10);

            var dictionaryEntries = ReadActiveDictionaryEntriesFromDictionaryAddress(dictAddress);

            if (dictionaryEntries == null)
                return null;

            var dictEntriesOfInterest = new List<UITreeNode.DictEntry>();

            var otherDictEntriesKeys = new List<string>();

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

                object getValueRepresentation(ulong valueOjectAddress)
                {
                    var genericRepresentation = new UITreeNode.DictEntryValueGenericRepresentation
                    {
                        address = valueOjectAddress,
                        pythonObjectTypeName = null
                    };

                    var value_pythonTypeName = getPythonTypeNameFromPythonObjectAddress(valueOjectAddress);

                    genericRepresentation.pythonObjectTypeName = value_pythonTypeName;

                    if (value_pythonTypeName == null)
                        return genericRepresentation;

                    specializedReadingFromPythonType.TryGetValue(value_pythonTypeName, out var specializedRepresentation);

                    if (specializedRepresentation == null)
                        return genericRepresentation;

                    return specializedRepresentation(genericRepresentation.address, memoryReader);
                }

                dictEntriesOfInterest.Add(new UITreeNode.DictEntry
                {
                    key = keyString,
                    value = getValueRepresentation(dictionaryEntry.value)
                });
            }

            {
                var _displayDictEntry = dictEntriesOfInterest.FirstOrDefault(c => c.key == "_display");

                if (_displayDictEntry != null && (_displayDictEntry.value is bool displayAsBool))
                    if (!displayAsBool)
                        return null;
            }

            UITreeNode[] ReadChildren()
            {
                if (maxDepth < 1)
                    return null;

                //  https://github.com/Arcitectus/Sanderling/blob/b07769fb4283e401836d050870121780f5f37910/guide/image/2015-01.eve-online-python-ui-tree-structure.png

                var childrenDictEntry = dictEntriesOfInterest.FirstOrDefault(c => c.key == "children");

                if (childrenDictEntry == null)
                    return null;

                var childrenEntryObjectAddress =
                    ((UITreeNode.DictEntryValueGenericRepresentation)childrenDictEntry.value).address;

                //  Console.WriteLine($"'children' dict entry of 0x{nodeAddress:X} points to 0x{childrenEntryObjectAddress:X}.");

                var pyChildrenListMemory = memoryReader.ReadBytes(childrenEntryObjectAddress, 0x18);

                if (!(pyChildrenListMemory?.Length == 0x18))
                    return null;

                var pyChildrenDictAddress = BitConverter.ToUInt64(pyChildrenListMemory, 0x10);

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

                var list_ob_size = BitConverter.ToUInt64(pythonListObjectMemory, 0x10);

                if (4000 < list_ob_size)
                    return null;

                var listEntriesSize = (int)list_ob_size * 8;

                var list_ob_item = BitConverter.ToUInt64(pythonListObjectMemory, 0x18);

                var listEntriesMemory = memoryReader.ReadBytes(list_ob_item, listEntriesSize);

                if (!(listEntriesMemory?.Length == listEntriesSize))
                    return null;

                var listEntries = TransformMemoryContent.AsULongArray(listEntriesMemory);

                //  Console.WriteLine($"Found {listEntries.Length} children entries for 0x{nodeAddress:X}: " + String.Join(", ", listEntries.Select(childAddress => $"0x{childAddress:X}").ToArray()));

                return
                     listEntries
                     .Select(childAddress => ReadUITreeFromAddress(childAddress, memoryReader, maxDepth - 1))
                     .ToArray();
            }

            var dictEntriesOfInterestLessNoneType =
                dictEntriesOfInterest
                .Where(c => !(((object)c.value as UITreeNode.DictEntryValueGenericRepresentation)?.pythonObjectTypeName == "NoneType"))
                .ToArray();

            var dictEntriesOfInterestJObject =
                new Newtonsoft.Json.Linq.JObject(
                    dictEntriesOfInterestLessNoneType.Select(dictEntry =>
                        new Newtonsoft.Json.Linq.JProperty(dictEntry.key, Newtonsoft.Json.Linq.JToken.FromObject(dictEntry.value))));

            return new UITreeNode
            {
                pythonObjectAddress = nodeAddress,
                pythonObjectTypeName = pythonObjectTypeName,
                dictEntriesOfInterest = dictEntriesOfInterestJObject,
                otherDictEntriesKeys = otherDictEntriesKeys.ToArray(),
                children = ReadChildren()?.Where(child => child != null)?.ToArray(),
            };
        }

        static string ReadPythonStringValue(ulong stringObjectAddress, IMemoryReader memoryReader, int maxLength)
        {
            //  https://github.com/python/cpython/blob/362ede2232107fc54d406bb9de7711ff7574e1d4/Include/stringobject.h

            var stringObjectMemory = memoryReader.ReadBytes(stringObjectAddress, 0x20);

            if (!(stringObjectMemory?.Length == 0x20))
                return "Failed to read string object memory.";

            var stringObject_ob_size = BitConverter.ToUInt64(stringObjectMemory, 0x10);

            if (0 < maxLength && maxLength < (int)stringObject_ob_size || int.MaxValue < stringObject_ob_size)
                return "String too long.";

            var stringBytes = memoryReader.ReadBytes(stringObjectAddress + 8 * 4, (int)stringObject_ob_size);

            if (!(stringBytes?.Length == (int)stringObject_ob_size))
                return "Failed to read string bytes.";

            return System.Text.Encoding.ASCII.GetString(stringBytes, 0, stringBytes.Length);
        }

        static double? ReadPythonFloatObjectValue(ulong floatObjectAddress, IMemoryReader memoryReader)
        {
            //  https://github.com/python/cpython/blob/362ede2232107fc54d406bb9de7711ff7574e1d4/Include/floatobject.h

            var pythonObjectMemory = memoryReader.ReadBytes(floatObjectAddress, 0x20);

            if (!(pythonObjectMemory?.Length == 0x20))
                return null;

            return BitConverter.ToDouble(pythonObjectMemory, 0x10);
        }
    }


    public interface IMemoryReader
    {
        byte[] ReadBytes(ulong startAddress, int length);
    }

    public class MemoryReaderFromProcessSample : IMemoryReader
    {
        readonly IImmutableList<(ulong baseAddress, byte[] content)> memoryRegionsOrderedByAddress;

        public MemoryReaderFromProcessSample(IImmutableList<(ulong baseAddress, byte[] content)> memoryRegions)
        {
            memoryRegionsOrderedByAddress =
                memoryRegions
                .OrderBy(memoryRegion => memoryRegion.baseAddress)
                .ToImmutableList();
        }

        public byte[] ReadBytes(ulong startAddress, int length)
        {
            var memoryRegion =
                memoryRegionsOrderedByAddress
                .Where(c => c.baseAddress <= startAddress)
                .OrderBy(c => c.baseAddress)
                .LastOrDefault();

            if (memoryRegion.content == null)
                return null;

            return
                memoryRegion.content
                .Skip((int)(startAddress - memoryRegion.baseAddress))
                .Take(length)
                .ToArray();
        }
    }


    public class MemoryReaderFromLiveProcess : IMemoryReader, IDisposable
    {
        IntPtr processHandle;

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

        public byte[] ReadBytes(ulong startAddress, int length)
        {
            var buffer = new byte[length];

            int numberOfBytesRead = 0;

            WinApi.ReadProcessMemory(processHandle, startAddress, buffer, buffer.Length, ref numberOfBytesRead);

            if (numberOfBytesRead == 0)
                return null;

            if (numberOfBytesRead == buffer.Length)
                return buffer;

            return buffer.AsSpan(0, numberOfBytesRead).ToArray();
        }
    }

    static public class WinApi
    {
        [DllImport("kernel32.dll")]
        static public extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        static public extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION64 lpBuffer, uint dwLength);

        [DllImport("kernel32.dll")]
        static public extern bool ReadProcessMemory(IntPtr hProcess, ulong lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

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

    public class UITreeNode
    {
        public ulong pythonObjectAddress { set; get; }

        public string pythonObjectTypeName { set; get; }

        public Newtonsoft.Json.Linq.JObject dictEntriesOfInterest { set; get; }

        public string[] otherDictEntriesKeys { set; get; }

        public UITreeNode[] children { set; get; }

        public class DictEntryValueGenericRepresentation
        {
            public ulong address { set; get; }

            public string pythonObjectTypeName { set; get; }
        }

        public class DictEntry
        {
            public string key { set; get; }
            public object value { set; get; }
        }

        public IEnumerable<UITreeNode> EnumerateSelfAndDescendants() =>
            new[] { this }
            .Concat((children ?? Array.Empty<UITreeNode>()).SelectMany(child => child?.EnumerateSelfAndDescendants() ?? ImmutableList<UITreeNode>.Empty));

        public UITreeNode WithOtherDictEntriesRemoved()
        {
            return new UITreeNode
            {
                pythonObjectAddress = pythonObjectAddress,
                pythonObjectTypeName = pythonObjectTypeName,
                dictEntriesOfInterest = dictEntriesOfInterest,
                otherDictEntriesKeys = null,
                children = children?.Select(child => child?.WithOtherDictEntriesRemoved()).ToArray(),
            };
        }
    }

    static class TransformMemoryContent
    {
        static public ulong[] AsULongArray(byte[] byteArray)
        {
            var ulongArray = new ulong[byteArray.Length / 8];
            Buffer.BlockCopy(byteArray, 0, ulongArray, 0, ulongArray.Length * 8);
            return ulongArray;
        }
    }

    class ProcessSample
    {
        static public byte[] ZipArchiveFromProcessSample(
            IImmutableList<(ulong baseAddress, byte[] content)> memoryRegions,
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
                    region => (IImmutableList<string>)(new[] { "Process", "Memory", $"0x{region.baseAddress:X}" }.ToImmutableList()),
                    region => region.content)
                .Add(new[] { "copy-memory-log" }.ToImmutableList(), System.Text.Encoding.UTF8.GetBytes(String.Join("\n", logEntries)))
                .AddRange(screenshotEntries);

            return Kalmit.ZipArchive.ZipArchiveFromEntries(zipArchiveEntries);
        }

        static public (IImmutableList<(ulong baseAddress, byte[] content)> memoryRegions, IImmutableList<string> copyMemoryLog) ProcessSampleFromZipArchive(byte[] sampleFile)
        {
            var files =
                Kalmit.ZipArchive.EntriesFromZipArchive(sampleFile);

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

                    return (baseAddress, content: fileSubpathAndContent.fileContent);
                }).ToImmutableList();

            return (memoryRegions, null);
        }
    }

    public class IntegersToStringJsonConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanRead => false;
        public override bool CanWrite => true;
        public override bool CanConvert(Type type) =>
            type == typeof(int) || type == typeof(long) || type == typeof(uint) || type == typeof(ulong);

        public override void WriteJson(
            Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override object ReadJson(
            Newtonsoft.Json.JsonReader reader, Type type, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}
