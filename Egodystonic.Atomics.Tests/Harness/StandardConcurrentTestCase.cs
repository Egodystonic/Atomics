// (c) Egodystonic Studios 2018


using System;
using System.Collections.Generic;
using System.Linq;

namespace Egodystonic.Atomics.Tests.Harness {
	sealed class StandardConcurrentTestCase<T> : IConcurrentTestCase<T> {
		readonly string _description;
		readonly Action<T> _writerThreadExecAction;
		readonly Action<T> _readerThreadExecAction;
		readonly ConcurrentTestCaseThreadConfig[] _threadConfigs;
		readonly TimeSpan _setUpTimeLimit;
		readonly TimeSpan _execTimeLimit;
		readonly TimeSpan _tearDownTimeLimit;

		public string Description => _description;
		public TimeSpan MaxSetUpWaitTime => _setUpTimeLimit;
		public TimeSpan MaxExecutionWaitTime => _execTimeLimit;
		public TimeSpan MaxTearDownWaitTime => _tearDownTimeLimit;
		public Action<T> GetExecutionAction(ThreadType threadType, int threadNumber, ConcurrentTestCaseThreadConfig threadConfig) => threadType == ThreadType.WriterThread ? _writerThreadExecAction : _readerThreadExecAction;
		public IEnumerable<ConcurrentTestCaseThreadConfig> GetAllTargetThreadConfigs() => _threadConfigs;

		public StandardConcurrentTestCase(string description, Action<T> writerThreadExecAction, Action<T> readerThreadExecAction, ConcurrentTestCaseThreadConfig[] threadConfigs, TimeSpan upTimeLimit, TimeSpan execTimeLimit, TimeSpan tearDownTimeLimit) {
			_description = description;
			_writerThreadExecAction = writerThreadExecAction;
			_readerThreadExecAction = readerThreadExecAction;
			_threadConfigs = threadConfigs;
			_setUpTimeLimit = upTimeLimit;
			_execTimeLimit = execTimeLimit;
			_tearDownTimeLimit = tearDownTimeLimit;
		}
	}
}