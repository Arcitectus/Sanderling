using Bib3;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Sanderling.Exe
{
	public partial class App : Application
	{
		class CLI
		{
			static public CLI LastInstance;

			[Option(Description = "Path to a file to load a bot from when the application starts.", ShortName = "")]
			public string LoadBotFromFile { get; }

			[Option(Description = "Start the loaded bot directly.", ShortName = "")]
			public bool StartBot { get; }

			[Option(Description = "How many times a bot should be restarted if it crashes.", ShortName = "")]
			public int BotCrashRetryCountMax { get; }

			public string ReportAllArguments() => Newtonsoft.Json.JsonConvert.SerializeObject(this);

			private void OnExecute()
			{
				Console.WriteLine("CLI.OnExecute");

				LastInstance = this;
			}
		}

		static public Int64 GetTimeStopwatch() => Bib3.Glob.StopwatchZaitMiliSictInt();

		public MainWindow Window => base.MainWindow as MainWindow;

		Bib3.FCL.GBS.ToggleButtonHorizBinär BotOperationPauseContinueToggleButton => Window?.Main?.ToggleButtonMotionEnable;

		UI.BotAPIExplorer BotAPIExplorer => Window?.Main?.DevToolsAPIExplorer;

		BotSharp.ScriptRun.ScriptRun ScriptRun => MainControl?.BotsNavigation?.ScriptRun;

		bool WasActivated = false;

		DispatcherTimer Timer;

		static string AssemblyDirectoryPath => Bib3.FCL.Glob.ZuProcessSelbsctMainModuleDirectoryPfaadBerecne().EnsureEndsWith(@"\");

		Sanderling.Script.Impl.HostToScript UIAPI;

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			WriteLogEntryWithTimeNow(new Log.LogEntry
			{
				Startup = new Log.StartupLogEntry
				{
					Args = e.Args,
				}
			});

			Exception parseCommandsFromArgumentsException = null;

			try
			{
				CommandLineApplication.Execute<CLI>(e.Args);
			}
			catch (Exception exception)
			{
				parseCommandsFromArgumentsException = exception;
			}

			WriteLogEntryWithTimeNow(new Log.LogEntry
			{
				ParseCommandsFromArguments = new Log.ParseCommandsFromArgumentsEntry
				{
					Arguments = CLI.LastInstance?.ReportAllArguments(),
					Exception = parseCommandsFromArgumentsException,
				}
			});
		}

		public App()
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

			AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
				UnhandledException(sender, e.ExceptionObject as Exception);

			CreateLogFile();

			UIAPI = new Sanderling.Script.Impl.HostToScript
			{
				MemoryMeasurementFunc = () => MemoryMeasurementLast,
			};
		}

		private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			var MatchFullName =
				AppDomain.CurrentDomain.GetAssemblies()
				?.FirstOrDefault(candidate => string.Equals(candidate.GetName().FullName, args?.Name));

			if (null != MatchFullName)
			{
				return MatchFullName;
			}

			var MatchName =
				AppDomain.CurrentDomain.GetAssemblies()
				?.FirstOrDefault(candidate => string.Equals(candidate.GetName().Name, args?.Name));

			return MatchName;
		}

		void TimerConstruct()
		{
			Timer = new DispatcherTimer(TimeSpan.FromSeconds(1.0 / 10), DispatcherPriority.Normal, Timer_Tick, Dispatcher);

			Timer.Start();
		}

		private void Application_Activated(object sender, EventArgs e)
		{
			if (WasActivated)
			{
				return;
			}

			WasActivated = true;

			ActivatedFirstTime();
		}

		BotSharp.ScriptRun.IScriptRunClient ScriptRunClientBuild(BotSharp.ScriptRun.ScriptRun run)
		{
			return new Sanderling.Script.Impl.ScriptRunClient
			{
				InvalidateMeasurementAction = FromScriptInvalidateMeasurement,
				MemoryMeasurementLastDelegate = () => MemoryMeasurementLast,
				FromScriptRequestMemoryMeasurementEvaluation = FromScriptRequestMemoryMeasurementEvaluation,
				FromScriptMotionExecute = FromScriptMotionExecute,
				GetWindowHandleDelegate = () => Motor?.WindowHandle ?? IntPtr.Zero,
				GetKillEveProcessAction = KillEveProcessAction,
				ExecutionStatusChangedDelegate = ScriptExecutionStatusChanged,
			};
		}

		void ActivatedFirstTime()
		{
			MainControl.BotsNavigation.ScriptParamBase = new BotSharp.ScriptParam
			{
				ImportAssembly = Script.ToScriptImport.ImportAssembly?.ToArray(),
				ImportNamespace = Sanderling.Script.Impl.ToScriptImport.ImportNamespace?.ToArray(),

				ScriptRunClientBuildDelegate = ScriptRunClientBuild,

				CompilationGlobalsType = typeof(Sanderling.Script.ToScriptGlobals),
			};

			MainControl.BotsNavigation.Configuration = BotsNavigationConfiguration();

			Window.KeyDown += Window_KeyDown;
			Window?.AddHandler(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, new RoutedEventHandler(ButtonClicked));

			TimerConstruct();

			Dispatcher.Invoke(ExecuteCommandsFromArguments);
		}

		void ExecuteCommandsFromArguments()
		{
			Exception exception = null;

			string argumentsReport = null;

			try
			{
				var arguments = CLI.LastInstance;

				argumentsReport = arguments?.ReportAllArguments();

				var botsNavigation = MainControl.BotsNavigation;

				var botFileName = arguments?.LoadBotFromFile;

				var bot = 0 < botFileName?.Length ? System.IO.File.ReadAllBytes(botFileName) : null;

				if (bot != null)
				{
					if (arguments.StartBot)
						botsNavigation.NavigateIntoOperateBot(bot, true);
					else
						botsNavigation.NavigateIntoPreviewBot(bot);
				}
			}
			catch (Exception e)
			{
				exception = e;
			}

			WriteLogEntryWithTimeNow(new Log.LogEntry
			{
				ExecuteCommandsFromArguments = new Log.ExecuteCommandsFromArgumentsEntry
				{
					Arguments = argumentsReport,
					Exception = exception,
				},
			});
		}

		void Timer_Tick(object sender, object e)
		{
			ProcessInput();

			Motor = GetMotor();

			InterfaceExchange();

			UpdateBotOperationPauseContinueToggleButton();

			UIPresent();
		}

		enum StartOrContinueBotTrigger
		{
			UserInterface,
			RetryAfterFail,
		}

		int RetryAfterBotFailCount = 0;

		void ContinueOrStartBotOperation(StartOrContinueBotTrigger trigger)
		{
			if (trigger == StartOrContinueBotTrigger.UserInterface)
				RetryAfterBotFailCount = 0;
			else
				++RetryAfterBotFailCount;

			WriteLogEntryWithTimeNow(
				new Log.LogEntry
				{
					ContinueOrStartBotOperation = new Log.ContinueOrStartBotOperationLogEntry
					{
						Trigger = trigger.ToString(),
					}
				});

			MainControl?.BotsNavigation?.ContinueOrStartBotOperation();
			UpdateBotOperationPauseContinueToggleButton();
		}

		void PauseBotOperation()
		{
			MainControl?.BotsNavigation?.PauseBotOperation();
			UpdateBotOperationPauseContinueToggleButton();
		}

		private void KillEveProcessAction()
		{
			Current.Dispatcher.Invoke(() =>
			{
				if (!EveOnlineClientProcessId.HasValue)
					return;

				var process = System.Diagnostics.Process.GetProcessById(EveOnlineClientProcessId.Value);

				process.Kill();
			});
		}

		void ScriptExecutionStatusChanged(
			Sanderling.Script.Impl.ScriptRunClient scriptRunClient,
			BotSharp.ScriptRun.ScriptRun scriptRun)
		{
			if (scriptRun.Status == BotSharp.ScriptRun.ScriptRunExecutionStatus.Failed)
			{
				if (RetryAfterBotFailCount < CLI.LastInstance.BotCrashRetryCountMax)
				{
					System.Threading.Tasks.Task.Run(() =>
					{
						System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(1)).Wait();

						Dispatcher.BeginInvoke(
							new Action(() => ContinueOrStartBotOperation(StartOrContinueBotTrigger.RetryAfterFail)));
					});
				}
			}
		}

		private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			UnhandledException(sender, e.Exception);
			e.Handled = true;
		}

		static int UnhandledExceptionCount = 0;

		void UnhandledException(object sender, Exception exception)
		{
			try
			{
				var exceptionIndex = System.Threading.Interlocked.Increment(ref UnhandledExceptionCount);

				var filePath = AssemblyDirectoryPath.PathToFilesysChild(
					"[" + DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ss.fff") + "][" + exceptionIndex + "].Exception");

				filePath.WriteToFileAndCreateDirectoryIfNotExisting(System.Text.Encoding.UTF8.GetBytes(exception.SictString()));
			}
			catch (Exception persistException)
			{
				Bib3.FCL.GBS.Extension.MessageBoxException(persistException);
			}

			Bib3.FCL.GBS.Extension.MessageBoxException(exception);
		}
	}
}
