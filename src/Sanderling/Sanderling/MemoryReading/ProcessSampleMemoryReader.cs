using System;

namespace Sanderling.MemoryReading
{
	public class ProcessSampleMemoryReader : IMemoryReader
	{
		readonly public Process.Measurement.Measurement ProcessSample;

		public ProcessSampleMemoryReader(Process.Measurement.Measurement processSample)
		{
			ProcessSample = processSample;
		}

		public MemoryReaderModuleInfo[] Modules()
		{
			throw new NotImplementedException();
		}

		public int ReadBytes(long Address, int BytesCount, byte[] DestinationArray)
		{
			foreach (var BaseAddressAndListOctet in ProcessSample.Process.MemoryBaseAddressAndListOctet)
			{
				var ToSkip = Address - BaseAddressAndListOctet.Key;
				var Rest = BaseAddressAndListOctet.Value.Length - ToSkip;

				if (0 <= ToSkip && 0 < Rest)
				{
					var ToCopyCount = (int)Math.Min(Rest, BytesCount);

					Buffer.BlockCopy(BaseAddressAndListOctet.Value, (int)ToSkip, DestinationArray, 0, ToCopyCount);

					return ToCopyCount;
				}
			}

			return 0;
		}

		public byte[] ReadBytes(long Address, int attemptBytesCount)
		{
			var buffer = new byte[attemptBytesCount];

			var resultBytesCount = ReadBytes(Address, attemptBytesCount, buffer);

			if(resultBytesCount < buffer.Length)
			{
				var slice = new byte[resultBytesCount];

				Buffer.BlockCopy(buffer, 0, slice, 0, resultBytesCount);

				return slice;
			}

			return buffer;
		}
	}
}
