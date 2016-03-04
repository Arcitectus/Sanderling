namespace Sanderling.Interface.MemoryStruct
{
	public interface IContainer : IUIElement
	{
		IUIElementText[] ButtonText { get; }

		IUIElementInputText[] InputText { get; }

		IUIElementText[] LabelText { get; }

		ISprite[] Sprite { get; }
	}

	public class Container : UIElement, IContainer
	{
		public IUIElementText[] ButtonText { set; get; }

		public IUIElementInputText[] InputText { set; get; }

		public IUIElementText[] LabelText { set; get; }

		public ISprite[] Sprite { set; get; }

		public Container()
		{
		}

		public Container(IUIElement Base)
			:
			base(Base)
		{
			var BaseAsContainer = Base as IContainer;

			ButtonText = BaseAsContainer?.ButtonText;
			LabelText = BaseAsContainer?.LabelText;
			InputText = BaseAsContainer?.InputText;
			Sprite = BaseAsContainer?.Sprite;
		}
	}

}
