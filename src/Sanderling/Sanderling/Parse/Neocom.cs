using System;
using System.Linq;
using Bib3.Geometrik;
using BotEngine.Common;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.Parse
{
	public interface INeocom : MemoryStruct.INeocom
	{
		MemoryStruct.IUIElement PeopleAndPlacesButton { get; }

		MemoryStruct.IUIElement ChatButton { get; }

		MemoryStruct.IUIElement MailButton { get; }

		MemoryStruct.IUIElement FittingButton { get; }

		MemoryStruct.IUIElement InventoryButton { get; }

		MemoryStruct.IUIElement MarketButton { get; }

	}

	public class Neocom : INeocom
	{
		public MemoryStruct.INeocom Raw { private set; get; }

		public MemoryStruct.IUIElement PeopleAndPlacesButton { private set; get; }

		public MemoryStruct.IUIElement ChatButton { private set; get; }

		public MemoryStruct.IUIElement MailButton { private set; get; }

		public MemoryStruct.IUIElement FittingButton { private set; get; }

		public MemoryStruct.IUIElement InventoryButton { private set; get; }

		public MemoryStruct.IUIElement MarketButton { private set; get; }

		public MemoryStruct.IUIElement EveMenuButton => Raw?.EveMenuButton;

		public MemoryStruct.IUIElement CharButton => Raw?.CharButton;

		public MemoryStruct.ISprite[] Button => Raw?.Button;

		public MemoryStruct.IUIElementText Clock => Raw?.Clock;

		public MemoryStruct.IUIElementText[] ButtonText => Raw?.ButtonText;

		public MemoryStruct.IUIElementInputText[] InputText => Raw?.InputText;

		public MemoryStruct.IUIElementText[] LabelText => Raw?.LabelText;

		public MemoryStruct.ISprite[] Sprite => Raw?.Sprite;

		public RectInt Region => Raw?.Region ?? default(RectInt);

		public Int32? InTreeIndex => Raw?.InTreeIndex;

		public Int32? ChildLastInTreeIndex => Raw?.ChildLastInTreeIndex;

		public MemoryStruct.IUIElement RegionInteraction => Raw?.RegionInteraction;

		public Int64 Id => Raw?.Id ?? 0;

		Neocom()
		{ }

		public Neocom(MemoryStruct.INeocom raw)
		{
			this.Raw = raw;

			if (null == raw)
			{
				return;
			}

			var ButtonWithTexturePathMatch = new Func<string, MemoryStruct.IUIElement>(texturePathRegexPattern =>
				raw?.Button?.FirstOrDefault(candidate => candidate?.TexturePath?.RegexMatchSuccess(texturePathRegexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false));

			PeopleAndPlacesButton = ButtonWithTexturePathMatch("peopleandplaces");

			ChatButton = ButtonWithTexturePathMatch("chat");

			MailButton = ButtonWithTexturePathMatch("mail");

			FittingButton = ButtonWithTexturePathMatch("fitting");

			InventoryButton = ButtonWithTexturePathMatch("items");

			MarketButton = ButtonWithTexturePathMatch("market");
		}
	}
}
