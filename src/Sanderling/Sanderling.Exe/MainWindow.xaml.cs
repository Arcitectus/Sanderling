using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Sanderling.Exe
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		public string TitleComputed =>
			"Sanderling v" + (TryFindResource("AppVersionId") ?? "");

		public IEnumerable<IEnumerable<System.Windows.Input.Key>> SetKeyBotMotionDisable()
		{
			yield return new[] { System.Windows.Input.Key.LeftCtrl, System.Windows.Input.Key.LeftAlt };
			yield return new[] { System.Windows.Input.Key.RightCtrl, System.Windows.Input.Key.RightAlt };
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			ProcessInput();
		}

		public void ProcessInput()
		{
			if (SetKeyBotMotionDisable()?.Any(setKey => setKey?.All(key => System.Windows.Input.Keyboard.IsKeyDown(key)) ?? false) ?? false)
			{
				Main?.BotMotionDisable();
			}
		}
	}
}
