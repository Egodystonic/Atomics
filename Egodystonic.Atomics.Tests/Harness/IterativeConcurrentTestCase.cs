// (c) Egodystonic Studios 2018


using System;
using System.Collections.Generic;
using System.Linq;

namespace Egodystonic.Atomics.Tests.Harness {
	sealed class IterativeConcurrentTestCase<T> : IConcurrentTestCase<T> {
		readonly Action<T> _writerThreadExecAction;
		readonly Action<T> _readerThreadExecAction;
		readonly int _numIterationsTotalPerThreadType;
		readonly bool _iterateWriters;
		readonly bool _iterateReaders;

		public string Description { get; }
		public ConcurrentTestCaseThreadConfig ThreadConfig { get; }
		public Action<T> GetExecutionAction(ThreadType threadType, int threadNumber) {
			if ((threadType == ThreadType.WriterThread && !_iterateWriters) || (threadType == ThreadType.ReaderThread && _iterateReaders)) {
				return threadType == ThreadType.WriterThread ? _writerThreadExecAction : _readerThreadExecAction;
			}

			var numIterationsThisThread = _numIterationsTotalPerThreadType / ThreadConfig.ThreadCount(threadType);
			if (threadNumber == ThreadConfig.ThreadCount(threadType) - 1) numIterationsThisThread += _numIterationsTotalPerThreadType % ThreadConfig.ThreadCount(threadType);

			if (threadType == ThreadType.WriterThread) {
				return ctx => {
					for (var i = 0; i < numIterationsThisThread; ++i) {
						_writerThreadExecAction(ctx);
					}
				};
			}
			else {
				return ctx => {
					for (var i = 0; i < numIterationsThisThread; ++i) {
						_readerThreadExecAction(ctx);
					}
				};
			}
		}

		public IterativeConcurrentTestCase(string description, Action<T> writerThreadExecAction, Action<T> readerThreadExecAction, ConcurrentTestCaseThreadConfig threadConfig, int numIterationsTotalPerThreadType, bool iterateWriters, bool iterateReaders) {
			Description = description;
			ThreadConfig = threadConfig;
			_writerThreadExecAction = writerThreadExecAction;
			_readerThreadExecAction = readerThreadExecAction;
			_numIterationsTotalPerThreadType = numIterationsTotalPerThreadType;
			_iterateWriters = iterateWriters;
			_iterateReaders = iterateReaders;
		}
	}
}