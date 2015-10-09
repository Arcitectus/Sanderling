namespace Sanderling.Interface.MemoryStruct
{

	public class WindowAgentDialogue : WindowAgent
	{
		public WindowAgentPane LeftPane;

		/// <summary>
		/// The Mission objectives (locations, items) and rewards are described in here in the "Html" member.
		/// </summary>
		public WindowAgentPane RightPane;

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

	public class WindowAgentPane : UIElement
	{
		public string Html;

		public WindowAgentPane()
		{
		}

		public WindowAgentPane(UIElement Base)
			: base(Base)
		{
		}
	}

	public class WindowAgent : Window
	{
		public WindowAgent()
		{
		}

		public WindowAgent(Window Base)
			:
			base(Base)
		{
		}
	}

}
