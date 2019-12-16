//  The approach for the setup is based on the guide from https://github.com/Arcitectus/Sanderling/blob/7441327667c254f80b1806431b8efbcca0e97c00/guide/how-to-adapt-eve-online-memory-reading.md
//  The guide in turn links to https://github.com/Arcitectus/Sanderling/blob/7441327667c254f80b1806431b8efbcca0e97c00/src/Sanderling/Sanderling.MemoryReading.Test/MemoryReadingDemo.cs
//  Another source is bots implementation: https://github.com/Viir/bots/blob/f90c6d77ccfdbf1982076e83dc2169012f4aeafd/implement/applications/eve-online/eve-online-warp-to-0-autopilot/src/Sanderling/SanderlingVolatileHostSetup.elm#

#r "nuget: Newtonsoft.Json, 9.0.1"

//  Copy assembly references from bots (less NewtonsoftJson):
#r "./lib/FE8A38EBCED27A112519023A7A1216C69FE0863BCA3EF766234E972E920096C1"
#r "./lib/11DCCA7041E1436B858BAC75E2577CA471ABA40208C4214ABD90A717DD89CEF6"
#r "./lib/5229128932E6AAFB5433B7AA5E05E6AFA3C19A929897E49F83690AB8FE273162"
#r "./lib/CADE001866564D185F14798ECFD077EDA6415E69D978748C19B98DDF0EE839BB"
#r "./lib/FE532D93F820980181F34C163E54F83726876CC9B02FEC72086FD3DC747793BC"
#r "./lib/C6E93D210F2A71438B9BEDDDA3D9E0CAB723A179BB9F2400A983EEF72FDF9FB5"
#r "./lib/831EF0489D9FA85C34C95F0670CC6393D1AD9548EE708E223C1AD87B51F7C7B3"
#r "./lib/137CF2631884C20D61F6C4FA122624ACE70780B3A24E12D9172AE3582EDA46E4"
#r "./lib/81110D44256397F0F3C572A20CA94BB4C669E5DE89F9348ABAD263FBD81C54B9"
#r "./lib/2A89B0F057A26E1273DECC0FC7FE9C2BB12683479E37076D23A1F73CCC324D13"

using Bib3;
using Sanderling.ExploreProcessMeasurement;

struct MemoryReadingResult
{
    public string partialPythonModelJson;

    public string reducedWithNamedNodesJson;
}

static string StringIdentifierFromValue(byte[] value)
{
    using (var sha = new System.Security.Cryptography.SHA256Managed())
    {
        return BitConverter.ToString(sha.ComputeHash(value)).Replace("-", "");
    }
}

/*
Use same serialization format as bots:
https://github.com/Viir/bots/blob/f90c6d77ccfdbf1982076e83dc2169012f4aeafd/implement/applications/eve-online/eve-online-warp-to-0-autopilot/src/Sanderling/SanderlingVolatileHostSetup.elm#L339
*/
string SerializeToJsonForBot<T>(T value) =>
    Newtonsoft.Json.JsonConvert.SerializeObject(
        value,
        //  Use settings to get same derivation as at https://github.com/Arcitectus/Sanderling/blob/ada11c9f8df2367976a6bcc53efbe9917107bfa7/src/Sanderling/Sanderling.MemoryReading.Test/MemoryReadingDemo.cs#L91-L97
        new Newtonsoft.Json.JsonSerializerSettings
        {
            //  Bot code does not expect properties with null values, see https://github.com/Viir/bots/blob/880d745b0aa8408a4417575d54ecf1f513e7aef4/explore/2019-05-14.eve-online-bot-framework/src/Sanderling_Interface_20190514.elm
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
            //	https://stackoverflow.com/questions/7397207/json-net-error-self-referencing-loop-detected-for-type/18223985#18223985
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
        });

BotEngine.Interface.IMemoryReader MemoryReaderFromLiveProcessId(int processId) =>
    new BotEngine.Interface.ProcessMemoryReader(processId);

Optimat.EveOnline.MemoryAuswertWurzelSuuce UITreeRootFromMemoryReader(
    BotEngine.Interface.IMemoryReader memoryReader) =>
    Sanderling.ExploreProcessMeasurement.Extension.SearchForUITreeRoot(memoryReader);

Sanderling.Interface.MemoryStruct.IMemoryMeasurement SanderlingMemoryReadingFromPartialPythonModel(
    Optimat.EveOnline.GbsAstInfo partialPython) =>
    Optimat.EveOnline.AuswertGbs.Extension.SensorikScnapscusKonstrukt(partialPython, null);

IEnumerable<T> EnumerateNodeFromTreeDFirst<T>(
    T root,
    Func<T, IEnumerable<T>> callbackEnumerateChildInNode,
    int? depthMax = null,
    int? depthMin = null) =>
    Bib3.Extension.EnumerateNodeFromTreeDFirst(root, callbackEnumerateChildInNode, depthMax, depthMin);

Optimat.EveOnline.AuswertGbs.UINodeInfoInTree PartialPythonModelFromUITreeRoot(
    BotEngine.Interface.IMemoryReader memoryReader,
    Optimat.EveOnline.MemoryAuswertWurzelSuuce uiTreeRoot) =>
    Optimat.EveOnline.AuswertGbs.Extension.SictAuswert(
        Sanderling.ExploreProcessMeasurement.Extension.ReadUITreeFromRoot(memoryReader, uiTreeRoot));

System.Diagnostics.Process[] GetWindowsProcessesLookingLikeEVEOnlineClient() =>
    System.Diagnostics.Process.GetProcessesByName("exefile");

MemoryReadingResult memoryReadingFromProcessMeasurementFile(string windowsProcessMeasurementFilePath)
{
    var windowsProcessMeasurementZipArchive = System.IO.File.ReadAllBytes(windowsProcessMeasurementFilePath);

    var measurementId = StringIdentifierFromValue(windowsProcessMeasurementZipArchive);

    Console.WriteLine("Loaded process sample " + measurementId + " from '" + windowsProcessMeasurementFilePath + "'");

    var windowsProcessMeasurement = BotEngine.Interface.Process.Snapshot.Extension.SnapshotFromZipArchive(windowsProcessMeasurementZipArchive);

    var memoryReader = new BotEngine.Interface.Process.Snapshot.SnapshotReader(windowsProcessMeasurement?.ProcessSnapshot?.MemoryBaseAddressAndListOctet);

    Console.WriteLine("I begin to search for the root of the UI tree...");

    //	The address of the root of the UI tree usually does not change in an EVE Online client process.
    //	Therefore the UI tree root search result is reused when reading the UI tree from the same process later.
    var searchForUITreeRoot = memoryReader.SearchForUITreeRoot();

    //  Console.WriteLine("searchForUITreeRoot: " + searchForUITreeRoot);

    Console.WriteLine($"I found { searchForUITreeRoot?.GbsMengeWurzelObj?.Count() } UI tree roots: { string.Join(",", searchForUITreeRoot?.GbsMengeWurzelObj?.Select(node => node.HerkunftAdrese)) }");

    Console.WriteLine("I read the partial python model of the UI tree...");

    var memoryReadingPartialPythonModel = memoryReader?.ReadUITreeFromRoot(searchForUITreeRoot);

    var allNodesFromPartialPythonModel =
        memoryReadingPartialPythonModel.EnumerateNodeFromTreeDFirst(node => node.GetListChild())
        .ToList();

    Console.WriteLine($"The tree in partial python model contains { allNodesFromPartialPythonModel.Count } nodes");

    var memoryReadingReducedWithNamedNodes =
        Optimat.EveOnline.AuswertGbs.Extension.SensorikScnapscusKonstrukt(memoryReadingPartialPythonModel, null);

    return new MemoryReadingResult
    {
        partialPythonModelJson = SerializeToJsonForBot(memoryReadingPartialPythonModel),
        reducedWithNamedNodesJson = SerializeToJsonForBot(memoryReadingReducedWithNamedNodes),
    };
}

//  https://github.com/Viir/bots/blob/f90c6d77ccfdbf1982076e83dc2169012f4aeafd/implement/applications/eve-online/eve-online-warp-to-0-autopilot/src/Sanderling/SanderlingVolatileHostSetup.elm#L148-L155

struct UiTreeRootSearchResultCache
{
    public System.Diagnostics.Process process;
    public Optimat.EveOnline.MemoryAuswertWurzelSuuce uiTreeRoot;
}

UiTreeRootSearchResultCache? uiTreeRootSearchResultCache = null;

MemoryReadingResult memoryReadingFromLiveProcess()
{
    if (uiTreeRootSearchResultCache?.process != null)
    {
        Console.WriteLine("Found a cache entry for process " + uiTreeRootSearchResultCache?.process.Id);

        if (uiTreeRootSearchResultCache.Value.process.HasExited)
        {
            Console.WriteLine("But process " + uiTreeRootSearchResultCache?.process.Id + " has exited.");
            uiTreeRootSearchResultCache = null;
        }
    }

    if (uiTreeRootSearchResultCache == null)
    {
        Console.WriteLine("Search for an EVE Online client process.");

        var eveOnlineClientProcess = GetWindowsProcessesLookingLikeEVEOnlineClient().FirstOrDefault();

        if (eveOnlineClientProcess == null)
        {
            Console.WriteLine("Did not find an EVE Online client process.");
            throw new Exception("Did not find an EVE Online client process.");
        }

        Console.WriteLine("Found an EVE Online client in process " + eveOnlineClientProcess.Id);

        var memoryReader = MemoryReaderFromLiveProcessId(eveOnlineClientProcess.Id);

        var uiTreeRoot = UITreeRootFromMemoryReader(memoryReader);

        uiTreeRootSearchResultCache = new UiTreeRootSearchResultCache { process = eveOnlineClientProcess, uiTreeRoot = uiTreeRoot };
    }

    //  TODO: Improve symbols: https://github.com/Arcitectus/Sanderling/commit/ada11c9f8df2367976a6bcc53efbe9917107bfa7
    var memoryReadingPartialPythonModel =
        PartialPythonModelFromUITreeRoot(
            MemoryReaderFromLiveProcessId(uiTreeRootSearchResultCache.Value.process.Id),
            uiTreeRootSearchResultCache?.uiTreeRoot);

    var memoryReadingReducedWithNamedNodes =
        SanderlingMemoryReadingFromPartialPythonModel(memoryReadingPartialPythonModel);

    return new MemoryReadingResult
    {
        partialPythonModelJson = SerializeToJsonForBot(memoryReadingPartialPythonModel),
        reducedWithNamedNodesJson = SerializeToJsonForBot(memoryReadingReducedWithNamedNodes),
    };
}

