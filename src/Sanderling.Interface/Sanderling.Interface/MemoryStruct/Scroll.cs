namespace Sanderling.Interface.MemoryStruct
{
	public interface IScroll : IUIElement
	{
		IColumnHeader[] ColumnHeader { get; }

		IUIElement Clipper { get; }

		IUIElement ScrollHandleBound { get; }

		IUIElement ScrollHandle { get; }
	}

	public class Scroll : UIElement, IScroll
	{
		public IColumnHeader[] ColumnHeader { set; get; }

		public IUIElement Clipper { set; get; }

		public IUIElement ScrollHandleBound { set; get; }

		public IUIElement ScrollHandle { set; get; }

		public Scroll()
			:
			this((IScroll)null)
		{
		}

		public Scroll(IUIElement @base)
			:
			base(@base)
		{
		}

		public Scroll(IScroll @base)
			:
			this((IUIElement)@base)
		{
			ColumnHeader = @base?.ColumnHeader;
			Clipper = @base?.Clipper;
			ScrollHandleBound = @base?.ScrollHandleBound;
			ScrollHandle = @base?.ScrollHandle;
		}
	}
}
