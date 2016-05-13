namespace Sanderling.Interface.MemoryStruct
{
	public interface INeocom : IContainer
	{
		IUIElement EveMenuButton { get; }

		IUIElement CharButton { get; }

		ISprite[] Button { get; }

		IUIElementText Clock { get; }
	}

	public class Neocom : Container, INeocom
	{
		public IUIElement EveMenuButton { set; get; }

		public IUIElement CharButton { set; get; }

		/// <summary>
		/// The type can be identified by TexturePath.
		/// examples of texture paths found in here:
		/// "res:/ui/Texture/WindowIcons/peopleandplaces.png"
		/// "res:/UI/Texture/WindowIcons/items.png"
		/// "res:/ui/Texture/WindowIcons/market.png"
		/// </summary>
		public ISprite[] Button { set; get; }

		public IUIElementText Clock { set; get; }

		public Neocom()
		{
		}

		public Neocom(IUIElement @base)
			: base(@base)
		{
		}
	}
}
