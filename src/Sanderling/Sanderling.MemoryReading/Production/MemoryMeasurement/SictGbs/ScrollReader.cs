using System;
using System.Linq;
using System.Text.RegularExpressions;
using Sanderling.Interface.MemoryStruct;
using Bib3.Geometrik;
using Bib3;

namespace Optimat.EveOnline.AuswertGbs
{
	public class ScrollReader
	{
		public const string MainContainerUIElementName = "maincontainer";
		public const string ClipperUIElementName = "__clipper";
		public const string ClipperContentUIElementName = "__content";

		readonly public UINodeInfoInTree ScrollNode;

		public UINodeInfoInTree ClipperContentNode
		{
			private set;
			get;
		}

		public Scroll Result
		{
			private set;
			get;
		}

		public ScrollReader(UINodeInfoInTree scrollNode)
		{
			ScrollNode = scrollNode;
		}

		virtual public void Read()
		{
			if (!(ScrollNode?.VisibleIncludingInheritance ?? false))
				return;

			var	mainContainerNode =
				ScrollNode?.FirstMatchingNodeFromSubtreeBreadthFirst(
					candidate => string.Equals(MainContainerUIElementName, candidate.Name, StringComparison.InvariantCultureIgnoreCase),
				2, 0);

			var	scrollHeadersContainerNode =
				ScrollNode?.FirstMatchingNodeFromSubtreeBreadthFirst(candidate =>
					candidate.PyObjTypNameIsContainer() &&
					(Regex.Match(candidate.Name ?? "", "scrollHeader", RegexOptions.IgnoreCase).Success) ||
					Regex.Match(candidate.PyObjTypName ?? "", "SortHeader", RegexOptions.IgnoreCase).Success ||
					candidate.PyObjTypNameMatchesRegexPatternIgnoreCase("ScrollColumnHeader"),
					3, 1);

			var	setCandidateForColumnHeaderNode =
				scrollHeadersContainerNode?.MatchingNodesFromSubtreeBreadthFirst(candidate =>
					Regex.Match(candidate.PyObjTypName ?? "", "ColumnHeader", RegexOptions.IgnoreCase).Success ||
					candidate.PyObjTypNameIsContainer(),
					null, 2, 1)
				?.ToArray();

			var listColumnHeader =
				setCandidateForColumnHeaderNode
				?.Select(SictAuswertGbsListColumnHeader.Read)
				?.Where((columnHeader) => null != columnHeader)
				?.Where((columnHeader) => !(columnHeader?.Text).IsNullOrEmpty())
				.TailmengeUnterste(ScrollNode)
				?.OrderBy((columnHeader) => columnHeader.Region.Center().A)
				?.GroupBy(header => header.Id)
				?.Select(headerGroup => headerGroup.FirstOrDefault())
				?.ToArray();

			var	mainContainerScrollControlsNode =
				mainContainerNode?.FirstMatchingNodeFromSubtreeBreadthFirst(
					candidate => string.Equals("ScrollControls", candidate.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			var	mainContainerScrollControlsScrollHandleNode =
				mainContainerScrollControlsNode?.FirstMatchingNodeFromSubtreeBreadthFirst(
					candidate => string.Equals("ScrollHandle", candidate.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
				3, 1);

			var	mainContainerClipperNode =
				mainContainerNode?.FirstMatchingNodeFromSubtreeBreadthFirst(
					candidate => string.Equals(ClipperUIElementName, candidate.Name, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			ClipperContentNode =
				mainContainerClipperNode?.FirstMatchingNodeFromSubtreeBreadthFirst(
					candidate => candidate.PyObjTypNameIsContainer() &&
					string.Equals(ClipperContentUIElementName, candidate.Name, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			Result = new Scroll(ScrollNode.AsUIElementIfVisible())
			{
				ColumnHeader = listColumnHeader,
				Clipper = mainContainerClipperNode.AsUIElementIfVisible(),
				ScrollHandleBound = mainContainerScrollControlsNode.AsUIElementIfVisible(),
				ScrollHandle = mainContainerScrollControlsScrollHandleNode.AsUIElementIfVisible(),
			};
		}
	}
}
