// (c) Egodystonic Studios 2018


using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.Harness.HarnessTests {
	[TestFixture]
	class ConcurrentTestCaseRunnerTest { // I heard you like tests so we put tests in your tests
		#region Test Fields
		readonly TimeSpan MaxThreadJoinTime = TimeSpan.FromMilliseconds(50);
		ConcurrentTestCaseRunner<DummyImmutableRef> _runner;
		#endregion

		#region Test Setup
		[OneTimeSetUp]
		public void SetUpClass() { }

		[OneTimeTearDown]
		public void TearDownClass() { }

		[SetUp]
		public void SetUpTest() {
			_runner = new ConcurrentTestCaseRunner<DummyImmutableRef>(() => new DummyImmutableRef(), MaxThreadJoinTime);
		}

		[TearDown]
		public void TearDownTest() { }

		static IConcurrentTestCase<DummyImmutableRef> CreateNewTestCase() {
			return new StandardConcurrentTestCase<DummyImmutableRef>(
				"No-Operation Test Case",
				null,
				null,
				new ConcurrentTestCaseThreadConfig(1, 0)
			);
		}

		static IConcurrentTestCase<DummyImmutableRef> CreateNewTestCase(Action<DummyImmutableRef> execAction) {
			return new StandardConcurrentTestCase<DummyImmutableRef>(
				"Custom Test Case",
				execAction,
				null,
				new ConcurrentTestCaseThreadConfig(1, 0)
			);
		}

		static IConcurrentTestCase<DummyImmutableRef> CreateNewTestCase(ConcurrentTestCaseThreadConfig threadConfig) {
			return new StandardConcurrentTestCase<DummyImmutableRef>(
				"No-Operation Test Case",
				null,
				null,
				threadConfig
			);
		}

		static IConcurrentTestCase<DummyImmutableRef> CreateNewTestCase(Action<DummyImmutableRef> writerAction, Action<DummyImmutableRef> readerAction) {
			return new StandardConcurrentTestCase<DummyImmutableRef>(
				"Custom Test Case",
				writerAction,
				readerAction,
				new ConcurrentTestCaseThreadConfig(1, 1)
			);
		}

		static IConcurrentTestCase<DummyImmutableRef> CreateNewTestCase(Action<DummyImmutableRef> writerAction, Action<DummyImmutableRef> readerAction, ConcurrentTestCaseThreadConfig threadConfig) {
			return new StandardConcurrentTestCase<DummyImmutableRef>(
				"Custom Test Case",
				writerAction,
				readerAction,
				threadConfig
			);
		}
		#endregion

		#region Tests
		[Test]
		public void ShouldRespectCustomWaitTimes() {
			const int CustomTimeLimitMillis = 50;

			var sleepingTestCase = CreateNewTestCase(_ => Thread.Sleep(CustomTimeLimitMillis * 20));
			var noopTestCase = CreateNewTestCase();
			_runner.ContextFactoryFunc = () => new DummyImmutableRef();


			_runner.MaxSetUpWaitTime = TimeSpan.FromMilliseconds(CustomTimeLimitMillis);
			_runner.AllThreadsSetUp = _ => Thread.Sleep(CustomTimeLimitMillis * 20);
			Assert.Throws<AggregateException>(() => _runner.ExecuteCustomTestCase(noopTestCase));

			_runner.MaxSetUpWaitTime = TestCaseRunnerDefaults.DefaultSetUpTimeLimit;
			_runner.AllThreadsSetUp = _ => { };
			Assert.DoesNotThrow(() => _runner.ExecuteCustomTestCase(noopTestCase));


			_runner.MaxExecutionWaitTime = TimeSpan.FromMilliseconds(CustomTimeLimitMillis);
			Assert.Throws<AggregateException>(() => _runner.ExecuteCustomTestCase(sleepingTestCase));

			_runner.MaxExecutionWaitTime = TestCaseRunnerDefaults.DefaultExecutionTimeLimit;
			Assert.DoesNotThrow(() => _runner.ExecuteCustomTestCase(sleepingTestCase));


			_runner.MaxTearDownWaitTime = TimeSpan.FromMilliseconds(CustomTimeLimitMillis);
			_runner.AllThreadsTearDown = _ => Thread.Sleep(CustomTimeLimitMillis * 20);
			Assert.Throws<AggregateException>(() => _runner.ExecuteCustomTestCase(noopTestCase));

			_runner.MaxTearDownWaitTime = TestCaseRunnerDefaults.DefaultTearDownTimeLimit;
			_runner.AllThreadsTearDown = _ => { };
			Assert.DoesNotThrow(() => _runner.ExecuteCustomTestCase(noopTestCase));
		}

		[Test]
		public void ShouldCreateNewContextObjectForEachTestCase() {
			_runner.ContextFactoryFunc = () => new DummyImmutableRef();
			var lastSetContextObject = new LockFreeReference<DummyImmutableRef>(null);
			var testCase = CreateNewTestCase(ctx => lastSetContextObject.Set(ctx));

			_runner.ExecuteCustomTestCase(testCase);
			var firstContextObj = lastSetContextObject.Value;
			_runner.ExecuteCustomTestCase(testCase);
			Assert.AreNotEqual(firstContextObj, lastSetContextObject.Value);
		}

		[Test]
		public void ShouldInvokeGlobalSetUpBeforeEachTestCase() {
			var globalSetUpInvocationCount = new LockFreeInt32(0);
			_runner.GlobalSetUp = (_, __) => globalSetUpInvocationCount.Increment();
			var testCase = CreateNewTestCase();

			_runner.ExecuteCustomTestCase(testCase);
			_runner.ExecuteCustomTestCase(testCase);
			_runner.ExecuteCustomTestCase(testCase);
			Assert.AreEqual(3, globalSetUpInvocationCount.Value);
		}

		[Test]
		public void ShouldInvokeSetUpFunctionsOnRelevantThreads() {
			var allThreadsSetUpInvokedThreads = new ConcurrentBag<Thread>();
			var writerThreadsSetUpInvokedThreads = new ConcurrentBag<Thread>();
			var readerThreadsSetUpInvokedThreads = new ConcurrentBag<Thread>();
			var threadConfig = new ConcurrentTestCaseThreadConfig(8, 16);
			var testCase = CreateNewTestCase(threadConfig);

			_runner.AllThreadsSetUp = _ => allThreadsSetUpInvokedThreads.Add(Thread.CurrentThread);
			_runner.WriterThreadSetUp = _ => writerThreadsSetUpInvokedThreads.Add(Thread.CurrentThread);
			_runner.ReaderThreadSetUp = _ => readerThreadsSetUpInvokedThreads.Add(Thread.CurrentThread);
			_runner.ExecuteCustomTestCase(testCase);

			Assert.AreEqual(threadConfig.TotalThreadCount, allThreadsSetUpInvokedThreads.Count);
			Assert.AreEqual(threadConfig.WriterThreadCount, writerThreadsSetUpInvokedThreads.Count);
			Assert.AreEqual(threadConfig.ReaderThreadCount, readerThreadsSetUpInvokedThreads.Count);
		}

		[Test]
		public void ShouldInvokeTearDownFunctionsOnRelevantThreads() {
			var allThreadsTearDownInvokedThreads = new ConcurrentBag<Thread>();
			var writerThreadsTearDownInvokedThreads = new ConcurrentBag<Thread>();
			var readerThreadsTearDownInvokedThreads = new ConcurrentBag<Thread>();
			var threadConfig = new ConcurrentTestCaseThreadConfig(8, 16);
			var testCase = CreateNewTestCase(threadConfig);

			_runner.AllThreadsTearDown = _ => allThreadsTearDownInvokedThreads.Add(Thread.CurrentThread);
			_runner.WriterThreadTearDown = _ => writerThreadsTearDownInvokedThreads.Add(Thread.CurrentThread);
			_runner.ReaderThreadTearDown = _ => readerThreadsTearDownInvokedThreads.Add(Thread.CurrentThread);
			_runner.ExecuteCustomTestCase(testCase);

			Assert.AreEqual(threadConfig.TotalThreadCount, allThreadsTearDownInvokedThreads.Count);
			Assert.AreEqual(threadConfig.WriterThreadCount, writerThreadsTearDownInvokedThreads.Count);
			Assert.AreEqual(threadConfig.ReaderThreadCount, readerThreadsTearDownInvokedThreads.Count);
		}

		[Test]
		public void ShouldCreateCorrectNumberOfThreads() {
			var writerThreadBag = new ConcurrentBag<Thread>();
			var readerThreadBag = new ConcurrentBag<Thread>();
			var threadConfig = new ConcurrentTestCaseThreadConfig(8, 16);
			var testCase = CreateNewTestCase(_ => writerThreadBag.Add(Thread.CurrentThread), _ => readerThreadBag.Add(Thread.CurrentThread), threadConfig);

			_runner.ExecuteCustomTestCase(testCase);

			Assert.AreEqual(threadConfig.WriterThreadCount, writerThreadBag.Distinct().Count());
			Assert.AreEqual(threadConfig.ReaderThreadCount, readerThreadBag.Distinct().Count());
		}

		[Test]
		public void ShouldCorrectlySerializeTestCase() {
			var setUpThreadBag = new ConcurrentBag<Thread>();
			var execThreadBag = new ConcurrentBag<Thread>();
			var tearDownThreadBag = new ConcurrentBag<Thread>();

			var setUpEvent = new ManualResetEvent(false);
			var execEvent = new ManualResetEvent(false);

			var threadConfig = new ConcurrentTestCaseThreadConfig(16, 0);
			_runner.AllThreadsSetUp = _ => {
				setUpThreadBag.Add(Thread.CurrentThread);
				// Force one thread (#11) to wait for the sake of the test
				if (Thread.CurrentThread.Name?.Contains("11", StringComparison.InvariantCultureIgnoreCase) ?? false) setUpEvent.WaitOne();
			};
			var testCase = CreateNewTestCase(
				_ => {
					execThreadBag.Add(Thread.CurrentThread);
					// Force one thread (#11) to wait for the sake of the test
					if (Thread.CurrentThread.Name?.Contains("11", StringComparison.InvariantCultureIgnoreCase) ?? false) execEvent.WaitOne();
				}, 
				null, 
				threadConfig
			);
			_runner.AllThreadsTearDown = _ => {
				tearDownThreadBag.Add(Thread.CurrentThread);
			};

			var runTask = Task.Run(() => _runner.ExecuteCustomTestCase(testCase));

			SpinWait.SpinUntil(() => setUpThreadBag.Count == threadConfig.TotalThreadCount);
			Thread.Sleep(250); // Just to make sure we're not getting a false positive by beating other threads to adding themselves to the bag
			Assert.AreEqual(0, execThreadBag.Count);
			setUpEvent.Set();

			SpinWait.SpinUntil(() => execThreadBag.Count == threadConfig.TotalThreadCount);
			Thread.Sleep(250); // Just to make sure we're not getting a false positive by beating other threads to adding themselves to the bag
			Assert.AreEqual(0, tearDownThreadBag.Count);
			execEvent.Set();

			runTask.Wait();
			Assert.AreEqual(threadConfig.TotalThreadCount, tearDownThreadBag.Count);
		}

		[Test]
		public void ShouldCorrectlyAggregateAndCollectExceptions() {
			var threadConfig = new ConcurrentTestCaseThreadConfig(16, 16);
			var testCase = new MockConcurrentTestCase<DummyImmutableRef> {
				ThreadConfig = threadConfig,
				OnGetExecutionAction = (threadType, threadIndex) => _ => throw new ApplicationException(threadType.ToString() + threadIndex + "/")
			};

			try {
				_runner.ExecuteCustomTestCase(testCase);
				Assert.Fail("No exception was thrown.");
			}
			catch (AggregateException e) {
				Assert.GreaterOrEqual(e.InnerExceptions.Count, threadConfig.TotalThreadCount);

				for (var w = 0; w < threadConfig.WriterThreadCount; ++w) {
					Assert.True(e.InnerExceptions.Count(ex => ex.ToString().Contains(ThreadType.WriterThread.ToString() + w + "/", StringComparison.InvariantCultureIgnoreCase)) == 1);
				}
				for (var r = 0; r < threadConfig.ReaderThreadCount; ++r) {
					Assert.True(e.InnerExceptions.Count(ex => ex.ToString().Contains(ThreadType.ReaderThread.ToString() + r + "/", StringComparison.InvariantCultureIgnoreCase)) == 1);
				}
			}
		}
		#endregion Tests
	}
}