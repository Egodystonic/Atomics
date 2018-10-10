// (c) Egodystonic Studios 2018


using System;
using System.Collections.Generic;
using System.Linq;

namespace Egodystonic.Atomics.Tests.Harness {
	public enum ThreadType {
		WriterThread,
		ReaderThread
	}

	interface IConcurrentTestCase<T> {
		string Description { get; }
		TimeSpan MaxSetUpWaitTime { get; }
		TimeSpan MaxExecutionWaitTime { get; }
		TimeSpan MaxTearDownWaitTime { get; }
		Action<T> GetExecutionAction(ThreadType threadType, int threadNumber, ConcurrentTestCaseThreadConfig threadConfig);
		IEnumerable<ConcurrentTestCaseThreadConfig> GetAllTargetThreadConfigs();
	}
}