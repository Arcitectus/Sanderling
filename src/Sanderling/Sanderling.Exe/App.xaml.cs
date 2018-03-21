﻿using Bib3;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Sanderling.Exe
{
	public partial class App : Application
	{
		static public Int64 GetTimeStopwatch() => Bib3.Glob.StopwatchZaitMiliSictInt();

		public MainWindow Window => base.MainWindow as MainWindow;

		Bib3.FCL.GBS.ToggleButtonHorizBinär BotOperationPauseContinueToggleButton => Window?.Main?.ToggleButtonMotionEnable;

		UI.BotAPIExplorer BotAPIExplorer => Window?.Main?.DevToolsAPIExplorer;

		BotSharp.ScriptRun.ScriptRun ScriptRun => MainControl?.BotsNavigation?.ScriptRun;

		bool WasActivated = false;

		DispatcherTimer Timer;

		static string AssemblyDirectoryPath => Bib3.FCL.Glob.ZuProcessSelbsctMainModuleDirectoryPfaadBerecne().EnsureEndsWith(@"\");

		Sanderling.Script.Impl.HostToScript UIAPI;

		public App()
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

			LogCreate();

			ConfigSetup();

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
				GetKillEveProcessAction = KillEveProcessAction
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
		}

		void Timer_Tick(object sender, object e)
		{
			ProcessInput();

			Motor = GetMotor();

			InterfaceExchange();

			UpdateBotOperationPauseContinueToggleButton();

			UIPresent();
		}

		void ContinueOrStartBotOperation()
		{
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
