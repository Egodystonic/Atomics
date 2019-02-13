// (c) Egodystonic Studios 2018


using System;

namespace Egodystonic.Atomics.Tests.Harness {
	public enum ThreadType {
		WriterThread,
		ReaderThread
	}

	interface IConcurrentTestCase<T> {
		string Description { get; }
		ConcurrentTestCaseThreadConfig ThreadConfig { get; }
		Action<T> GetExecutionAction(ThreadType threadType, int threadIndex);
	}
}