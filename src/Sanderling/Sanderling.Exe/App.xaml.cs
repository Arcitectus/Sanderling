using BotEngine.Motor;
using Sanderling.Motor;
using System;
using System.Windows;
using System.Windows.Threading;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

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

		Type[] ScriptAssemblyAndNamespaceTypeAddition => new[]
		{
			typeof(Script.IHostToScript),
			typeof(MemoryStruct.MemoryMeasurement),
		};

		BotScript.ScriptRun ScriptRun;

		bool WasActivated = false;

		DispatcherTimer Timer;

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


		void ActivatedFirstTime()
		{
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
			var Editor = Window?.Main?.Bot?.IDE?.Editor;

			var Script = Editor?.Text;

			if (null == ScriptRun || (ScriptRun?.Completed ?? false))
			{
				ScriptRun = new BotScript.ScriptRun();

				ScriptRun.Start(
					Script,
					ScriptAssemblyAndNamespaceTypeAddition,
					new Script.ToScriptGlobals()
					{
						HostSanderling = new Script.HostToScript()
						{
							LastMemoryMeasurementFunc = FromScriptRequestMeasurementLast,
							MotionExecuteFunc = FromScriptMotionExecute,
						}
					});
			}

			ScriptRun?.Continue();

			ScriptExchange();
		}

		void ScriptExchange()
		{
			ToggleButtonMotionEnable.ButtonReczIsChecked = null != ScriptRun && !(ScriptRun?.Completed ?? false);

			Window?.Main?.Bot?.IDE?.Run?.Present(ScriptRun);
		}

	}
}
