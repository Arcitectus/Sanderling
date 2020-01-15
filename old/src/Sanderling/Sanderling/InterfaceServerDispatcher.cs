namespace Sanderling
{
	public class SimpleInterfaceServerDispatcher : BotEngine.Interface.SimpleInterfaceServerDispatcher
	{
		public override bool AppInterfaceAvailable =>
			this.InterfaceAppManager?.ClientRequest(new Interface.ToInterfaceRequest()) != null;
	}
}
