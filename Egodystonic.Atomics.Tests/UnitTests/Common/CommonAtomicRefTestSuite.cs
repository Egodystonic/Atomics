// (c) Egodystonic Studios 2018


using System;
using System.Collections.Generic;
using System.Linq;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.UnitTests.Common {
	abstract class CommonAtomicRefTestSuite<TTarget> : CommonAtomicTestSuite<DummyImmutableRef, TTarget> where TTarget : IAtomic<DummyImmutableRef>, new() {
		[Test]
		public void GetAndSet() {
			const int NumIterations = 1_000_000;
			var atomicLong = new AtomicLong(0L);
			var runner = NewRunner(new DummyImmutableRef(0L));

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => target.Set(new DummyImmutableRef(atomicLong.Increment().NewValue)),
				NumIterations,
				target => target.Get(),
				(prev, cur) => Assert.True(prev.LongProp <= cur.LongProp)
			);
		}

		[Test]
		public void Exchange() {
			const int NumIterations = 300_000;
			var atomicLong = new AtomicLong(0L);
			var runner = NewRunner(new DummyImmutableRef(0L));
			runner.GlobalSetUp = _ => atomicLong.Set(0L);
			runner.AllThreadsTearDown = target => Assert.AreEqual(NumIterations, target.Value.LongProp);

			// Test: Method always exhibits coherency for consecutive reads from external threads
			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					var newLongValue = atomicLong.Increment().NewValue;
					var prev = target.Exchange(new DummyImmutableRef(newLongValue));
					Assert.AreEqual(prev.LongProp, newLongValue - 1L);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.True(cur.LongProp >= prev.LongProp)
			);
		}

		[Test]
		public void TryExchange() {
			const int NumIterations = 100_000;

			var runner = NewRunner(new DummyImmutableRef(0L));

			// Test: Return value of method is always consistent
			runner.ExecuteFreeThreadedTests(
				target => {
					var curValue = target.Value;
					var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
					var (wasSet, prevValue) = target.TryExchange(newValue, curValue);
					if (wasSet) Assert.AreEqual(curValue, prevValue);
					else Assert.AreNotEqual(curValue, prevValue);
				},
				NumIterations
			);

			// Test: Method does what is expected and is safe from race conditions
			runner.AllThreadsTearDown = target => Assert.AreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.LongProp == NumIterations) return;
						var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
						var (wasSet, prevValue) = target.TryExchange(newValue, curValue);
						if (wasSet) Assert.AreEqual(curValue, prevValue);
						else Assert.AreNotEqual(curValue, prevValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// Test: Method always exhibits coherency for consecutive reads from external threads
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curValue = target.Value;
					var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
					target.TryExchange(newValue, curValue);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.True(cur.LongProp >= prev.LongProp)
			);
		}

		[Test]
		public void PredicatedTryExchange() {
			const int NumIterations = 100_000;

			var runner = NewRunner(new DummyImmutableRef(0L));

			// Test: Return value of TryExchange is always consistent
			runner.ExecuteFreeThreadedTests(
				target => {
					var curValue = target.Value;
					var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
					var (wasSet, prevValue) = target.TryExchange(newValue, (c, n) => c.LongProp == n.LongProp - 1L);
					if (wasSet) Assert.AreEqual(curValue, prevValue);
					else Assert.AreNotEqual(curValue, prevValue);
				},
				NumIterations
			);

			// Test: Method does what is expected and is safe from race conditions
			runner.AllThreadsTearDown = target => Assert.AreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.LongProp == NumIterations) return;
						var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
						var (wasSet, prevValue) = target.TryExchange(newValue, (c, n) => c.LongProp == n.LongProp - 1L);
						if (wasSet) Assert.AreEqual(curValue, prevValue);
						else Assert.AreNotEqual(curValue, prevValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// Test: Method always exhibits coherency for consecutive reads from external threads
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curValue = target.Value;
					var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
					target.TryExchange(newValue, (c, n) => c.LongProp == n.LongProp - 1L);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.True(cur.LongProp >= prev.LongProp)
			);
		}

		[Test]
		public void MappedExchange() {
			const int NumIterations = 300_000;

			var runner = NewRunner(new DummyImmutableRef(0L));
			runner.AllThreadsTearDown = target => Assert.AreEqual(NumIterations, target.Value.LongProp);

			// Test: Method does what is expected and is safe from race conditions
			runner.ExecuteFreeThreadedTests(
				target => {
					var (prev, cur) = target.Exchange(dummy => new DummyImmutableRef(dummy.LongProp + 1L));
					Assert.AreEqual(prev.LongProp, cur.LongProp - 1L);
				},
				NumIterations
			);

			// Test: Method always exhibits coherency for consecutive reads from external threads
			runner.ExecuteContinuousCoherencyTests(
				target => target.Exchange(dummy => new DummyImmutableRef(dummy.LongProp + 1L)),
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.True(cur.LongProp >= prev.LongProp),
				dummy => dummy.LongProp == NumIterations
			);
		}

		[Test]
		public void MappedTryExchange() {
			const int NumIterations = 100_000;

			var runner = NewRunner(new DummyImmutableRef(0L));

			// Test: Return value of method is always consistent
			runner.ExecuteFreeThreadedTests(
				target => {
					var curValue = target.Value;
					var (wasSet, prevValue, newValue) = target.TryExchange(c => new DummyImmutableRef(c.LongProp + 1L), curValue);
					if (wasSet) {
						Assert.AreEqual(curValue, prevValue);
						Assert.AreEqual(prevValue.LongProp + 1L, newValue.LongProp);
					}
					else Assert.AreNotEqual(curValue, prevValue);
				},
				NumIterations
			);

			// Test: Method does what is expected and is safe from race conditions
			runner.AllThreadsTearDown = target => Assert.AreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.LongProp == NumIterations) return;
						var (wasSet, prevValue, newValue) = target.TryExchange(c => new DummyImmutableRef(c.LongProp + 1L), curValue);
						if (wasSet) {
							Assert.AreEqual(curValue, prevValue);
							Assert.AreEqual(prevValue.LongProp + 1L, newValue.LongProp);
						}
						else Assert.AreNotEqual(curValue, prevValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// Test: Method always exhibits coherency for consecutive reads from external threads
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curValue = target.Value;
					target.TryExchange(c => new DummyImmutableRef(c.LongProp + 1L), curValue);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.True(cur.LongProp >= prev.LongProp)
			);
		}

		[Test]
		public void MappedPredicatedTryExchange() {
			const int NumIterations = 100_000;

			var runner = NewRunner(new DummyImmutableRef(0L));

			// Test: Return value of method is always consistent
			runner.ExecuteFreeThreadedTests(
				target => {
					var (wasSet, prevValue, newValue) = target.TryExchange(c => new DummyImmutableRef(c.LongProp + 1L), (c, n) => c.LongProp == n.LongProp - 1L);
					if (wasSet) Assert.AreEqual(prevValue.LongProp + 1L, newValue.LongProp);
					else Assert.AreNotEqual(prevValue.LongProp + 1L, newValue.LongProp);
				},
				NumIterations
			);

			// Test: Method does what is expected and is safe from race conditions
			runner.AllThreadsTearDown = target => Assert.AreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var (wasSet, prevValue, newValue) = target.TryExchange(c => new DummyImmutableRef(c.LongProp + 1L), (c, n) => c.LongProp == n.LongProp - 1L && c.LongProp < NumIterations);
						if (wasSet) Assert.AreEqual(prevValue.LongProp + 1L, newValue.LongProp);
						else if (prevValue.LongProp == NumIterations || newValue.LongProp == NumIterations) return;
						else Assert.AreNotEqual(prevValue.LongProp + 1L, newValue.LongProp);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// Test: Method always exhibits coherency for consecutive reads from external threads
			runner.ExecuteContinuousCoherencyTests(
				target => {
					target.TryExchange(c => new DummyImmutableRef(c.LongProp + 1L), (c, n) => c.LongProp == n.LongProp - 1L);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.True(cur.LongProp >= prev.LongProp)
			);
		}
	}
}