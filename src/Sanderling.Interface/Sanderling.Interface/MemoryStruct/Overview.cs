using System;

namespace Sanderling.Interface.MemoryStruct
{
	public class WindowOverView : Window, ICloneable
	{
		public Tab[] PresetTab;

		/// <summary>
		/// contains the Overview entries for the in space objects.
		/// </summary>
		public ListViewAndControl ListView;

		/// <summary>
		/// A Label in place of the Viewport which would contain overview entries ("Nothing Found").
		/// </summary>
		public string ViewportOverallLabelString;

		public WindowOverView(Window Base)
			:
			base(Base)
		{
		}

		public WindowOverView()
		{
		}

		public WindowOverView Copy() => this.CopyByPolicyMemoryMeasurement();

		public object Clone()
		{
			return Copy();
		}
	}

	public class OverviewEntry : ListEntry
	{
		/// <summary>
		/// contains EWar icons such as jamming or scrambling.
		/// </summary>
		public Sprite[] RightIcon;

		public OverviewEntry(ListEntry Base)
			:
			base(Base)
		{
		}

		public OverviewEntry()
		{
		}
	}


}
