using Bib3;
using Bib3.FCL.GBS.Inspektor;
using BotEngine;
using System;
using System.Linq;
using System.Windows.Controls;

namespace Sanderling.UI
{
	/// <summary>
	/// Interaction logic for BotAPIExplorer.xaml
	/// </summary>
	public partial class BotAPIExplorer : UserControl
	{
		public BotAPIExplorer()
		{
			InitializeComponent();

			TreeView.TreeViewView = new ApiTreeViewNodeView();
		}

		public void Present(Script.IHostToScript Api)
		{
			TimeAndOrigin.Present(Api?.MemoryMeasurement);

			ApiRoot = Api;
		}

		object ApiRoot
		{
			set
			{
				TreeView.TreeView.Präsentiire(value);
			}

			get
			{
				return TreeView?.TreeView?.Wurzel;
			}
		}

		private void SearchObjectButtonSelectObject_Click(System.Object sender, System.Windows.RoutedEventArgs e)
		{
			Bib3.FCL.GBS.Extension.CatchNaacMessageBoxException(() =>
			{
				var Id = SearchObjectFilterId.Text?.TryParseInt64();

				var NodeValuePredicate = new Func<object, bool>(c => (c as IObjectIdInt64)?.Id == Id);

				var SetObjectPathMatch =
					TreeView?.TreeViewView?.EnumeratePathToNodeSatisfyingPredicateBreadthFirst(ApiRoot, NodeValuePredicate);

				var ObjectPathMatch = SetObjectPathMatch?.FirstOrDefault();

				if (null == ObjectPathMatch)
				{
					throw new ArgumentException("no match for given search criteria.");
				}

				TreeView?.TreeView?.ExpandPath(ObjectPathMatch?.Keys(), true);
			});
		}
	}

}
