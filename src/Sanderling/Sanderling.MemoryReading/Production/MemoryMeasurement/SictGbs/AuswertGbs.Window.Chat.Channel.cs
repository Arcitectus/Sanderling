using System;
using System.Linq;
using Sanderling.Interface.MemoryStruct;
using Bib3;
using Bib3.Geometrik;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsWindowChatChannel : SictAuswertGbsWindow
	{
		new static public WindowChatChannel BerecneFürWindowAst(
			SictGbsAstInfoSictAuswert windowAst)
		{
			if (null == windowAst)
				return null;

			var WindowAuswert = new SictAuswertGbsWindowChatChannel(windowAst);

			WindowAuswert.Berecne();

			return WindowAuswert.ErgeebnisScpezChatChannel;
		}

		public SictAuswertGbsListViewport<IListEntry> ViewportMessageAuswert
		{
			private set;
			get;
		}

		public SictAuswertGbsListViewport<IChatParticipantEntry> ViewportParticipantAuswert
		{
			private set;
			get;
		}

		public WindowChatChannel ErgeebnisScpezChatChannel
		{
			private set;
			get;
		}

		public SictAuswertGbsWindowChatChannel(SictGbsAstInfoSictAuswert windowStackAst)
			:
			base(windowStackAst)
		{
		}

		static public ChatParticipantEntry ListEntryParticipantConstruct(
			SictGbsAstInfoSictAuswert ast,
			IColumnHeader[] listHeader,
			RectInt? regionConstraint)
		{
			var ListEntry = SictAuswertGbsListViewport<IListEntry>.ListEntryKonstruktSctandard(ast, listHeader, regionConstraint);

			if (null == ListEntry)
				return null;

			var StatusIconSprite =
				ast.MengeChildAstTransitiiveHüle()
				?.OfType<SictGbsAstInfoSictAuswert>()
				?.FirstOrDefault(k => k.PyObjTypNameIsSprite() && (k.PyObjTypName?.ToLower().Contains("status") ?? false))
				?.AlsSprite();

			var SetFlagWithStateIconNode =
				ast?.MengeChildAstTransitiiveHüle()
				?.OfType<SictGbsAstInfoSictAuswert>()
				?.Where(k => k?.PyObjTypNameMatchesRegexPatternIgnoreCase("FlagIconWithState") ?? false)
				?.ToArray();

			var SetFlagWithStateIcon =
				SetFlagWithStateIconNode
				?.Select(flagNode =>
				{
					var FlagIcon = flagNode.AlsSprite();

					var childSprite =
						flagNode?.MengeChildAstTransitiiveHüle()?.OfType<SictGbsAstInfoSictAuswert>()
						?.Where(k => k.PyObjTypNameIsSprite())
						?.Select(k => k?.AlsSprite())?.WhereNotDefault()?.FirstOrDefault();

					if (null != FlagIcon)
					{
						FlagIcon.Texture0Id = FlagIcon.Texture0Id ?? childSprite?.Texture0Id;
						FlagIcon.TexturePath = FlagIcon.TexturePath ?? childSprite?.TexturePath;
						FlagIcon.Color = FlagIcon.Color ?? childSprite?.Color;
					}

					return FlagIcon;
				})
				?.WhereNotDefault()
				?.ToArrayIfNotEmpty();

			return new ChatParticipantEntry(ListEntry)
			{
				NameLabel = ast.GröösteLabel().AsUIElementTextIfTextNotEmpty(),
				StatusIcon = StatusIconSprite,
				FlagIcon = SetFlagWithStateIcon,
			};
		}

		override public void Berecne()
		{
			base.Berecne();

			var AstMainContainer = base.AstMainContainer;

			if (null == AstMainContainer)
				return;

			if (!(true == AstMainContainer.SictbarMitErbe))
				return;

			var SetScrollAst =
				AstMainContainer.SuuceFlacMengeAst(ast => ast.PyObjTypNameIsScroll())
				?.ToArray();

			var ViewportSetMessageAst =
				SetScrollAst
				?.Where(ast => ast?.LaageInParentA < AstMainContainer?.LaageInParentA)
				?.GröösteAst();

			//	userlist isc auf recte saite.
			var ViewportSetParticipantAst =
				SetScrollAst
				?.Where(ast => AstMainContainer?.LaageInParentA < ast?.LaageInParentA)
				?.GröösteAst();

			var FunkIsOther = new Func<IObjectIdInMemory, bool>(obj =>
			  !(ViewportSetMessageAst?.EnthaltAstMitHerkunftAdrese(obj.Id) ?? false) &&
			  !(ViewportSetParticipantAst?.EnthaltAstMitHerkunftAdrese(obj.Id) ?? false));

			var LabelOther =
				AstMainContainer.ExtraktMengeLabelString()
				?.Where(label => FunkIsOther(label))
				?.OrdnungLabel()
				?.ToArray();

			if (null != ViewportSetMessageAst)
			{
				ViewportMessageAuswert = new SictAuswertGbsListViewport<IListEntry>(ViewportSetMessageAst, SictAuswertGbsListViewport<IListEntry>.ListEntryKonstruktSctandard);
				ViewportMessageAuswert.Berecne();
			}

			if (null != ViewportSetParticipantAst)
			{
				ViewportParticipantAuswert = new SictAuswertGbsListViewport<IChatParticipantEntry>(ViewportSetParticipantAst, ListEntryParticipantConstruct);
				ViewportParticipantAuswert.Berecne();
			}

			var MessageInputAst =
				AstMainContainer
				?.SuuceFlacMengeAst(node => node?.PyObjTypNameMatchesRegexPattern("EditPlainText") ?? false)
				?.OrderByDescending(node => node.Grööse?.BetraagQuadriirt ?? -1)
				?.FirstOrDefault();

			ErgeebnisScpezChatChannel = new WindowChatChannel(
				base.Ergeebnis)
			{
				MessageView = ViewportMessageAuswert?.Ergeebnis,
				ParticipantView = ViewportParticipantAuswert?.Ergeebnis,
				MessageInput = MessageInputAst?.AsUIElementInputText(),
			};
		}
	}

}
