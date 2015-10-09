# Sanderling
Sanderling is a botting framework for [eve online](https://www.eveonline.com).
It reads information about the game state from the memory of the eve online client process and delivers it to the consuming application packaged in a graph of appropriately linked CLR objects.

### Features
* **safe**: Windows won't tell on you for reading another process' memory. That is why using this tool with the eve online client is not detectable.
* **accurate & robust**: In contrast to extracting information from the game window via  image processing, this solution won't be thrown off by a noisy background or non-default UI settings.
* **comprehensive**: provides all information that is needed to build an autopilot, mining, trading or mission bot.

### Sample
to get a complete sample that sets up the reading and gets all available memory contents:

 * get the sources from `https://github.com/Arcitectus/Sanderling.git`
 * open the solution `src\sample\sample.sln` in [Visual Studio 2015](https://www.visualstudio.com/).

### Contact
Spotted a bug or have a question or feature request? File an issue [here](https://github.com/Arcitectus/Sanderling/issues) or post on the forum at [http://forum.botengine.de/cat/eve-online](http://forum.botengine.de/cat/eve-online).

### Applications
a complete bot built on this solution can be found at [https://github.com/Arcitectus/Optimat.EveOnline/releases](https://github.com/Arcitectus/Optimat.EveOnline/releases) 
