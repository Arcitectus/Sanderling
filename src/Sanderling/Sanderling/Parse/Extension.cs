using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.Parse
{
	static public class Extension
	{
		static public IMemoryMeasurement Parse(this MemoryStruct.IMemoryMeasurement MemoryMeasurement) =>
			null == MemoryMeasurement ? null : new Parse.MemoryMeasurement(MemoryMeasurement);

		static public IModuleButtonTooltip ParseAsModuleButtonTooltip(this MemoryStruct.IContainer Container) =>
			null == Container ? null : new ModuleButtonTooltip(Container);

		static public INeocom Parse(this MemoryStruct.INeocom Neocom) =>
			null == Neocom ? null : new Neocom(Neocom);
	}
}
