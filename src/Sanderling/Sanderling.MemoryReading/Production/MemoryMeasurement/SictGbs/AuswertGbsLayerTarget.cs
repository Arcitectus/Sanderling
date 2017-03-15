using System;
using Bib3;
using System.Linq;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs
{
	public	class SictAuswertGbsLayerTarget
	{
		readonly public SictGbsAstInfoSictAuswert LayerTargetNode;

		public SictGbsAstInfoSictAuswert[] SetTargetNode
		{
			private set;
			get;
		}

		public SictAuswertGbsTarget[] MengeFensterTargetAuswert
		{
			private set;
			get;
		}

		public ShipUiTarget[] SetTarget
		{
			private set;
			get;
		}

		public SictAuswertGbsLayerTarget(SictGbsAstInfoSictAuswert layerTargetNode)
		{
			this.LayerTargetNode = layerTargetNode;
		}

		public	void Berecne()
		{
			if (!(LayerTargetNode?.SictbarMitErbe ?? false))
				return;

			SetTargetNode =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAst(
				LayerTargetNode,
				(kandidaat) => string.Equals("TargetInBar", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),	null,	4);

			MengeFensterTargetAuswert =
				SetTargetNode.Select((targetNode) => new SictAuswertGbsTarget(targetNode)).ToArray();

			foreach (var AuswertTarget in MengeFensterTargetAuswert)
				AuswertTarget.Berecne();

			SetTarget =
				MengeFensterTargetAuswert
				?.Select((targetAuswert) => targetAuswert.Ergeebnis)
				?.WhereNotDefault()
				?.ToArray();
		}
	}
}
