using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Sanderling.ExploreProcessMeasurement
{
	public class Program
	{
		static string CommandsOverview =>
			String.Join("\n", new[]
			{
				"Available commands are:",
				"----",
				String.Join("\n----\n", namedCommands.Select(DescriptionForNamedCommand)),
				"----",
			});

		static string DescriptionForNamedCommand(KeyValuePair<string, CommandDescription> namedCommand) =>
			"Command '" + namedCommand.Key + "':\n" + namedCommand.Value.description;

		static IDictionary<string, CommandDescription> namedCommands =>
			new Dictionary<string, CommandDescription>()
			{
				{ "load-process-measurement-from-file",
					new CommandDescription
					{
						@delegate = LoadProcessMeasurementFromFile,
						description = "Load a process measurement from the given file path.",
					}
				},
				{ "read-memory-measurement-from-process-measurement",
					new CommandDescription
					{
						@delegate = ReadMemoryMeasurementFromProcessMeasurement,
						description = "Read a sanderling memory measurement from the currently loaded process measurement.",
					}
				},
				{ "explore-memory-measurement",
					new CommandDescription
					{
						@delegate = ExploreMemoryMeasurement,
						description = "Explore the sanderling memory measurement at the given path.",
					}
				},
			};

		class CommandDescription
		{
			public Func<AppState, IReadOnlyList<string>, (AppState, string)> @delegate;

			public string description;
		}

		class AppState
		{
			public Process.Measurement.Measurement processMeasurement;

			public Interface.MemoryStruct.IMemoryMeasurement memoryMeasurement;

			public AppState WithProcessMeasurement(Process.Measurement.Measurement processMeasurement) =>
				new AppState
				{
					processMeasurement = processMeasurement,
				};

			public AppState WithMemoryMeasurement(Interface.MemoryStruct.IMemoryMeasurement memoryMeasurement) =>
				new AppState
				{
					processMeasurement = processMeasurement,
					memoryMeasurement = memoryMeasurement,
				};
		}

		static (AppState, string) LoadProcessMeasurementFromFile(AppState appState, IReadOnlyList<string> args)
		{
			var filePath = args?.FirstOrDefault();

			if (!(0 < filePath?.Length))
				return (appState, "Error: No file path given.");

			try
			{
				var processMeasurementFile = System.IO.File.ReadAllBytes(filePath);

				var processMeasurementFileHashSHA1 = new SHA1Managed().ComputeHash(processMeasurementFile);

				var processMeasurement = Process.Measurement.Extension.MeasurementFromZipArchive(processMeasurementFile);

				return
					(appState.WithProcessMeasurement(processMeasurement),
					"Loaded process measurement with SHA1 of " + BitConverter.ToString(processMeasurementFileHashSHA1).Replace("-", ""));
			}
			catch (Exception e)
			{
				return (appState, e.ToString());
			}
		}

		static (AppState, string) ReadMemoryMeasurementFromProcessMeasurement(AppState appState, IReadOnlyList<string> args)
		{
			if (appState.processMeasurement == null)
				return (appState, "Error: No process measurement loaded.");

			var memoryReader = new BotEngine.Interface.Process.Snapshot.SnapshotReader(
				appState.processMeasurement?.Process?.MemoryBaseAddressAndListOctet);

			var memoryMeasurement = memoryReader.MemoryMeasurement();

			return (appState.WithMemoryMeasurement(memoryMeasurement),
				"memory measurement read succesfully\n" + MemoryMeasurementStats(memoryMeasurement));
		}

		static string MemoryMeasurementStats(Interface.MemoryStruct.IMemoryMeasurement memoryMeasurement) =>
			String.Join("\n",
			new[]
			{
				Interface.MemoryStruct.Extension.EnumerateReferencedUIElementTransitive(memoryMeasurement).Count() + " UIElements found.",
			});

		static string ExplorationPathMeasurementSymbol => "memoryMeasurement";

		static (AppState, string) ExploreMemoryMeasurement(AppState appState, IReadOnlyList<string> args)
		{
			var memoryMeasurement = appState?.memoryMeasurement;

			if (memoryMeasurement == null)
				return (appState, "Error: No memory measurement loaded.");

			var pathToExplore = args?.FirstOrDefault();

			var objectFoundAtPath = EvaluateExpression(appState.memoryMeasurement, pathToExplore);

			var objectDetailsLines =
				objectFoundAtPath == null ? new[] { "null" } : new[]
			{
				objectFoundAtPath.ToString(),
				"Type.FullName: " + objectFoundAtPath.GetType().FullName,
				"----",
				ExplorePropertiesMessage(objectFoundAtPath),
			};

			var messageLines = new[]
			{
				"Object found at path '" + pathToExplore + "':\n",
			}.Concat(objectDetailsLines).ToList();

			return (appState.WithMemoryMeasurement(memoryMeasurement), String.Join("\n", messageLines));
		}

		static string ExplorePropertiesMessage(object obj)
		{
			if (obj == null)
				return null;

			var properties = obj.GetType().GetProperties();

			return "Properties of this object:\n-\n" +
				String.Join("\n", properties.Select(property => property.Name + ": " + property.GetValue(obj)?.ToString()));
		}

		static object EvaluateExpression(Interface.MemoryStruct.IMemoryMeasurement memoryMeasurement, string path)
		{
			var pathCsharpExpression = nameof(ExplorationGlobalSymbols.MemoryMeasurement) + path;

			var assembliesToImport = new[]
			{
				typeof(System.Linq.Enumerable),
				typeof(Interface.MemoryStruct.MemoryMeasurement),
				typeof(ExplorationGlobalSymbols),
			}.Select(type => type.Assembly).Distinct().ToList();

			var options =
				ScriptOptions.Default
				.AddReferences(assembliesToImport)
				.AddImports("System", "System.Linq", "System.Collections.Generic");

			return CSharpScript.EvaluateAsync(
				pathCsharpExpression,
				options,
				new ExplorationGlobalSymbols { MemoryMeasurement = memoryMeasurement }).Result;
		}

		public class ExplorationGlobalSymbols
		{
			public Interface.MemoryStruct.IMemoryMeasurement MemoryMeasurement { set; get; }
		}

		static AppState initAppState => new AppState { };

		static void Main(string[] args)
		{
			Console.WriteLine("Welcome to the Sanderling Process Measurement Explorer.");
			Console.WriteLine(CommandsOverview);

			AppState appState = initAppState;

			while (true)
			{
				Console.Write("> ");

				var commandLine = Console.ReadLine();

				var (commandName, commandArguments) = CommandNameAndArgumentsFromCommandline(commandLine);

				if (!namedCommands.TryGetValue(commandName, out var command))
				{
					Console.WriteLine("I did not find a command named '" + commandName + "'");
					continue;
				}

				try
				{
					var (nextAppState, message) = command.@delegate(appState, commandArguments);

					Console.WriteLine(message);
					appState = nextAppState;
				}
				catch (Exception e)
				{
					Console.WriteLine("Error executing command '" + commandName + "':\n" + e.ToString());
				}
			}
		}

		static (string name, IReadOnlyList<string> arguments) CommandNameAndArgumentsFromCommandline(string commandline)
		{
			var commandSymbolAndArguments =
				Regex.Matches(commandline, CommandLineArgumentRegexPattern)
				.OfType<Match>()
				.Select(match => match.Groups["arg"].Value)
				.ToList();

			return (commandSymbolAndArguments.First(), commandSymbolAndArguments.Skip(1).ToList());
		}

		static string CommandLineArgumentRegexPattern => "((\\\"(?<arg>[^\"]*))\\\"|(?<arg>[^\\s]+))(\\s|$)";
	}
}
