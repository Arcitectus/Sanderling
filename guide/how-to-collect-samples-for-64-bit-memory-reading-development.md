# How to Collect Samples for 64-bit Memory Reading Development

This guide explains how to save an example of a game client process to a file. Such examples are to support the development of memory reading frameworks or bots.

The tool we use in this guide works only for 64-bit processes.

The tool copies the memory contents of a chosen Windows process (such as a game client) and takes a screenshot from its main window and writes those to a file. This data is used in development to correlate screen contents with memory contents.

Here you can see a typical scenario where we use this tool: https://forum.botlab.org/t/mining-bot-i-cannot-see-the-ore-hold-capacity-gauge/3101

**Steps to collect a sample:**

+ Download and unpack the zip-archive from <https://github.com/Arcitectus/Sanderling/releases/download/v2024-01-03/read-memory-64-bit-self-contained-single-file-exe-801f28d6ad5afbecca7ad83024a634cbb15a2b3e-win-x64.zip>
+ Find the game client in the Windows Task Manager.
+ Make sure the name of the game client displayed in the Windows Task Manager does not contain `(32 bit)`.
+ Read the process ID of the game client process in the `PID` column in the Task Manager.
+ Ensure the game client window is visible and not minimized.
+ Use the Windows Command Prompt to run the tool, using the following command:

```cmd
read-memory-64-bit.exe  save-process-sample  --pid=12345
```

+ The tool then creates a process sample file in the directory currently selected in the Command Prompt. When successful, the program exits with a message like the following:

```cmd
Saved sample F2CC4E4EC28482747A05172990F7B54CFABAA7F80C2DB83B81E86D3F41523551 to file 'process-sample-F2CC4E4EC2.zip'.
```

Since this tool does not interfere with the game client's operation, the game client can change data structures during the timespan needed to copy the memory contents. Because the memory contents are copied at different times, there is a risk of inconsistencies in the copy, which in turn can make it unusable for development. There are two ways to counter this risk:

+ Using more samples to increase the chance that one is good.
+ Reducing the time needed to copy the memory contents. You can achieve this by reducing the memory usage of the game client. Memory usage is often affected by settings, such as the resolution or quality of textures used for graphics rendering. Reducing memory use has the added benefit of smaller sample files.


### Troubleshooting

#### `Parameter is not valid` in `Bitmap..ctor`

```txt
Unhandled exception. System.ArgumentException: Parameter is not valid.
   at System.Drawing.SafeNativeMethods.Gdip.CheckStatus(Int32 status)
   at System.Drawing.Bitmap..ctor(Int32 width, Int32 height, PixelFormat format)
   at read_memory_64_bit.Program.GetScreenshotOfWindowClientAreaAsBitmap(IntPtr windowHandle) 
```

This error happens when the main window of the chosen process is minimized.

To avoid the error, ensure the game client window is visible and not minimized.
