using System.Collections.Generic;
using System.Linq;

namespace Sanderling.Interface.MemoryStruct
{
	public class ChatParticipant : ListEntry
	{
		public IUIElementText NameLabel;

		public ISprite StatusIcon;

		public ChatParticipant(IListEntry Base)
			:
			base(Base)
		{
		}

		public ChatParticipant()
		{
		}
	}

	public class ChatMessage : ListEntry
	{
		public ChatMessage(ChatMessage Base)
			:
			base(Base)
		{
		}

		public ChatMessage()
		{
		}

	}

	public class WindowChatChannel : Window
	{
		public IListViewAndControl ParticipantView;

		public IListViewAndControl MessageView;

		public IEnumerable<ChatParticipant> Participant => ParticipantView?.Entry?.OfType<ChatParticipant>();

		public IEnumerable<ChatMessage> Message => MessageView?.Entry?.OfType<ChatMessage>();

		public WindowChatChannel(IWindow Window)
			:
			base(Window)
		{
		}

		public WindowChatChannel()
		{
		}
	}

}
