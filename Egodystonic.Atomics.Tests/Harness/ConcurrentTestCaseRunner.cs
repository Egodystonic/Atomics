// (c) Egodystonic Studios 2018


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.Harness {
	static class ConcurrentTestCaseRunner {
		public struct RunnerFactory<T, TTarget> where TTarget : IAtomic<T>, new() {
			public ConcurrentTestCaseRunner<TTarget> NewRunner() => new ConcurrentTestCaseRunner<TTarget>(() => new TTarget());
			public ConcurrentTestCaseRunner<TTarget> NewRunner(T initialVal) => new ConcurrentTestCaseRunner<TTarget>(() => {
				var result = new TTarget();
				result.Set(initialVal);
				return result;
			});
		}
	}

	sealed partial class ConcurrentTestCaseRunner<T> {
		const int DefaultMaxThreadJoinTimeMillis = 2000;
		readonly TimeSpan _maxThreadJoinTime;
		Func<T> _contextFactoryFunc;

		public TimeSpan MaxSetUpWaitTime { get; set; } = TestCaseRunnerDefaults.DefaultSetUpTimeLimit;
		public TimeSpan MaxExecutionWaitTime { get; set; } = TestCaseRunnerDefaults.DefaultExecutionTimeLimit;
		public TimeSpan MaxTearDownWaitTime { get; set; } = TestCaseRunnerDefaults.DefaultTearDownTimeLimit;

		public Func<T> ContextFactoryFunc {
			get => _contextFactoryFunc;
			set => _contextFactoryFunc = value ?? throw new ArgumentNullException(nameof(value));
		}
		public Action<T, ConcurrentTestCaseThreadConfig> GlobalSetUp { get; set; }

		public Action<T> AllThreadsSetUp { get; set; }
		public Action<T> WriterThreadSetUp { get; set; }
		public Action<T> ReaderThreadSetUp { get; set; }

		public Action<T> AllThreadsTearDown { get; set; }
		public Action<T> WriterThreadTearDown { get; set; }
		public Action<T> ReaderThreadTearDown { get; set; }

		public ConcurrentTestCaseRunner(Func<T> contextFactoryFunc) : this(contextFactoryFunc, TimeSpan.FromMilliseconds(DefaultMaxThreadJoinTimeMillis)) { }

		public ConcurrentTestCaseRunner(Func<T> contextFactoryFunc, TimeSpan maxThreadJoinTime) {
			ContextFactoryFunc = contextFactoryFunc;
			_maxThreadJoinTime = maxThreadJoinTime;
		}

		public void ExecuteCustomTestCase(IConcurrentTestCase<T> testCase) {
			const string StartingCaseMessage = "Starting case: ";
			Console.WriteLine($"{StartingCaseMessage}{testCase.Description}");

			var threadConfig = testCase.ThreadConfig;
			var collectedErrors = new ConcurrentBag<Exception>();
			Action<Exception> errorHandleFunc = e => collectedErrors.Add(e);
			var cancellationTokSource = new CancellationTokenSource();
			var createdThreads = new Thread[threadConfig.TotalThreadCount];

			try {
				var contextObject = ContextFactoryFunc();
				GlobalSetUp?.Invoke(contextObject, threadConfig);
				var barrier = new Barrier(threadConfig.TotalThreadCount + 1);

				for (var i = 0; i < threadConfig.TotalThreadCount; ++i) {
					var threadType = i < threadConfig.WriterThreadCount ? ThreadType.WriterThread : ThreadType.ReaderThread;
					var threadIndexForThreadType = (threadType == ThreadType.WriterThread ? i : i - threadConfig.WriterThreadCount);
					var setupAction = AllThreadsSetUp + (threadType == ThreadType.WriterThread ? WriterThreadSetUp : ReaderThreadSetUp);
					var executionAction = testCase.GetExecutionAction(threadType, threadIndexForThreadType);
					var teardownAction = AllThreadsTearDown + (threadType == ThreadType.WriterThread ? WriterThreadTearDown : ReaderThreadTearDown);
					var threadName = threadType == ThreadType.WriterThread ? $"Writer {threadIndexForThreadType + 1}/{threadConfig.WriterThreadCount}" : $"Reader {threadIndexForThreadType + 1}/{threadConfig.ReaderThreadCount}";
					createdThreads[i] = new Thread(ThreadEntryPoint) { IsBackground = true, Name = threadName };
					createdThreads[i].Start(new TestThreadContext(
						threadName,
						contextObject,
						setupAction,
						executionAction,
						teardownAction,
						barrier,
						cancellationTokSource.Token,
						errorHandleFunc
					));
				}

				var padding = new String(' ', StartingCaseMessage.Length);
				Console.WriteLine($"{padding}{testCase.Description} => Setup...");
				if (!barrier.SignalAndWait(MaxSetUpWaitTime)) throw new TimeoutException($"Setup did not completion within the specified time limit ({MaxSetUpWaitTime}).");
				Console.WriteLine($"{padding}{testCase.Description} => Execution...");
				if (!barrier.SignalAndWait(MaxExecutionWaitTime)) throw new TimeoutException($"Execution did not completion within the specified time limit ({MaxExecutionWaitTime}).");
				Console.WriteLine($"{padding}{testCase.Description} => Tear Down...");
				if (!barrier.SignalAndWait(MaxTearDownWaitTime)) throw new TimeoutException($"Teardown did not completion within the specified time limit ({MaxTearDownWaitTime}).");
				Console.WriteLine($"{padding}{testCase.Description} => Complete!");
			}
			catch (Exception e) {
				collectedErrors.Add(e);
			}
			finally {
				var joinTasks = new List<Task>();
				foreach (var thread in createdThreads) {
					if (thread == null) continue;
					joinTasks.Add(Task.Run(() => {
						var joinSuccess = thread.ThreadState != ThreadState.Unstarted && thread.Join(_maxThreadJoinTime);
						if (!joinSuccess) throw new TimeoutException($"Thread \"{thread.Name}\" could not be joined.");
					}));
				}

				foreach (var task in joinTasks) {
					task.Wait();
					if (!task.IsFaulted) continue;

					foreach (var innerExc in task.Exception.InnerExceptions) collectedErrors.Add(innerExc);
				}

				if (collectedErrors.Any()) {
					foreach (var collectedError in collectedErrors.Where(err => err.Data.Contains(TestThreadContext.ExceptionSourceKey))) {
						Console.WriteLine($"{collectedError.Data[TestThreadContext.ExceptionSourceKey]}: {Environment.NewLine}{collectedError}{Environment.NewLine}");
					}
					throw new AggregateException(collectedErrors);
				}
			}
		}

		static void ThreadEntryPoint(object parametersAsObject) => ((TestThreadContext)parametersAsObject).Execute();
	}
}