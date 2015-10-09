namespace Sanderling.Interface.MemoryStruct
{
	public class ModuleButtonTooltipRow : UIElement
	{
		public UIElementText[] ListLabelString;

		public string ShortcutText;

		public ObjectIdInMemory[] IconTextureId;

		public ModuleButtonTooltipRow()
		{
		}

		public ModuleButtonTooltipRow(UIElement Base)
			:
			base(Base)
		{
		}

	}


	public class ModuleButtonTooltip : UIElement
	{
		public UIElementText[] ListLabelString;

		public ModuleButtonTooltipRow[] ListRow;

		public ModuleButtonTooltip()
		{
		}

		public ModuleButtonTooltip(UIElement Base)
			:
			base(Base)
		{
		}

	}

}
