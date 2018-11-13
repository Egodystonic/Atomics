// (c) Egodystonic Studios 2018


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.UnitTests.Common {
	abstract class CommonAtomicNumericTestSuite<T, TTarget> : CommonAtomicTestSuite<T, TTarget> where TTarget : IAtomic<T>, new() where T : IComparable<T>, IComparable, IEquatable<T> {
		protected abstract T Zero { get; }
		protected abstract T Convert(int operand);
		protected abstract T Add(T lhs, int rhs);
		protected abstract T Sub(T lhs, int rhs);

		[Test]
		public void GetAndSet() {
			const int NumIterations = 1_000_000;
			var runner = NewRunner(Zero);

			var atomicInt = new AtomicInt();

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					target.Set(Convert(atomicInt.Increment().NewValue));
				},
				NumIterations,
				target => target.Get(),
				(prev, cur) => Assert.LessOrEqual(prev, cur)
			);
		}

		[Test]
		public void Exchange() {
			const int NumIterations = 300_000;

			var atomicInt = new AtomicInt(0);
			var runner = NewRunner(Zero);
			runner.GlobalSetUp = _ => { atomicInt.Set(0); };
			runner.AllThreadsTearDown = target => {
				Assert.AreEqual(Convert(NumIterations), target.Value);
			};

			// Test: Method always exhibits coherency for consecutive reads from external threads
			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					var newValue = Convert(atomicInt.Increment().NewValue);
					var prev = target.Exchange(newValue);
					Assert.AreEqual(prev, Sub(newValue, 1));
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					Assert.LessOrEqual(prev, cur);
				}
			);
		}

		[Test]
		public void TryExchange() {
			const int NumIterations = 100_000;
			Assert.True((NumIterations & 1) == 0); // Need NumIterations to be an even number for the teardown assertions

			var runner = NewRunner(new DummyImmutableVal(0, 0));

			// Test: Return value of method is always consistent
			runner.ExecuteFreeThreadedTests(
				target => {
					var curValue = target.Value;
					var newValue = curValue.Bravo < curValue.Alpha
						? new DummyImmutableVal(curValue.Alpha, curValue.Bravo + 1)
						: new DummyImmutableVal(curValue.Alpha + 1, curValue.Bravo);
					var (wasSet, prevValue) = target.TryExchange(newValue, curValue);
					if (wasSet) Assert.AreEqual(curValue, prevValue);
					else Assert.AreNotEqual(curValue, prevValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			// Test: Method does what is expected and is safe from race conditions
			runner.AllThreadsTearDown = target => Assert.AreEqual(NumIterations, target.Value.Alpha);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var newValue = new DummyImmutableVal(curValue.Alpha + 1, 0);
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
					var newValue = new DummyImmutableVal(0, curValue.Bravo + 1);
					target.TryExchange(newValue, curValue);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.True(cur.Bravo >= prev.Bravo)
			);
		}

		[Test]
		public void PredicatedTryExchange() {
			const int NumIterations = 100_000;

			var runner = NewRunner(new DummyImmutableVal(1, 1));

			// Test: Return value of TryExchange is always consistent
			runner.ExecuteFreeThreadedTests(
				target => {
					var curValue = target.Value;
					var newValue = new DummyImmutableVal(curValue.Alpha + curValue.Bravo, curValue.Alpha);
					var (wasSet, prevValue) = target.TryExchange(newValue, c => c.Alpha == newValue.Bravo);
					if (wasSet) Assert.AreEqual(curValue, prevValue);
					else Assert.AreNotEqual(curValue, prevValue);
				},
				NumIterations
			);

			// Test: Method does what is expected and is safe from race conditions
			runner.AllThreadsTearDown = target => {
				Assert.AreEqual(NumIterations + 1, target.Value.Alpha);
				Assert.AreEqual(-1 * NumIterations + 1, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations + 1) return;
						var newValue = new DummyImmutableVal(curValue.Alpha + 1, curValue.Bravo - 1);
						var (wasSet, prevValue) = target.TryExchange(newValue, c => c.Alpha + 1 == newValue.Alpha && c.Bravo - 1 == newValue.Bravo);
						if (wasSet) Assert.AreEqual(curValue, prevValue);
						else Assert.AreNotEqual(curValue, prevValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// Test: Return value of TryExchange is always consistent
			runner.ExecuteFreeThreadedTests(
				target => {
					var curValue = target.Value;
					var newValue = new DummyImmutableVal(curValue.Alpha + curValue.Bravo, curValue.Alpha);
					var (wasSet, prevValue) = target.TryExchange(newValue, (c, n) => c.Alpha == n.Bravo);
					if (wasSet) Assert.AreEqual(curValue, prevValue);
					else Assert.AreNotEqual(curValue, prevValue);
				},
				NumIterations
			);


			// Test: Method does what is expected and is safe from race conditions
			runner.AllThreadsTearDown = target => {
				Assert.AreEqual(NumIterations + 1, target.Value.Alpha);
				Assert.AreEqual(-1 * NumIterations + 1, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations + 1) return;
						var newValue = new DummyImmutableVal(curValue.Alpha + 1, curValue.Bravo - 1);
						var (wasSet, prevValue) = target.TryExchange(newValue, (c, n) => c.Alpha + 1 == n.Alpha && c.Bravo - 1 == n.Bravo);
						if (wasSet) Assert.AreEqual(curValue, prevValue);
						else Assert.AreNotEqual(curValue, prevValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// Test: Method always exhibits coherency for consecutive reads from external threads
			runner.ExecuteContinuousCoherencyTests(
				target => {
					checked {
						var curValue = target.Value;
						var newValue = new DummyImmutableVal(curValue.Alpha + 64, curValue.Bravo + 32);
						target.TryExchange(newValue, c => c.Alpha == newValue.Alpha + 64);
					}
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					Assert.GreaterOrEqual(cur.Alpha, prev.Alpha);
					Assert.GreaterOrEqual(cur.Bravo, prev.Bravo);
					Assert.AreEqual(1, cur.Alpha & 0b111111);
					Assert.AreEqual(1, cur.Bravo & 0b11111);
				}
			);
			runner.ExecuteContinuousCoherencyTests(
				target => {
					checked {
						var curValue = target.Value;
						var newValue = new DummyImmutableVal(curValue.Alpha + 64, curValue.Bravo + 32);
						target.TryExchange(newValue, (c, n) => c.Alpha == n.Alpha + 64);
					}
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					Assert.GreaterOrEqual(cur.Alpha, prev.Alpha);
					Assert.GreaterOrEqual(cur.Bravo, prev.Bravo);
					Assert.AreEqual(1, cur.Alpha & 0b111111);
					Assert.AreEqual(1, cur.Bravo & 0b11111);
				}
			);
		}

		[Test]
		public void MappedExchange() {
			const int NumIterations = 300_000;


			var runner = NewRunner(new DummyImmutableVal(0, 0));
			runner.AllThreadsTearDown = target => {
				Assert.AreEqual(NumIterations, target.Value.Alpha);
				Assert.AreEqual(NumIterations * 2, target.Value.Bravo);
			};

			// Test: Method always exhibits coherency for consecutive reads from external threads
			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					var res = target.Exchange(curValue => new DummyImmutableVal(curValue.Alpha + 1, curValue.Bravo + 2));
					Assert.AreEqual(res.NewValue.Alpha, res.PreviousValue.Alpha + 1);
					Assert.AreEqual(res.NewValue.Bravo, res.PreviousValue.Bravo + 2);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					Assert.LessOrEqual(prev.Alpha, cur.Alpha);
					Assert.LessOrEqual(prev.Bravo, cur.Bravo);
				}
			);
		}

		[Test]
		public void MappedTryExchange() {
			const int NumIterations = 100_000;
			Assert.True((NumIterations & 1) == 0); // Need NumIterations to be an even number for the teardown assertions

			var runner = NewRunner(new DummyImmutableVal(0, 0));

			// Test: Return value of method is always consistent
			runner.ExecuteFreeThreadedTests(
				target => {
					var curValue = target.Value;
					
					var (wasSet, prevValue, newValue) = target.TryExchange(
						c => c.Bravo < c.Alpha
							? new DummyImmutableVal(c.Alpha, c.Bravo + 1)
							: new DummyImmutableVal(c.Alpha + 1, c.Bravo), 
						curValue
					);

					if (wasSet) {
						Assert.AreEqual(curValue, prevValue);
						Assert.AreEqual(prevValue.Bravo < prevValue.Alpha ? new DummyImmutableVal(prevValue.Alpha, prevValue.Bravo + 1) : new DummyImmutableVal(prevValue.Alpha + 1, prevValue.Bravo), newValue);
					}

					else Assert.AreNotEqual(curValue, prevValue);
				},
				NumIterations
			);

			// Test: Method does what is expected and is safe from race conditions
			runner.AllThreadsTearDown = target => Assert.AreEqual(NumIterations, target.Value.Alpha);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var (wasSet, prevValue, newValue) = target.TryExchange(c => new DummyImmutableVal(c.Alpha + 1, 0), curValue);
						if (wasSet) {
							Assert.AreEqual(curValue, prevValue);
							Assert.AreEqual(new DummyImmutableVal(prevValue.Alpha + 1, 0), newValue);
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
					target.TryExchange(c => new DummyImmutableVal(0, c.Bravo + 1), curValue);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.True(cur.Bravo >= prev.Bravo)
			);
		}

		[Test]
		public void MappedPredicatedTryExchange() {
			const int NumIterations = 100_000;

			var runner = NewRunner(new DummyImmutableVal(1, 1));

			// Test: Return value of TryExchange is always consistent
			runner.ExecuteFreeThreadedTests(
				target => {
					var curValue = target.Value;
					var (wasSet, prevValue, newValue) = target.TryExchange(c => new DummyImmutableVal(c.Alpha + c.Bravo, c.Alpha), c => c.Alpha == curValue.Alpha);
					if (wasSet) {
						Assert.AreEqual(curValue, prevValue);
						Assert.AreEqual(new DummyImmutableVal(prevValue.Alpha + prevValue.Bravo, prevValue.Alpha), newValue);
					}
					else Assert.AreNotEqual(curValue, prevValue);
				},
				NumIterations
			);

			// Test: Method does what is expected and is safe from race conditions
			runner.AllThreadsTearDown = target => {
				Assert.AreEqual(NumIterations + 1, target.Value.Alpha);
				Assert.AreEqual(-1 * NumIterations + 1, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations + 1) return;
						var (wasSet, prevValue, newValue) = target.TryExchange(c => new DummyImmutableVal(c.Alpha + 1, c.Bravo - 1), c => c.Alpha == curValue.Alpha && c.Bravo == curValue.Bravo);
						if (wasSet) {
							Assert.AreEqual(curValue, prevValue);
							Assert.AreEqual(new DummyImmutableVal(prevValue.Alpha + 1, prevValue.Bravo - 1), newValue);
						}
						else Assert.AreNotEqual(curValue, prevValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// Test: Return value of TryExchange is always consistent
			runner.ExecuteFreeThreadedTests(
				target => {
					var curValue = target.Value;
					var (wasSet, prevValue, newValue) = target.TryExchange(c => new DummyImmutableVal(c.Alpha + c.Bravo, c.Alpha), (c, n) => c.Alpha == n.Bravo);
					if (wasSet) {
						Assert.AreEqual(new DummyImmutableVal(prevValue.Alpha + prevValue.Bravo, prevValue.Alpha), newValue);
					}
					else Assert.AreNotEqual(curValue, prevValue);
				},
				NumIterations
			);


			// Test: Method does what is expected and is safe from race conditions
			runner.AllThreadsTearDown = target => {
				Assert.AreEqual(NumIterations + 1, target.Value.Alpha);
				Assert.AreEqual(-1 * NumIterations + 1, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations + 1) return;
						var (wasSet, prevValue, newValue) = target.TryExchange(c => new DummyImmutableVal(c.Alpha + 1, c.Bravo - 1), (c, n) => c.Alpha + 1 == n.Alpha && c.Bravo - 1 == n.Bravo && c.Alpha <= NumIterations);
						if (wasSet) {
							Assert.AreEqual(new DummyImmutableVal(prevValue.Alpha + 1, prevValue.Bravo - 1), newValue);
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
					target.TryExchange(c => new DummyImmutableVal(c.Alpha + 64, c.Bravo + 32), c => c.Alpha + 64 == curValue.Alpha);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					Assert.GreaterOrEqual(cur.Alpha, prev.Alpha);
					Assert.GreaterOrEqual(cur.Bravo, prev.Bravo);
					Assert.AreEqual(1, cur.Alpha & 0b111111);
					Assert.AreEqual(1, cur.Bravo & 0b11111);
				}
			);
			runner.ExecuteContinuousCoherencyTests(
				target => {
					target.TryExchange(c => new DummyImmutableVal(c.Alpha + 64, c.Bravo + 32), (c, n) => c.Alpha + 64 == n.Alpha);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					Assert.GreaterOrEqual(cur.Alpha, prev.Alpha);
					Assert.GreaterOrEqual(cur.Bravo, prev.Bravo);
					Assert.AreEqual(1, cur.Alpha & 0b111111);
					Assert.AreEqual(1, cur.Bravo & 0b11111);
				}
			);
		}
	}
}