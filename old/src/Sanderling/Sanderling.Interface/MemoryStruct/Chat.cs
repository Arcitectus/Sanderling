using System.Collections.Generic;
using System.Linq;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IChatParticipantEntry : IListEntry
	{
		IUIElementText NameLabel { get; }

		ISprite StatusIcon { get; }

		IEnumerable<ISprite> FlagIcon { get; }
	}

	public class ChatParticipantEntry : ListEntry, IChatParticipantEntry
	{
		public IUIElementText NameLabel
		{
			set;
			get;
		}

		public ISprite StatusIcon
		{
			set;
			get;
		}

		public IEnumerable<ISprite> FlagIcon
		{
			set;
			get;
		}

		public ChatParticipantEntry(IListEntry @base)
				:
				base(@base)
		{
		}

		public ChatParticipantEntry()
		{
		}
	}

	public class ChatMessage : ListEntry
	{
		public ChatMessage(ChatMessage @base)
			:
			base(@base)
		{
		}

		public ChatMessage()
		{
		}
	}

	public class WindowChatChannel : Window
	{
		public IListViewAndControl<IChatParticipantEntry> ParticipantView;

		public IListViewAndControl MessageView;

		public IEnumerable<IChatParticipantEntry> Participant => ParticipantView?.Entry?.OfType<IChatParticipantEntry>();

		public IEnumerable<ChatMessage> Message => MessageView?.Entry?.OfType<ChatMessage>();

		public IUIElementInputText MessageInput { set; get; }

		public WindowChatChannel(IWindow window)
			:
			base(window)
		{
		}

		public WindowChatChannel()
		{
		}
	}
}
