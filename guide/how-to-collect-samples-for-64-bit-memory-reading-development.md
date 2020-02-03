# How to Collect Samples for 64-bit Memory Reading Development

This guide explains how to save an example of a game client process to a file. Such a sample allows seeing the state in which your EVE Online client was at the time you take the sample. These samples can also be used as training data for bot development.

The tool we use in this guide works only for 64-bit processes.

The tool copies the memory contents of a chosen Windows process (such as a game client) and takes a screenshot from its main window and writes those to a file. This data is used in development to correlate screen contents with memory contents.

**Steps to collect a sample:**

+ Download and unpack the zip-archive from [https://github.com/Arcitectus/Sanderling/releases/download/v2020-01-20/2020-01-20.read-memory-64-bit.zip](https://github.com/Arcitectus/Sanderling/releases/download/v2020-01-20/2020-01-20.read-memory-64-bit.zip)
+ Find the game client in the Windows Task Manager.
+ Make sure the name of the game client displayed in the Windows Task Manager does not contain `(32 bit)`.
+ Read the process ID of the game client process in the `PID` column in the Task Manager.
+ Use the Windows Command Prompt to run the tool, using the following command:
```cmd
read-memory-64-bit.exe  save-process-sample  --pid=12345
```
+ The tool then creates a process sample file in the directory currently selected in the Command Prompt. When successful, the program exits with a message like the following:
```cmd
Saved sample F2CC4E4EC28482747A05172990F7B54CFABAA7F80C2DB83B81E86D3F41523551 to file 'process-sample-F2CC4E4EC2.zip'.
```
