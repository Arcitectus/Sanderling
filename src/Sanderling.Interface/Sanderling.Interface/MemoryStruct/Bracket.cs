namespace Sanderling.Interface.MemoryStruct
{
	public interface IInSpaceBracket : IContainer
	{
		string Name { get; }
	}

	public class InSpaceBracket : Container, IInSpaceBracket
	{
		public string Name { set; get; }

		public InSpaceBracket()
			:
			this(null)
		{
		}

		public InSpaceBracket(IUIElement @base)
			:
			base(@base)
		{
			var BaseSpec = @base as IInSpaceBracket;

			Name = BaseSpec?.Name;
		}
	}
}
