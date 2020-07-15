module EveOnline.VolatileHostScript exposing (setupScript)


setupScript : String
setupScript =
    """
#r "sha256:FE8A38EBCED27A112519023A7A1216C69FE0863BCA3EF766234E972E920096C1"
#r "sha256:5229128932E6AAFB5433B7AA5E05E6AFA3C19A929897E49F83690AB8FE273162"
#r "sha256:CADE001866564D185F14798ECFD077EDA6415E69D978748C19B98DDF0EE839BB"
#r "sha256:FE532D93F820980181F34C163E54F83726876CC9B02FEC72086FD3DC747793BC"
#r "sha256:831EF0489D9FA85C34C95F0670CC6393D1AD9548EE708E223C1AD87B51F7C7B3"
#r "sha256:B9B4E633EA6C728BAD5F7CBBEF7F8B842F7E10181731DBE5EC3CD995A6F60287"
#r "sha256:81110D44256397F0F3C572A20CA94BB4C669E5DE89F9348ABAD263FBD81C54B9"
#r "sha256:1CE5129364865C5D50DC4ED71E330D3FF4F04054541E461ABDFE543D254307E2"

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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Runtime.InteropServices;


byte[] SHA256FromByteArray(byte[] array)
{
    using (var hasher = new SHA256Managed())
        return hasher.ComputeHash(buffer: array);
}

string ToStringBase16(byte[] array) => BitConverter.ToString(array).Replace("-", "");


class Request
{
    public object ListGameClientProcessesRequest;

    public SearchUIRootAddressStructure SearchUIRootAddress;

    public GetMemoryReadingStructure GetMemoryReading;

    public TaskOnWindow<EffectOnWindowStructure> EffectOnWindow;

    public ConsoleBeepStructure[] EffectConsoleBeepSequence;

    public class SearchUIRootAddressStructure
    {
        public int processId;
    }

    public class GetMemoryReadingStructure
    {
        public int processId;

        public ulong uiRootAddress;
    }

    public class TaskOnWindow<Task>
    {
        public string windowId;

        public bool bringWindowToForeground;

        public Task task;
    }

    public class EffectOnWindowStructure
    {
        public MouseMoveToStructure mouseMoveTo;

        public SimpleMouseClickAtLocation simpleMouseClickAtLocation;

        public KeyboardKey keyDown;

        public KeyboardKey keyUp;
    }

    public class KeyboardKey
    {
        public int virtualKeyCode;
    }

    public class MouseMoveToStructure
    {
        public Location2d location;
    }

    public class SimpleMouseClickAtLocation
    {
        public Location2d location;

        public MouseButton mouseButton;
    }

    public class Location2d
    {
        public Int64 x, y;
    }

    public enum MouseButton
    {
        left, right,
    }

    public struct ConsoleBeepStructure
    {
        public int frequency;

        public int durationInMs;
    }
}

class Response
{
    public GameClientProcessSummaryStruct[] ListGameClientProcessesResponse;

    public SearchUIRootAddressResultStructure SearchUIRootAddressResult;

    public GetMemoryReadingResultStructure GetMemoryReadingResult;

    public object effectExecuted;

    public class GameClientProcessSummaryStruct
    {
        public int processId;

        public string mainWindowTitle;

        public int mainWindowZIndex;
    }

    public class SearchUIRootAddressResultStructure
    {
        public int processId;

        public string uiRootAddress;
    }

    public class GetMemoryReadingResultStructure
    {
        public object ProcessNotFound;

        public CompletedStructure Completed;

        public class CompletedStructure
        {
            public string mainWindowId;

            public string serialRepresentationJson;
        }
    }
}

string serialRequest(string serializedRequest)
{
    var requestStructure = Newtonsoft.Json.JsonConvert.DeserializeObject<Request>(serializedRequest);

    var response = request(requestStructure);

    return SerializeToJsonForBot(response);
}

Response request(Request request)
{
    SetProcessDPIAware();

    if (request.ListGameClientProcessesRequest != null)
    {
        return new Response
        {
            ListGameClientProcessesResponse =
                ListGameClientProcesses().ToArray(),
        };
    }

    if (request.SearchUIRootAddress != null)
    {
        var uiTreeRootAddress = FindUIRootAddressFromProcessId(request.SearchUIRootAddress.processId);

        return new Response
        {
            SearchUIRootAddressResult = new Response.SearchUIRootAddressResultStructure
            {
                processId = request.SearchUIRootAddress.processId,
                uiRootAddress = uiTreeRootAddress?.ToString(),
            },
        };
    }

    if (request.GetMemoryReading != null)
    {
        var processId = request.GetMemoryReading.processId;

        if (!GetWindowsProcessesLookingLikeEVEOnlineClient().Select(proc => proc.Id).Contains(processId))
            return new Response
            {
                GetMemoryReadingResult = new Response.GetMemoryReadingResultStructure
                {
                    ProcessNotFound = new object(),
                }
            };

        var process = System.Diagnostics.Process.GetProcessById(processId);

        string serialRepresentationJson = null;

        using (var memoryReader = new read_memory_64_bit.MemoryReaderFromLiveProcess(processId))
        {
            var uiTree = read_memory_64_bit.EveOnline64.ReadUITreeFromAddress(request.GetMemoryReading.uiRootAddress, memoryReader, 99);

            if(uiTree != null)
                serialRepresentationJson = Newtonsoft.Json.JsonConvert.SerializeObject(
                    uiTree.WithOtherDictEntriesRemoved(),
                    //  Support popular JSON parsers: Wrap large integers in a string to work around limitations there. (https://discourse.elm-lang.org/t/how-to-parse-a-json-object/4977)
                    new read_memory_64_bit.IntegersToStringJsonConverter()
                    );
        }

        return new Response
        {
            GetMemoryReadingResult = new Response.GetMemoryReadingResultStructure
            {
                Completed = new Response.GetMemoryReadingResultStructure.CompletedStructure
                {
                    mainWindowId = process.MainWindowHandle.ToInt64().ToString(),
                    serialRepresentationJson = serialRepresentationJson,
                },
            },
        };
    }

    if (request?.EffectOnWindow?.task != null)
    {
        var windowHandle = new IntPtr(long.Parse(request.EffectOnWindow.windowId));

        ExecuteEffectOnWindow(request.EffectOnWindow.task, windowHandle, request.EffectOnWindow.bringWindowToForeground);

        return new Response
        {
            effectExecuted = new object(),
        };
    }

    if (request?.EffectConsoleBeepSequence != null)
    {
        foreach (var beep in request?.EffectConsoleBeepSequence)
        {
            if(beep.frequency == 0) //  Avoid exception "The frequency must be between 37 and 32767."
                System.Threading.Thread.Sleep(beep.durationInMs);
            else
                System.Console.Beep(beep.frequency, beep.durationInMs);
        }

        return new Response
        {
            effectExecuted = new object(),
        };
    }

    return null;
}

ulong? FindUIRootAddressFromProcessId(int processId)
{
    var candidatesAddresses =
        read_memory_64_bit.EveOnline64.EnumeratePossibleAddressesForUIRootObjectsFromProcessId(processId);

    using (var memoryReader = new read_memory_64_bit.MemoryReaderFromLiveProcess(processId))
    {
        var uiTrees =
            candidatesAddresses
            .Select(candidateAddress => read_memory_64_bit.EveOnline64.ReadUITreeFromAddress(candidateAddress, memoryReader, 99))
            .ToList();

        return
            uiTrees
            .OrderByDescending(uiTree => uiTree?.EnumerateSelfAndDescendants().Count() ?? -1)
            .FirstOrDefault()
            ?.pythonObjectAddress;
    }
}

void ExecuteEffectOnWindow(
    Request.EffectOnWindowStructure effectOnWindow,
    IntPtr windowHandle,
    bool bringWindowToForeground)
{
    if (bringWindowToForeground)
        EnsureWindowIsForeground(windowHandle);

    //  TODO: Consolidate mouseMoveTo and simpleMouseClickAtLocation?

    if (effectOnWindow?.mouseMoveTo != null)
    {
        //  Build motion description based on https://github.com/Arcitectus/Sanderling/blob/ada11c9f8df2367976a6bcc53efbe9917107bfa7/src/Sanderling/Sanderling/Motor/Extension.cs#L24-L131

        var mousePosition = new Bib3.Geometrik.Vektor2DInt(
            effectOnWindow.mouseMoveTo.location.x,
            effectOnWindow.mouseMoveTo.location.y);

        var mouseButtons = new BotEngine.Motor.MouseButtonIdEnum[]{};

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

    if (effectOnWindow?.keyDown != null)
    {
        var virtualKeyCode = (WindowsInput.Native.VirtualKeyCode)effectOnWindow.keyDown.virtualKeyCode;

        (MouseActionForKeyUpOrDown(keyCode: virtualKeyCode, buttonUp: false)
        ??
        (() => new WindowsInput.InputSimulator().Keyboard.KeyDown(virtualKeyCode)))();
    }

    if (effectOnWindow?.keyUp != null)
    {
        var virtualKeyCode = (WindowsInput.Native.VirtualKeyCode)effectOnWindow.keyUp.virtualKeyCode;

        (MouseActionForKeyUpOrDown(keyCode: virtualKeyCode, buttonUp: true)
        ??
        (() => new WindowsInput.InputSimulator().Keyboard.KeyUp(virtualKeyCode)))();
    }
}

static System.Action MouseActionForKeyUpOrDown(WindowsInput.Native.VirtualKeyCode keyCode, bool buttonUp)
{
    WindowsInput.IMouseSimulator mouseSimulator() => new WindowsInput.InputSimulator().Mouse;

    var method = keyCode switch
    {
        WindowsInput.Native.VirtualKeyCode.LBUTTON =>
            buttonUp ?
            (System.Func<WindowsInput.IMouseSimulator>)mouseSimulator().LeftButtonUp
            : mouseSimulator().LeftButtonDown,
        WindowsInput.Native.VirtualKeyCode.RBUTTON =>
            buttonUp ?
            (System.Func<WindowsInput.IMouseSimulator>)mouseSimulator().RightButtonUp
            : mouseSimulator().RightButtonDown,
        _ => null
    };

    if (method != null)
        return () => method();

    return null;
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

            // https://stackoverflow.com/questions/7397207/json-net-error-self-referencing-loop-detected-for-type/18223985#18223985
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
        });


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

    [DllImport("user32.dll")]
    static public extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    /*
    https://stackoverflow.com/questions/19867402/how-can-i-use-enumwindows-to-find-windows-with-a-specific-caption-title/20276701#20276701
    https://stackoverflow.com/questions/295996/is-the-order-in-which-handles-are-returned-by-enumwindows-meaningful/296014#296014
    */
    public static System.Collections.Generic.IReadOnlyList<IntPtr> ListWindowHandlesInZOrder()
    {
        IntPtr found = IntPtr.Zero;
        System.Collections.Generic.List<IntPtr> windowHandles = new System.Collections.Generic.List<IntPtr>();

        EnumWindows(delegate (IntPtr wnd, IntPtr param)
        {
            windowHandles.Add(wnd);

            // return true here so that we iterate all windows
            return true;
        }, IntPtr.Zero);

        return windowHandles;
    }
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


System.Diagnostics.Process[] GetWindowsProcessesLookingLikeEVEOnlineClient() =>
    System.Diagnostics.Process.GetProcessesByName("exefile");


System.Collections.Generic.IReadOnlyList<Response.GameClientProcessSummaryStruct> ListGameClientProcesses()
{
    var allWindowHandlesInZOrder = WinApi.ListWindowHandlesInZOrder();

    int? zIndexFromWindowHandle(IntPtr windowHandleToSearch) =>
        allWindowHandlesInZOrder
        .Select((windowHandle, index) => (windowHandle, index: (int?)index))
        .FirstOrDefault(handleAndIndex => handleAndIndex.windowHandle == windowHandleToSearch)
        .index;

    var processes =
        GetWindowsProcessesLookingLikeEVEOnlineClient()
        .Select(process =>
        {
            return new Response.GameClientProcessSummaryStruct
            {
                processId = process.Id,
                mainWindowTitle = process.MainWindowTitle,
                mainWindowZIndex = zIndexFromWindowHandle(process.MainWindowHandle) ?? 9999,
            };
        })
        .ToList();

    return processes;
}

string InterfaceToHost_Request(string request)
{
    return serialRequest(request);
}

"""
