namespace Sanderling.Interface.MemoryStruct
{
	public class Container : UIElement
	{
		public UIElementText[] ButtonText;

		public UIElementInputText[] InputText;

		public UIElementText[] LabelText;

		public Container()
		{
		}

		public Container(UIElement Base)
			:
			base(Base)
		{
			var BaseAsContainer = Base as Container;

			ButtonText = BaseAsContainer?.ButtonText;
			LabelText = BaseAsContainer?.LabelText;
			InputText = BaseAsContainer?.InputText;
		}
	}

	public class Window : Container
	{
		public bool? isModal;

		public string Caption;

		public bool? HeaderButtonsVisible;

		/// <summary>
		/// e.g. pin, minimize, close
		/// </summary>
		public Sprite[] HeaderButton;

		public Window()
		{
		}

		public Window(UIElement Base)
			:
			base(Base)
		{
			var BaseAsWindow = Base as Window;

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
		public Window TabSelectedWindow;

		public WindowStack()
		{
		}

		public WindowStack(Window Base)
			:
			base(Base)
		{
		}
	}

}
