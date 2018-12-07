using System;
using System.Diagnostics;
using System.Threading;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;
using Egodystonic.Atomics.Tests.Harness;
using Egodystonic.Atomics.Tests.UnitTests.Common;
using NUnit.Framework;
using static Egodystonic.Atomics.Tests.Harness.ConcurrentTestCaseRunner;

namespace Egodystonic.Atomics.Tests.UnitTests {
	[TestFixture]
	class AtomicBoolTest {
		#region Test Fields
		RunnerFactory<bool, AtomicBool> _runnerFactory;
		#endregion

		#region Test Setup
		[OneTimeSetUp]
		public void SetUpClass() => _runnerFactory = new RunnerFactory<bool, AtomicBool>();

		[OneTimeTearDown]
		public void TearDownClass() { }

		[SetUp]
		public void SetUpTest() { }

		[TearDown]
		public void TearDownTest() { }
		#endregion

		#region Tests
		[Test]
		public void GetAndSet() {
			const int NumIterations = 3;
			var runner = _runnerFactory.NewRunner(false);

			var atomicInt = new AtomicInt(0);

			runner.GlobalSetUp = _ => atomicInt.Set(0);
			runner.AllThreadsTearDown = target => Assert.True(target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					var iterationNumber = atomicInt.Increment().NewValue;
					Assert.False(target.Get());
					target.Set(iterationNumber == NumIterations);
				},
				NumIterations
			);
		}

		[Test]
		public void Exchange() {
			const int NumIterations = 5_000_000;
			var runner = _runnerFactory.NewRunner(false);

			runner.ExecuteCustomTestCase(new IterativeConcurrentTestCase<AtomicBool>(
				"Iterative Writer vs Reader Custom Test Case",
				target => {
					while (target.Value) { }
					Assert.False(target.Exchange(true).PreviousValue);
				},
				target => {
					while (!target.Value) { }
					Assert.True(target.Exchange(false).PreviousValue);
				},
				new ConcurrentTestCaseThreadConfig(1, 1),
				NumIterations,
				true, 
				true
			));
		}

		[Test]
		public void TryExchange() {
			const int NumIterations = 1_000_000;
			var runner = _runnerFactory.NewRunner(false);

			runner.ExecuteFreeThreadedTests(
				target => {
					var curValue = target.Value;
					var res = target.TryExchange(!curValue, curValue);
					if (res.ValueWasSet) Assert.AreEqual(curValue, res.PreviousValue);
					else Assert.AreNotEqual(curValue, res.PreviousValue);
				},
				NumIterations
			);
		}

		[Test]
		public void PredicatedTryExchange() {
			const int NumIterations = 300_000;
			var runner = _runnerFactory.NewRunner(false);

			runner.ExecuteFreeThreadedTests(
				target => {
					var curValue = target.Value;
					var res = target.TryExchange(!curValue, (c, n) => n != c);
					if (res.ValueWasSet) Assert.AreEqual(curValue, res.PreviousValue);
					else Assert.AreNotEqual(curValue, res.PreviousValue);
				},
				NumIterations
			);
		}

		[Test]
		public void MappedExchange() {
			const int NumIterations = 500_000;
			var runner = _runnerFactory.NewRunner(false);

			runner.AllThreadsTearDown = target => Assert.AreEqual((NumIterations & 1) == 1, target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					var res = target.Exchange(c => !c);
					Assert.AreNotEqual(res.PreviousValue, res.NewValue);
				},
				NumIterations
			);
		}

		[Test]
		public void MappedTryExchange() {
			const int NumIterations = 300_000;
			var runner = _runnerFactory.NewRunner(false);

			runner.ExecuteFreeThreadedTests(
				target => {
					var curValue = target.Value;
					var res = target.TryExchange(c => !c, curValue);
					if (res.ValueWasSet) {
						Assert.AreEqual(curValue, res.PreviousValue);
						Assert.AreNotEqual(res.PreviousValue, res.NewValue);
					}
					else Assert.AreNotEqual(curValue, res.PreviousValue);
				},
				NumIterations
			);
		}

		[Test]
		public void MappedPredicatedTryExchange() {
			const int NumIterations = 300_000;
			var runner = _runnerFactory.NewRunner(false);

			runner.ExecuteFreeThreadedTests(
				target => {
					var res = target.TryExchange(c => !c, (c, n) => n != c);
					if (res.ValueWasSet) Assert.AreNotEqual(res.PreviousValue, res.NewValue);
					else Assert.AreEqual(res.PreviousValue, res.NewValue);
				},
				NumIterations
			);
		}
		#endregion Tests
	}
}