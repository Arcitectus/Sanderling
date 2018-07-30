using BotEngine.Interface;

namespace Sanderling.Interface.MemoryStruct
{
	public interface ISprite : IUIElement
	{
		ColorORGB Color { get; }

		string Name { get; }

		IObjectIdInMemory Texture0Id { get; }

		string TexturePath { get; }

		string HintText { get; }
	}

	public class Sprite : UIElement, ISprite
	{
		public ColorORGB Color { set; get; }

		public string Name { set; get; }

		public IObjectIdInMemory Texture0Id { set; get; }

		public string TexturePath { set; get; }

		public string HintText { set; get; }

		public Sprite(IUIElement @base)
			:
			base(@base)
		{
		}

		public Sprite()
		{
		}
	}
}
