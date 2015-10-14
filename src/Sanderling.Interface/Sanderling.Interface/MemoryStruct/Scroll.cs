using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		public Scroll(IUIElement Base)
			:
			base(Base)
		{
		}

		public Scroll(IScroll Base)
			:
			this((IUIElement)Base)
		{
			ColumnHeader = Base?.ColumnHeader;
			Clipper = Base?.Clipper;
			ScrollHandleBound = Base?.ScrollHandleBound;
			ScrollHandle = Base?.ScrollHandle;
		}
	}

}
