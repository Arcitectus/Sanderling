namespace Sanderling.Interface.MemoryStruct
{
	public class MessageBox : Window
	{
		public string TopCaptionText;

		public string MainEditText;

		public MessageBox(IWindow Base)
			:
			base(Base)
		{
		}

		public MessageBox()
		{
		}
	}

	public class HybridWindow : MessageBox
	{
		public HybridWindow(MessageBox Base)
			:
			base(Base)
		{
		}

		public HybridWindow()
		{
		}
	}

}
