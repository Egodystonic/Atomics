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
	abstract class CommonLockingRefAtomicTestSuite<TTarget> : CommonLockingAtomicTestSuite<DummyImmutableRef, TTarget> where TTarget : ILockingAtomic<DummyImmutableRef>, new() {
		public CommonLockingRefAtomicTestSuite() : base(
			(a, b) => a == b,
			new DummyImmutableRef("aaa", 0),
			new DummyImmutableRef("aaa", -1),
			new DummyImmutableRef("bbb", 0),
			new DummyImmutableRef("bbb", -1)
		) { }

		public void Assert_Coherency_GetAndSetAndValue() {
			const int NumIterations = 1_000_000;
			var atomicLong = new LockFreeInt64(0L);
			var runner = NewRunner(new DummyImmutableRef(0L));

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					var newLongVal = atomicLong.IncrementAndGet();
					target.Set(new DummyImmutableRef(newLongVal));
				},
				NumIterations,
				target => target.Get(),
				(prev, cur) => {
					FastAssertTrue(prev.LongProp <= cur.LongProp);
				}
			);

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					var newLongVal = atomicLong.IncrementAndGet();
					target.Value = new DummyImmutableRef(newLongVal);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					FastAssertTrue(prev.LongProp <= cur.LongProp);
				}
			);
		}

		public void Assert_Coherency_Set() {
			const int NumIterations = 1_000_000;
			var atomicLong = new LockFreeInt64(0L);

			// void Set(T newValue, out T previousValue);
			var runner = NewRunner(new DummyImmutableRef(0L));
			runner.GlobalSetUp = (_, __) => atomicLong.Set(0L);
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.LongProp);
			};
			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					target.Set(
						new DummyImmutableRef(atomicLong.IncrementAndGet()),
						out var prevValue
					);
					FastAssertEqual(atomicLong.Value - 1, prevValue.LongProp);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					FastAssertTrue(prev.LongProp <= cur.LongProp);
				}
			);
			runner.GlobalSetUp = null;
			runner.AllThreadsTearDown = null;

			// T Set(Func<T, T> valueMapFunc);
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.LongProp);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					var curVal = target.Value;
					var newVal = target.Set(t => new DummyImmutableRef(t.LongProp + 1L));
					FastAssertTrue(curVal.LongProp < newVal.LongProp);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			// T Set(Func<T, T> valueMapFunc, out T previousValue);
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.LongProp);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					var curVal = target.Value;
					var newVal = target.Set(t => new DummyImmutableRef(t.LongProp + 1), out var prevVal);
					FastAssertTrue(curVal.LongProp < newVal.LongProp);
					FastAssertEqual(newVal.LongProp - 1, prevVal.LongProp);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;
		}

		public void Assert_Coherency_TryGet() {
			const int NumIterations = 1_000_000;

			var runner = NewRunner(new DummyImmutableRef(0L));
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.LongProp);
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

			var runner = NewRunner(new DummyImmutableRef(0L));
			runner.GlobalSetUp = (target, _) => target.Set(new DummyImmutableRef(0L));

			// bool TrySet(T newValue, Func<T, bool> setPredicate);
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.LongProp);
			};
			runner.ExecuteContinuousCoherencyTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.LongProp == NumIterations) return;
						var newValue = new DummyImmutableRef(curValue.LongProp + 1);
						target.TrySet(newValue, v => v.LongProp + 1 == newValue.LongProp);
						FastAssertTrue(target.Value.LongProp > curValue.LongProp);
					}
				},
				target => target.Value,
				(prev, cur) => {
					FastAssertTrue(cur.LongProp >= prev.LongProp);
				},
				v => v.LongProp == NumIterations
			);
			runner.AllThreadsTearDown = null;

			// bool TrySet(T newValue, Func<T, bool> setPredicate, out T previousValue);
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.LongProp);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.LongProp == NumIterations) return;
						var newValue = new DummyImmutableRef(curValue.LongProp + 1);
						var wasSet = target.TrySet(newValue, v => v.LongProp + 1 == newValue.LongProp, out var prevValue);
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
				FastAssertEqual(NumIterations, target.Value.LongProp);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.LongProp == NumIterations) return;
						target.TrySet(v => new DummyImmutableRef(v.LongProp + 1), v => v == curValue);
						FastAssertTrue(target.Value.LongProp > curValue.LongProp);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// bool TrySet(Func<T, T> valueMapFunc, Func<T, bool> setPredicate, out T previousValue);
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.LongProp);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.LongProp == NumIterations) return;
						var wasSet = target.TrySet(v => new DummyImmutableRef(v.LongProp + 1), v => v == curValue, out var prevValue);
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
				FastAssertEqual(NumIterations, target.Value.LongProp);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.LongProp == NumIterations) return;
						var wasSet = target.TrySet(v => new DummyImmutableRef(v.LongProp + 1), v => v == curValue, out var prevValue, out var newValue);
						if (wasSet) {
							FastAssertEqual(curValue, prevValue);
							FastAssertEqual(prevValue.LongProp + 1, newValue.LongProp);
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
				FastAssertEqual(NumIterations, target.Value.LongProp);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.LongProp == NumIterations) return;
						target.TrySet(v => new DummyImmutableRef(v.LongProp + 1), (c, n) => c.LongProp == curValue.LongProp && n.LongProp == c.LongProp + 1);
						FastAssertTrue(target.Value.LongProp > curValue.LongProp);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// bool TrySet(Func<T, T> valueMapFunc, Func<T, T, bool> setPredicate, out T previousValue);
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.LongProp);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.LongProp == NumIterations) return;
						var wasSet = target.TrySet(v => new DummyImmutableRef(v.LongProp + 1), (c, n) => c.LongProp == curValue.LongProp && n.LongProp == c.LongProp + 1, out var prevValue);
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
				FastAssertEqual(NumIterations, target.Value.LongProp);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.LongProp == NumIterations) return;
						var wasSet = target.TrySet(v => new DummyImmutableRef(v.LongProp + 1), (c, n) => c.LongProp == curValue.LongProp && n.LongProp == c.LongProp + 1, out var prevValue, out var newValue);
						if (wasSet) {
							FastAssertEqual(curValue, prevValue);
							FastAssertEqual(prevValue.LongProp + 1, newValue.LongProp);
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