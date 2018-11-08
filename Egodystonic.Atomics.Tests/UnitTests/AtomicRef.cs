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
	class AtomicRefTest : CommonAtomicRefTestSuite<AtomicRef<DummyImmutableRef>> {
		#region Test Fields
		RunnerFactory<DummyEquatableRef, AtomicRef<DummyEquatableRef>> _equatableRefRunnerFactory;
		#endregion

		#region Test Setup
		[OneTimeSetUp]
		public void SetUpClass() => _equatableRefRunnerFactory = new RunnerFactory<DummyEquatableRef, AtomicRef<DummyEquatableRef>>();

		[OneTimeTearDown]
		public void TearDownClass() { }

		[SetUp]
		public void SetUpTest() { }

		[TearDown]
		public void TearDownTest() { }
		#endregion

		#region Tests
		[Test]
		public void EquatableTryExchange() {
			const int NumIterations = 100_000;

			var runner = _equatableRefRunnerFactory.NewRunner(new DummyEquatableRef(0L));

			// Test: Return value of method is always consistent
			runner.ExecuteFreeThreadedTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					var newValue = new DummyEquatableRef(curValue.LongProp + 1L);
					var (wasSet, prevValue) = atomicRef.TryExchange(newValue, curValue);
					if (wasSet) Assert.AreEqual(curValue, prevValue);
					else Assert.AreNotEqual(curValue, prevValue);
				},
				NumIterations
			);

			// Test: Method does what is expected and is safe from race conditions
			runner.AllThreadsTearDown = atomicRef => Assert.AreEqual(NumIterations, atomicRef.Value.LongProp);
			runner.ExecuteFreeThreadedTests(
				atomicRef => {
					while (true) {
						var curValue = atomicRef.Value;
						if (curValue.LongProp == NumIterations) return;
						var newValue = new DummyEquatableRef(curValue.LongProp + 1L);
						var (wasSet, prevValue) = atomicRef.TryExchange(newValue, curValue);
						if (wasSet) Assert.AreEqual(curValue, prevValue);
						else Assert.AreNotEqual(curValue, prevValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// Test: Method always exhibits coherency for consecutive reads from external threads
			runner.ExecuteContinuousCoherencyTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					var newValue = new DummyEquatableRef(curValue.LongProp + 1L);
					atomicRef.TryExchange(newValue, curValue);
				},
				NumIterations,
				atomicRef => atomicRef.Value,
				(prev, cur) => Assert.True(cur.LongProp >= prev.LongProp)
			);

			// Test: Custom equality is used for types that provide it
			var dummyImmutableA = new DummyImmutableRef("Xenoprimate Rules");
			var dummyImmutableB = new DummyImmutableRef("Xenoprimate Rules");
			var dummyEquatableA = new DummyEquatableRef("Xenoprimate Rules");
			var dummyEquatableB = new DummyEquatableRef("Xenoprimate Rules");
			var atomicImmutable = new AtomicRef<DummyImmutableRef>();
			var atomicEquatable = new AtomicRef<DummyEquatableRef>();

			atomicImmutable.Set(dummyImmutableA);
			Assert.AreEqual(false, atomicImmutable.TryExchange(new DummyImmutableRef(), dummyImmutableB).ValueWasSet);
			atomicEquatable.Set(dummyEquatableA);
			Assert.AreEqual(true, atomicEquatable.TryExchange(new DummyEquatableRef(), dummyEquatableB).ValueWasSet);
		}

		[Test]
		public void EquatablePredicatedTryExchange() {
			const int NumIterations = 100_000;

			var equatableRunner = _equatableRefRunnerFactory.NewRunner(new DummyEquatableRef(0L));

			// Test: Return value of TryExchange is always consistent
			equatableRunner.ExecuteFreeThreadedTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					var newValue = new DummyEquatableRef(curValue.LongProp + 1L);
					var (wasSet, prevValue) = atomicRef.TryExchange(newValue, c => c.LongProp == newValue.LongProp - 1L);
					if (wasSet) Assert.AreEqual(curValue, prevValue);
					else Assert.AreNotEqual(curValue, prevValue);
				},
				NumIterations
			);

			// Test: Method does what is expected and is safe from race conditions
			equatableRunner.AllThreadsTearDown = atomicRef => Assert.AreEqual(NumIterations, atomicRef.Value.LongProp);
			equatableRunner.ExecuteFreeThreadedTests(
				atomicRef => {
					while (true) {
						var curValue = atomicRef.Value;
						if (curValue.LongProp == NumIterations) return;
						var newValue = new DummyEquatableRef(curValue.LongProp + 1L);
						var (wasSet, prevValue) = atomicRef.TryExchange(newValue, c => c.LongProp == newValue.LongProp - 1L);
						if (wasSet) Assert.AreEqual(curValue, prevValue);
						else Assert.AreNotEqual(curValue, prevValue);
					}
				}
			);
			equatableRunner.AllThreadsTearDown = null;

			// Test: Return value of TryExchange is always consistent
			equatableRunner.ExecuteFreeThreadedTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					var newValue = new DummyEquatableRef(curValue.LongProp + 1L);
					var (wasSet, prevValue) = atomicRef.TryExchange(newValue, (c, n) => c.LongProp == n.LongProp - 1L);
					if (wasSet) Assert.AreEqual(curValue, prevValue);
					else Assert.AreNotEqual(curValue, prevValue);
				},
				NumIterations
			);

			// Test: Method does what is expected and is safe from race conditions
			equatableRunner.AllThreadsTearDown = atomicRef => Assert.AreEqual(NumIterations, atomicRef.Value.LongProp);
			equatableRunner.ExecuteFreeThreadedTests(
				atomicRef => {
					while (true) {
						var curValue = atomicRef.Value;
						if (curValue.LongProp == NumIterations) return;
						var newValue = new DummyEquatableRef(curValue.LongProp + 1L);
						var (wasSet, prevValue) = atomicRef.TryExchange(newValue, (c, n) => c.LongProp == n.LongProp - 1L);
						if (wasSet) Assert.AreEqual(curValue, prevValue);
						else Assert.AreNotEqual(curValue, prevValue);
					}
				}
			);
			equatableRunner.AllThreadsTearDown = null;

			// Test: Method always exhibits coherency for consecutive reads from external threads
			equatableRunner.ExecuteContinuousCoherencyTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					var newValue = new DummyEquatableRef(curValue.LongProp + 1L);
					atomicRef.TryExchange(newValue, c => c.LongProp == newValue.LongProp - 1L);
				},
				NumIterations,
				atomicRef => atomicRef.Value,
				(prev, cur) => Assert.True(cur.LongProp >= prev.LongProp)
			);
			equatableRunner.ExecuteContinuousCoherencyTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					var newValue = new DummyEquatableRef(curValue.LongProp + 1L);
					atomicRef.TryExchange(newValue, (c, n) => c.LongProp == n.LongProp - 1L);
				},
				NumIterations,
				atomicRef => atomicRef.Value,
				(prev, cur) => Assert.True(cur.LongProp >= prev.LongProp)
			);
		}

		[Test]
		public void EquatableMappedTryExchange() {
			const int NumIterations = 100_000;

			var equatableRunner = _equatableRefRunnerFactory.NewRunner(new DummyEquatableRef(0L));

			// Test: Return value of method is always consistent
			equatableRunner.ExecuteFreeThreadedTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					var (wasSet, prevValue, newValue) = atomicRef.TryExchange(c => new DummyEquatableRef(c.LongProp + 1L), curValue);
					if (wasSet) {
						Assert.AreEqual(curValue, prevValue);
						Assert.AreEqual(new DummyEquatableRef(prevValue.LongProp + 1L), newValue);
					}
					else Assert.AreNotEqual(curValue, prevValue);
				},
				NumIterations
			);

			// Test: Method does what is expected and is safe from race conditions
			equatableRunner.AllThreadsTearDown = atomicRef => Assert.AreEqual(NumIterations, atomicRef.Value.LongProp);
			equatableRunner.ExecuteFreeThreadedTests(
				atomicRef => {
					while (true) {
						var curValue = atomicRef.Value;
						if (curValue.LongProp == NumIterations) return;
						var (wasSet, prevValue, newValue) = atomicRef.TryExchange(c => new DummyEquatableRef(c.LongProp + 1L), curValue);
						if (wasSet) {
							Assert.AreEqual(curValue, prevValue);
							Assert.AreEqual(new DummyEquatableRef(prevValue.LongProp + 1L), newValue);
						}
						else Assert.AreNotEqual(curValue, prevValue);
					}
				}
			);
			equatableRunner.AllThreadsTearDown = null;

			// Test: Method always exhibits coherency for consecutive reads from external threads
			equatableRunner.ExecuteContinuousCoherencyTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					atomicRef.TryExchange(c => new DummyEquatableRef(c.LongProp + 1L), curValue);
				},
				NumIterations,
				atomicRef => atomicRef.Value,
				(prev, cur) => Assert.True(cur.LongProp >= prev.LongProp)
			);

			// Test: Custom equality is used for types that provide it
			var dummyImmutableA = new DummyImmutableRef("Xenoprimate Rules");
			var dummyImmutableB = new DummyImmutableRef("Xenoprimate Rules");
			var dummyEquatableA = new DummyEquatableRef("Xenoprimate Rules");
			var dummyEquatableB = new DummyEquatableRef("Xenoprimate Rules");
			var atomicImmutable = new AtomicRef<DummyImmutableRef>();
			var atomicEquatable = new AtomicRef<DummyEquatableRef>();

			atomicImmutable.Set(dummyImmutableA);
			Assert.AreEqual(false, atomicImmutable.TryExchange(c => new DummyImmutableRef(), dummyImmutableB).ValueWasSet);
			atomicEquatable.Set(dummyEquatableA);
			Assert.AreEqual(true, atomicEquatable.TryExchange(c => new DummyEquatableRef(), dummyEquatableB).ValueWasSet);
		}

		[Test]
		public void EquatableMappedPredicatedTryExchange() {
			const int NumIterations = 100_000;

			var equatableRunner = _equatableRefRunnerFactory.NewRunner(new DummyEquatableRef(0L));

			// Test: Return value of method is always consistent
			equatableRunner.ExecuteFreeThreadedTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					var (wasSet, prevValue, newValue) = atomicRef.TryExchange(c => new DummyEquatableRef(c.LongProp + 1L), c => c.LongProp == curValue.LongProp);
					if (wasSet) {
						Assert.AreEqual(curValue, prevValue);
						Assert.AreEqual(new DummyEquatableRef(prevValue.LongProp + 1L), newValue);
					}
					else Assert.AreNotEqual(curValue, prevValue);
				},
				NumIterations
			);

			// Test: Method does what is expected and is safe from race conditions
			equatableRunner.AllThreadsTearDown = atomicRef => Assert.AreEqual(NumIterations, atomicRef.Value.LongProp);
			equatableRunner.ExecuteFreeThreadedTests(
				atomicRef => {
					while (true) {
						var curValue = atomicRef.Value;
						if (curValue.LongProp == NumIterations) return;
						var (wasSet, prevValue, newValue) = atomicRef.TryExchange(c => new DummyEquatableRef(c.LongProp + 1L), c => c.LongProp == curValue.LongProp);
						if (wasSet) {
							Assert.AreEqual(curValue, prevValue);
							Assert.AreEqual(new DummyEquatableRef(prevValue.LongProp + 1L), newValue);
						}
						else Assert.AreNotEqual(curValue, prevValue);
					}
				}
			);
			equatableRunner.AllThreadsTearDown = null;

			// Test: Return value of method is always consistent
			equatableRunner.ExecuteFreeThreadedTests(
				atomicRef => {
					var (wasSet, prevValue, newValue) = atomicRef.TryExchange(c => new DummyEquatableRef(c.LongProp + 1L), (c, n) => c.LongProp == n.LongProp - 1L);
					if (wasSet) Assert.AreEqual(new DummyEquatableRef(prevValue.LongProp + 1L), newValue);
					else Assert.AreNotEqual(new DummyEquatableRef(prevValue.LongProp + 1L), newValue);
				},
				NumIterations
			);

			// Test: Method does what is expected and is safe from race conditions
			equatableRunner.AllThreadsTearDown = atomicRef => Assert.AreEqual(NumIterations, atomicRef.Value.LongProp);
			equatableRunner.ExecuteFreeThreadedTests(
				atomicRef => {
					while (true) {
						var (wasSet, prevValue, newValue) = atomicRef.TryExchange(c => new DummyEquatableRef(c.LongProp + 1L), (c, n) => c.LongProp == n.LongProp - 1L && c.LongProp < NumIterations);
						if (wasSet) Assert.AreEqual(new DummyEquatableRef(prevValue.LongProp + 1L), newValue);
						else if (prevValue.LongProp == NumIterations || newValue.LongProp == NumIterations) return;
						else Assert.AreNotEqual(new DummyEquatableRef(prevValue.LongProp + 1L), newValue);
					}
				}
			);
			equatableRunner.AllThreadsTearDown = null;

			// Test: Method always exhibits coherency for consecutive reads from external threads
			equatableRunner.ExecuteContinuousCoherencyTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					atomicRef.TryExchange(c => new DummyEquatableRef(c.LongProp + 1L), c => c.LongProp == curValue.LongProp);
				},
				NumIterations,
				atomicRef => atomicRef.Value,
				(prev, cur) => Assert.True(cur.LongProp >= prev.LongProp)
			);
			equatableRunner.ExecuteContinuousCoherencyTests(
				atomicRef => {
					atomicRef.TryExchange(c => new DummyEquatableRef(c.LongProp + 1L), (c, n) => c.LongProp == n.LongProp - 1L);
				},
				NumIterations,
				atomicRef => atomicRef.Value,
				(prev, cur) => Assert.True(cur.LongProp >= prev.LongProp)
			);
		}

		[Test]
		public void AtomicStringMultipleOperations() { // This test to try and make sure we won't see anything odd from string interning etc.
			const int NumFreeThreadedIterations = 50_000;
			const int NumCoherencyIterations = 15_000;

			var runner = new ConcurrentTestCaseRunner<AtomicRef<string>>(() => new AtomicRef<string>("abcdefghijklmnopqrstuvwxyz"));

			runner.ExecuteFreeThreadedTests(
				ar => {
					var tryExchangeRes = ar.TryExchange(str => str, str => str.Length == 26);
					Assert.True(tryExchangeRes.ValueWasSet);
					Assert.AreEqual(tryExchangeRes.PreviousValue, tryExchangeRes.NewValue);
				},
				NumFreeThreadedIterations
			);

			runner.ExecuteContinuousCoherencyTests(
				atomicRef => atomicRef.Exchange(str => str + str[str.Length - 26]),
				NumCoherencyIterations,
				atomicRef => atomicRef.Value,
				(prev, cur) => {
					Assert.LessOrEqual(prev.Length, cur.Length);
					if (cur.Length > 26) Assert.AreEqual(cur[cur.Length - 1], cur[cur.Length - 27]);
				}
			);
		}
		#endregion Tests
	}
}