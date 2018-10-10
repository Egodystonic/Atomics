// (c) Egodystonic Studios 2018


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.Harness {
	sealed partial class ConcurrentTestCaseRunner<T> {
		const int MaxThreadJoinTimeMillis = 2000;

		public Func<T> ContextFactoryFunc { get; set; }
		public Action<T> GlobalSetUp { get; set; }

		public Action<T> AllThreadsSetUp { get; set; }
		public Action<T> WriterThreadSetUp { get; set; }
		public Action<T> ReaderThreadSetUp { get; set; }

		public Action<T> AllThreadsTearDown { get; set; }
		public Action<T> WriterThreadTearDown { get; set; }
		public Action<T> ReaderThreadTearDown { get; set; }

		void ExecuteTestCase(IConcurrentTestCase<T> testCase) {
			Console.WriteLine($"STARTING TEST CASE: {testCase.Description}");

			var targetThreadConfigs = testCase.GetAllTargetThreadConfigs().ToArray();

			for (var iterationIndex = 0; iterationIndex < targetThreadConfigs.Length; ++iterationIndex) {
				var threadConfig = targetThreadConfigs[iterationIndex];
				Console.WriteLine($"Starting '{testCase.Description}' iteration {iterationIndex + 1}/{targetThreadConfigs.Length}: " +
								  $"{threadConfig.WriterThreadCount} writers, {threadConfig.ReaderThreadCount} readers...");
				var collectedErrors = new ConcurrentBag<Exception>();
				Action<Exception> errorHandleFunc = e => collectedErrors.Add(e);
				var cancellationTokSource = new CancellationTokenSource();
				var createdThreads = new Thread[threadConfig.TotalThreadCount];

				try {
					var contextObject = ContextFactoryFunc != null ? ContextFactoryFunc() : default;
					GlobalSetUp?.Invoke(contextObject);
					var barrier = new Barrier(threadConfig.TotalThreadCount + 1);

					for (var i = 0; i < threadConfig.TotalThreadCount; ++i) {
						var threadType = i < threadConfig.WriterThreadCount ? ThreadType.WriterThread : ThreadType.ReaderThread;
						var threadNumOfThisType = (threadType == ThreadType.WriterThread ? i : threadConfig.WriterThreadCount - i);
						var setupAction = AllThreadsSetUp + (threadType == ThreadType.WriterThread ? WriterThreadSetUp : ReaderThreadSetUp);
						var executionAction = testCase.GetExecutionAction(threadType, threadNumOfThisType, threadConfig);
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

					if (!barrier.SignalAndWait(testCase.MaxSetUpWaitTime)) throw new TimeoutException($"Setup did not completion within the specified time limit ({testCase.MaxSetUpWaitTime}).");
					if (!barrier.SignalAndWait(testCase.MaxExecutionWaitTime)) throw new TimeoutException($"Execution did not completion within the specified time limit ({testCase.MaxExecutionWaitTime}).");
					if (!barrier.SignalAndWait(testCase.MaxTearDownWaitTime)) throw new TimeoutException($"Teardown did not completion within the specified time limit ({testCase.MaxTearDownWaitTime}).");
				}
				catch (Exception e) {
					collectedErrors.Add(e);
				}
				finally {
					foreach (var thread in createdThreads) {
						var joinSuccess = thread.ThreadState == ThreadState.Unstarted || thread.Join(MaxThreadJoinTimeMillis);
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

			Console.WriteLine($"TEST CASE ENDED: {testCase.Description}");
		}

		static void ThreadEntryPoint(object parametersAsObject) => ((ThreadEntryPointParametersWrapper)parametersAsObject).Execute();
	}
}