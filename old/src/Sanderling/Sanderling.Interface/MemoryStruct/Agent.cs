namespace Sanderling.Interface.MemoryStruct
{
	public interface IWindowAgentPane : IUIElement
	{
		string Html { get; }
	}

	public interface IWindowAgent : IWindow
	{
	}

	public interface IWindowAgentDialogue : IWindowAgent
	{
		IWindowAgentPane LeftPane { get; }

		/// <summary>
		/// The Mission objectives (locations, items) and rewards are described in here in the "Html" member.
		/// </summary>
		IWindowAgentPane RightPane { get; }
	}

	public class WindowAgentDialogue : WindowAgent, IWindowAgentDialogue
	{
		public IWindowAgentPane LeftPane { set; get; }

		public IWindowAgentPane RightPane { set; get; }

		public WindowAgentDialogue()
		{
		}

		public WindowAgentDialogue(WindowAgent @base)
			:
			base(@base)
		{
		}
	}

	public class WindowAgentBrowser : WindowAgent, IWindowAgent
	{
		public WindowAgentBrowser(WindowAgent @base)
			:
			base(@base)
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

		public WindowAgentPane(IUIElement @base)
			: base(@base)
		{
		}
	}

	public class WindowAgent : Window, IWindowAgent
	{
		public WindowAgent()
		{
		}

		public WindowAgent(IWindow @base)
			:
			base(@base)
		{
		}
	}
}
