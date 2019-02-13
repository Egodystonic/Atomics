// (c) Egodystonic Studios 2018


using System;

namespace Egodystonic.Atomics.Tests.Harness {
	sealed class IterativeConcurrentTestCase<T> : IConcurrentTestCase<T> {
		readonly Action<T> _writerThreadExecAction;
		readonly Action<T> _readerThreadExecAction;
		readonly Action _writerThreadCompletionAction;
		readonly Action _readerThreadCompletionAction;
		readonly int _numIterationsTotalPerThreadType;
		readonly bool _iterateWriters;
		readonly bool _iterateReaders;

		public string Description { get; }
		public ConcurrentTestCaseThreadConfig ThreadConfig { get; }
		public Action<T> GetExecutionAction(ThreadType threadType, int threadIndex) {
			if ((threadType == ThreadType.WriterThread && !_iterateWriters) || (threadType == ThreadType.ReaderThread && !_iterateReaders)) {
				return threadType == ThreadType.WriterThread ? _writerThreadExecAction : _readerThreadExecAction;
			}

			var numIterationsThisThread = _numIterationsTotalPerThreadType / ThreadConfig.ThreadCount(threadType);
			if (threadIndex == ThreadConfig.ThreadCount(threadType) - 1) numIterationsThisThread += _numIterationsTotalPerThreadType % ThreadConfig.ThreadCount(threadType);

			if (threadType == ThreadType.WriterThread) {
				return ctx => {
					try {
						for (var i = 0; i < numIterationsThisThread; ++i) {
							_writerThreadExecAction(ctx);
						}
					}
					finally {
						_writerThreadCompletionAction?.Invoke();
					}
				};
			}
			else {
				return ctx => {
					try {
						for (var i = 0; i < numIterationsThisThread; ++i) {
							_readerThreadExecAction(ctx);
						}
					}
					finally {
						_readerThreadCompletionAction?.Invoke();
					}
				};
			}
		}

		public IterativeConcurrentTestCase(string description, Action<T> writerThreadExecAction, Action<T> readerThreadExecAction, ConcurrentTestCaseThreadConfig threadConfig, int numIterationsTotalPerThreadType, bool iterateWriters, bool iterateReaders)
			:this(description, writerThreadExecAction, readerThreadExecAction, threadConfig, numIterationsTotalPerThreadType, iterateWriters, iterateReaders, null, null) { }

		public IterativeConcurrentTestCase(string description, Action<T> writerThreadExecAction, Action<T> readerThreadExecAction, ConcurrentTestCaseThreadConfig threadConfig, int numIterationsTotalPerThreadType, bool iterateWriters, bool iterateReaders, Action writerThreadCompletionAction, Action readerThreadCompletionAction) {
			Description = description;
			ThreadConfig = threadConfig;
			_writerThreadExecAction = writerThreadExecAction;
			_readerThreadExecAction = readerThreadExecAction;
			_numIterationsTotalPerThreadType = numIterationsTotalPerThreadType;
			_iterateWriters = iterateWriters;
			_iterateReaders = iterateReaders;
			_writerThreadCompletionAction = writerThreadCompletionAction;
			_readerThreadCompletionAction = readerThreadCompletionAction;
		}
	}
}