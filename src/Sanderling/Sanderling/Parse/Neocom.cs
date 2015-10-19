using MemoryStruct = Sanderling.Interface.MemoryStruct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BotEngine.Common;

namespace Sanderling.Parse
{
	public interface INeocom
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
		public MemoryStruct.Neocom Raw { private set; get; }

		public MemoryStruct.IUIElement PeopleAndPlacesButton { private set; get; }

		public MemoryStruct.IUIElement ChatButton { private set; get; }

		public MemoryStruct.IUIElement MailButton { private set; get; }

		public MemoryStruct.IUIElement FittingButton { private set; get; }

		public MemoryStruct.IUIElement InventoryButton { private set; get; }

		public MemoryStruct.IUIElement MarketButton { private set; get; }

		Neocom()
		{ }

		public Neocom(MemoryStruct.Neocom Raw)
		{
			this.Raw = Raw;

			var ButtonWithTexturePathMatch = new Func<string, MemoryStruct.IUIElement>(TexturePathRegexPattern =>
				Raw?.Button?.FirstOrDefault(candidate => candidate?.TexturePath?.RegexMatchSuccess(TexturePathRegexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false));

			PeopleAndPlacesButton = ButtonWithTexturePathMatch("peopleandplaces");

			ChatButton = ButtonWithTexturePathMatch("chat");

			MailButton = ButtonWithTexturePathMatch("mail");

			FittingButton = ButtonWithTexturePathMatch("fitting");

			InventoryButton = ButtonWithTexturePathMatch("items");

			MarketButton = ButtonWithTexturePathMatch("market");
		}
	}
}
