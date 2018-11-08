// (c) Egodystonic Studios 2018


using System;
using Egodystonic.Atomics.Tests.Harness;

namespace Egodystonic.Atomics.Tests.UnitTests.Common {
	abstract class CommonAtomicTestSuite<T, TTarget> where TTarget : IAtomic<T>, new() {
		readonly ConcurrentTestCaseRunner.RunnerFactory<T, TTarget> _runnerFactory = new ConcurrentTestCaseRunner.RunnerFactory<T, TTarget>();

		protected ConcurrentTestCaseRunner<TTarget> NewRunner() => _runnerFactory.NewRunner();
		protected ConcurrentTestCaseRunner<TTarget> NewRunner(T initialValue) => _runnerFactory.NewRunner(initialValue);
	}
}