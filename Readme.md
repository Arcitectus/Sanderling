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

## Where are the Bots?

Some bots using Sanderling:

+ Warp-To-0 Autopilot: [https://github.com/Viir/bots/blob/master/guide/eve-online/how-to-automate-traveling-in-eve-online-using-a-warp-to-0-autopilot.md](https://github.com/Viir/bots/blob/master/guide/eve-online/how-to-automate-traveling-in-eve-online-using-a-warp-to-0-autopilot.md)

+ Mining asteroids: [https://github.com/Viir/bots/blob/master/guide/eve-online/how-to-automate-mining-asteroids-in-eve-online.md](https://github.com/Viir/bots/blob/master/guide/eve-online/how-to-automate-mining-asteroids-in-eve-online.md)

+ List of EVE Online Bots for Beginners: [https://forum.botengine.org/t/list-of-eve-online-bots-for-beginners/629](https://forum.botengine.org/t/list-of-eve-online-bots-for-beginners/629)

+ Most up-to-date: [https://botcatalog.org/](https://botcatalog.org/)

## Contributing

### Issues and other Feedback

Spotted a bug or have a feature request? Post on the [BotEngine forum](https://forum.botengine.org) or file an issue [on GitHub](https://github.com/Arcitectus/Sanderling/issues).
For communication here, supported languages are English, Spanish, and German.


### Pull Requests

The only supported language for pull request titles and commit messages is English.

