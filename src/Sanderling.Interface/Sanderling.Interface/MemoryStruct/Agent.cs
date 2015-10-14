namespace Sanderling.Interface.MemoryStruct
{
	public interface IWindowAgentPane : IUIElement
	{
		string Html { get; }
	}

	public interface IWindowAgent : IWindow
	{
	}

	public interface IWindowAgentDialogue
	{
		WindowAgentPane LeftPane { get; }

		/// <summary>
		/// The Mission objectives (locations, items) and rewards are described in here in the "Html" member.
		/// </summary>
		WindowAgentPane RightPane { get; }
	}

	public class WindowAgentDialogue : WindowAgent, IWindowAgentDialogue
	{
		public WindowAgentPane LeftPane { set; get; }

		public WindowAgentPane RightPane { set; get; }

		public WindowAgentDialogue()
		{
		}

		public WindowAgentDialogue(WindowAgent Base)
			:
			base(Base)
		{
		}
	}

	public class WindowAgentBrowser : WindowAgent
	{
		public WindowAgentBrowser(WindowAgent Base)
			:
			base(Base)
		{
		}

		public WindowAgentBrowser()
		{
		}
	}

	public class WindowAgentPane : UIElement, IWindowAgentPane
	{
		public string Html { set; get; }

		public WindowAgentPane()
		{
		}

		public WindowAgentPane(IUIElement Base)
			: base(Base)
		{
		}
	}

	public class WindowAgent : Window, IWindowAgent
	{
		public WindowAgent()
		{
		}

		public WindowAgent(IWindow Base)
			:
			base(Base)
		{
		}
	}

}
