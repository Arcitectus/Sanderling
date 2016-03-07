using Bib3;
using BotEngine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Sanderling.Exe
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		static public Int64 GetTimeStopwatch() => Bib3.Glob.StopwatchZaitMiliSictInt();

		BotEngine.Client.LicenseClientConfig LicenseClientConfig => ConfigReadFromUI()?.LicenseClient;

		public MainWindow Window => base.MainWindow as MainWindow;

		Bib3.FCL.GBS.ToggleButtonHorizBinär ToggleButtonMotionEnable => Window?.Main?.ToggleButtonMotionEnable;

		BotSharp.UI.Wpf.IDE ScriptIDE => Window?.Main?.Bot?.IDE;

		UI.BotAPIExplorer BotAPIExplorer => Window?.Main?.Bot?.APIExplorer;

		BotSharp.ScriptRun ScriptRun => ScriptIDE?.ScriptRun;

		bool WasActivated = false;

		DispatcherTimer Timer;

		static string AssemblyDirectoryPath => Bib3.FCL.Glob.ZuProcessSelbsctMainModuleDirectoryPfaadBerecne().EnsureEndsWith(@"\");

		Sanderling.Script.HostToScript UIAPI;

		public App()
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

			UIAPI = new Sanderling.Script.HostToScript()
			{
				MemoryMeasurementFunc = new Func<FromProcessMeasurement<Interface.MemoryMeasurementEvaluation>>(() => MemoryMeasurementLast),
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

		Script.ToScriptGlobals ToScriptGlobalsConstruct(Action ScriptExecutionCheck) =>
			new Script.ToScriptGlobals()
			{
				Sanderling = new Sanderling.Script.HostToScript()
				{
					MemoryMeasurementFunc = () =>
					{
						ScriptExecutionCheck?.Invoke();
						return FromScriptRequestMemoryMeasurementEvaluation();
					},

					MotionExecuteFunc = MotionParam =>
					{
						ScriptExecutionCheck?.Invoke();
						return FromScriptMotionExecute(MotionParam);
					},

					InvalidateMeasurementAction = FromScriptInvalidateMeasurement,

					WindowHandleFunc = () => ((Motor.WindowMotor)Motor)?.WindowHandle ?? IntPtr.Zero,
				}
			};

		void ActivatedFirstTime()
		{
			ScriptIDE.ScriptRunGlobalsFunc = ToScriptGlobalsConstruct;

			ScriptIDE.ScriptParamBase = new BotSharp.ScriptParam()
			{
				ImportAssembly = Script.ToScriptImport.ImportAssembly?.ToArray(),
				ImportNamespace = Sanderling.Script.ToScriptImport.ImportNamespace?.ToArray(),
				CompilationOption = new BotSharp.CodeAnalysis.CompilationOption()
				{
					InstrumentationOption = BotSharp.CodeAnalysis.Default.InstrumentationOption,
				},
				PreRunCallback = new Action<BotSharp.ScriptRun>(ScriptRun =>
				{
					ScriptRun.InstrumentationCallbackSynchronousFirstTime = new Action<BotSharp.SourceLocation>(ScriptSourceLocation =>
					{
						//	make sure script runs on same culture independend of host culture.
						System.Threading.Thread.CurrentThread.CurrentCulture = Parse.Culture.ParseCulture;
					});
				}),
			};

			ScriptIDE.ChooseScriptFromIncludedScripts.SetScript =
				ListScriptIncluded?.Select(ScriptIdAndContent => new KeyValuePair<string, Func<string>>(ScriptIdAndContent.Key, () => ScriptIdAndContent.Value))?.ToArray();

			ScriptIDE.ScriptWriteToOrReadFromFile.DefaultFilePath = DefaultScriptPath;
			ScriptIDE.ScriptWriteToOrReadFromFile?.ReadFromFile();
			if (!(0 < ScriptIDE.Editor.Document.Text?.Length))
				ScriptIDE.Editor.Document.Text = ListScriptIncluded?.FirstOrDefault().Value ?? "";

			Window.KeyDown += Window_KeyDown;
			Window?.AddHandler(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, new RoutedEventHandler(ButtonClicked));

			Window?.Main?.ConfigFromModelToView(ConfigDefaultConstruct());

			ConfigFileControl.DefaultFilePath = ConfigFilePath;
			ConfigFileControl.CallbackGetValueToWrite = ConfigReadFromUISerialized;
			ConfigFileControl.CallbackValueRead = ConfigWriteToUIDeSerialized;
			ConfigFileControl.ReadFromFile();

			TimerConstruct();
		}

		void Timer_Tick(object sender, object e)
		{
			ProcessInput();

			Motor = GetMotor();

			InterfaceExchange();

			UIPresentScript();

			UIPresent();
		}

		void ButtonClicked(object sender, RoutedEventArgs e)
		{
			var OriginalSource = e?.OriginalSource;

			if (null != OriginalSource)
			{
				if (OriginalSource == ToggleButtonMotionEnable?.ButtonLinx)
				{
					ScriptRunPause();
				}

				if (OriginalSource == ToggleButtonMotionEnable?.ButtonRecz)
				{
					ScriptRunPlay();
				}
			}
		}

		void ScriptRunPlay()
		{
			ScriptIDE.ScriptRunContinueOrStart();

			UIPresentScript();
		}

		void ScriptRunPause()
		{
			ScriptIDE.ScriptPause();

			UIPresentScript();
		}

		private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			try
			{
				var FilePath = AssemblyDirectoryPath.PathToFilesysChild(DateTime.Now.SictwaiseKalenderString(".", 0) + " Exception");

				FilePath.WriteToFileAndCreateDirectoryIfNotExisting(Encoding.UTF8.GetBytes(e.Exception.SictString()));

				var Message = "exception written to file: " + FilePath;

				MessageBox.Show(Message, Message, MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
			catch (Exception PersistException)
			{
				Bib3.FCL.GBS.Extension.MessageBoxException(PersistException);
			}

			Bib3.FCL.GBS.Extension.MessageBoxException(e.Exception);

			e.Handled = true;
		}
	}
}
