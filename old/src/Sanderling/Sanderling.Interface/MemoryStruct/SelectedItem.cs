namespace Sanderling.Interface.MemoryStruct
{
	public interface IWindowSelectedItemView : IWindow
	{
		ISprite[] ActionSprite { get; }
	}

	public class WindowSelectedItemView : Window, IWindowSelectedItemView
	{
		public ISprite[] ActionSprite { set; get; }

		public WindowSelectedItemView(IWindow @base)
			:
			base(@base)
		{
		}

		public WindowSelectedItemView()
		{
		}
	}
}
