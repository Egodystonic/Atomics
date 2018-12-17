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
		public static ConcurrentTestCaseThreadConfig[] DefaultWriterReaderThreadConfigs = {
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
		public static ConcurrentTestCaseThreadConfig[] DefaultSingleWriterThreadConfigs = {
			new ConcurrentTestCaseThreadConfig(1, 1),
			new ConcurrentTestCaseThreadConfig(1, 2),
			new ConcurrentTestCaseThreadConfig(1, 4),
			new ConcurrentTestCaseThreadConfig(1, 8),
			new ConcurrentTestCaseThreadConfig(1, 16),
			new ConcurrentTestCaseThreadConfig(1, 32)
		};
		public static ConcurrentTestCaseThreadConfig[] DefaultSingleReaderThreadConfigs = {
			new ConcurrentTestCaseThreadConfig(1, 1),
			new ConcurrentTestCaseThreadConfig(2, 1),
			new ConcurrentTestCaseThreadConfig(4, 1),
			new ConcurrentTestCaseThreadConfig(8, 1),
			new ConcurrentTestCaseThreadConfig(16, 1),
			new ConcurrentTestCaseThreadConfig(32, 1)
		};
		public static ConcurrentTestCaseThreadConfig[] DefaultSingleWriterSingleReaderThreadConfigs = {
			new ConcurrentTestCaseThreadConfig(1, 1)
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

		public void ExecuteWriterReaderTests(Action<T> writerFunction, Action<T> readerFunction) {
			foreach (var threadConfig in DefaultWriterReaderThreadConfigs) {
				ExecuteCustomTestCase(new StandardConcurrentTestCase<T>(
					$"Writer-Reader; {threadConfig.WriterThreadCount} writers, {threadConfig.ReaderThreadCount} readers",
					writerFunction,
					readerFunction,
					threadConfig
				));
			}
		}

		public void ExecuteWriterReaderTests(Action<T> writerFunctionSingleIteration, Action<T> readerFunctionSingleIteration, int totalIterationCount) {
			foreach (var threadConfig in DefaultWriterReaderThreadConfigs) {
				ExecuteCustomTestCase(new IterativeConcurrentTestCase<T>(
					$"Iterated Writer-Reader (n = {totalIterationCount}); {threadConfig.WriterThreadCount} writers, {threadConfig.ReaderThreadCount} readers",
					writerFunctionSingleIteration,
					readerFunctionSingleIteration,
					threadConfig,
					totalIterationCount,
					true,
					true
				));
			}
		}

		public void ExecuteWriterReaderTests(Action<T> writerFunctionSingleIteration, Action<T> readerFunctionSingleIteration, int totalIterationCount, bool iterateWriterFunc, bool iterateReaderFunc) {
			foreach (var threadConfig in DefaultWriterReaderThreadConfigs) {
				ExecuteCustomTestCase(new IterativeConcurrentTestCase<T>(
					$"Iterated Writer-Reader (n = {totalIterationCount}); {threadConfig.WriterThreadCount} writers{(iterateWriterFunc ? String.Empty : " (non-iterated)")}, {threadConfig.ReaderThreadCount} readers{(iterateReaderFunc ? String.Empty : " (non-iterated)")}",
					writerFunctionSingleIteration,
					readerFunctionSingleIteration,
					threadConfig,
					totalIterationCount,
					iterateWriterFunc,
					iterateReaderFunc
				));
			}
		}

		public void ExecuteSingleWriterTests(Action<T> writerFunction, Action<T> readerFunction) {
			foreach (var threadConfig in DefaultSingleWriterThreadConfigs) {
				ExecuteCustomTestCase(new StandardConcurrentTestCase<T>(
					$"Single-Writer; {threadConfig.ReaderThreadCount} readers",
					writerFunction,
					readerFunction,
					threadConfig
				));
			}
		}

		public void ExecuteSingleWriterTests(Action<T> writerFunctionSingleIteration, Action<T> readerFunctionSingleIteration, int totalIterationCount) {
			foreach (var threadConfig in DefaultSingleWriterThreadConfigs) {
				ExecuteCustomTestCase(new IterativeConcurrentTestCase<T>(
					$"Iterated Single-Writer (n = {totalIterationCount}); {threadConfig.ReaderThreadCount} readers",
					writerFunctionSingleIteration,
					readerFunctionSingleIteration,
					threadConfig,
					totalIterationCount,
					true,
					true
				));
			}
		}

		public void ExecuteSingleWriterTests(Action<T> writerFunctionSingleIteration, Action<T> readerFunctionSingleIteration, int totalIterationCount, bool iterateWriterFunc, bool iterateReaderFunc) {
			foreach (var threadConfig in DefaultSingleWriterThreadConfigs) {
				ExecuteCustomTestCase(new IterativeConcurrentTestCase<T>(
					$"Iterated Single-Writer (n = {totalIterationCount}); {(iterateWriterFunc ? String.Empty : "(non-iterated writer)")}, {threadConfig.ReaderThreadCount} readers{(iterateReaderFunc ? String.Empty : " (non-iterated)")}",
					writerFunctionSingleIteration,
					readerFunctionSingleIteration,
					threadConfig,
					totalIterationCount,
					iterateWriterFunc,
					iterateReaderFunc
				));
			}
		}

		public void ExecuteSingleReaderTests(Action<T> writerFunction, Action<T> readerFunction) {
			foreach (var threadConfig in DefaultSingleReaderThreadConfigs) {
				ExecuteCustomTestCase(new StandardConcurrentTestCase<T>(
					$"Single-Reader; {threadConfig.WriterThreadCount} writers",
					writerFunction,
					readerFunction,
					threadConfig
				));
			}
		}

		public void ExecuteSingleReaderTests(Action<T> writerFunctionSingleIteration, Action<T> readerFunctionSingleIteration, int totalIterationCount) {
			foreach (var threadConfig in DefaultSingleReaderThreadConfigs) {
				ExecuteCustomTestCase(new IterativeConcurrentTestCase<T>(
					$"Iterated Single-Reader (n = {totalIterationCount}); {threadConfig.WriterThreadCount} writers",
					writerFunctionSingleIteration,
					readerFunctionSingleIteration,
					threadConfig,
					totalIterationCount,
					true,
					true
				));
			}
		}

		public void ExecuteSingleReaderTests(Action<T> writerFunctionSingleIteration, Action<T> readerFunctionSingleIteration, int totalIterationCount, bool iterateWriterFunc, bool iterateReaderFunc) {
			foreach (var threadConfig in DefaultSingleReaderThreadConfigs) {
				ExecuteCustomTestCase(new IterativeConcurrentTestCase<T>(
					$"Iterated Single-Reader (n = {totalIterationCount}); {(iterateReaderFunc ? String.Empty : "(non-iterated reader)")}, {threadConfig.WriterThreadCount} writers{(iterateWriterFunc ? String.Empty : " (non-iterated)")}",
					writerFunctionSingleIteration,
					readerFunctionSingleIteration,
					threadConfig,
					totalIterationCount,
					iterateWriterFunc,
					iterateReaderFunc
				));
			}
		}

		public void ExecuteSingleWriterSingleReaderTests(Action<T> writerFunction, Action<T> readerFunction) {
			foreach (var threadConfig in DefaultSingleWriterSingleReaderThreadConfigs) {
				ExecuteCustomTestCase(new StandardConcurrentTestCase<T>(
					$"Single-Writer / Single-Reader",
					writerFunction,
					readerFunction,
					threadConfig
				));
			}
		}

		public void ExecuteSingleWriterSingleReaderTests(Action<T> writerFunctionSingleIteration, Action<T> readerFunctionSingleIteration, int totalIterationCount) {
			foreach (var threadConfig in DefaultSingleWriterSingleReaderThreadConfigs) {
				ExecuteCustomTestCase(new IterativeConcurrentTestCase<T>(
					$"Iterated Single-Writer / Single-Reader (n = {totalIterationCount})",
					writerFunctionSingleIteration,
					readerFunctionSingleIteration,
					threadConfig,
					totalIterationCount,
					true,
					true
				));
			}
		}

		public void ExecuteSingleWriterSingleReaderTests(Action<T> writerFunctionSingleIteration, Action<T> readerFunctionSingleIteration, int totalIterationCount, bool iterateWriterFunc, bool iterateReaderFunc) {
			foreach (var threadConfig in DefaultSingleWriterSingleReaderThreadConfigs) {
				ExecuteCustomTestCase(new IterativeConcurrentTestCase<T>(
					$"Iterated Single-Writer / Single-Reader (n = {totalIterationCount}); {(iterateReaderFunc ? String.Empty : "(non-iterated reader)")}, {threadConfig.WriterThreadCount} writers{(iterateWriterFunc ? String.Empty : " (non-iterated)")}",
					writerFunctionSingleIteration,
					readerFunctionSingleIteration,
					threadConfig,
					totalIterationCount,
					iterateWriterFunc,
					iterateReaderFunc
				));
			}
		}

		public void ExecuteContinuousCoherencyTests<U>(Action<T> execFunction, Func<T, U> valueExtractionFunc, Action<U, U> coherencyAssertionAction, Func<U, bool> readerCompletionPredicate) {
			foreach (var threadConfig in DefaultWriterReaderThreadConfigs) {
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

		public void ExecuteContinuousCoherencyTests<U>(Action<T> execFunctionSingleIteration, int totalIterationCount, Func<T, U> valueExtractionFunc, Action<U, U> coherencyAssertionAction) {
			foreach (var threadConfig in DefaultWriterReaderThreadConfigs) {
				var writerCompletionCounter = new CountdownEvent(threadConfig.WriterThreadCount);

				ExecuteCustomTestCase(new IterativeConcurrentTestCase<T>(
					$"Iterated Continuous Coherency (n = {totalIterationCount}); {threadConfig.WriterThreadCount} writers, {threadConfig.ReaderThreadCount} coherency spinners",
					execFunctionSingleIteration,
					ctx => {
						var spinner = new SpinWait();
						var prevValue = valueExtractionFunc(ctx);
						while (!writerCompletionCounter.IsSet) {
							var curValue = valueExtractionFunc(ctx);
							coherencyAssertionAction(prevValue, curValue);
							prevValue = curValue;
							spinner.SpinOnce();
						}
					},
					threadConfig,
					totalIterationCount,
					iterateWriters: true,
					iterateReaders: false,
					() => writerCompletionCounter.Signal(),
					null
				));
			}
		}

		public void ExecuteContinuousCoherencyTests<U>(Action<T> execFunctionSingleIteration, int totalIterationCount, Func<T, U> valueExtractionFunc, Action<U, U> coherencyAssertionAction, Func<U, bool> readerCompletionPredicate) {
			foreach (var threadConfig in DefaultWriterReaderThreadConfigs) {
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

		public void ExecuteContinuousSingleWriterCoherencyTests<U>(Action<T> execFunction, Func<T, U> valueExtractionFunc, Action<U, U> coherencyAssertionAction, Func<U, bool> readerCompletionPredicate) {
			foreach (var threadConfig in DefaultSingleWriterThreadConfigs) {
				ExecuteCustomTestCase(new StandardConcurrentTestCase<T>(
					$"Continuous Single-Writer Coherency; {threadConfig.ReaderThreadCount} coherency spinners",
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

		public void ExecuteContinuousSingleWriterCoherencyTests<U>(Action<T> execFunctionSingleIteration, int totalIterationCount, Func<T, U> valueExtractionFunc, Action<U, U> coherencyAssertionAction) {
			foreach (var threadConfig in DefaultSingleWriterThreadConfigs) {
				var writerCompletionCounter = new CountdownEvent(threadConfig.WriterThreadCount);

				ExecuteCustomTestCase(new IterativeConcurrentTestCase<T>(
					$"Iterated Single-Writer Continuous Coherency (n = {totalIterationCount}); {threadConfig.ReaderThreadCount} coherency spinners",
					execFunctionSingleIteration,
					ctx => {
						var spinner = new SpinWait();
						var prevValue = valueExtractionFunc(ctx);
						while (!writerCompletionCounter.IsSet) {
							var curValue = valueExtractionFunc(ctx);
							coherencyAssertionAction(prevValue, curValue);
							prevValue = curValue;
							spinner.SpinOnce();
						}
					},
					threadConfig,
					totalIterationCount,
					iterateWriters: true,
					iterateReaders: false,
					() => writerCompletionCounter.Signal(),
					null
				));
			}
		}

		public void ExecuteContinuousSingleWriterCoherencyTests<U>(Action<T> execFunctionSingleIteration, int totalIterationCount, Func<T, U> valueExtractionFunc, Action<U, U> coherencyAssertionAction, Func<U, bool> readerCompletionPredicate) {
			foreach (var threadConfig in DefaultSingleWriterThreadConfigs) {
				ExecuteCustomTestCase(new IterativeConcurrentTestCase<T>(
					$"Iterated Single-Writer Continuous Coherency (n = {totalIterationCount}); {threadConfig.ReaderThreadCount} coherency spinners",
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

		public void ExecuteContinuousSingleReaderCoherencyTests<U>(Action<T> execFunction, Func<T, U> valueExtractionFunc, Action<U, U> coherencyAssertionAction, Func<U, bool> readerCompletionPredicate) {
			foreach (var threadConfig in DefaultSingleReaderThreadConfigs) {
				ExecuteCustomTestCase(new StandardConcurrentTestCase<T>(
					$"Continuous Single-Reader Coherency; {threadConfig.WriterThreadCount} writers",
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

		public void ExecuteContinuousSingleReaderCoherencyTests<U>(Action<T> execFunctionSingleIteration, int totalIterationCount, Func<T, U> valueExtractionFunc, Action<U, U> coherencyAssertionAction) {
			foreach (var threadConfig in DefaultSingleReaderThreadConfigs) {
				var writerCompletionCounter = new CountdownEvent(threadConfig.WriterThreadCount);

				ExecuteCustomTestCase(new IterativeConcurrentTestCase<T>(
					$"Iterated Single-Reader Continuous Coherency (n = {totalIterationCount}); {threadConfig.WriterThreadCount} writers",
					execFunctionSingleIteration,
					ctx => {
						var spinner = new SpinWait();
						var prevValue = valueExtractionFunc(ctx);
						while (!writerCompletionCounter.IsSet) {
							var curValue = valueExtractionFunc(ctx);
							coherencyAssertionAction(prevValue, curValue);
							prevValue = curValue;
							spinner.SpinOnce();
						}
					},
					threadConfig,
					totalIterationCount,
					iterateWriters: true,
					iterateReaders: false,
					() => writerCompletionCounter.Signal(),
					null
				));
			}
		}

		public void ExecuteContinuousSingleReaderCoherencyTests<U>(Action<T> execFunctionSingleIteration, int totalIterationCount, Func<T, U> valueExtractionFunc, Action<U, U> coherencyAssertionAction, Func<U, bool> readerCompletionPredicate) {
			foreach (var threadConfig in DefaultSingleReaderThreadConfigs) {
				ExecuteCustomTestCase(new IterativeConcurrentTestCase<T>(
					$"Iterated Single-Reader Continuous Coherency (n = {totalIterationCount}); {threadConfig.WriterThreadCount} writers",
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