using Bib3;
using System.Collections.Generic;
using System.Reflection;

namespace Sanderling.Exe.Script
{
	static public class ToScriptImport
	{
		static public Microsoft.CodeAnalysis.MetadataReference AssemblySelfReference = Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(Assembly.GetCallingAssembly().Location);

		static public IEnumerable<Microsoft.CodeAnalysis.MetadataReference> ImportAssembly =>
			Sanderling.Script.Impl.ToScriptImport.ImportAssembly.ConcatNullable(new[] { AssemblySelfReference });
	}
}
