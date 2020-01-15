using Optimat.EveOnline;
using Optimat.EveOnline.AuswertGbs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanderling.MemoryReading.Production
{
	static public class Extension
	{
		static readonly Bib3.RefNezDiferenz.SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer
			KonvertGbsAstInfoRictliinieMitScatescpaicer =
			new Bib3.RefNezDiferenz.SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer(
				Bib3.RefNezDiferenz.NewtonsoftJson.SictMengeTypeBehandlungRictliinieNewtonsoftJson.KonstruktMengeTypeBehandlungRictliinie(
				new KeyValuePair<Type, Type>[]{
					new KeyValuePair<Type, Type>(typeof(GbsAstInfo), typeof(UINodeInfoInTree)),
					new KeyValuePair<Type, Type>(typeof(GbsAstInfo[]), typeof(UINodeInfoInTree[])),
		}));

		static public IEnumerable<T[]> SuuceFlacMengeAstMitPfaad<T>(
			this T SuuceWurzel)
			where T : GbsAstInfo =>
			Bib3.Extension.EnumeratePathToNodeFromTree(SuuceWurzel, Ast => Ast.GetListChild()?.OfType<T>());

		static public T[] SuuceFlacMengeAstMitPfaadFrüheste<T>(
			this T SuuceWurzel,
			Func<T, bool> Prädikaat)
			where T : GbsAstInfo =>
			SuuceFlacMengeAstMitPfaad(SuuceWurzel)
			?.Where(pfaad => Prädikaat(pfaad?.LastOrDefault()))
			?.FirstOrDefault();

	}
}
