// (c) Egodystonic Studios 2018


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.Harness {
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
		public Action<T> GlobalSetUp { get; set; }

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
			Console.WriteLine($"Starting case: {testCase.Description}");

			var threadConfig = testCase.ThreadConfig;
			var collectedErrors = new ConcurrentBag<Exception>();
			Action<Exception> errorHandleFunc = e => collectedErrors.Add(e);
			var cancellationTokSource = new CancellationTokenSource();
			var createdThreads = new Thread[threadConfig.TotalThreadCount];

			try {
				var contextObject = ContextFactoryFunc();
				GlobalSetUp?.Invoke(contextObject);
				var barrier = new Barrier(threadConfig.TotalThreadCount + 1);

				for (var i = 0; i < threadConfig.TotalThreadCount; ++i) {
					var threadType = i < threadConfig.WriterThreadCount ? ThreadType.WriterThread : ThreadType.ReaderThread;
					var threadNumOfThisType = (threadType == ThreadType.WriterThread ? i : threadConfig.WriterThreadCount - i);
					var setupAction = AllThreadsSetUp + (threadType == ThreadType.WriterThread ? WriterThreadSetUp : ReaderThreadSetUp);
					var executionAction = testCase.GetExecutionAction(threadType, threadNumOfThisType);
					var teardownAction = AllThreadsTearDown + (threadType == ThreadType.WriterThread ? WriterThreadTearDown : ReaderThreadTearDown);
					var threadName = threadType == ThreadType.WriterThread ? $"Writer {threadNumOfThisType + 1}/{threadConfig.WriterThreadCount}" : $"Reader {threadNumOfThisType + 1}/{threadConfig.ReaderThreadCount}";
					createdThreads[i] = new Thread(ThreadEntryPoint) { IsBackground = true, Name = threadName };
					createdThreads[i].Start(new ThreadEntryPointParametersWrapper(
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

				if (!barrier.SignalAndWait(MaxSetUpWaitTime)) throw new TimeoutException($"Setup did not completion within the specified time limit ({MaxSetUpWaitTime}).");
				if (!barrier.SignalAndWait(MaxExecutionWaitTime)) throw new TimeoutException($"Execution did not completion within the specified time limit ({MaxExecutionWaitTime}).");
				if (!barrier.SignalAndWait(MaxTearDownWaitTime)) throw new TimeoutException($"Teardown did not completion within the specified time limit ({MaxTearDownWaitTime}).");
			}
			catch (Exception e) {
				collectedErrors.Add(e);
			}
			finally {
				foreach (var thread in createdThreads) {
					var joinSuccess = thread.ThreadState == ThreadState.Unstarted || thread.Join(_maxThreadJoinTime);
					if (!joinSuccess) Console.WriteLine($"Warning: \"{thread.Name}\" can not be joined.");
				}

				if (collectedErrors.Any()) {
					foreach (var collectedError in collectedErrors.Where(err => err.Data.Contains(ThreadEntryPointParametersWrapper.ExceptionSourceKey))) {
						Console.WriteLine($"{collectedError.Data[ThreadEntryPointParametersWrapper.ExceptionSourceKey]} reported error: {Environment.NewLine}{collectedError.Message}{Environment.NewLine}");
					}
					throw new AggregateException(collectedErrors);
				}
			}
		}

		static void ThreadEntryPoint(object parametersAsObject) => ((ThreadEntryPointParametersWrapper)parametersAsObject).Execute();
	}
}