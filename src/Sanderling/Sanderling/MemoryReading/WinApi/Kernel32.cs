using System;
using System.Runtime.InteropServices;

namespace Sanderling.MemoryReading.WinApi
{
	static	public	class Kernel32
	{
		public enum PROCESS_ACCESS_RIGHT
		{
			PROCESS_CREATE_PROCESS = 0x0080,
			PROCESS_CREATE_THREAD = 0x0002,
			PROCESS_DUP_HANDLE = 0x0040,
			PROCESS_QUERY_INFORMATION = 0x0400,
			PROCESS_SET_INFORMATION = 0x0200,
			PROCESS_SET_QUOTA = 0x0100,
			PROCESS_SUSPEND_RESUME = 0x0800,
			PROCESS_TERMINATE = 0x0001,
			PROCESS_VM_OPERATION = 0x0008,
			PROCESS_VM_READ = 0x0010,
			PROCESS_VM_WRITE = 0x0020,
			SYNCHRONIZE = 0x00100000,
		}

		[DllImport("kernel32.dll")]
		static extern public Int32 CloseHandle(IntPtr hObject);

		[DllImport("kernel32.dll")]
		static extern public IntPtr OpenProcess(
			PROCESS_ACCESS_RIGHT dwDesiredAccess,
			Int32 bInheritHandle,
			UInt32 dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern public UInt32 ReadProcessMemory(
			IntPtr hProcess,
			IntPtr lpBaseAddress,
			byte[] lpBuffer,
			IntPtr size,
			[Out]	out	IntPtr lpNumberOfBytesRead);
	}
}
