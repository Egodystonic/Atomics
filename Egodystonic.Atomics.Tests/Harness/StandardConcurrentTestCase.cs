// (c) Egodystonic Studios 2018


using System;

namespace Egodystonic.Atomics.Tests.Harness {
	sealed class StandardConcurrentTestCase<T> : IConcurrentTestCase<T> {
		readonly Action<T> _writerThreadExecAction;
		readonly Action<T> _readerThreadExecAction;

		public string Description { get; }
		public ConcurrentTestCaseThreadConfig ThreadConfig { get; }
		public Action<T> GetExecutionAction(ThreadType threadType, int threadIndex) => threadType == ThreadType.WriterThread ? _writerThreadExecAction : _readerThreadExecAction;

		public StandardConcurrentTestCase(string description, Action<T> writerThreadExecAction, Action<T> readerThreadExecAction, ConcurrentTestCaseThreadConfig threadConfig) {
			Description = description;
			ThreadConfig = threadConfig;
			_writerThreadExecAction = writerThreadExecAction;
			_readerThreadExecAction = readerThreadExecAction;
		}
	}
}