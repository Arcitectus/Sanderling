# Sanderling

**Sanderling helps you read information from the [EVE Online](https://www.eveonline.com) game client.**

Sanderling is the eyes of bots and monitoring tools. It helps programs see the game client in a structured way, detecting objects and reading information about the game world. It also reads the locations of elements in the game clients' graphical user interface (e.g., in-game windows, overview entries, buttons, etc.). You can use this information to interact with the game client using mouse input.

<br>

![Visualization of data read from the EVE Online client memory.](guide/image/2015-12-13.uitree.extract.png)

<br>

## Features

+ **safe**: does not inject into or write to the EVE Online client. That is why using it with EVE Online is not detectable.
+ **accurate & robust**: Sanderling uses memory reading to get information about the game state and user interface. In contrast to screen scraping, this approach won't be thrown off by a noisy background or non-default UI settings.
+ **comprehensive**: Sanderling memory reading is used in [mining](https://github.com/Viir/bots/blob/master/guide/eve-online/how-to-automate-mining-asteroids-in-eve-online.md), trading, mission running and [anomaly ratting](https://github.com/botengine-de/A-Bot) bots.

## Repository contents

Below is an overview of the software components maintained in this repository. The tools and projects listed below come primarily in the form of source code, and you might have to build them from source if you want to use the latest version. For guidance on the build process(es), see the respective subdirectories containing the source codes.
If you don't want to build the tools from source, you can find pre-built and ready-to-execute binaries in the [`releases` section on GitHub](https://github.com/Arcitectus/Sanderling/releases).

### Memory reading library

This library implements the functionality to read from the memory of (64-bit) EVE Online client processes. It reads the UI tree from the game client user interface. It is written using the C# programming language, and the build output is the .NET Core assembly `read-memory-64-bit.dll`.

Location in the repository: [/implement/read-memory-64-bit](/implement/read-memory-64-bit)

### Tool to get and save process samples and memory readings

This software integrates multiple functions in a single executable file:

+ Saving process samples to files for later inspection, automated testing, and collaboration. This function is not specific to EVE Online but can be used with other game clients as well.
+ Reading the structures in the EVE Online clients' memory. You can let it read from a live process or a previously saved process sample. It also offers to save the memory reading results into a JSON file.

The compiled binary is distributed in the file `read-memory-64-bit.exe`.

Location in the repository: [/implement/read-memory-64-bit](/implement/read-memory-64-bit)

### Memory reading parsing library

This library takes the result of an EVE Online memory reading and transforms it into a format that is easier to use for integrating applications like bots.

The UI tree in the EVE Online client can contain thousands of nodes and tens of thousands of individual properties. Because of this vast amount of data, navigating in there can be time-consuming. To make this easier, this library filters and transforms the memory reading result into a form that contains less redundant information and uses names more closely related to the experience of players; for example, the overview window or ship modules.

The input for this library is the JSON string, as we get it from the memory reading tool. In contrast to the low-level memory reading library, it is written in a high-level language better suited for the development of user interfaces and bots.

Location in the repository: [/implement/alternate-ui/elm-app/src/Sanderling/MemoryReading.elm](/implement/alternate-ui/elm-app/src/Sanderling/MemoryReading.elm)

### Alternate UI for EVE Online

TODO: Describe the alternative user interface for the EVE Online game client.

## Bots

Bots are not in the scope of this repository, but I sure have some links with further information.

There is this guide on developing bots: [https://github.com/Viir/bots/blob/master/guide/eve-online/how-to-develop-eve-online-bots.md](https://github.com/Viir/bots/blob/master/guide/eve-online/how-to-develop-eve-online-bots.md)

And these are some bots using Sanderling:

+ Warp-To-0 Autopilot: [https://github.com/Viir/bots/blob/master/guide/eve-online/how-to-automate-traveling-in-eve-online-using-a-warp-to-0-autopilot.md](https://github.com/Viir/bots/blob/master/guide/eve-online/how-to-automate-traveling-in-eve-online-using-a-warp-to-0-autopilot.md)

+ Mining asteroids: [https://github.com/Viir/bots/blob/master/guide/eve-online/how-to-automate-mining-asteroids-in-eve-online.md](https://github.com/Viir/bots/blob/master/guide/eve-online/how-to-automate-mining-asteroids-in-eve-online.md)

+ List of EVE Online Bots for Beginners: [https://forum.botengine.org/t/list-of-eve-online-bots-for-beginners/629](https://forum.botengine.org/t/list-of-eve-online-bots-for-beginners/629)

+ Most up-to-date list of bots: [https://botcatalog.org/](https://botcatalog.org/)

## Contributing

### Issues and other Feedback

Spotted a bug or have a feature request? Post on the [BotEngine forum](https://forum.botengine.org) or file an issue [on GitHub](https://github.com/Arcitectus/Sanderling/issues).
For communication here, supported languages are English, Spanish, and German.


### Pull Requests

The only supported language for pull request titles and commit messages is English.

