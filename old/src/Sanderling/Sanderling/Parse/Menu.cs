using BotEngine.Common;
using System.Linq;
using System.Text.RegularExpressions;
using MemoryStruct = Sanderling.Interface.MemoryStruct;
using Bib3.Geometrik;
using System.Collections.Generic;

namespace Sanderling.Parse
{
	public interface IMenuEntry : MemoryStruct.IMenuEntry
	{
		bool IsExpandable { get; }
	}

	public interface IMenu : MemoryStruct.IMenu
	{
		new IEnumerable<IMenuEntry> Entry { get; }
	}

	public partial class MenuEntry : IMenuEntry
	{
		MemoryStruct.IMenuEntry raw;

		static readonly Regex IsExpandableSpriteTexturePathRegex = Regex.Escape("1_16_14.png").AsRegexCompiledIgnoreCase();

		public bool IsExpandable => raw?.Sprite?.Any(sprite => IsExpandableSpriteTexturePathRegex.MatchSuccess(sprite?.TexturePath ?? "")) ?? false;

		MenuEntry()
		{ }

		public MenuEntry(MemoryStruct.IMenuEntry raw)
		{
			this.raw = raw;
		}
	}

	public partial class MenuEntry
	{
		public bool? HighlightVisible => raw?.HighlightVisible;

		public string Text => raw?.Text;

		public IEnumerable<MemoryStruct.IUIElementText> ButtonText => raw?.ButtonText;

		public IEnumerable<MemoryStruct.IUIElementInputText> InputText => raw?.InputText;

		public IEnumerable<MemoryStruct.IUIElementText> LabelText => raw?.LabelText;

		public IEnumerable<MemoryStruct.ISprite> Sprite => raw?.Sprite;

		public RectInt Region => raw?.Region ?? default(RectInt);

		public int? InTreeIndex => raw?.InTreeIndex;

		public int? ChildLastInTreeIndex => raw?.ChildLastInTreeIndex;

		public MemoryStruct.IUIElement RegionInteraction => raw?.RegionInteraction;

		public long Id => raw?.Id ?? 0;
	}

	public partial class Menu : IMenu
	{
		MemoryStruct.IMenu raw;

		public IEnumerable<IMenuEntry> Entry { get; private set; }

		Menu()
		{ }

		public Menu(MemoryStruct.IMenu raw)
		{
			this.raw = raw;

			Entry = raw?.Entry?.Select(entry => entry?.Parse())?.ToArray();
		}
	}

	public partial class Menu : IMenu
	{
		public int? ChildLastInTreeIndex => raw?.ChildLastInTreeIndex;

		public long Id => raw?.Id ?? 0;

		public int? InTreeIndex => raw?.InTreeIndex;

		public RectInt Region => raw?.Region ?? default(RectInt);

		public MemoryStruct.IUIElement RegionInteraction => raw?.RegionInteraction;

		IEnumerable<MemoryStruct.IMenuEntry> MemoryStruct.IMenu.Entry => Entry;
	}

	static public class MenuExtension
	{
		static public MemoryStruct.IMenuEntry EntryFirstMatchingRegexPattern(
			this MemoryStruct.IMenu menu,
			string regexPattern,
			RegexOptions regexOptions = RegexOptions.None) =>
			null == regexPattern ? null :
			menu?.Entry?.FirstOrDefault(entry => entry?.Text?.RegexMatchSuccess(regexPattern, regexOptions) ?? false);

		static public IMenuEntry Parse(this MemoryStruct.IMenuEntry raw) =>
			null == raw ? null : new MenuEntry(raw);

		static public IMenu Parse(this MemoryStruct.IMenu raw) =>
			null == raw ? null : new Menu(raw);
	}
}
