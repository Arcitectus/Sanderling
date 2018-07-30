using System;
using System.Collections.Generic;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IOverviewEntry : IListEntry
	{
		/// <summary>
		/// contains EWar icons such as jamming or scrambling.
		/// </summary>
		ISprite[] RightIcon { get; }

		IEnumerable<string> MainIconSetIndicatorName { get; }
	}

	public interface IWindowOverview : IWindow
	{
		Tab[] PresetTab { get; }

		/// <summary>
		/// contains the Overview entries for the in space objects.
		/// </summary>
		IListViewAndControl<IOverviewEntry> ListView { get; }

		/// <summary>
		/// A Label in place of the Viewport which would contain overview entries ("Nothing Found").
		/// </summary>
		string ViewportOverallLabelString { get; }
	}

	public class WindowOverView : Window, IWindowOverview, ICloneable
	{
		public Tab[] PresetTab { set; get; }

		/// <summary>
		/// contains the Overview entries for the in space objects.
		/// </summary>
		public IListViewAndControl<IOverviewEntry> ListView { set; get; }

		/// <summary>
		/// A Label in place of the Viewport which would contain overview entries ("Nothing Found").
		/// </summary>
		public string ViewportOverallLabelString { set; get; }

		public WindowOverView(IWindow @base)
			:
			base(@base)
		{
		}

		public WindowOverView()
		{
		}

		public WindowOverView Copy() => this.CopyByPolicyMemoryMeasurement();

		public object Clone() => Copy();
	}

	public class OverviewEntry : ListEntry, IOverviewEntry
	{
		public ISprite[] RightIcon { set; get; }

		public IEnumerable<string> MainIconSetIndicatorName { set; get; }

		public OverviewEntry(IListEntry @base)
			:
			base(@base)
		{
		}

		public OverviewEntry()
		{
		}
	}
}
