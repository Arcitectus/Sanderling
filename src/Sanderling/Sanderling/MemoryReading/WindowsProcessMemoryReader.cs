using Sanderling.MemoryReading.WinApi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanderling.MemoryReading
{
	/// <summary>
	/// reads memory from the process identified by <paramref name="ProcessId"/>.
	/// </summary>
	public class WindowsProcessMemoryReader : IMemoryReader, IDisposable
	{
		readonly public int ProcessId;

		IntPtr ProcessHandle;

		MemoryReaderModuleInfo[] ModulesCache;

		public	MemoryReaderModuleInfo[]	Modules()
		{
			//	assuming that Modules havent changed since call to constructor.
			return ModulesCache;
		}

		static	public	MemoryReaderModuleInfo[]	ModulesOfProcess(int ProcessId)
		{
			var Process = System.Diagnostics.Process.GetProcessById(ProcessId);

			var Modules = new List<MemoryReaderModuleInfo>();

			foreach (var Module in Process.Modules.OfType<System.Diagnostics.ProcessModule>())
			{
				Modules.Add(new MemoryReaderModuleInfo(Module.ModuleName, Module.BaseAddress.ToInt64()));
			}

			return Modules.ToArray();
		}

		public WindowsProcessMemoryReader(
			int ProcessId)
		{
			this.ProcessId = ProcessId;

			ProcessHandle = Kernel32.OpenProcess(Kernel32.PROCESS_ACCESS_RIGHT.PROCESS_VM_READ, 0, (uint)ProcessId);

			ModulesCache = ModulesOfProcess(ProcessId);
		}

		public WindowsProcessMemoryReader(
			System.Diagnostics.Process Process)
			:
			this(Process.Id)
		{
		}

		public void Dispose()
		{
			Kernel32.CloseHandle(ProcessHandle);
		}

		static public IntPtr? CastToIntPtrAvoidOverflow(Int64 Address)
		{
			if (4 == IntPtr.Size)
			{
				if (Address < UInt32.MinValue)
				{
					return null;
				}

				if (UInt32.MaxValue < Address)
				{
					return null;
				}
			}

			return (IntPtr)((Int32)Address);
		}

		public byte[] ReadBytes(Int64 Address, int BytesCount)
		{
			var Buffer = new byte[BytesCount];

			var lpNumberOfBytesRead = IntPtr.Zero;

			var AddressAsIntPtr = CastToIntPtrAvoidOverflow(Address);

			if(!AddressAsIntPtr.HasValue)
			{
				return null;
			}

			var Error = Kernel32.ReadProcessMemory(ProcessHandle, AddressAsIntPtr.Value, Buffer, (IntPtr)Buffer.Length, out	lpNumberOfBytesRead);

			var NumberOfBytesRead = (int)lpNumberOfBytesRead;

			if (NumberOfBytesRead < 1)
			{
				return null;
			}

			if (Buffer.Length == NumberOfBytesRead)
			{
				return Buffer;
			}

			return Buffer.Take(NumberOfBytesRead).ToArray();
		}
	}
}
