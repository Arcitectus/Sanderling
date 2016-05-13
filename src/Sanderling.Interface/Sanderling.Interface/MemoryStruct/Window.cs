namespace Sanderling.Interface.MemoryStruct
{
	public interface IWindow : IContainer
	{
		bool? isModal { get; }

		string Caption { get; }

		bool? HeaderButtonsVisible { get; }

		/// <summary>
		/// e.g. pin, minimize, close
		/// </summary>
		ISprite[] HeaderButton { get; }

	}

	public class Window : Container, IWindow
	{
		public bool? isModal { set; get; }

		public string Caption { set; get; }

		public bool? HeaderButtonsVisible { set; get; }

		/// <summary>
		/// e.g. pin, minimize, close
		/// </summary>
		public ISprite[] HeaderButton { set; get; }

		public Window()
		{
		}

		public Window(IUIElement @base)
			:
			base(@base)
		{
			var BaseAsWindow = @base as IWindow;

			isModal = BaseAsWindow?.isModal;
			Caption = BaseAsWindow?.Caption;
			HeaderButtonsVisible = BaseAsWindow?.HeaderButtonsVisible;
			HeaderButton = BaseAsWindow?.HeaderButton;
		}
	}

	/// <summary>
	/// In the eve online UI, windows can be stacked.
	/// </summary>
	public class WindowStack : Window
	{
		/// <summary>
		/// Contains one element for each window in this stack.
		/// </summary>
		public Tab[] Tab;

		/// <summary>
		/// Window whose tab is currently selected (and therefore the only window in the stack which is currently visible).
		/// </summary>
		public IWindow TabSelectedWindow;

		public WindowStack()
		{
		}

		public WindowStack(IWindow @base)
			:
			base(@base)
		{
		}
	}
}
