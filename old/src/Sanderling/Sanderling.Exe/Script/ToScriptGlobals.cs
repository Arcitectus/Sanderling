namespace Sanderling.Exe.Script
{
	/// <summary>
	/// For the time beeing, the type used for the globals must reside in an Assembly which can be resolved by MetadataReference.CreateFromAssemblyInternal because that is what Microsoft.CodeAnalysis.Scripting.Script uses.
	/// (Indexed on: October 16: (http://source.roslyn.codeplex.com/#Microsoft.CodeAnalysis.Scripting/Script.cs,200))
	/// </summary>
	public class ToScriptGlobals : Sanderling.Script.ToScriptGlobals
	{
	}
}
