namespace Sanderling.Interface.MemoryStruct
{
	public class WindowStationLobby : Window
	{
		public IUIElementText[] AboveServicesLabel;

		public IUIElement ButtonUndock;

		/// <summary>
		/// Station services.
		/// The type can be identified by TexturePath.
		/// Some examples:
		/// "res:/ui/Texture/WindowIcons/fitting.png"
		/// "res:/UI/Texture/WindowIcons/Industry.png"
		/// </summary>
		public ISprite[] ServiceButton;

		public LobbyAgentEntry[] AgentEntry;

		/// <summary>
		/// Label which are displayed between Agent Entries ("available to you", "Agents of interest").
		/// </summary>
		public IUIElementText[] AgentEntryHeader;

		public WindowStationLobby(IWindow Base)
			:
			base(Base)
		{
		}

		public WindowStationLobby()
		{
		}

	}

	public class LobbyAgentEntry : UIElement
	{
		public IUIElementText[] LabelText;

		public IUIElement StartConversationButton;

		public LobbyAgentEntry(IUIElement Base)
			:
			base(Base)
		{
		}

		public LobbyAgentEntry()
		{
		}
	}

}
