using System;
using System.Collections.Generic;

namespace Sanderling.Interface.MemoryStruct
{
	public interface ISquadronsUI
	{
		IEnumerable<ISquadronUI> SetSquadron { get; }

		IUIElement LaunchAllButton { get; }

		IUIElement OpenBayButton { get; }

		IUIElement RecallAllButton { get; }
	}

	public interface ISquadronUI : IUIElement
	{
		ISquadronContainer Squadron { get; }

		IEnumerable<ISquadronAbilityIcon> SetAbilityIcon { get; }
	}

	public interface ISquadronContainer : IContainer
	{
		int? SquadronNumber { get; }

		ISquadronHealth Health { get; }

		bool? IsSelected { get; }

		string Hint { get; }
	}

	public interface ISquadronHealth
	{
		int? SquadronSizeMax { get; }

		int? SquadronSizeCurrent { get; }
	}

	public interface ISquadronAbilityIcon : IUIElement
	{
		int? Quantity { get; }

		bool? RampActive { get; }
	}

	public class SquadronsUI : Container, ISquadronsUI
	{
		public IUIElement LaunchAllButton { set; get; }

		public IUIElement OpenBayButton { set; get; }

		public IUIElement RecallAllButton { set; get; }

		public IEnumerable<ISquadronUI> SetSquadron { set; get; }

		public SquadronsUI()
		{
		}

		public SquadronsUI(IUIElement @base)
			:
			base(@base)
		{
		}
	}

	public class SquadronUI : UIElement, ISquadronUI
	{
		public ISquadronContainer Squadron { set; get; }

		public IEnumerable<ISquadronAbilityIcon> SetAbilityIcon { set; get; }

		public SquadronUI()
		{
		}

		public SquadronUI(IUIElement @base)
			:
			base(@base)
		{
		}
	}

	public class SquadronContainer : Container, ISquadronContainer
	{
		public int? SquadronNumber { set; get; }

		public ISquadronHealth Health { set; get; }

		public bool? IsSelected { set; get; }

		public string Hint { set; get; }

		public SquadronContainer()
		{
		}

		public SquadronContainer(IUIElement @base)
			:
			base(@base)
		{
		}
	}

	public class SquadronHealth : ISquadronHealth
	{
		public int? SquadronSizeMax { set; get; }

		public int? SquadronSizeCurrent { set; get; }
	}

	public class SquadronAbilityIcon : UIElement, ISquadronAbilityIcon
	{
		public int? Quantity { set; get; }

		public bool? RampActive { set; get; }

		public SquadronAbilityIcon()
		{
		}

		public SquadronAbilityIcon(IUIElement @base)
			:
			base(@base)
		{
		}
	}
}
