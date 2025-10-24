using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;

namespace read_memory_64_bit;


public class EveOnline64
{
    static public IImmutableList<ulong> EnumeratePossibleAddressesForUIRootObjectsFromProcessId(int processId)
    {
        var memoryReader = new MemoryReaderFromLiveProcess(processId);

        var (committedMemoryRegions, _) = ReadCommittedMemoryRegionsWithoutContentFromProcessId(processId);

        return EnumeratePossibleAddressesForUIRootObjects(committedMemoryRegions, memoryReader);
    }

    static public (IImmutableList<SampleMemoryRegion> memoryRegions, IImmutableList<string> logEntries)
        ReadCommittedMemoryRegionsWithContentFromProcessId(int processId)
    {
        var genericResult = ReadCommittedMemoryRegionsFromProcessId(processId, readContent: true);

        return genericResult;
    }

    static public (IImmutableList<(ulong baseAddress, ulong length)> memoryRegions, IImmutableList<string> logEntries)
        ReadCommittedMemoryRegionsWithoutContentFromProcessId(int processId)
    {
        var genericResult = ReadCommittedMemoryRegionsFromProcessId(processId, readContent: false);

        var memoryRegions =
            genericResult.memoryRegions
            .Select(memoryRegion => (baseAddress: memoryRegion.baseAddress, length: memoryRegion.length))
            .ToImmutableList();

        return (memoryRegions, genericResult.logEntries);
    }

    static public (IImmutableList<SampleMemoryRegion> memoryRegions, IImmutableList<string> logEntries)
        ReadCommittedMemoryRegionsFromProcessId(
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

        var processHandle =
            WinApi.OpenProcess(
                (int)(WinApi.ProcessAccessFlags.QueryInformation | WinApi.ProcessAccessFlags.VirtualMemoryRead),
                false,
                dwProcessId: processId);

        long address = 0;

        var committedRegions = new List<SampleMemoryRegion>();

        do
        {
            int result = WinApi.VirtualQueryEx(
                processHandle,
                lpAddress: (IntPtr)address,
                out WinApi.MEMORY_BASIC_INFORMATION64 m,
                (uint)Marshal.SizeOf<WinApi.MEMORY_BASIC_INFORMATION64>());

            var regionProtection = (WinApi.MemoryInformationProtection)m.Protect;

            logLine(
                $"{m.BaseAddress}-" +
                $"{(uint)m.BaseAddress + (uint)m.RegionSize - 1} : {m.RegionSize}" +
                $" bytes result={result}, state={(WinApi.MemoryInformationState)m.State}" +
                $", type={(WinApi.MemoryInformationType)m.Type}, protection={regionProtection}");

            if (address == (long)m.BaseAddress + (long)m.RegionSize)
                break;

            address = (long)m.BaseAddress + (long)m.RegionSize;

            if (m.State != (int)WinApi.MemoryInformationState.MEM_COMMIT)
                continue;

            var protectionFlagsToSkip =
                WinApi.MemoryInformationProtection.PAGE_GUARD |
                WinApi.MemoryInformationProtection.PAGE_NOACCESS;

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

                WinApi.ReadProcessMemory(
                    processHandle,
                    regionBaseAddress,
                    regionContentBuffer,
                    (UIntPtr)regionContentBuffer.LongLength,
                    ref bytesRead);

                if (bytesRead.ToUInt64() != (ulong)regionContentBuffer.LongLength)
                {
                    throw new Exception(
                        $"Failed to ReadProcessMemory at 0x{regionBaseAddress:X}: Only read " + bytesRead + " bytes.");
                }

                regionContent = regionContentBuffer;
            }

            committedRegions.Add(new SampleMemoryRegion(
                baseAddress: regionBaseAddress,
                length: m.RegionSize,
                content: regionContent));

        } while (true);

        logLine(
            $"Found {committedRegions.Count} committed regions with a total size of " +
            $"{committedRegions.Select(region => (long)region.length).Sum()}.");

        return (committedRegions.ToImmutableList(), logEntries.ToImmutableList());
    }

    static public IImmutableList<ulong> EnumeratePossibleAddressesForUIRootObjects(
        IEnumerable<(ulong baseAddress, ulong length)> memoryRegions,
        IMemoryReader memoryReader)
    {
        var memoryRegionsOrderedByAddress =
            memoryRegions
            .OrderBy(memoryRegion => memoryRegion.baseAddress)
            .ToImmutableArray();

        string ReadNullTerminatedAsciiStringFromAddressUpTo255(ulong address)
        {
            var asMemory = memoryReader.ReadBytes(address, 0x100);

            if (asMemory is null)
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

        ReadOnlyMemory<ulong>? ReadMemoryRegionContentAsULongArray((ulong baseAddress, ulong length) memoryRegion)
        {
            var lengthAsInt = (int)memoryRegion.length;

            if (lengthAsInt < 0)
                return null;

            if (int.MaxValue < memoryRegion.length)
            {
                /*
                 * TODO: Check if Windows API could possibly report such large memory regions.
                 * If that is the case, implement reading in chunks.
                 * */
                return null;
            }

            var asByteArray =
                memoryReader.ReadBytes(memoryRegion.baseAddress, lengthAsInt);

            if (asByteArray is null)
                return null;

            return TransformMemoryContent.AsULongMemory(asByteArray.Value);
        }

        IEnumerable<ulong> EnumerateCandidatesForPythonTypeObjectType()
        {
            IEnumerable<ulong> EnumerateCandidatesForPythonTypeObjectTypeInMemoryRegion((ulong baseAddress, ulong length) memoryRegion)
            {
                var memoryRegionContentAsULongArray =
                    ReadMemoryRegionContentAsULongArray(memoryRegion);

                if (memoryRegionContentAsULongArray is null)
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

                    if (candidate_tp_name is not "type")
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

                if (memoryRegionContentAsULongArray is null)
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

                    if (candidate_tp_name is null)
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

                if (memoryRegionContentAsULongArray is null)
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
        .Add("Bunch", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_Bunch))
        /*
         * 2024-05-26 observed dict entry with key "_setText" pointing to a python object of type "Link".
         * The client used that instance of "Link" to display "Current Solar System" label in the location info panel.
         * */
        .Add("Link", new Func<ulong, LocalMemoryReadingTools, object>(ReadingFromPythonType_Link));

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

        if (dictionaryEntries is null)
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

        if (dictionaryEntries is null)
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

    static object ReadingFromPythonType_Link(ulong address, LocalMemoryReadingTools memoryReadingTools)
    {
        var pythonObjectTypeName = memoryReadingTools.GetPythonTypeNameFromPythonObjectAddress(address);

        var linkMemory = memoryReadingTools.memoryReader.ReadBytes(address, 0x40);

        if (linkMemory is null)
            return null;

        var linkMemoryAsLongMemory = TransformMemoryContent.AsULongMemory(linkMemory.Value);

        /*
         * 2024-05-26 observed a reference to a dictionary object at offset 6 * 4 bytes.
         * */

        var firstDictReference =
            linkMemoryAsLongMemory
            .ToArray()
            .Where(reference =>
            {
                var referencedObjectTypeName = memoryReadingTools.GetPythonTypeNameFromPythonObjectAddress(reference);

                return referencedObjectTypeName is "dict";
            })
            .FirstOrDefault();

        if (firstDictReference is 0)
            return null;

        var dictEntries =
            memoryReadingTools.getDictionaryEntriesWithStringKeys(firstDictReference)
            ?.ToImmutableDictionary(
                keySelector: dictEntry => dictEntry.Key,
                elementSelector: dictEntry => memoryReadingTools.GetDictEntryValueRepresentation(dictEntry.Value));

        return new UITreeNode(
            pythonObjectAddress: address,
            pythonObjectTypeName: pythonObjectTypeName,
            dictEntriesOfInterest: dictEntries,
            otherDictEntriesKeys: null,
            children: null);
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

            //  Console.WriteLine($"dictMemory is {(dictMemory is null ? "not " : "")}ok for 0x{dictionaryAddress:X}");

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

            if (dictionaryEntries is null)
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

        if (dictionaryEntries is null)
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

                if (value_pythonTypeName is null)
                    return genericRepresentation;

                specializedReadingFromPythonType.TryGetValue(value_pythonTypeName, out var specializedRepresentation);

                if (specializedRepresentation is null)
                    return genericRepresentation;

                return specializedRepresentation(genericRepresentation.address, localMemoryReadingTools);
            });
        }

        localMemoryReadingTools.GetDictEntryValueRepresentation = GetDictEntryValueRepresentation;

        foreach (var dictionaryEntry in dictionaryEntries)
        {
            var keyObject_type_name = getPythonTypeNameFromPythonObjectAddress(dictionaryEntry.key);

            //  Console.WriteLine($"Dict entry type name is '{keyObject_type_name}'");

            if (keyObject_type_name is not "str")
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

            if (_displayDictEntry is not null && (_displayDictEntry.value is bool displayAsBool))
                if (!displayAsBool)
                    return null;
        }

        UITreeNode[] ReadChildren()
        {
            if (maxDepth < 1)
                return null;

            //  https://github.com/Arcitectus/Sanderling/blob/b07769fb4283e401836d050870121780f5f37910/guide/image/2015-01.eve-online-python-ui-tree-structure.png

            var childrenDictEntry = dictEntriesOfInterest.FirstOrDefault(entry => entry.key == "children");

            if (childrenDictEntry is null)
                return null;

            var childrenEntryObjectAddress =
                ((UITreeNode.DictEntryValueGenericRepresentation)childrenDictEntry.value).address;

            //  Console.WriteLine($"'children' dict entry of 0x{nodeAddress:X} points to 0x{childrenEntryObjectAddress:X}.");

            var pyChildrenListMemory = memoryReader.ReadBytes(childrenEntryObjectAddress, 0x18);

            if (!(pyChildrenListMemory?.Length == 0x18))
                return null;

            var pyChildrenDictAddress = BitConverter.ToUInt64(pyChildrenListMemory.Value.Span[0x10..]);

            var pyChildrenDictEntries = ReadActiveDictionaryEntriesFromDictionaryAddress(pyChildrenDictAddress);

            //  Console.WriteLine($"Found {(pyChildrenDictEntries is null ? "no" : "some")} children dictionary entries for 0x{nodeAddress:X}");

            if (pyChildrenDictEntries is null)
                return null;

            var childrenEntry =
                pyChildrenDictEntries
                .FirstOrDefault(dictionaryEntry =>
                {
                    if (getPythonTypeNameFromPythonObjectAddress(dictionaryEntry.key) is not "str")
                        return false;

                    var keyString = readPythonStringValueMaxLength4000(dictionaryEntry.key);

                    return keyString == "_childrenObjects";
                });

            //  Console.WriteLine($"Found {(childrenEntry.value == 0 ? "no" : "a")} dictionary entry for children of 0x{nodeAddress:X}");

            if (childrenEntry.value == 0)
                return null;

            if (getPythonTypeNameFromPythonObjectAddress(childrenEntry.value).Equals("PyChildrenList"))
            {
                pyChildrenListMemory = memoryReader.ReadBytes(childrenEntry.value, 0x18);

                if (!(pyChildrenListMemory?.Length == 0x18))
                    return null;

                pyChildrenDictAddress = BitConverter.ToUInt64(pyChildrenListMemory.Value.Span[0x10..]);

                pyChildrenDictEntries = ReadActiveDictionaryEntriesFromDictionaryAddress(pyChildrenDictAddress);

                if (pyChildrenDictEntries is null)
                    return null;

                childrenEntry =
                    pyChildrenDictEntries
                    .FirstOrDefault(dictionaryEntry =>
                    {
                        if (getPythonTypeNameFromPythonObjectAddress(dictionaryEntry.key) is not "str")
                            return false;

                        var keyString = readPythonStringValueMaxLength4000(dictionaryEntry.key);

                        return keyString == "_childrenObjects";
                    });
            }

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
                 [.. listEntries
                 .ToArray()
                 .Select(childAddress => ReadUITreeFromAddress(childAddress, memoryReader, maxDepth - 1, cache))];
        }

        var dictEntriesOfInterestLessNoneType =
            dictEntriesOfInterest
            .Where(entry =>
            !(entry.value is UITreeNode.DictEntryValueGenericRepresentation entryValue && entryValue.pythonObjectTypeName is "NoneType"))
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
            children: ReadChildren()?.Where(child => child is not null)?.ToArray()
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

        if (pythonObjectMemory?.Length is not 0x20)
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
            new JavaScript.Int64JsonConverter(),
            new JavaScript.UInt64JsonConverter()
            }
        };
}
