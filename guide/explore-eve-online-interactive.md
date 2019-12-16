# Explore EVE Online Interactive

Install the tool from [https://github.com/filipw/dotnet-script](https://github.com/filipw/dotnet-script)

After installing `dotnet-script`, you can enter an interactive session in Windows PowerShell using the following command:

```PowerShell
dotnet-script
```

In this interactive session, you can use C-Sharp syntax to run custom code. You can enter a C-Sharp expression or statement and use the `Enter` key to evaluate/execute it. Following is a simple example:

```PowerShell
1 + 3
```

When you enter this simple expression, the interactive responds with `4` as the return value. After starting a new scripting session, execution of the first command can take several seconds because it triggers some setup work that is only required once per session. Subsequent commands don't have this overhead and can complete in less than one second.

Leave the interactive session with the key-combo `CTRL` + `C`.

For working with the EVE Online client, we run a setup script on entering the interactive session. Instead of using the command as above, we add the path to a setup script from this repository. The full command then looks as follows:

```PowerShell
dotnet-script -i "prepare-for-explore-eve-online-client.csx"
```

Before running this command, switch to the directory containing the script file and the `lib` directory. To get these files and directories to your local system, you can use the `Download ZIP` button in the GitHub UI. I see this currently points to the following URL: [https://github.com/Arcitectus/Sanderling/archive/master.zip](https://github.com/Arcitectus/Sanderling/archive/master.zip). In this Zip-Archive, you also find this guide, the setup script, and the `lib` directory, all contained in the `guide` directory.

After entering the `dotnet-script` interactive with said command, we can begin exploring the EVE Online client's memory.

To get a memory reading from a currently running EVE Online process, use the following command in the interactive:

```dotnet-script
var memoryReading = memoryReadingFromLiveProcess();
```

Then you can get the JSON representation consumed by bots out of this reading result using the `reducedWithNamedNodesJson` property on the `memoryReading`.

You can also save this to a file using the following command:

```dotnet-script
File.WriteAllText("reducedWithNamedNodesJson.json", memoryReading.reducedWithNamedNodesJson);
```
