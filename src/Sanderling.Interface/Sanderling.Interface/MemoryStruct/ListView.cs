using BotEngine.Interface;
using System.Collections.Generic;

namespace Sanderling.Interface.MemoryStruct
{
	public class ColumnHeader : UIElementText
	{
		public int? ColumnIndex;

		/// <summary>
		/// positive means ascending, negative descending.
		/// </summary>
		public int? SortDirection;

		public ColumnHeader()
			:
			this((ColumnHeader)null)
		{
		}

		public ColumnHeader(UIElementText Base)
			:
			base(Base)
		{
			ColumnIndex = (Base as ColumnHeader)?.ColumnIndex;
			SortDirection = (Base as ColumnHeader)?.SortDirection;
		}
	}

	/// <summary>
	/// can represent Item or Group.
	/// </summary>
	public class ListEntry : Container
	{
		public int? ContentBoundLeft;

		/// <summary>
		/// for each column, a reference of its header and the content for the cell.
		/// </summary>
		public KeyValuePair<ColumnHeader, string>[] ListColumnCellLabel;

		public UIElement GroupExpander;

		public bool? IsGroup;

		public bool? IsExpanded;

		public bool? IsSelected;

		public ColorORGB[] ListBackgroundColor;

		public Sprite[] SetSprite;

		public ListEntry()
			:
			this((ListEntry)null)
		{
		}

		public ListEntry(UIElement Base)
			:
			base(Base)
		{
		}

		public ListEntry(ListEntry Base)
			:
			this((UIElement)Base)
		{
			ContentBoundLeft = Base?.ContentBoundLeft;

			ListColumnCellLabel = Base?.ListColumnCellLabel;

			GroupExpander = Base?.GroupExpander;
			IsGroup = Base?.IsGroup;
			IsExpanded = Base?.IsExpanded;
			IsSelected = Base?.IsSelected;
			ListBackgroundColor = Base?.ListBackgroundColor;
			SetSprite = Base?.SetSprite;
		}
	}


	public class ListViewAndControl : UIElement
	{
		public ColumnHeader[] ColumnHeader;

		public ListEntry[] Entry;

		public Scroll Scroll;

		public ListViewAndControl()
		{
		}

		public ListViewAndControl(UIElement Base)
			:
			base(Base)
		{
		}
	}

}
