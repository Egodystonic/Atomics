using System;
using System.Diagnostics;
using System.Threading;
using Egodystonic.Atomics.Awaitables;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;
using Egodystonic.Atomics.Tests.Harness;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.UnitTests {
	[TestFixture]
	class AtomicRefTest {
		#region Test Fields
		#endregion

		#region Test Setup
		[OneTimeSetUp]
		public void SetUpClass() { }

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
			const int NumIterations = 1_000_000;
			var atomicLong = new AtomicLong(0L);
			var runner = NewRunner(new DummyImmutableRef(0L));

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				atomicRef => atomicRef.Set(new DummyImmutableRef(atomicLong.Increment().NewValue)),
				NumIterations,
				atomicRef => atomicRef.Get(),
				(prev, cur) => Assert.True(prev.LongProp <= cur.LongProp)
			);
		}

		[Test]
		public void Exchange() {
			const int NumIterations = 300_000;
			var atomicLong = new AtomicLong(0L);
			var runner = NewRunner(new DummyImmutableRef(0L));
			runner.GlobalSetUp = _ => atomicLong.Set(0L);
			runner.AllThreadsTearDown = atomicRef => Assert.AreEqual(NumIterations, atomicRef.Value.LongProp);

			// Test: Method always exhibits coherency for consecutive reads from external threads
			runner.ExecuteContinuousSingleWriterCoherencyTests(
				atomicRef => {
					var newLongValue = atomicLong.Increment().NewValue;
					var prev = atomicRef.Exchange(new DummyImmutableRef(newLongValue));
					Assert.AreEqual(prev.LongProp, newLongValue - 1L);
				},
				NumIterations,
				atomicRef => atomicRef.Value,
				(prev, cur) => Assert.True(cur.LongProp >= prev.LongProp)
			);
		}

		[Test]
		public void TryExchange() {
			const int NumIterations = 100_000;

			var equatableRunner = NewRunner(new DummyEquatableRef(0L));

			// Test: Return value of method is always consistent
			equatableRunner.ExecuteFreeThreadedTests(
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
			equatableRunner.AllThreadsTearDown = atomicRef => Assert.AreEqual(NumIterations, atomicRef.Value.LongProp);
			equatableRunner.ExecuteFreeThreadedTests(
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
			equatableRunner.AllThreadsTearDown = null;

			// Test: Method always exhibits coherency for consecutive reads from external threads
			equatableRunner.ExecuteContinuousCoherencyTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					var newValue = new DummyEquatableRef(curValue.LongProp + 1L);
					atomicRef.TryExchange(newValue, curValue);
				},
				NumIterations,
				atomicRef => atomicRef.Value,
				(prev, cur) => Assert.True(cur.LongProp >= prev.LongProp)
			);

			var immutableRunner = NewRunner(new DummyImmutableRef(0L));

			// Test: Return value of method is always consistent
			immutableRunner.ExecuteFreeThreadedTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
					var (wasSet, prevValue) = atomicRef.TryExchange(newValue, curValue);
					if (wasSet) Assert.AreEqual(curValue, prevValue);
					else Assert.AreNotEqual(curValue, prevValue);
				},
				NumIterations
			);

			// Test: Method does what is expected and is safe from race conditions
			immutableRunner.AllThreadsTearDown = atomicRef => Assert.AreEqual(NumIterations, atomicRef.Value.LongProp);
			immutableRunner.ExecuteFreeThreadedTests(
				atomicRef => {
					while (true) {
						var curValue = atomicRef.Value;
						if (curValue.LongProp == NumIterations) return;
						var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
						var (wasSet, prevValue) = atomicRef.TryExchange(newValue, curValue);
						if (wasSet) Assert.AreEqual(curValue, prevValue);
						else Assert.AreNotEqual(curValue, prevValue);
					}
				}
			);
			immutableRunner.AllThreadsTearDown = null;

			// Test: Method always exhibits coherency for consecutive reads from external threads
			immutableRunner.ExecuteContinuousCoherencyTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
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
		public void PredicatedTryExchange() {
			const int NumIterations = 100_000;

			var equatableRunner = NewRunner(new DummyEquatableRef(0L));

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

			var immutableRunner = NewRunner(new DummyImmutableRef(0L));

			// Test: Return value of TryExchange is always consistent
			immutableRunner.ExecuteFreeThreadedTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
					var (wasSet, prevValue) = atomicRef.TryExchange(newValue, c => c.LongProp == newValue.LongProp - 1L);
					if (wasSet) Assert.AreEqual(curValue, prevValue);
					else Assert.AreNotEqual(curValue, prevValue);
				},
				NumIterations
			);

			// Test: Method does what is expected and is safe from race conditions
			immutableRunner.AllThreadsTearDown = atomicRef => Assert.AreEqual(NumIterations, atomicRef.Value.LongProp);
			immutableRunner.ExecuteFreeThreadedTests(
				atomicRef => {
					while (true) {
						var curValue = atomicRef.Value;
						if (curValue.LongProp == NumIterations) return;
						var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
						var (wasSet, prevValue) = atomicRef.TryExchange(newValue, c => c.LongProp == newValue.LongProp - 1L);
						if (wasSet) Assert.AreEqual(curValue, prevValue);
						else Assert.AreNotEqual(curValue, prevValue);
					}
				}
			);
			immutableRunner.AllThreadsTearDown = null;

			// Test: Return value of TryExchange is always consistent
			immutableRunner.ExecuteFreeThreadedTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
					var (wasSet, prevValue) = atomicRef.TryExchange(newValue, (c, n) => c.LongProp == n.LongProp - 1L);
					if (wasSet) Assert.AreEqual(curValue, prevValue);
					else Assert.AreNotEqual(curValue, prevValue);
				},
				NumIterations
			);

			// Test: Method does what is expected and is safe from race conditions
			immutableRunner.AllThreadsTearDown = atomicRef => Assert.AreEqual(NumIterations, atomicRef.Value.LongProp);
			immutableRunner.ExecuteFreeThreadedTests(
				atomicRef => {
					while (true) {
						var curValue = atomicRef.Value;
						if (curValue.LongProp == NumIterations) return;
						var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
						var (wasSet, prevValue) = atomicRef.TryExchange(newValue, (c, n) => c.LongProp == n.LongProp - 1L);
						if (wasSet) Assert.AreEqual(curValue, prevValue);
						else Assert.AreNotEqual(curValue, prevValue);
					}
				}
			);
			immutableRunner.AllThreadsTearDown = null;

			// Test: Method always exhibits coherency for consecutive reads from external threads
			immutableRunner.ExecuteContinuousCoherencyTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
					atomicRef.TryExchange(newValue, c => c.LongProp == newValue.LongProp - 1L);
				},
				NumIterations,
				atomicRef => atomicRef.Value,
				(prev, cur) => Assert.True(cur.LongProp >= prev.LongProp)
			);
			immutableRunner.ExecuteContinuousCoherencyTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
					atomicRef.TryExchange(newValue, (c, n) => c.LongProp == n.LongProp - 1L);
				},
				NumIterations,
				atomicRef => atomicRef.Value,
				(prev, cur) => Assert.True(cur.LongProp >= prev.LongProp)
			);
		}

		[Test]
		public void MappedExchange() {
			const int NumIterations = 300_000;

			var runner = NewRunner(new DummyImmutableRef(0L));
			runner.AllThreadsTearDown = atomicRef => Assert.AreEqual(NumIterations, atomicRef.Value.LongProp);

			// Test: Method does what is expected and is safe from race conditions
			runner.ExecuteFreeThreadedTests(
				atomicRef => {
					var (prev, cur) = atomicRef.Exchange(dummy => new DummyImmutableRef(dummy.LongProp + 1L));
					Assert.AreEqual(prev.LongProp, cur.LongProp - 1L);
				}, 
				NumIterations
			);

			// Test: Method always exhibits coherency for consecutive reads from external threads
			runner.ExecuteContinuousCoherencyTests(
				atomicRef => atomicRef.Exchange(dummy => new DummyImmutableRef(dummy.LongProp + 1L)),
				NumIterations,
				atomicRef => atomicRef.Value,
				(prev, cur) => Assert.True(cur.LongProp >= prev.LongProp),
				dummy => dummy.LongProp == NumIterations
			);
		}

		[Test]
		public void MappedTryExchange() {
			const int NumIterations = 100_000;

			var equatableRunner = NewRunner(new DummyEquatableRef(0L));

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

			var immutableRunner = NewRunner(new DummyImmutableRef(0L));

			// Test: Return value of method is always consistent
			immutableRunner.ExecuteFreeThreadedTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					var (wasSet, prevValue, newValue) = atomicRef.TryExchange(c => new DummyImmutableRef(c.LongProp + 1L), curValue);
					if (wasSet) {
						Assert.AreEqual(curValue, prevValue);
						Assert.AreEqual(prevValue.LongProp + 1L, newValue.LongProp);
					}
					else Assert.AreNotEqual(curValue, prevValue);
				},
				NumIterations
			);

			// Test: Method does what is expected and is safe from race conditions
			immutableRunner.AllThreadsTearDown = atomicRef => Assert.AreEqual(NumIterations, atomicRef.Value.LongProp);
			immutableRunner.ExecuteFreeThreadedTests(
				atomicRef => {
					while (true) {
						var curValue = atomicRef.Value;
						if (curValue.LongProp == NumIterations) return;
						var (wasSet, prevValue, newValue) = atomicRef.TryExchange(c => new DummyImmutableRef(c.LongProp + 1L), curValue);
						if (wasSet) {
							Assert.AreEqual(curValue, prevValue);
							Assert.AreEqual(prevValue.LongProp + 1L, newValue.LongProp);
						}
						else Assert.AreNotEqual(curValue, prevValue);
					}
				}
			);
			immutableRunner.AllThreadsTearDown = null;

			// Test: Method always exhibits coherency for consecutive reads from external threads
			immutableRunner.ExecuteContinuousCoherencyTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					atomicRef.TryExchange(c => new DummyImmutableRef(c.LongProp + 1L), curValue);
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
		public void MappedPredicatedTryExchange() {
			const int NumIterations = 100_000;

			var equatableRunner = NewRunner(new DummyEquatableRef(0L));

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

			var immutableRunner = NewRunner(new DummyImmutableRef(0L));

			// Test: Return value of method is always consistent
			immutableRunner.ExecuteFreeThreadedTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					var (wasSet, prevValue, newValue) = atomicRef.TryExchange(c => new DummyImmutableRef(c.LongProp + 1L), c => c.LongProp == curValue.LongProp);
					if (wasSet) {
						Assert.AreEqual(curValue, prevValue);
						Assert.AreEqual(prevValue.LongProp + 1L, newValue.LongProp);
					}
					else Assert.AreNotEqual(curValue, prevValue);
				},
				NumIterations
			);

			// Test: Method does what is expected and is safe from race conditions
			immutableRunner.AllThreadsTearDown = atomicRef => Assert.AreEqual(NumIterations, atomicRef.Value.LongProp);
			immutableRunner.ExecuteFreeThreadedTests(
				atomicRef => {
					while (true) {
						var curValue = atomicRef.Value;
						if (curValue.LongProp == NumIterations) return;
						var (wasSet, prevValue, newValue) = atomicRef.TryExchange(c => new DummyImmutableRef(c.LongProp + 1L), c => c.LongProp == curValue.LongProp);
						if (wasSet) {
							Assert.AreEqual(curValue, prevValue);
							Assert.AreEqual(prevValue.LongProp + 1L, newValue.LongProp);
						}
						else Assert.AreNotEqual(curValue, prevValue);
					}
				}
			);
			immutableRunner.AllThreadsTearDown = null;

			// Test: Return value of method is always consistent
			immutableRunner.ExecuteFreeThreadedTests(
				atomicRef => {
					var (wasSet, prevValue, newValue) = atomicRef.TryExchange(c => new DummyImmutableRef(c.LongProp + 1L), (c, n) => c.LongProp == n.LongProp - 1L);
					if (wasSet) Assert.AreEqual(prevValue.LongProp + 1L, newValue.LongProp);
					else Assert.AreNotEqual(prevValue.LongProp + 1L, newValue.LongProp);
				},
				NumIterations
			);

			// Test: Method does what is expected and is safe from race conditions
			immutableRunner.AllThreadsTearDown = atomicRef => Assert.AreEqual(NumIterations, atomicRef.Value.LongProp);
			immutableRunner.ExecuteFreeThreadedTests(
				atomicRef => {
					while (true) {
						var (wasSet, prevValue, newValue) = atomicRef.TryExchange(c => new DummyImmutableRef(c.LongProp + 1L), (c, n) => c.LongProp == n.LongProp - 1L && c.LongProp < NumIterations);
						if (wasSet) Assert.AreEqual(prevValue.LongProp + 1L, newValue.LongProp);
						else if (prevValue.LongProp == NumIterations || newValue.LongProp == NumIterations) return;
						else Assert.AreNotEqual(prevValue.LongProp + 1L, newValue.LongProp);
					}
				}
			);
			immutableRunner.AllThreadsTearDown = null;

			// Test: Method always exhibits coherency for consecutive reads from external threads
			immutableRunner.ExecuteContinuousCoherencyTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					atomicRef.TryExchange(c => new DummyImmutableRef(c.LongProp + 1L), c => c.LongProp == curValue.LongProp);
				},
				NumIterations,
				atomicRef => atomicRef.Value,
				(prev, cur) => Assert.True(cur.LongProp >= prev.LongProp)
			);
			immutableRunner.ExecuteContinuousCoherencyTests(
				atomicRef => {
					atomicRef.TryExchange(c => new DummyImmutableRef(c.LongProp + 1L), (c, n) => c.LongProp == n.LongProp - 1L);
				},
				NumIterations,
				atomicRef => atomicRef.Value,
				(prev, cur) => Assert.True(cur.LongProp >= prev.LongProp)
			);
		}

		static ConcurrentTestCaseRunner<AtomicRef<T>> NewRunner<T>() where T : class => new ConcurrentTestCaseRunner<AtomicRef<T>>(() => new AtomicRef<T>());
		static ConcurrentTestCaseRunner<AtomicRef<T>> NewRunner<T>(T initialValue) where T : class => new ConcurrentTestCaseRunner<AtomicRef<T>>(() => new AtomicRef<T>(initialValue));
		#endregion Tests
	}
}