using System.Collections.Generic;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IContainer : IUIElement
	{
		IEnumerable<IUIElementText> ButtonText { get; }

		IEnumerable<IUIElementInputText> InputText { get; }

		IEnumerable<IUIElementText> LabelText { get; }

		IEnumerable<ISprite> Sprite { get; }
	}

	public class Container : UIElement, IContainer
	{
		public IEnumerable<IUIElementText> ButtonText { set; get; }

		public IEnumerable<IUIElementInputText> InputText { set; get; }

		public IEnumerable<IUIElementText> LabelText { set; get; }

		public IEnumerable<ISprite> Sprite { set; get; }

		public Container()
		{
		}

		public Container(IUIElement @base)
			:
			base(@base)
		{
			var BaseAsContainer = @base as IContainer;

			ButtonText = BaseAsContainer?.ButtonText;
			LabelText = BaseAsContainer?.LabelText;
			InputText = BaseAsContainer?.InputText;
			Sprite = BaseAsContainer?.Sprite;
		}
	}
}
