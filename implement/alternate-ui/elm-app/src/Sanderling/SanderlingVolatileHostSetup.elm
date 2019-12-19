module Sanderling.SanderlingVolatileHostSetup exposing ( sanderlingSetupScript )


sanderlingSetupScript : String
sanderlingSetupScript =
    """
#r "sha256:FE8A38EBCED27A112519023A7A1216C69FE0863BCA3EF766234E972E920096C1"
#r "sha256:11DCCA7041E1436B858BAC75E2577CA471ABA40208C4214ABD90A717DD89CEF6"
#r "sha256:5229128932E6AAFB5433B7AA5E05E6AFA3C19A929897E49F83690AB8FE273162"
#r "sha256:CADE001866564D185F14798ECFD077EDA6415E69D978748C19B98DDF0EE839BB"
#r "sha256:FE532D93F820980181F34C163E54F83726876CC9B02FEC72086FD3DC747793BC"
#r "sha256:C6E93D210F2A71438B9BEDDDA3D9E0CAB723A179BB9F2400A983EEF72FDF9FB5"
#r "sha256:831EF0489D9FA85C34C95F0670CC6393D1AD9548EE708E223C1AD87B51F7C7B3"
#r "sha256:137CF2631884C20D61F6C4FA122624ACE70780B3A24E12D9172AE3582EDA46E4"
#r "sha256:B9B4E633EA6C728BAD5F7CBBEF7F8B842F7E10181731DBE5EC3CD995A6F60287"
#r "sha256:81110D44256397F0F3C572A20CA94BB4C669E5DE89F9348ABAD263FBD81C54B9"
#r "sha256:2A89B0F057A26E1273DECC0FC7FE9C2BB12683479E37076D23A1F73CCC324D13"
#r "mscorlib"
#r "netstandard"
#r "System"
#r "System.Collections.Immutable"
#r "System.ComponentModel.Primitives"
#r "System.IO.Compression"
#r "System.Net"
#r "System.Net.WebClient"
#r "System.Private.Uri"
#r "System.Linq"
#r "System.Security.Cryptography.Algorithms"
#r "System.Security.Cryptography.Primitives"
using Sanderling.ExploreProcessMeasurement;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
var trigger_assembly_load = new BotEngine.Interface.ColorHSV().ToString();
byte[] SHA256FromByteArray(byte[] array)
{
    using (var hasher = new SHA256Managed())
        return hasher.ComputeHash(buffer: array);
}
string ToStringBase16(byte[] array) => BitConverter.ToString(array).Replace("-", "");
class Request
{
    public object getEveOnlineProcessesIds;
    public GetMemoryMeasurement getMemoryMeasurement;
    public TaskOnWindow<EffectOnWindow> effectOnWindow;
    public class GetMemoryMeasurement
    {
        public int processId;
    }
    public class TaskOnWindow<Task>
    {
        public string windowId;
        public bool bringWindowToForeground;
        public Task task;
    }
    public class EffectOnWindow
    {
        public SimpleMouseClickAtLocation simpleMouseClickAtLocation;
        public SimpleDragAndDrop simpleDragAndDrop;
        public KeyboardKey keyDown;
        public KeyboardKey keyUp;
    }
    public class KeyboardKey
    {
        public int virtualKeyCode;
    }
    public class SimpleMouseClickAtLocation
    {
        public Location2d location;
        public MouseButton mouseButton;
    }
    public class SimpleDragAndDrop
    {
        public Location2d startLocation;
        public MouseButton mouseButton;
        public Location2d endLocation;
    }
    public class Location2d
    {
        public Int64 x, y;
    }
    public enum MouseButton
    {
        left, right,
    }
}
class Response
{
    public int[] eveOnlineProcessesIds;
    public GetMemoryMeasurementResult getMemoryMeasurementResult;
    public object effectExecuted;
    public class GetMemoryMeasurementResult
    {
        public object processNotFound;
        public Completed completed;
        public class Completed
        {
            public string mainWindowId;
            public string partialPythonJson;
            public string reducedWithNamedNodesJson;
        }
    }
}
string serialRequest(string serializedRequest)
{
    var requestStructure = Newtonsoft.Json.JsonConvert.DeserializeObject<Request>(serializedRequest);
    var response = request(requestStructure);
    return SerializeToJsonForBot(response);
}
struct UiTreeRootSearchResultCache
{
    public int processId;
    public Optimat.EveOnline.MemoryAuswertWurzelSuuce uiTreeRoot;
}
UiTreeRootSearchResultCache? uiTreeRootSearchResultCache = null;
Response request(Request request)
{
    SetProcessDPIAware();
    if (request.getEveOnlineProcessesIds != null)
    {
        return new Response
        {
            eveOnlineProcessesIds =
                GetWindowsProcessesLookingLikeEVEOnlineClient().Select(proc => proc.Id).ToArray(),
        };
    }
    if (request.getMemoryMeasurement != null)
    {
        var processId = request.getMemoryMeasurement.processId;
        if (!GetWindowsProcessesLookingLikeEVEOnlineClient().Select(proc => proc.Id).Contains(processId))
            return new Response
            {
                getMemoryMeasurementResult = new Response.GetMemoryMeasurementResult
                {
                    processNotFound = new object(),
                }
            };
        var process = System.Diagnostics.Process.GetProcessById(processId);
        var memoryReader = MemoryReaderFromLiveProcessId(processId);
        if (uiTreeRootSearchResultCache?.processId != processId)
            uiTreeRootSearchResultCache = null;
        var uiTreeRoot =
            uiTreeRootSearchResultCache?.uiTreeRoot ?? UITreeRootFromMemoryReader(memoryReader);
        //  TODO: Improve symbols: https://github.com/Arcitectus/Sanderling/commit/ada11c9f8df2367976a6bcc53efbe9917107bfa7
        var partialPython = PartialPythonModelFromUITreeRoot(memoryReader, uiTreeRoot);
        var memoryMeasurement = SanderlingMemoryMeasurementFromPartialPythonModel(partialPython);
        uiTreeRootSearchResultCache = new UiTreeRootSearchResultCache { processId = processId, uiTreeRoot = uiTreeRoot };
        return new Response
        {
            getMemoryMeasurementResult = new Response.GetMemoryMeasurementResult
            {
                completed = new Response.GetMemoryMeasurementResult.Completed
                {
                    mainWindowId = process.MainWindowHandle.ToInt64().ToString(),
                    partialPythonJson = SerializeToJsonForBot(partialPython),
                    reducedWithNamedNodesJson = SerializeToJsonForBot(memoryMeasurement),
                },
            },
        };
    }
    if (request?.effectOnWindow?.task != null)
    {
        var windowHandle = new IntPtr(long.Parse(request.effectOnWindow.windowId));
        ExecuteEffectOnWindow(request.effectOnWindow.task, windowHandle, request.effectOnWindow.bringWindowToForeground);
        return new Response
        {
            effectExecuted = new object(),
        };
    }
    return null;
}
void ExecuteEffectOnWindow(
    Request.EffectOnWindow effectOnWindow,
    IntPtr windowHandle,
    bool bringWindowToForeground)
{
    if (bringWindowToForeground)
        EnsureWindowIsForeground(windowHandle);
    //  TODO: Consolidate simpleMouseClickAtLocation and simpleDragAndDrop?
    if (effectOnWindow?.simpleMouseClickAtLocation != null)
    {
        //  Build motion description based on https://github.com/Arcitectus/Sanderling/blob/ada11c9f8df2367976a6bcc53efbe9917107bfa7/src/Sanderling/Sanderling/Motor/Extension.cs#L24-L131
        var mousePosition = new Bib3.Geometrik.Vektor2DInt(
            effectOnWindow.simpleMouseClickAtLocation.location.x,
            effectOnWindow.simpleMouseClickAtLocation.location.y);
        var mouseButton =
            effectOnWindow.simpleMouseClickAtLocation.mouseButton == Request.MouseButton.right
            ? BotEngine.Motor.MouseButtonIdEnum.Right : BotEngine.Motor.MouseButtonIdEnum.Left;
        var mouseButtons = new BotEngine.Motor.MouseButtonIdEnum[]
        {
            mouseButton,
        };
        var windowMotor = new Sanderling.Motor.WindowMotor(windowHandle);
        var motionSequence = new BotEngine.Motor.Motion[]{
            new BotEngine.Motor.Motion(
                mousePosition: mousePosition,
                mouseButtonDown: mouseButtons,
                windowToForeground: bringWindowToForeground),
            new BotEngine.Motor.Motion(
                mousePosition: mousePosition,
                mouseButtonUp: mouseButtons,
                windowToForeground: bringWindowToForeground),
        };
        windowMotor.ActSequenceMotion(motionSequence);
    }
    if (effectOnWindow?.simpleDragAndDrop != null)
    {
        //  Build motion description based on https://github.com/Arcitectus/Sanderling/blob/ada11c9f8df2367976a6bcc53efbe9917107bfa7/src/Sanderling/Sanderling/Motor/Extension.cs#L24-L131
        var startMousePosition = new Bib3.Geometrik.Vektor2DInt(
            effectOnWindow.simpleDragAndDrop.startLocation.x,
            effectOnWindow.simpleDragAndDrop.startLocation.y);
        var endMousePosition = new Bib3.Geometrik.Vektor2DInt(
            effectOnWindow.simpleDragAndDrop.endLocation.x,
            effectOnWindow.simpleDragAndDrop.endLocation.y);
        var mouseButton =
            effectOnWindow.simpleDragAndDrop.mouseButton == Request.MouseButton.right
            ? BotEngine.Motor.MouseButtonIdEnum.Right : BotEngine.Motor.MouseButtonIdEnum.Left;
        var mouseButtons = new BotEngine.Motor.MouseButtonIdEnum[]
        {
            mouseButton,
        };
        var windowMotor = new Sanderling.Motor.WindowMotor(windowHandle);
        var motionSequence = new BotEngine.Motor.Motion[]{
            new BotEngine.Motor.Motion(
                mousePosition: startMousePosition,
                mouseButtonDown: mouseButtons,
                windowToForeground: bringWindowToForeground),
            new BotEngine.Motor.Motion(
                mousePosition: endMousePosition,
                mouseButtonDown: new BotEngine.Motor.MouseButtonIdEnum[]{},
                windowToForeground: bringWindowToForeground),
            new BotEngine.Motor.Motion(
                mousePosition: endMousePosition,
                mouseButtonUp: mouseButtons,
                windowToForeground: bringWindowToForeground),
        };
        windowMotor.ActSequenceMotion(motionSequence);
    }
    if (effectOnWindow?.keyDown != null)
    {
        new WindowsInput.InputSimulator().Keyboard.KeyDown((WindowsInput.Native.VirtualKeyCode)effectOnWindow.keyDown.virtualKeyCode);
    }
    if (effectOnWindow?.keyUp != null)
    {
        new WindowsInput.InputSimulator().Keyboard.KeyUp((WindowsInput.Native.VirtualKeyCode)effectOnWindow.keyUp.virtualKeyCode);
    }
}
static void EnsureWindowIsForeground(
    IntPtr windowHandle)
{
    var PreviousForegroundWindowHandle = BotEngine.WinApi.User32.GetForegroundWindow();
    if (PreviousForegroundWindowHandle == windowHandle)
    {
        return;
    }
    BotEngine.WinApi.User32.SetForegroundWindow(windowHandle);
}
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
Optimat.EveOnline.AuswertGbs.UINodeInfoInTree PartialPythonModelFromUITreeRoot(
    BotEngine.Interface.IMemoryReader memoryReader,
    Optimat.EveOnline.MemoryAuswertWurzelSuuce uiTreeRoot) =>
    Optimat.EveOnline.AuswertGbs.Extension.SictAuswert(
        Sanderling.ExploreProcessMeasurement.Extension.ReadUITreeFromRoot(memoryReader, uiTreeRoot));
Sanderling.Interface.MemoryStruct.IMemoryMeasurement SanderlingMemoryMeasurementFromPartialPythonModel(
    Optimat.EveOnline.GbsAstInfo partialPython) =>
    Optimat.EveOnline.AuswertGbs.Extension.SensorikScnapscusKonstrukt(partialPython, null);
IEnumerable<T> EnumerateNodeFromTreeDFirst<T>(
    T root,
    Func<T, IEnumerable<T>> callbackEnumerateChildInNode,
    int? depthMax = null,
    int? depthMin = null) =>
    Bib3.Extension.EnumerateNodeFromTreeDFirst(root, callbackEnumerateChildInNode, depthMax, depthMin);
IEnumerable<Sanderling.Interface.MemoryStruct.IUIElement> EnumerateReferencedSanderlingUIElementsTransitive(
    object parent) =>
    parent == null ? null :
    Sanderling.Interface.MemoryStruct.Extension.EnumerateReferencedUIElementTransitive(parent)
    .Distinct();
void SetProcessDPIAware()
{
    //  https://www.google.com/search?q=GetWindowRect+dpi
    //  https://github.com/dotnet/wpf/issues/859
    //  https://github.com/dotnet/winforms/issues/135
    WinApi.SetProcessDPIAware();
}
static public class WinApi
{
    [DllImport("user32.dll", SetLastError = true)]
    static public extern bool SetProcessDPIAware();
}
struct Rectangle
{
    public Rectangle(Int64 left, Int64 top, Int64 right, Int64 bottom)
    {
        this.left = left;
        this.top = top;
        this.right = right;
        this.bottom = bottom;
    }
    readonly public Int64 top, left, bottom, right;
    override public string ToString() =>
        Newtonsoft.Json.JsonConvert.SerializeObject(this);
}
struct UINodeMostPopularProperties
{
    public readonly Int64? pythonObjectAddress;
    public readonly string pythonTypeName;
    public readonly Rectangle? region;
    public UINodeMostPopularProperties(Optimat.EveOnline.AuswertGbs.UINodeInfoInTree uiNode)
    {
        pythonObjectAddress = uiNode.PyObjAddress;
        pythonTypeName = uiNode.PyObjTypName;
        var uiNodeRegion = RawRectFromUITreeNode(uiNode);
        region =
            uiNodeRegion.HasValue ? (Rectangle?)NamesFromRawRectInt(uiNodeRegion.Value) : null;
    }
}
static Rectangle NamesFromRawRectInt(Bib3.Geometrik.RectInt raw) =>
    new Rectangle(left: raw.Min0, top: raw.Min1, right: raw.Max0, bottom: raw.Max1);
Func<Optimat.EveOnline.AuswertGbs.UINodeInfoInTree, bool> UITreeNodeRegionIntersectsRectangle(Rectangle rectangle) =>
    uiNode =>
    {
        var uiNodeRegion = RawRectFromUITreeNode(uiNode);
        if (!uiNodeRegion.HasValue)
            return false;
        return
            !Bib3.Geometrik.RectExtension.IsEmpty(Bib3.Geometrik.Geometrik.Intersection(
                uiNodeRegion.Value,
                Bib3.Geometrik.RectInt.FromMinPointAndMaxPoint(
                    new Bib3.Geometrik.Vektor2DInt(rectangle.left, rectangle.top),
                    new Bib3.Geometrik.Vektor2DInt(rectangle.right, rectangle.bottom))));
    };
static Bib3.Geometrik.RectInt? RawRectFromUITreeNode(Optimat.EveOnline.AuswertGbs.UINodeInfoInTree node) =>
    Optimat.EveOnline.AuswertGbs.Glob.FläceAusGbsAstInfoMitVonParentErbe(node);
Func<Sanderling.Interface.MemoryStruct.IUIElement, bool> UIElementRegionIntersectsRectangle(Rectangle rectangle) =>
    uiElement =>
    !Bib3.Geometrik.RectExtension.IsEmpty(Bib3.Geometrik.Geometrik.Intersection(
        uiElement.Region,
        Bib3.Geometrik.RectInt.FromMinPointAndMaxPoint(
            new Bib3.Geometrik.Vektor2DInt(rectangle.left, rectangle.top),
            new Bib3.Geometrik.Vektor2DInt(rectangle.right, rectangle.bottom))));
System.Diagnostics.Process[] GetWindowsProcessesLookingLikeEVEOnlineClient() =>
    System.Diagnostics.Process.GetProcessesByName("exefile");
IReadOnlyList<T> FindNodesOnPathFromTreeNodeToDescendant<T>(T pathRoot, Func<T, IEnumerable<T>> getChildrenFromNode, T descendant)
    =>
    FindNodesOnPathFromTreeNodeToDescendantMatchingPredicate(
        pathRoot,
        getChildrenFromNode,
        candidate => ((object)candidate == null && (object)descendant == null) || (candidate?.Equals(descendant) ?? false));
IReadOnlyList<T> FindNodesOnPathFromTreeNodeToDescendantMatchingPredicate<T>(
    T pathRoot,
    Func<T, IEnumerable<T>> getChildrenFromNode,
    Func<T, bool> descendantPredicate) =>
    Bib3.Extension.EnumeratePathToNodeFromTreeBFirst(pathRoot, getChildrenFromNode)
    .FirstOrDefault(path => descendantPredicate(path.Last()));
"Sanderling Setup Completed"
"""
