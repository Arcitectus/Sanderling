# Read Memory From the 64-Bit EVE Online Client

For a guide on the functionality not specific to EVE Online, see [https://github.com/Viir/bots/blob/main/guide/how-to-collect-samples-for-64-bit-memory-reading-development.md](https://github.com/Viir/bots/blob/main/guide/how-to-collect-samples-for-64-bit-memory-reading-development.md)

## read-memory-64-bit.exe

This software helps you read from 64-bit game client processes.

Use the `--pid` parameter to specify the ID of the game client process to read from. You can find this ID in the Windows Task Manager in the `PID` column in the `Processes` tab.

### save-process-sample

The `save-process-sample` command saves a sample of the process to a file. This sample includes all committed memory regions, so we can use it to support memory reading development.

Below is an example of a full command to use the tool:
```cmd
read-memory-64-bit.exe  save-process-sample  --pid=12345
```

## Sources

+ Discussion of the approach to read the UI Tree from the EVE Online client process. (Artifacts for 32-bit back then):
https://forum.botengine.org/t/advanced-do-it-yourself-memory-reading-in-eve-online/68

I found some articles that might help understand CPython in general:

+ Your Guide to the CPython Source Code: [https://realpython.com/cpython-source-code-guide/#memory-management-in-cpython](https://realpython.com/cpython-source-code-guide/#memory-management-in-cpython)
+ Python consumes a lot of memory or how to reduce the size of objects? [https://habr.com/en/post/458518/](https://habr.com/en/post/458518/)
