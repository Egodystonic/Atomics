// (c) Egodystonic Studios 2018


using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;
using Egodystonic.Atomics.Tests.Harness;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.UnitTests.Common {
	abstract class CommonLockingValueAtomicTestSuite<TTarget> : CommonLockingAtomicTestSuite<DummyImmutableVal, TTarget> where TTarget : ILockingAtomic<DummyImmutableVal>, new() {
		public CommonLockingValueAtomicTestSuite() : base(
			(a, b) => a == b,
			new DummyImmutableVal(-1, 0),
			new DummyImmutableVal(0, -1),
			new DummyImmutableVal(0, 0),
			new DummyImmutableVal(-1, -1)
		) { }

		public void Assert_Coherency_GetAndSetAndValue() {
			const int NumIterations = 1_000_000;
			var atomicLong = new LockFreeInt64(0L);
			var runner = NewRunner(new DummyImmutableVal(0, 0));

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					unsafe {
						var newLongVal = atomicLong.IncrementAndGet();
						target.Set(*(DummyImmutableVal*) &newLongVal);
					}
				},
				NumIterations,
				target => target.Get(),
				(prev, cur) => {
					unsafe {
						FastAssertTrue(*(long*) &prev <= *(long*) &cur);
					}
				}
			);

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					unsafe {
						var newLongVal = atomicLong.IncrementAndGet();
						target.Value = *(DummyImmutableVal*) &newLongVal;
					}
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					unsafe {
						FastAssertTrue(*(long*) &prev <= *(long*) &cur);
					}
				}
			);
		}

		public void Assert_Coherency_Set() {
			const int NumIterations = 1_000_000;
			var atomicIntA = new LockFreeInt32(0);
			var atomicIntB = new LockFreeInt32(0);

			// void Set(T newValue, out T previousValue);
			var runner = NewRunner(new DummyImmutableVal(0, 0));
			runner.GlobalSetUp = (_, __) => { atomicIntA.Set(0); atomicIntB.Set(0); };
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					target.Set(
						new DummyImmutableVal(atomicIntA.IncrementAndGet(), atomicIntB.DecrementAndGet()),
						out var prevValue
					);
					FastAssertEqual(atomicIntA.Value - 1, prevValue.Alpha);
					FastAssertEqual(atomicIntB.Value + 1, prevValue.Bravo);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					FastAssertTrue(prev.Alpha <= cur.Alpha);
					FastAssertTrue(prev.Bravo >= cur.Alpha);
				}
			);
			runner.GlobalSetUp = null;
			runner.AllThreadsTearDown = null;

			// T Set(Func<T, T> valueMapFunc);
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(-NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					var curVal = target.Value;
					var newVal = target.Set(t => new DummyImmutableVal(t.Alpha + 1, t.Bravo - 1));
					FastAssertTrue(curVal.Alpha < newVal.Alpha);
					FastAssertTrue(curVal.Bravo > newVal.Bravo);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			// T Set(Func<T, T> valueMapFunc, out T previousValue);
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(-NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					var curVal = target.Value;
					var newVal = target.Set(t => new DummyImmutableVal(t.Alpha + 1, t.Bravo - 1), out var prevVal);
					FastAssertTrue(curVal.Alpha < newVal.Alpha);
					FastAssertTrue(curVal.Bravo > newVal.Bravo);
					FastAssertEqual(newVal.Alpha - 1, prevVal.Alpha);
					FastAssertEqual(newVal.Bravo + 1, prevVal.Bravo);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;
		}

		public void Assert_Coherency_TryGet() {
			const int NumIterations = 1_000_000;

			var runner = NewRunner(new DummyImmutableVal(0, 0));
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(-NumIterations, target.Value.Bravo);
			};
			runner.ExecuteWriterReaderTests(
				target => {
					
				},
				target => {
					var curVal = target.Value;
					var wasRetrieved = target.TryGet(v => v == curVal, out var newVal);
					if (wasRetrieved) FastAssertEqual(curVal, newVal);
					else FastAssertNotEqual(curVal, newVal);
				},
				NumIterations
			);
			runner.GlobalSetUp = null;
			runner.AllThreadsTearDown = null;
		}

		public void Assert_Coherency_TrySet() {
			const int NumIterations = 1_000_000;

			var runner = NewRunner(new DummyImmutableVal(0, 0));
			runner.GlobalSetUp = (target, _) => target.Set(new DummyImmutableVal(0, 0));

			// bool TrySet(T newValue, Func<T, bool> setPredicate);
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(-NumIterations, target.Value.Bravo);
			};
			runner.ExecuteContinuousCoherencyTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var newValue = new DummyImmutableVal(curValue.Alpha + 1, curValue.Bravo - 1);
						target.TrySet(newValue, v => v.Alpha + 1 == newValue.Alpha && v.Bravo - 1 == newValue.Bravo);
						FastAssertTrue(target.Value.Alpha > curValue.Alpha);
						FastAssertTrue(target.Value.Bravo < curValue.Alpha);
					}
				},
				target => target.Value,
				(prev, cur) => {
					FastAssertTrue(cur.Alpha >= prev.Alpha);
					FastAssertTrue(cur.Bravo <= prev.Bravo);
				},
				v => v.Alpha == NumIterations
			);
			runner.AllThreadsTearDown = null;

			// bool TrySet(T newValue, Func<T, bool> setPredicate, out T previousValue);
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(-NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var newValue = new DummyImmutableVal(curValue.Alpha + 1, curValue.Bravo - 1);
						var wasSet = target.TrySet(newValue, v => v.Alpha + 1 == newValue.Alpha && v.Bravo - 1 == newValue.Bravo, out var prevValue);
						if (wasSet) {
							FastAssertEqual(curValue, prevValue);
						}
						else {
							FastAssertNotEqual(curValue, prevValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// bool TrySet(Func<T, T> valueMapFunc, Func<T, bool> setPredicate);
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(-NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						target.TrySet(v => new DummyImmutableVal(v.Alpha + 1, v.Bravo - 1), v => v == curValue);
						FastAssertTrue(target.Value.Alpha > curValue.Alpha);
						FastAssertTrue(target.Value.Bravo < curValue.Alpha);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// bool TrySet(Func<T, T> valueMapFunc, Func<T, bool> setPredicate, out T previousValue);
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(-NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var wasSet = target.TrySet(v => new DummyImmutableVal(v.Alpha + 1, v.Bravo - 1), v => v == curValue, out var prevValue);
						if (wasSet) {
							FastAssertEqual(curValue, prevValue);
						}
						else {
							FastAssertNotEqual(curValue, prevValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// bool TrySet(Func<T, T> valueMapFunc, Func<T, bool> setPredicate, out T previousValue, out T newValue);
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(-NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var wasSet = target.TrySet(v => new DummyImmutableVal(v.Alpha + 1, v.Bravo - 1), v => v == curValue, out var prevValue, out var newValue);
						if (wasSet) {
							FastAssertEqual(curValue, prevValue);
							FastAssertEqual(prevValue.Alpha + 1, newValue.Alpha);
							FastAssertEqual(prevValue.Bravo - 1, newValue.Bravo);
						}
						else {
							FastAssertNotEqual(curValue, prevValue);
							FastAssertEqual(prevValue, newValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// bool TrySet(Func<T, T> valueMapFunc, Func<T, T, bool> setPredicate);
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(-NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						target.TrySet(v => new DummyImmutableVal(v.Alpha + 1, v.Bravo - 1), (c, n) => c.Alpha == curValue.Alpha && n.Bravo == c.Bravo + 1);
						FastAssertTrue(target.Value.Alpha > curValue.Alpha);
						FastAssertTrue(target.Value.Bravo < curValue.Alpha);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// bool TrySet(Func<T, T> valueMapFunc, Func<T, T, bool> setPredicate, out T previousValue);
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(-NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var wasSet = target.TrySet(v => new DummyImmutableVal(v.Alpha + 1, v.Bravo - 1), (c, n) => c.Alpha == curValue.Alpha && n.Bravo == c.Bravo + 1, out var prevValue);
						if (wasSet) {
							FastAssertEqual(curValue, prevValue);
						}
						else {
							FastAssertNotEqual(curValue, prevValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// bool TrySet(Func<T, T> valueMapFunc, Func<T, T, bool> setPredicate, out T previousValue, out T newValue);
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(-NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var wasSet = target.TrySet(v => new DummyImmutableVal(v.Alpha + 1, v.Bravo - 1), (c, n) => c.Alpha == curValue.Alpha && n.Bravo == c.Bravo + 1, out var prevValue, out var newValue);
						if (wasSet) {
							FastAssertEqual(curValue, prevValue);
							FastAssertEqual(prevValue.Alpha + 1, newValue.Alpha);
							FastAssertEqual(prevValue.Bravo - 1, newValue.Bravo);
						}
						else {
							FastAssertNotEqual(curValue, prevValue);
							FastAssertEqual(prevValue, newValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}
	}
}