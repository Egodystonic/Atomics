// (c) Egodystonic Studios 2018


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Egodystonic.Atomics.Tests.Harness.TestCaseRunnerDefaults;

namespace Egodystonic.Atomics.Tests.Harness {
	static class TestCaseRunnerDefaults {
		public static readonly TimeSpan DefaultSetUpTimeLimit = TimeSpan.FromSeconds(5d);
		public static readonly TimeSpan DefaultTearDownTimeLimit = TimeSpan.FromSeconds(5d);
		public static readonly TimeSpan DefaultExecutionTimeLimit = TimeSpan.FromSeconds(10d);

		public static ConcurrentTestCaseThreadConfig[] DefaultFreeThreadedThreadConfigs = {
			new ConcurrentTestCaseThreadConfig(1, 0),
			new ConcurrentTestCaseThreadConfig(2, 0),
			new ConcurrentTestCaseThreadConfig(4, 0),
			new ConcurrentTestCaseThreadConfig(8, 0),
			new ConcurrentTestCaseThreadConfig(16, 0),
			new ConcurrentTestCaseThreadConfig(32, 0)
		};
		public static ConcurrentTestCaseThreadConfig[] DefaultReaderWriterThreadConfigs = {
			new ConcurrentTestCaseThreadConfig(1, 1),
			new ConcurrentTestCaseThreadConfig(2, 1),
			new ConcurrentTestCaseThreadConfig(4, 1),
			new ConcurrentTestCaseThreadConfig(8, 1),
			new ConcurrentTestCaseThreadConfig(16, 1),
			new ConcurrentTestCaseThreadConfig(32, 1),

			new ConcurrentTestCaseThreadConfig(1, 2),
			new ConcurrentTestCaseThreadConfig(2, 2),
			new ConcurrentTestCaseThreadConfig(4, 2),
			new ConcurrentTestCaseThreadConfig(8, 2),
			new ConcurrentTestCaseThreadConfig(16, 2),
			new ConcurrentTestCaseThreadConfig(32, 2),

			new ConcurrentTestCaseThreadConfig(1, 4),
			new ConcurrentTestCaseThreadConfig(2, 4),
			new ConcurrentTestCaseThreadConfig(4, 4),
			new ConcurrentTestCaseThreadConfig(8, 4),
			new ConcurrentTestCaseThreadConfig(16, 4),
			new ConcurrentTestCaseThreadConfig(32, 4),

			new ConcurrentTestCaseThreadConfig(1, 8),
			new ConcurrentTestCaseThreadConfig(2, 8),
			new ConcurrentTestCaseThreadConfig(4, 8),
			new ConcurrentTestCaseThreadConfig(8, 8),
			new ConcurrentTestCaseThreadConfig(16, 8),
			new ConcurrentTestCaseThreadConfig(32, 8),

			new ConcurrentTestCaseThreadConfig(1, 16),
			new ConcurrentTestCaseThreadConfig(2, 16),
			new ConcurrentTestCaseThreadConfig(4, 16),
			new ConcurrentTestCaseThreadConfig(8, 16),
			new ConcurrentTestCaseThreadConfig(16, 16),
			new ConcurrentTestCaseThreadConfig(32, 16),

			new ConcurrentTestCaseThreadConfig(1, 32),
			new ConcurrentTestCaseThreadConfig(2, 32),
			new ConcurrentTestCaseThreadConfig(4, 32),
			new ConcurrentTestCaseThreadConfig(8, 32),
			new ConcurrentTestCaseThreadConfig(16, 32),
			new ConcurrentTestCaseThreadConfig(32, 32),
		};
	}

	sealed partial class ConcurrentTestCaseRunner<T> {
		public void ExecuteFreeThreadedTests(Action<T> execFunction) {
			foreach (var threadConfig in DefaultFreeThreadedThreadConfigs) {
				ExecuteCustomTestCase(new StandardConcurrentTestCase<T>(
					$"Free-Threaded; {threadConfig.TotalThreadCount} threads",
					execFunction,
					null,
					threadConfig
				));
			}
		}

		public void ExecuteFreeThreadedTests(Action<T> execFunctionSingleIteration, int totalIterationCount) {
			foreach (var threadConfig in DefaultFreeThreadedThreadConfigs) {
				ExecuteCustomTestCase(new IterativeConcurrentTestCase<T>(
					$"Iterated Free-Threaded (n = {totalIterationCount}); {threadConfig.TotalThreadCount} threads",
					execFunctionSingleIteration,
					null,
					threadConfig,
					totalIterationCount,
					iterateWriters: true,
					iterateReaders: false
				));
			}
		}

		public void ExecuteContinuousCoherencyTests<U>(Action<T> execFunction, Func<T, U> valueExtractionFunc, Action<U, U> coherencyAssertionAction, Func<U, bool> readerCompletionPredicate, TimeSpan setUpTimeLimit, TimeSpan execTimeLimit, TimeSpan tearDownTimeLimit) {
			foreach (var threadConfig in DefaultReaderWriterThreadConfigs) {
				ExecuteCustomTestCase(new StandardConcurrentTestCase<T>(
					$"Continuous Coherency; {threadConfig.WriterThreadCount} writers, {threadConfig.ReaderThreadCount} coherency spinners",
					execFunction,
					ctx => {
						var spinner = new SpinWait();
						var prevValue = valueExtractionFunc(ctx);
						while (true) {
							var curValue = valueExtractionFunc(ctx);
							coherencyAssertionAction(prevValue, curValue);
							if (readerCompletionPredicate(curValue)) break;
							prevValue = curValue;
							spinner.SpinOnce();
						}
					},
					threadConfig
				));
			}
		}

		public void ExecuteContinuousCoherencyTests<U>(Action<T> execFunctionSingleIteration, int totalIterationCount, Func<T, U> valueExtractionFunc, Action<U, U> coherencyAssertionAction, Func<U, bool> readerCompletionPredicate) {
			foreach (var threadConfig in DefaultReaderWriterThreadConfigs) {
				ExecuteCustomTestCase(new IterativeConcurrentTestCase<T>(
					$"Iterated Continuous Coherency (n = {totalIterationCount}); {threadConfig.WriterThreadCount} writers, {threadConfig.ReaderThreadCount} coherency spinners",
					execFunctionSingleIteration,
					ctx => {
						var spinner = new SpinWait();
						var prevValue = valueExtractionFunc(ctx);
						while (true) {
							var curValue = valueExtractionFunc(ctx);
							coherencyAssertionAction(prevValue, curValue);
							if (readerCompletionPredicate(curValue)) break;
							prevValue = curValue;
							spinner.SpinOnce();
						}
					},
					threadConfig,
					totalIterationCount,
					iterateWriters: true,
					iterateReaders: false
				));
			}
		}
	}
}