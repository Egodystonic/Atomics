// (c) Egodystonic Studios 2018


using System;
using System.Collections.Generic;
using System.Linq;

namespace Egodystonic.Atomics.Tests.Harness.HarnessTests {
	sealed class MockConcurrentTestCase<T> : IConcurrentTestCase<T> {
		public Func<ThreadType, int, Action<T>> OnGetExecutionAction;

		public string Description { get; set; } = "Mock Case";
		public ConcurrentTestCaseThreadConfig ThreadConfig { get; set; } = new ConcurrentTestCaseThreadConfig(1, 0);
		public Action<T> GetExecutionAction(ThreadType threadType, int threadIndex) {
			if (OnGetExecutionAction != null) return OnGetExecutionAction(threadType, threadIndex);
			return _ => { };
		}
	}
}