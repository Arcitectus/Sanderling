using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using McMaster.Extensions.CommandLineUtils;

namespace read_memory_64_bit
{
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

    class Program
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION64 lpBuffer, uint dwLength);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, ulong lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        static string AppVersionId => "2019-12-28";

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
                    var sourceFileArgument = argumentFromParameterName("--source-file");
                    var outputFileArgument = argumentFromParameterName("--output-file");
                    var removeOtherDictEntriesArgument = argumentFromParameterName("--remove-other-dict-entries");

                    if (!processIdArgument.isPresent && !sourceFileArgument.isPresent)
                        throw new Exception("Where should I read from?");

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

                    var readUiTreesStopwatch = System.Diagnostics.Stopwatch.StartNew();

                    var uiTrees =
                        uiRootCandidatesAddresses
                        .Select(uiTreeRoot => EveOnline64.ReadUITreeFromAddress(uiTreeRoot, memoryReader, 99))
                        .Where(uiTree => uiTree != null)
                        .ToImmutableList();

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

                    Console.WriteLine($"Read {uiTrees.Count} UI trees in {(int)readUiTreesStopwatch.Elapsed.TotalSeconds} seconds:" + string.Join("", uiTreesReport));

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

                            var uiTreeAsJson = System.Text.Json.JsonSerializer.Serialize(uiTreePreparedForFile);

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
            var logEntries = new List<string>();

            void logLine(string lineText)
            {
                logEntries.Add(lineText);
                Console.WriteLine(lineText);
            }

            logLine("Reading from process " + processId + ".");

            var processHandle = OpenProcess(
                (int)(ProcessAccessFlags.QueryInformation | ProcessAccessFlags.VirtualMemoryRead), false, processId);

            long address = 0;

            var committedRegions = new List<(ulong baseAddress, byte[] content)>();

            do
            {
                MEMORY_BASIC_INFORMATION64 m;
                int result = VirtualQueryEx(processHandle, (IntPtr)address, out m, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION64)));

                var regionProtection = (MemoryInformationProtection)m.Protect;

                logLine($"{m.BaseAddress}-{(uint)m.BaseAddress + (uint)m.RegionSize - 1} : {m.RegionSize} bytes result={result}, state={(MemoryInformationState)m.State}, type={(MemoryInformationType)m.Type}, protection={regionProtection}");

                if (address == (long)m.BaseAddress + (long)m.RegionSize)
                    break;

                address = (long)m.BaseAddress + (long)m.RegionSize;

                if (m.State != (int)MemoryInformationState.MEM_COMMIT)
                    continue;

                var protectionFlagsToSkip = MemoryInformationProtection.PAGE_GUARD | MemoryInformationProtection.PAGE_NOACCESS;

                var matchingFlagsToSkip = protectionFlagsToSkip & regionProtection;

                if (matchingFlagsToSkip != 0)
                {
                    logLine($"Skipping region beginning at {m.BaseAddress:X} as it has flags {matchingFlagsToSkip}.");
                    continue;
                }

                var regionBaseAddress = m.BaseAddress;

                int bytesRead = 0;
                byte[] regionContentBuffer = new byte[(long)m.RegionSize];

                ReadProcessMemory(processHandle, regionBaseAddress, regionContentBuffer, regionContentBuffer.Length, ref bytesRead);

                if (bytesRead != regionContentBuffer.Length)
                    throw new Exception($"Failed to ReadProcessMemory at 0x{regionBaseAddress:X}: Only read " + bytesRead + " bytes.");

                committedRegions.Add((regionBaseAddress, regionContentBuffer));

            } while (true);

            logLine($"Found {committedRegions.Count} committed regions with a total size of {committedRegions.Select(region => region.content.Length).Sum()}.");

            return ProcessSample.ZipArchiveFromProcessSample(committedRegions.ToImmutableList(), logEntries.ToImmutableList());
        }
    }


    class EveOnline64
    {
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
            "_texture", "_hint", "_display"
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
            }));

        static public UITreeNode ReadUITreeFromAddress(ulong nodeAddress, IMemoryReader memoryReader, int maxDepth)
        {
            var uiNodeObjectMemory = memoryReader.ReadBytes(nodeAddress, 0x30);

            if (!(0x30 == uiNodeObjectMemory?.Length))
                return null;

            string getPythonTypeNameFromTypeObjectAddress(ulong typeObjectAddress)
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

                var dict_ob_type = dictMemoryAsLongArray[1];

                var dictTypeName = getPythonTypeNameFromTypeObjectAddress(dict_ob_type);

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

            var candidate_ob_type = BitConverter.ToUInt64(uiNodeObjectMemory, 8);

            var pythonObjectTypeName = getPythonTypeNameFromTypeObjectAddress(candidate_ob_type);

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
                var keyObjectMemory = memoryReader.ReadBytes(dictionaryEntry.key, 0x18);

                if (!(keyObjectMemory?.Length == 0x18))
                    continue;

                var keyObject_ob_type = BitConverter.ToUInt64(keyObjectMemory, 8);

                var keyObject_type_name = getPythonTypeNameFromTypeObjectAddress(keyObject_ob_type);

                //  Console.WriteLine($"Dict entry type name is '{keyObject_type_name}'");

                if (keyObject_type_name != "str")
                    continue;

                var keyString = ReadPythonStringValue(dictionaryEntry.key, memoryReader, 0x1000);

                if (!DictEntriesOfInterestKeys.Contains(keyString))
                {
                    otherDictEntriesKeys.Add(keyString);
                    continue;
                }

                object getValueRepresentation()
                {
                    var genericRepresentation = new UITreeNode.DictEntryValueGenericRepresentation
                    {
                        address = dictionaryEntry.value,
                        pythonObjectTypeName = null
                    };

                    var value_memory = memoryReader.ReadBytes(dictionaryEntry.value, 0x10);

                    if (!(value_memory?.Length == 0x10))
                        return genericRepresentation;

                    var value_ob_type = BitConverter.ToUInt64(value_memory, 8);

                    var value_pythonTypeName = getPythonTypeNameFromTypeObjectAddress(value_ob_type);

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
                    value = getValueRepresentation()
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
                        var keyObjectMemory = memoryReader.ReadBytes(dictionaryEntry.key, 0x20);

                        if (!(keyObjectMemory?.Length == 0x20))
                            return false;

                        if (getPythonTypeNameFromTypeObjectAddress(BitConverter.ToUInt64(keyObjectMemory, 8)) != "str")
                            return false;

                        var keyString = ReadPythonStringValue(dictionaryEntry.key, memoryReader, 1000);

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

            return new UITreeNode
            {
                pythonObjectAddress = nodeAddress,
                pythonObjectTypeName = pythonObjectTypeName,
                dictEntriesOfInterest = dictEntriesOfInterestLessNoneType,
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

    /// <summary>
    /// Offsets from https://docs.python.org/2/c-api/structures.html
    /// </summary>
    public class PyObject
    {
        public const int Offset_ob_refcnt = 0;
        public const int Offset_ob_type = 8;
    }

    /// <summary>
    /// Offsets from https://docs.python.org/2/c-api/typeobj.html
    /// </summary>
    public class PyTypeObject : PyObject
    {
    }

    public class UITreeNode
    {
        public ulong pythonObjectAddress { set; get; }

        public string pythonObjectTypeName { set; get; }

        public DictEntry[] dictEntriesOfInterest { set; get; }

        public string[] otherDictEntriesKeys { set; get; }

        public UITreeNode[] children { set; get; }

        public class DictEntry
        {
            public string key { set; get; }
            public object value { set; get; }
        }

        public class DictEntryValueGenericRepresentation
        {
            public ulong address { set; get; }

            public string pythonObjectTypeName { set; get; }
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
            IImmutableList<string> logEntries)
        {
            var zipArchiveEntries =
                memoryRegions.ToImmutableDictionary(
                    region => (IImmutableList<string>)(new[] { "Process", "Memory", $"0x{region.baseAddress:X}" }.ToImmutableList()),
                    region => region.content)
                .Add(new[] { "copy-memory-log" }.ToImmutableList(), System.Text.Encoding.UTF8.GetBytes(String.Join("\n", logEntries)));

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
}
