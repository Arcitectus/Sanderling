using BotEngine;
using Newtonsoft.Json;
using System.Linq;
using System.Text;

namespace EveClientMemoryDump.Exe
{
	class Program
	{
		static void Main(string[] args)
		{
			var processSampleFilePath = args[0];

			var outputFilePath = args?.ElementAtOrDefault(1) ?? (processSampleFilePath + ".memoryreading.report");

			var processSample = Process.Measurement.Extension.MeasurementFromZipArchive(Bib3.Glob.InhaltAusDataiMitPfaad(processSampleFilePath));

			var reader = new Sanderling.MemoryReading.Python.PythonMemoryReader(new Sanderling.MemoryReading.ProcessSampleMemoryReader(processSample));

			var report = MemoryReadingReportBuilder.BuildReport(reader);

			Bib3.Glob.ScraibeInhaltNaacDataiPfaad(
				outputFilePath,
				Encoding.UTF8.GetBytes(report.SerializeToString(Formatting.Indented)));
		}
	}
}
