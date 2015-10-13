using Bib3;
using System;
using System.Linq;
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

		public const string ConfigApiVersionDefaultAddress = @"http://sanderling.api.botengine.de:4034/api";

		BotEngine.LicenseClientConfig LicenseClientConfig => ConfigReadFromUI()?.LicenseClient;

		public MainWindow Window => base.MainWindow as MainWindow;

		Bib3.FCL.GBS.ToggleButtonHorizBinär ToggleButtonMotionEnable => Window?.Main?.ToggleButtonMotionEnable;

		BotScript.UI.Wpf.IDE ScriptIDE => Window?.Main?.Bot?.IDE;

		BotScript.ScriptRun ScriptRun => ScriptIDE?.ScriptRun;

		bool WasActivated = false;

		DispatcherTimer Timer;

		string AssemblyDirectoryPath => Bib3.FCL.Glob.ZuProcessSelbsctMainModuleDirectoryPfaadBerecne().ScteleSicerEndung(@"\");

		void TimerConstruct()
		{
			Timer = new DispatcherTimer(TimeSpan.FromSeconds(1.0 / 4), DispatcherPriority.Normal, Timer_Tick, Dispatcher);

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
				HostSanderling = new Script.HostToScript()
				{
					MemoryMeasurementFunc = () =>
					{
						ScriptExecutionCheck?.Invoke();
						return FromScriptRequestMemoryMeasurement();
					},

					MotionExecuteFunc = MotionParam =>
					{
						ScriptExecutionCheck?.Invoke();
						return FromScriptMotionExecute(MotionParam);
					},
				}
			};

		void ActivatedFirstTime()
		{
			ScriptIDE.ScriptRunGlobalsFunc = ToScriptGlobalsConstruct;

			ScriptIDE.ScriptParamBase = new BotScript.ScriptParam()
			{
				AssemblyAddition = Script.Script.AssemblyAddition?.ToArray(),
				NamespaceAddition = Script.Script.NamespaceAddition?.ToArray(),
			};

			ScriptIDE.ScriptWriteToOrReadFromFile.DefaultFilePath = DefaultScriptPath;
			ScriptIDE.Editor.Document.Text = DefaultScript;

			Window?.AddHandler(System.Windows.Controls.Primitives.ToggleButton.CheckedEvent, new RoutedEventHandler(ToggleButtonClick));

			Window?.Main?.ConfigFromModelToView(ConfigDefaultConstruct());

			ConfigFileControl.DefaultFilePath = ConfigFilePath;
			ConfigFileControl.CallbackGetValueToWrite = ConfigReadFromUISerialized;
			ConfigFileControl.CallbackValueRead = ConfigWriteToUIDeSerialized;
			ConfigFileControl.ReadFromFile();

			TimerConstruct();
		}

		void Timer_Tick(object sender, object e)
		{
			Window?.ProcessInput();

			Motor = GetMotor();

			ScriptExchange();

			LicenseClientExchange();

			LicenseClientInspect?.Present(LicenseClient?.Wert);

			InterfaceToEveControl?.Measurement?.Present(MemoryMeasurementLast);
		}

		void ToggleButtonClick(object sender, RoutedEventArgs e)
		{
			var OriginalSource = e?.OriginalSource;

			if (null != OriginalSource)
			{
				if (OriginalSource == ToggleButtonMotionEnable?.ButtonRecz)
				{
					ScriptRunPlay();
				}
			}
		}

		void ScriptRunPlay()
		{
			ScriptIDE.ScriptRunContinueOrStart();

			ScriptExchange();
		}

		void ScriptExchange()
		{
			ToggleButtonMotionEnable.ButtonReczIsChecked = null != ScriptRun && !(ScriptRun?.HasCompleted ?? false);

			ScriptIDE?.Present();
		}

	}
}
