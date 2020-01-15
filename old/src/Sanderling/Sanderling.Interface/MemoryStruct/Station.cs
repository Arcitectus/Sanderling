namespace Sanderling.Interface.MemoryStruct
{
	public interface IWindowStation : IWindow
	{
		IUIElementText[] AboveServicesLabel { get; }

		IUIElement UndockButton { get; }

		/// <summary>
		/// Station services.
		/// The type can be identified by TexturePath.
		/// Some examples:
		/// "res:/ui/Texture/WindowIcons/fitting.png"
		/// "res:/UI/Texture/WindowIcons/Industry.png"
		/// </summary>
		ISprite[] ServiceButton { get; }

		LobbyAgentEntry[] AgentEntry { get; }

		/// <summary>
		/// Label which are displayed between Agent Entries ("available to you", "Agents of interest").
		/// </summary>
		IUIElementText[] AgentEntryHeader { get; }
	}

	public class WindowStation : Window, IWindowStation
	{
		public IUIElementText[] AboveServicesLabel { set; get; }

		public IUIElement UndockButton { set; get; }

		public ISprite[] ServiceButton { set; get; }

		public LobbyAgentEntry[] AgentEntry { set; get; }

		public IUIElementText[] AgentEntryHeader { set; get; }

		public WindowStation(IWindow @base)
			:
			base(@base)
		{
		}

		public WindowStation()
		{
		}
	}

	public class LobbyAgentEntry : UIElement
	{
		public IUIElementText[] LabelText;

		public IUIElement StartConversationButton;

		public LobbyAgentEntry(IUIElement @base)
			:
			base(@base)
		{
		}

		public LobbyAgentEntry()
		{
		}
	}
}
