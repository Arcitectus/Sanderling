using Sanderling.Interface.MemoryStruct;

namespace Sanderling.Script
{
	static public class Extension
	{
		static public BotEngine.Motor.MotionResult MouseClick(
			this IHostToScript Host,
			UIElement Destination,
			BotEngine.Motor.MouseButtonIdEnum MouseButton) =>
			Host?.MotionExecute(new Motor.MotionParam()
			{
				MouseListWaypoint = new[] { new Motor.MotionParamMouseRegion() { UIElement = Destination }, },
				MouseButton = new[] { MouseButton },
			});

		static public BotEngine.Motor.MotionResult MouseClickLeft(
			this IHostToScript Host,
			UIElement Destination) =>
			MouseClick(Host, Destination, BotEngine.Motor.MouseButtonIdEnum.Left);

		static public BotEngine.Motor.MotionResult MouseClickRight(
			this IHostToScript Host,
			UIElement Destination) =>
			MouseClick(Host, Destination, BotEngine.Motor.MouseButtonIdEnum.Right);

	}
}
