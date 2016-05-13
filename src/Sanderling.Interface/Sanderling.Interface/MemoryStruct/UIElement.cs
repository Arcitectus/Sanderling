using Bib3.Geometrik;
using BotEngine;
using System;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IObjectIdInMemory : IObjectIdInt64
	{
	}

	public interface IUIElement : IObjectIdInMemory
	{
		RectInt Region { get; }

		/// <summary>
		/// Element occludes other Elements with lower Value.
		/// </summary>
		int? InTreeIndex { get; }

		int? ChildLastInTreeIndex { get; }

		/// <summary>
		/// Region used to select or open contextmenu.
		/// </summary>
		IUIElement RegionInteraction { get; }
	}

	public interface IUIElementText : IUIElement
	{
		string Text { get; }
	}

	public interface IUIElementInputText : IUIElementText
	{
	}

	public interface ISelectable
	{
		bool? IsSelected { get; }
	}

	public interface IExpandable
	{
		IUIElement ExpandToggleButton { get; }

		bool? IsExpanded { get; }
	}

	public class ObjectIdInMemory : ObjectIdInt64, IObjectIdInMemory
	{
		ObjectIdInMemory()
		{
		}

		public ObjectIdInMemory(IObjectIdInt64 @base)
			: base(@base)
		{
		}

		public ObjectIdInMemory(Int64 id)
			: base(id)
		{
		}
	}

	public class UIElement : ObjectIdInMemory, IUIElement
	{
		public RectInt Region { set; get; }

		public int? InTreeIndex { set; get; }

		public int? ChildLastInTreeIndex { set; get; }

		virtual public IUIElement RegionInteraction => this;

		public UIElement()
			:
			this((IUIElement)null)
		{
		}

		public UIElement(IObjectIdInMemory @base)
			:
			base(@base)
		{
		}

		public UIElement(IUIElement @base)
			:
			this((IObjectIdInMemory)@base)
		{
			Region = @base?.Region ?? RectInt.Empty;

			InTreeIndex = @base?.InTreeIndex;
			ChildLastInTreeIndex = @base?.ChildLastInTreeIndex;
		}

		public override string ToString() =>
			this.SensorObjectToString();
	}

	public class UIElementText : UIElement, IUIElementText
	{
		public string Text { set; get; }

		public UIElementText(
			IUIElement @base,
			string text = null)
			:
			base(@base)
		{
			this.Text = text;
		}

		public UIElementText(IUIElementText @base)
			:
			this(@base, @base?.Text)
		{
		}

		public UIElementText()
		{
		}
	}

	public class UIElementInputText : UIElementText, IUIElementInputText
	{
		public UIElementInputText(IUIElement @base, string label = null)
			:
			base(@base, label)
		{
		}

		public UIElementInputText(IUIElementText @base)
			:
			this(@base, @base?.Text)
		{
		}

		public UIElementInputText()
		{
		}
	}
}
