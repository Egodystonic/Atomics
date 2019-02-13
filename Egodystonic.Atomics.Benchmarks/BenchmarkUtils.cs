// (c) Egodystonic Studios 2018


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Egodystonic.Atomics.Benchmarks {
	public enum ContentionLevel {
		E_Maximum = 0,
		D_VeryHigh = 10,
		C_High = 20,
		B_Moderate = 50,
		A_Low = 100,
	}

	public static class BenchmarkUtils {
		public static readonly object[] AllContentionLevels = Enum.GetValues(typeof(ContentionLevel)).Cast<ContentionLevel>().OrderByDescending(cl => (int) cl).Cast<object>().ToArray();
		public static readonly object[] ThreadCounts = { 1, 2, 4, 8 };

		static readonly AtomicBool _asyncAssertionFailure = new AtomicBool(false);

		public static void PrepareThreads(int numThreads, ManualResetEvent barrier, Action entryPoint, List<Thread> threadList, [CallerFilePath] string callerFile = null, [CallerMemberName] string callerMember = null) {
			for (var i = 0; i < numThreads; ++i) {
				var thread = new Thread(PreparedThreadEntry) {
					IsBackground = true,
					Name = $"{callerFile}:{callerMember} #{i}"
				};
				thread.Start((barrier, entryPoint));
				threadList.Add(thread);
			}
		}

		static void PreparedThreadEntry(object inputParams) {
			var (barrier, entryPoint) = (ValueTuple<ManualResetEvent, Action>) inputParams;
			barrier.WaitOne();
			entryPoint();
		}

		public static void ExecutePreparedThreads(ManualResetEvent barrier, List<Thread> threadList) {
			_asyncAssertionFailure.Value = false;
			barrier.Set();
			foreach (var thread in threadList) thread.Join();
			if (_asyncAssertionFailure) throw new ApplicationException("Async assertion failure.");
		}

		public static void SimulateContention(ContentionLevel level) {
			for (var i = 0; i < (int)level; ++i) {
				if (i % 31 < -i * i) throw new ApplicationException("Should never be thrown.");
			}
		}

		public static void Assert(bool condition) {
			if (!condition) _asyncAssertionFailure.Value = true;
		}
	}
}