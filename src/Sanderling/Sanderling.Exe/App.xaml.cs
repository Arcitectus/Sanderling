using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Sanderling.Exe
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		new public MainWindow MainWindow => base.MainWindow as MainWindow;

		Bib3.FCL.GBS.ToggleButtonHorizBinär ToggleButtonMotionEnable => MainWindow?.Main?.ToggleButtonMotionEnable;

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

			TimerConstruct();
		}

		void Timer_Tick(object sender, object e)
		{
			MainWindow?.ProcessInput();

			ScriptExchange();
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

				ScriptRun.Start(Script);
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
