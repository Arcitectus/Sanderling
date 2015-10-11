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
		public const string ConfigApiVersionDefaultAddress = @"http://sanderling.api.botengine.de:4034/api";

		BotEngine.LicenseClientConfig LicenseClientConfig = new BotEngine.LicenseClientConfig() { ApiVersionAddress = ConfigApiVersionDefaultAddress };

        new public MainWindow MainWindow => base.MainWindow as MainWindow;

		Bib3.FCL.GBS.ToggleButtonHorizBinär ToggleButtonMotionEnable => MainWindow?.Main?.ToggleButtonMotionEnable;

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
			MainWindow?.AddHandler(System.Windows.Controls.Primitives.ToggleButton.CheckedEvent, new RoutedEventHandler(ToggleButtonClick));

			LicenseClientConfigControl.DataContext =
				new BotEngine.UI.AutoDependencyPropertyComp<BotEngine.LicenseClientConfig>(LicenseClientConfig);

			TimerConstruct();
		}

		void Timer_Tick(object sender, object e)
		{
			MainWindow?.ProcessInput();

			ScriptExchange();

			LicenseClientExchange();

			LicenseClientInspect?.Present(LicenseClient?.Wert);
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
			var Editor = MainWindow?.Main?.Bot?.IDE?.Editor;

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
							LastMemoryMeasurementFunc = new Func<MemoryStruct.MemoryMeasurement>(() => this.MemoryMeasurementLast?.Wert),
						}
					});
			}

			ScriptRun?.Continue();

			ScriptExchange();
		}

		void ScriptExchange()
		{
			ToggleButtonMotionEnable.ButtonReczIsChecked = null != ScriptRun && !(ScriptRun?.Completed ?? false);

			MainWindow?.Main?.Bot?.IDE?.Run?.Present(ScriptRun);
		}

	}
}
