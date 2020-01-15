using BotEngine.Common;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Sanderling.Parse
{
	public class DroneLabel
	{
		public string Name;

		public string Status;
	}

	static public class Drone
	{
		public const string DroneEntryLabelRegexPattern = "([^\\(]+)\\(([^\\)]+)";

		static public Regex DroneEntryLabelRegex = DroneEntryLabelRegexPattern.AsRegexCompiledIgnoreCase();

		static public DroneLabel AsDroneLabel(this string droneLabel)
		{
			var Name = droneLabel?.Trim();
			string Status = null;

			var Match = DroneEntryLabelRegex.Match(droneLabel ?? "");

			if (Match.Success)
			{
				Name = Match.Groups[1].Value?.Trim();
				Status = Match.Groups[2].Value?.RemoveXmlTag()?.Trim();
			}

			return new DroneLabel()
			{
				Name = Name,
				Status = Status,
			};
		}
	}
}
