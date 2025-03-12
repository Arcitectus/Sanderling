# Alternate UI for EVE Online

![Alternate UI for EVE Online, this part shows the Overview.](./../../guide/image/2020-01-30.eve-online-overview-alternate-ui-and-game-client.png)

The alternate UI is a web-based user interface for the EVE Online client. Because of the HTML based rendering, this user interface is better accessible with screen-readers.

The alternate UI also lets you play the game from other devices that cannot run the EVE Online client but have a web browser. This way, you can play the game from your android smartphone or iPhone. This remote-play is possible because of the division into a frontend and backend, which communicate only via HTTP. The backend runs on the same machine as the EVE Online client and runs an HTTP server. The web-based frontend then connects to this HTTP server to read the game client's contents and send input commands.

This tool also shows the UI tree from the game client and presents the properties of the UI nodes in text form.

![Alternate UI for EVE Online, Visualization of the UI tree](./../../guide/image/2020-07-12-visualize-ui-tree.png)

There can be more than a thousand nodes in the UI tree, even in simple scenarios. And each of the nodes, in turn, can have many properties. So we have tens of thousands of properties in the UI tree when more objects are on the screen.

This quantity might make for a confusing impression, so I introduced a way to better focus on what is of interest to you in a given moment: You can expand and collapse individual nodes of the UI tree. For a collapsed node, it only shows a small summary, not all properties. When you get the first memory reading, all nodes are displayed collapsed, so only the summary of the root node is shown. You can expand that, and then the children of the root node. This way, you can descend into the part you are interested in.

![Screenshot showing both the game client and the tree view in the alternate UI, illustrating the relations between the different representations of the same UI elements.](./../../guide/image/2020-03-11-eve-online-parsed-user-interface-inventory-inspect.png)

There are two ways to get a memory reading into this interface:

+ Load from a live EVE Online client process. This user interface offers you input elements to interact with input elements in the EVE Online client. Note: When you send an input to the EVE Online client this way, the tool will switch the input focus to the EVE Online window and bring to the foreground. In case you run this user interface on the same desktop as the EVE Online client: To avoid interference between web browser window and game client window, place them side-by-side, so that they don't overlap.

+ Load from a file: You can load memory readings in JSON format you have saved earlier. Since this memory reading does not correspond to a live process, we use this option only to explore the general structure of information found in the game client's memory.

## Guide on the Parsing Library and Examples

Besides the program to read the UI tree from the game client, there is also a parsing library to help you make sense of the raw UI tree.

For a guide on the structures in the parsed memory reading, see https://to.botlab.org/guide/parsed-user-interface-of-the-eve-online-game-client

Developers use the parsing library to make ratting, mining, and mission running bots and intel tools. Following are some links to bots and tools using the parsing library:

+ <https://forum.botlab.org/t/list-of-eve-online-bots-for-beginners/629>
+ <https://catalog.botlab.org/?q=eve%2Bonline>

## Setup

These instructions to run the alternate UI start with the program source code. Here we use a tool called `pine` to compile the program from source code and run it as a web server.

Download the zip archive from <https://github.com/pine-vm/pine/releases/download/v0.4.0/pine-separate-assemblies-888533a4202c45996ea9fc8563620eb628ca2768-win-x64.zip> and extract it.

The extracted files contain the `pine` tool used to run Elm programs.

## Usage

To start the software:

+ Start PowerShell.
+ Run the `pine.exe` file from the zip archive you downloaded in the setup section. You can do this by navigating to the folder where you extracted the zip archive and then running the command `.\pine.exe`.

You then might see an error message like this:

```txt
You must install or update .NET to run this application.

App: C:\Users\Shadow\Downloads\pine-separate-assemblies-888533a4202c45996ea9fc8563620eb628ca2768-win-x64\pine.exe
Architecture: x64
Framework: 'Microsoft.NETCore.App', version '9.0.0' (x64)
.NET location: C:\Program Files\dotnet\

The following frameworks were found:
  3.1.18 at [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
  6.0.3 at [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
  7.0.9 at [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]

Learn about framework resolution:
https://aka.ms/dotnet/app-launch-failed

To install missing framework, download:
https://aka.ms/dotnet-core-applaunch?framework=Microsoft.NETCore.App&framework_version=9.0.0&arch=x64&rid=win10-x64
```

To resolve this issue, you need to install the .NET runtime version 9.X.X You can find the download link at <https://dotnet.microsoft.com/download/dotnet/9.0>

+ In the next command, we use the `pine.exe` file we got from the zip archive in the setup section. Below is an example of the complete command; you only need to replace the file path to the executable file:

```txt
."C:\replace-this-the-path-on-your-system\pine.exe"  run-server  --public-urls="http://*:80"  --deploy=https://github.com/Arcitectus/Sanderling/tree/b13fc46fa37e4b1e8feee53e4921632ad7dfa574/implement/alternate-ui/source
```

+ The command starts a web server and the shell window will display an output like this:

```txt
I got no path to a persistent store for the process. This process will not be persisted!
Loading app config to deploy...
This path looks like a URL into a remote git repository. Trying to load from there...
This path points to commit b13fc46fa37e4b1e8feee53e4921632ad7dfa574
Loaded source composition f6dec381281892b000dccfd505d637833dbfe04a4a0c8e164b07404ffb023d73 from 'https://github.com/Arcitectus/Sanderling/tree/b13fc46fa37e4b1e8feee53e4921632ad7dfa574/implement/alternate-ui/source'.
Starting web server with admin interface (using engine Pine { Caching = True, DynamicPGOShare =  })...
info: ElmTime.Platform.WebService.StartupAdminInterface[0]
      Begin to build the process live representation.
info: ElmTime.Platform.WebService.StartupAdminInterface[0]
      Begin to restore the process state.
info: ElmTime.Platform.WebService.StartupAdminInterface[0]
      Found 1 composition log records to use for restore.
info: ElmTime.Platform.WebService.StartupAdminInterface[0]
      Restored the process state in 1 seconds.
info: ElmTime.Platform.WebService.StartupAdminInterface[0]
      Completed building the process live representation.
info: ElmTime.Platform.WebService.PublicAppState[0]
      I did not find 'letsEncryptOptions' in the configuration. I continue without Let's Encrypt.
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://[::]:80
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: ElmTime.Platform.WebService.StartupAdminInterface[0]
      Started the public app at 'http://[::]:80'.
Completed starting the web server with the admin interface at 'http://*:4000'.
```

+ As the program keeps running, it will eventually write more to the same shell window, so the last output there can become something else.
+ With the command above, the program will try to use network port 80 on your system. In case this network port is already in use by another process, the command fails. In this case you get an error message containing the following text:

> System.IO.IOException: Failed to bind to address http://[::]:80: address already in use.

After starting the web server, you don't need to look at the shell window anymore, but leave it in the background. Closing the shell window would also stop the web server process.

Use a web browser (tested with Chrome and Firefox) to navigate to http://localhost:80/
There you find the Alternate EVE Online UI.

At the top, you find a section titled 'Select a source for the memory reading'. Here are two radio buttons to choose between the two possible sources:

+ From file
+ From live game client process

### Reading from file

Here you can load memory readings from JSON files.
After loading a memory reading, you can inspect it:

> Successfully read the reading from the file. Below is an interactive tree view to explore this reading. You can expand and collapse individual nodes.

### Reading from live process

When reading from a live process, the system needs to perform a setup steps, including the search for the root of the UI tree in the EVE Online client process. During the setup stage you will see diverse messages informing about the current step.

The memory reading setup should complete within 20 seconds. 

If no EVE Online client is started, it displays following message:

> Looks like there is no EVE Online client process started. I continue looking in case one is started...

As long as reading from live process is selected, the program tries once per seconds to get a new memory reading from the game client.

When setup is complete you see following message:

> Successfully read from the memory of the live process

Below is a button labeled:

> Click here to download this reading to a JSON file.

The memory reading file you can download here is useful for collaboration: In the 'Reading from file' section, people can load this file into the UI to see the same memory reading that you had on your system. 

Under the save button, you get tools for closer examination of the memory reading:

> Below is an interactive tree view to explore this reading. You can expand and collapse individual nodes.

### Enabling the Elm Inspection ('Debugger') Tool

To use the Elm inspection tool in the frontend, open the page http://localhost:80/with-inspector instead of http://localhost:80
