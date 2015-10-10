using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sanderling.Script
{
	/// <summary>
	/// with 2015.09.09	Microsoft.CodeAnalysis.Scripting.CSharp v1.1.0.0: CSharpScript.RunAsync seems to run at least the compilation syncronously.
	/// For this reason, the call to RunAsync is encapsulated in a Task.
	/// </summary>
	public class Session
	{
		public class ToScriptGlobals
		{
			public IHostToScript Host;
		}

		private static readonly Type[] _assemblyTypes =
		{
			typeof (object),
			//	typeof (Task),
			typeof (List<>),
			typeof (Regex),
			//	typeof (StringBuilder),
			typeof (Uri),
			typeof (Enumerable),
			//	typeof (ObjectExtensions),
			typeof(BotScript.IHostToScript),
			typeof(Sanderling.Script.IHostToScript),
		};

		readonly public ConcurrentQueue<object> LogQueue = new ConcurrentQueue<object>();

		readonly object Lock = new object();

		readonly System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();

		Task<ScriptState<object>> RunTask;

		public Exception RunException => RunTask?.Exception;

		IHostToScript IHostToScript = new HostToScript();

		public bool Started => null != RunTask;

		public bool Completed => RunTask?.IsCompleted ?? false;

		static ScriptOptions ScriptOptions =>
			ScriptOptions.Default
			.AddNamespaces(_assemblyTypes.Select(x => x.Namespace))
			.AddReferences(_assemblyTypes.Select(x => x.Assembly));

		public void Start(string Script)
		{
			lock (Lock)
			{
				if (null != RunTask)
				{
					return;
				}

				RunTask = new Task<ScriptState<object>>(() =>
				{
					return
					CSharpScript.RunAsync(
						Script,
						ScriptOptions,
						globals: new ToScriptGlobals() { Host = IHostToScript }).Result;
				},
				cancellationToken);

				RunTask.ContinueWith<ScriptState<object>>(t =>
			   {
				   var Exc = t.Exception;

				   if (null != Exc)
				   {
					   LogQueue.Enqueue(Exc);
				   }

				   return t.Result;
			   });

				RunTask.Start();
			}
		}
	}
}
