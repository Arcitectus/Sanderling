using System.Collections.Generic;
using System.Linq;

namespace Sanderling.Interface.MemoryStruct
{
	public class ChatParticipant : ListEntry
	{
		public UIElementText NameLabel;

		public Sprite StatusIcon;

		public ChatParticipant(ListEntry Base)
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
		public ListViewAndControl ParticipantView;

		public ListViewAndControl MessageView;

		public IEnumerable<ChatParticipant> Participant => ParticipantView?.Entry?.OfType<ChatParticipant>();

		public IEnumerable<ChatMessage> Message => MessageView?.Entry?.OfType<ChatMessage>();

		/// <summary>
		/// Label not contained in Message or Participant.
		/// </summary>
		public UIElementText[] LabelOther;

		public WindowChatChannel(Window Window)
			:
			base(Window)
		{
		}

		public WindowChatChannel()
		{
		}
	}

}
