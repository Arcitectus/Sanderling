using BotEngine.Interface;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Sanderling.Interface.MemoryStruct
{
	/// <summary>
	/// can represent Item or Group.
	/// </summary>
	public interface IListEntry : IContainer, ISelectable
	{
		int? ContentBoundLeft { get; }

		/// <summary>
		/// for each column, a reference of its header and the content for the cell.
		/// </summary>
		KeyValuePair<IColumnHeader, string>[] ListColumnCellLabel { get; }

		IUIElement GroupExpander { get; }

		bool? IsGroup { get; }

		bool? IsExpanded { get; }

		ColorORGB[] ListBackgroundColor { get; }

		ISprite[] SetSprite { get; }
	}

	public interface IColumnHeader : IUIElementText
	{
		int? ColumnIndex { get; }

		/// <summary>
		/// positive means ascending, negative descending.
		/// </summary>
		int? SortDirection { get; }
	}

	public interface IListViewAndControl
	{
		IColumnHeader[] ColumnHeader { get; }

		IListEntry[] Entry { get; }

		IScroll Scroll { get; }
	}

	/// <summary>
	/// A list view + the elements to control the view.
	/// </summary>
	public interface IListViewAndControl<out EntryT> : IUIElement, IListViewAndControl
		where EntryT : IListEntry
	{
		new EntryT[] Entry { get; }
	}

	public class ColumnHeader : UIElementText, IColumnHeader
	{
		public int? ColumnIndex { set; get; }

		public int? SortDirection { set; get; }

		public ColumnHeader()
			:
			this((IColumnHeader)null)
		{
		}

		public ColumnHeader(IUIElementText Base)
			:
			base(Base)
		{
			ColumnIndex = (Base as IColumnHeader)?.ColumnIndex;
			SortDirection = (Base as IColumnHeader)?.SortDirection;
		}
	}

	public class ListEntry : Container, IListEntry
	{
		public int? ContentBoundLeft { set; get; }

		public KeyValuePair<IColumnHeader, string>[] ListColumnCellLabel { set; get; }

		public IUIElement GroupExpander { set; get; }

		public bool? IsGroup { set; get; }

		public bool? IsExpanded { set; get; }

		public bool? IsSelected { set; get; }

		public ColorORGB[] ListBackgroundColor { set; get; }

		public ISprite[] SetSprite { set; get; }

		public ListEntry()
			:
			this(null)
		{
		}

		public ListEntry(IUIElement Base)
			:
			base(Base)
		{
			var BaseAsListEntry = Base as IListEntry;

			ContentBoundLeft = BaseAsListEntry?.ContentBoundLeft;

			ListColumnCellLabel = BaseAsListEntry?.ListColumnCellLabel;

			GroupExpander = BaseAsListEntry?.GroupExpander;
			IsGroup = BaseAsListEntry?.IsGroup;
			IsExpanded = BaseAsListEntry?.IsExpanded;
			IsSelected = BaseAsListEntry?.IsSelected;
			ListBackgroundColor = BaseAsListEntry?.ListBackgroundColor;
			SetSprite = BaseAsListEntry?.SetSprite;
		}
	}

	public class ListViewAndControl<EntryT> : UIElement, IListViewAndControl<EntryT>
		where EntryT : class, IListEntry
	{
		public IColumnHeader[] ColumnHeader { set; get; }

		public EntryT[] Entry { set; get; }

		public IScroll Scroll { set; get; }

		IListEntry[] IListViewAndControl.Entry => Entry;

		public ListViewAndControl()
		{
		}

		public ListViewAndControl(IUIElement Base)
			:
			base(Base)
		{
		}
	}

}
