# Sanderling

**Sanderling helps you automate tasks in [EVE Online](https://www.eveonline.com) and read information from the game client.**

## Features

+ **safe**: does not inject into or write to the EVE Online client. That is why using it with EVE Online is not detectable.
+ **accurate & robust**: Sanderling uses memory reading to retrieve information about the game state. In contrast to screen scraping, this approach won't be thrown off by a noisy background or non-default UI settings.
+ **easy to use**: You will achieve quick results with the [integrated script engine](https://github.com/Arcitectus/Sanderling/wiki/Script-Engine) and API explorer.
+ **comprehensive**: Sanderling is used to build mining, trading, mission running and [anomaly ratting](https://github.com/botengine-de/A-Bot) bots.

## Requirements

+ the application requires Microsoft .NET Framework 4.6.1 which can be downloaded from [https://www.microsoft.com/download/details.aspx?id=49982](https://www.microsoft.com/download/details.aspx?id=49982).

## Getting Started

To start with automation in EVE Online, see the [List of EVE Online Bots for Beginners](https://forum.botengine.org/t/list-of-eve-online-bots-for-beginners/629)

## Feedback

Spotted a bug or have a feature request? Post on the [forum](https://forum.botengine.org) or file an issue [on GitHub](https://github.com/Arcitectus/Sanderling/issues).

## Need Help?

Do you have a question or need help with the development of your bot? Get in contact with other developers on the [BotEngine Forum](https://forum.botengine.org).

## Information For Developers

### Bot Creators

This is a list of guides and resources for bot developers:

+ [Bot Creator Guide](https://github.com/Arcitectus/Sanderling/wiki/Bot-Creator-Guide)
+ Terplas [beginners guide for botting in EVE Online](https://forum.botengine.org/t/terpla-adventures-or-blog-style-guide-for-begginers/953)
+ [Explaining the different types of memory readings](https://forum.botengine.org/t/sanderling-framework-differences-between-memorymeasurement-memorymeasurementparsed-and-memorymeasurementaccu/1256)
+ [How to Select a Target](https://forum.botengine.org/t/how-to-select-a-target/600)
+ [How to Activate a Ship Module](https://forum.botengine.org/t/how-to-activate-a-ship-module-in-eve-online/602)

### Building from source
The source code uses C# 7 features. It is recommended to use [Visual Studio](https://www.visualstudio.com/) version 2017 or newer for building.

### Contributing
See the [Contributing Guide](Contributing.md)

<br><br><br><br>

![Visualization of data read from the EVE Online client memory.](image/uitree.extract.png)
