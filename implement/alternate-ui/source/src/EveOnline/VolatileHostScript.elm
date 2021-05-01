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

//  "System.Drawing.Common"
#r "sha256:C5333AA60281006DFCFBBC0BC04C217C581EFF886890565E994900FB60448B02"

//  "System.Drawing.Primitives"
#r "sha256:CA24032E6D39C44A01D316498E18FE9A568D59C6009842029BC129AA6B989BCD"

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Runtime.InteropServices;


int readingFromGameCount = 0;
var generalStopwatch = System.Diagnostics.Stopwatch.StartNew();

var readingFromGameHistory = new Queue<ReadingFromGameClient>();


byte[] SHA256FromByteArray(byte[] array)
{
    using (var hasher = new SHA256Managed())
        return hasher.ComputeHash(buffer: array);
}

string ToStringBase16(byte[] array) => BitConverter.ToString(array).Replace("-", "");


struct ReadingFromGameClient
{
    public IntPtr windowHandle;

    public string readingId;

    public int[][] pixels_1x1_R8G8B8;
}

class Request
{
    public object ListGameClientProcessesRequest;

    public SearchUIRootAddressStructure SearchUIRootAddress;

    public ReadFromWindowStructure ReadFromWindow;

    public TaskOnWindow<EffectSequenceElement[]> EffectSequenceOnWindow;

    public ConsoleBeepStructure[] EffectConsoleBeepSequence;

    public GetImageDataFromSpecificReadingStructure? GetImageDataFromReading;

    public class SearchUIRootAddressStructure
    {
        public int processId;
    }

    public class ReadFromWindowStructure
    {
        public string windowId;

        public GetImageDataFromReadingStructure getImageData;

        public ulong uiRootAddress;
    }

    public struct GetImageDataFromSpecificReadingStructure
    {
        public string readingId;

        public GetImageDataFromReadingStructure getImageData;
    }

    public struct GetImageDataFromReadingStructure
    {
        public Rect2d[] screenshot1x1Rects;
    }

    public class TaskOnWindow<Task>
    {
        public string windowId;

        public bool bringWindowToForeground;

        public Task task;
    }

    public class EffectSequenceElement
    {
        public EffectOnWindowStructure effect;

        public int? delayMilliseconds;
    }

    public class EffectOnWindowStructure
    {
        public MouseMoveToStructure MouseMoveTo;

        public KeyboardKey KeyDown;

        public KeyboardKey KeyUp;
    }

    public class KeyboardKey
    {
        public int virtualKeyCode;
    }

    public class MouseMoveToStructure
    {
        public Location2d location;
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

    public ReadFromWindowResultStructure ReadFromWindowResult;

    public GetImageDataFromReadingResultStructure? GetImageDataFromReadingResult;

    public string FailedToBringWindowToFront;

    public object CompletedEffectSequenceOnWindow;

    public object CompletedOtherEffect;

    public class GameClientProcessSummaryStruct
    {
        public int processId;

        public string mainWindowId;

        public string mainWindowTitle;

        public int mainWindowZIndex;
    }

    public class SearchUIRootAddressResultStructure
    {
        public int processId;

        public string uiRootAddress;
    }

    public class ReadFromWindowResultStructure
    {
        public object ProcessNotFound;

        public CompletedStructure Completed;

        public class CompletedStructure
        {
            public int processId;

            public Location2d windowClientRectOffset;

            public string readingId;

            public GetImageDataFromReadingResultStructure imageData;

            public string memoryReadingSerialRepresentationJson;
        }
    }

    public struct GetImageDataFromReadingResultStructure
    {
        public ImageCrop[] screenshot1x1Rects;
    }
}

public struct ImageCrop
{
    public Location2d offset;

    public int[][] pixels_R8G8B8;
}

public struct Rect2d
{
    public int x, y, width, height;
}

public struct Location2d
{
    public Int64 x, y;
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

    if (request.ReadFromWindow != null)
    {
        var readingFromGameIndex = System.Threading.Interlocked.Increment(ref readingFromGameCount);

        var readingId = readingFromGameIndex.ToString("D6") + "-" + generalStopwatch.ElapsedMilliseconds;

        var windowId = request.ReadFromWindow.windowId;
        var windowHandle = new IntPtr(long.Parse(windowId));

        WinApi.GetWindowThreadProcessId(windowHandle, out var processIdUnsigned);

        if (processIdUnsigned == 0)
            return new Response
            {
                ReadFromWindowResult = new Response.ReadFromWindowResultStructure
                {
                    ProcessNotFound = new object(),
                }
            };

        var processId = (int)processIdUnsigned;

        var windowRect = new WinApi.Rect();
        WinApi.GetWindowRect(windowHandle, ref windowRect);

        var clientRectOffsetFromScreen = new WinApi.Point(0, 0);
        WinApi.ClientToScreen(windowHandle, ref clientRectOffsetFromScreen);

        var windowClientRectOffset =
            new Location2d
            { x = clientRectOffsetFromScreen.x - windowRect.left, y = clientRectOffsetFromScreen.y - windowRect.top };

        string memoryReadingSerialRepresentationJson = null;

        using (var memoryReader = new read_memory_64_bit.MemoryReaderFromLiveProcess(processId))
        {
            var uiTree = read_memory_64_bit.EveOnline64.ReadUITreeFromAddress(request.ReadFromWindow.uiRootAddress, memoryReader, 99);

            if(uiTree != null)
            {
                memoryReadingSerialRepresentationJson = Newtonsoft.Json.JsonConvert.SerializeObject(
                    uiTree.WithOtherDictEntriesRemoved(),
                    //  Support popular JSON parsers: Wrap large integers in a string to work around limitations there. (https://discourse.elm-lang.org/t/how-to-parse-a-json-object/4977)
                    new read_memory_64_bit.IntegersToStringJsonConverter()
                    );
            }
        }

        {
            /*
            Maybe taking screenshots needs the window to be not occluded by other windows.
            We can review this later.
            */
            var setForegroundWindowError = SetForegroundWindowInWindows.TrySetForegroundWindow(windowHandle);

            if(setForegroundWindowError != null)
            {
                return new Response
                {
                    FailedToBringWindowToFront = setForegroundWindowError,
                };
            }
        }

        var pixels_1x1_R8G8B8 = GetScreenshotOfWindowAsPixelsValues_R8G8B8(windowHandle);

        var historyEntry = new ReadingFromGameClient
        {
            windowHandle = windowHandle,
            readingId = readingId,
            pixels_1x1_R8G8B8 = pixels_1x1_R8G8B8,
        };

        var imageData = CompileImageDataFromReadingResult(request.ReadFromWindow.getImageData, historyEntry);

        readingFromGameHistory.Enqueue(historyEntry);

        while(4 < readingFromGameHistory.Count)
        {
            readingFromGameHistory.Dequeue();
        }

        return new Response
        {
            ReadFromWindowResult = new Response.ReadFromWindowResultStructure
            {
                Completed = new Response.ReadFromWindowResultStructure.CompletedStructure
                {
                    processId = processId,
                    windowClientRectOffset = windowClientRectOffset,
                    memoryReadingSerialRepresentationJson = memoryReadingSerialRepresentationJson,
                    readingId = readingId,
                    imageData = imageData,
                },
            },
        };
    }

    if (request?.GetImageDataFromReading?.readingId != null)
    {
        var historyEntry =
            readingFromGameHistory
            .Cast<ReadingFromGameClient?>()
            .FirstOrDefault(c => c?.readingId == request?.GetImageDataFromReading?.readingId);

        if (historyEntry == null)
        {
            return new Response
            {
                GetImageDataFromReadingResult = new Response.GetImageDataFromReadingResultStructure
                {
                },
            };
        }

        return new Response
        {
            GetImageDataFromReadingResult =
                CompileImageDataFromReadingResult(request.GetImageDataFromReading.Value.getImageData, historyEntry.Value),
        };
    }

    if (request?.EffectSequenceOnWindow?.task != null)
    {
        var windowHandle = new IntPtr(long.Parse(request.EffectSequenceOnWindow.windowId));

        if (request.EffectSequenceOnWindow.bringWindowToForeground)
        {
            var setForegroundWindowError = SetForegroundWindowInWindows.TrySetForegroundWindow(windowHandle);

            if(setForegroundWindowError != null)
            {
                return new Response
                {
                    FailedToBringWindowToFront = setForegroundWindowError,
                };
            }
        }

        foreach(var sequenceElement in request.EffectSequenceOnWindow.task)
        {
            if(sequenceElement?.effect != null)
                ExecuteEffectOnWindow(sequenceElement.effect, windowHandle, request.EffectSequenceOnWindow.bringWindowToForeground);

            if(sequenceElement?.delayMilliseconds != null)
                System.Threading.Thread.Sleep(sequenceElement.delayMilliseconds.Value);
        }

        return new Response
        {
            CompletedEffectSequenceOnWindow = new object(),
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
            CompletedOtherEffect = new object(),
        };
    }

    return null;
}

Response.GetImageDataFromReadingResultStructure CompileImageDataFromReadingResult(
    Request.GetImageDataFromReadingStructure request,
    ReadingFromGameClient historyEntry)
{
    ImageCrop[] screenshot1x1Rects = null;

    if (historyEntry.pixels_1x1_R8G8B8 != null)
    {
        screenshot1x1Rects =
            request.screenshot1x1Rects
            .Select(rect =>
            {
                var cropPixels = CopyRectangularCrop(historyEntry.pixels_1x1_R8G8B8, rect);

                return new ImageCrop
                {
                    pixels_R8G8B8 = cropPixels,
                    offset = new Location2d { x = rect.x, y = rect.y },
                };
            }).ToArray();
    }

    return new Response.GetImageDataFromReadingResultStructure
    {
        screenshot1x1Rects = screenshot1x1Rects,
    };
}

int[][] CopyRectangularCrop(int[][] original, Rect2d rect)
{
    return
        original
        .Skip(rect.y)
        .Take(rect.height)
        .Select(rowPixels =>
        {
            if (rect.x == 0 && rect.width == rowPixels.Length)
                return rowPixels;

            var cropRowPixels = new int[rect.width];

            System.Buffer.BlockCopy(rowPixels, rect.x * 4, cropRowPixels, 0, rect.width * 4);

            return cropRowPixels;
        })
        .ToArray();
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
        BotEngine.WinApi.User32.SetForegroundWindow(windowHandle);

    if (effectOnWindow?.MouseMoveTo != null)
    {
        //  Build motion description based on https://github.com/Arcitectus/Sanderling/blob/ada11c9f8df2367976a6bcc53efbe9917107bfa7/src/Sanderling/Sanderling/Motor/Extension.cs#L24-L131

        var mousePosition = new Bib3.Geometrik.Vektor2DInt(
            effectOnWindow.MouseMoveTo.location.x,
            effectOnWindow.MouseMoveTo.location.y);

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

    if (effectOnWindow?.KeyDown != null)
    {
        var virtualKeyCode = (WindowsInput.Native.VirtualKeyCode)effectOnWindow.KeyDown.virtualKeyCode;

        (MouseActionForKeyUpOrDown(keyCode: virtualKeyCode, buttonUp: false)
        ??
        (() => new WindowsInput.InputSimulator().Keyboard.KeyDown(virtualKeyCode)))();
    }

    if (effectOnWindow?.KeyUp != null)
    {
        var virtualKeyCode = (WindowsInput.Native.VirtualKeyCode)effectOnWindow.KeyUp.virtualKeyCode;

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

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

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

    [DllImport("user32.dll")]
    static public extern IntPtr ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    static public extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

    [DllImport("user32.dll", SetLastError = false)]
    static public extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static public extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    static public extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

    [DllImport("user32.dll", SetLastError=true)]
    static public extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
}

static public class SetForegroundWindowInWindows
{
    static public int AltKeyPlusSetForegroundWindowWaitTimeMilliseconds = 60;

    /// <summary>
    /// </summary>
    /// <param name="windowHandle"></param>
    /// <returns>null in case of success</returns>
    static public string TrySetForegroundWindow(IntPtr windowHandle)
    {
        try
        {
            /*
            * For the conditions for `SetForegroundWindow` to work, see https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setforegroundwindow
            * */
            BotEngine.WinApi.User32.SetForegroundWindow(windowHandle);

            if (BotEngine.WinApi.User32.GetForegroundWindow() == windowHandle)
                return null;

            var windowsInZOrder = WinApi.ListWindowHandlesInZOrder();

            var windowIndex = windowsInZOrder.ToList().IndexOf(windowHandle);

            if (windowIndex < 0)
                return "Did not find window for this handle";

            {
                var simulator = new WindowsInput.InputSimulator();

                simulator.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.MENU);
                BotEngine.WinApi.User32.SetForegroundWindow(windowHandle);
                simulator.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.MENU);

                System.Threading.Thread.Sleep(AltKeyPlusSetForegroundWindowWaitTimeMilliseconds);

                if (BotEngine.WinApi.User32.GetForegroundWindow() == windowHandle)
                    return null;

                return "Alt key plus SetForegroundWindow approach was not successful.";
            }
        }
        catch (Exception e)
        {
            return "Exception: " + e.ToString();
        }
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
                mainWindowId = process.MainWindowHandle.ToInt64().ToString(),
                mainWindowTitle = process.MainWindowTitle,
                mainWindowZIndex = zIndexFromWindowHandle(process.MainWindowHandle) ?? 9999,
            };
        })
        .ToList();

    return processes;
}

public int[][] GetScreenshotOfWindowAsPixelsValues_R8G8B8(IntPtr windowHandle)
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

public System.Drawing.Bitmap GetScreenshotOfWindowAsBitmap(IntPtr windowHandle)
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

string InterfaceToHost_Request(string request)
{
    return serialRequest(request);
}

"""
