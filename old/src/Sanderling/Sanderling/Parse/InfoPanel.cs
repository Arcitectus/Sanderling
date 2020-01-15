using Bib3.Geometrik;
using Sanderling.Interface.MemoryStruct;
using System.Text.RegularExpressions;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.Parse
{
	public interface IInfoPanelSystem : MemoryStruct.IInfoPanelSystem
	{
		int? SecurityLevelMilli { get; }
	}

	public partial class InfoPanelSystem : IInfoPanelSystem
	{
		MemoryStruct.IInfoPanelSystem raw;

		public int? SecurityLevelMilli { private set; get; }

		public InfoPanelSystem(MemoryStruct.IInfoPanelSystem raw)
		{
			this.raw = raw;

			SecurityLevelMilli = raw?.HeaderText?.SecurityLevelMilliFromInfoPanelSystemHeaderText();
		}
	}

	public partial class InfoPanelSystem : IInfoPanelSystem
	{
		public int? ChildLastInTreeIndex => raw?.ChildLastInTreeIndex;

		public IContainer ExpandedContent => raw?.ExpandedContent;

		public IUIElement ExpandToggleButton => raw?.ExpandToggleButton;

		public IContainer HeaderContent => raw?.HeaderContent;

		public string HeaderText => raw?.HeaderText;

		public long Id => raw?.Id ?? 0;

		public int? InTreeIndex => raw?.InTreeIndex;

		public bool? IsExpanded => raw?.IsExpanded;

		public IUIElement ListSurroundingsButton => raw?.ListSurroundingsButton;

		public RectInt Region => raw?.Region ?? default(RectInt);

		public IUIElement RegionInteraction => raw?.RegionInteraction;
	}

	static public class InfoPanelExtension
	{
		/// <summary>
		/// <hint='Security status'>0.4</hint>
		/// </summary>
		static readonly string SecurityLevelMilliFromInfoPanelSystemHeaderTextSuffixPattern = Regex.Escape(@"<hint='Security status'>");

		static public int? SecurityLevelMilliFromInfoPanelSystemHeaderText(this string headerText) =>
			(int?)headerText?.NumberParseDecimalMilliBetween(SecurityLevelMilliFromInfoPanelSystemHeaderTextSuffixPattern, @"\s*\<", RegexOptions.IgnoreCase);

		static public IInfoPanelSystem Parse(this MemoryStruct.IInfoPanelSystem raw) =>
			null == raw ? null : new InfoPanelSystem(raw);
	}
}
