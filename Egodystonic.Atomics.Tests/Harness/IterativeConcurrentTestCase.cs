// (c) Egodystonic Studios 2018


using System;
using System.Collections.Generic;
using System.Linq;

namespace Egodystonic.Atomics.Tests.Harness {
	sealed class IterativeConcurrentTestCase<T> : IConcurrentTestCase<T> {
		readonly string _description;
		readonly Action<T> _writerThreadExecAction;
		readonly Action<T> _readerThreadExecAction;
		readonly ConcurrentTestCaseThreadConfig[] _threadConfigs;
		readonly int _numIterationsTotalPerThreadType;
		readonly bool _iterateWriters;
		readonly bool _iterateReaders;
		readonly TimeSpan _setUpTimeLimit;
		readonly TimeSpan _execTimeLimit;
		readonly TimeSpan _tearDownTimeLimit;

		public string Description => _description;
		public TimeSpan MaxSetUpWaitTime => _setUpTimeLimit;
		public TimeSpan MaxExecutionWaitTime => _execTimeLimit;
		public TimeSpan MaxTearDownWaitTime => _tearDownTimeLimit;
		public Action<T> GetExecutionAction(ThreadType threadType, int threadNumber, ConcurrentTestCaseThreadConfig threadConfig) {
			if ((threadType == ThreadType.WriterThread && !_iterateWriters) || (threadType == ThreadType.ReaderThread && _iterateReaders)) {
				return threadType == ThreadType.WriterThread ? _writerThreadExecAction : _readerThreadExecAction;
			}

			var numIterationsThisThread = _numIterationsTotalPerThreadType / threadConfig.ThreadCount(threadType);
			if (threadNumber == threadConfig.ThreadCount(threadType) - 1) numIterationsThisThread += _numIterationsTotalPerThreadType % threadConfig.ThreadCount(threadType);

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
		public IEnumerable<ConcurrentTestCaseThreadConfig> GetAllTargetThreadConfigs() => _threadConfigs;

		public IterativeConcurrentTestCase(string description, Action<T> writerThreadExecAction, Action<T> readerThreadExecAction, ConcurrentTestCaseThreadConfig[] threadConfigs, int numIterationsTotalPerThreadType, bool iterateWriters, bool iterateReaders, TimeSpan upTimeLimit, TimeSpan execTimeLimit, TimeSpan tearDownTimeLimit) {
			_description = description;
			_writerThreadExecAction = writerThreadExecAction;
			_readerThreadExecAction = readerThreadExecAction;
			_threadConfigs = threadConfigs;
			_numIterationsTotalPerThreadType = numIterationsTotalPerThreadType;
			_iterateWriters = iterateWriters;
			_iterateReaders = iterateReaders;
			_setUpTimeLimit = upTimeLimit;
			_execTimeLimit = execTimeLimit;
			_tearDownTimeLimit = tearDownTimeLimit;
		}
	}
}