// (c) Egodystonic Studios 2019
// Author: Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;
using Egodystonic.Atomics.Tests.Harness;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.UnitTests.Common {
	sealed class CommonNonLockingValueAtomicTestSuite<TTarget> : CommonNonLockingAtomicTestSuite<DummyImmutableVal, TTarget> where TTarget : INonLockingAtomic<DummyImmutableVal>, new() {
		public CommonNonLockingValueAtomicTestSuite() : base(
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

		public void Assert_Coherency_Swap() {
			const int NumIterations = 3_000_000;

			var atomicIntA = new LockFreeInt32(0);
			var atomicIntB = new LockFreeInt32(0);
			var runner = NewRunner(new DummyImmutableVal(0, 0));

			runner.GlobalSetUp = (_, __) => { atomicIntA.Set(0); atomicIntB.Set(0); };
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					var newA = atomicIntA.IncrementAndGet();
					var newB = atomicIntB.IncrementAndGet();
					var newValue = new DummyImmutableVal(newA, newB);
					var prev = target.Swap(newValue);
					FastAssertEqual(prev.Alpha, newA - 1);
					FastAssertEqual(prev.Bravo, newB - 1);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					FastAssertTrue(prev.Alpha <= cur.Alpha);
					FastAssertTrue(prev.Bravo <= cur.Bravo);
				}
			);
			runner.GlobalSetUp = null;
			runner.AllThreadsTearDown = null;
		}

		public void Assert_Coherency_TrySwap() {
			const int NumIterations = 2_000_000;

			var runner = NewRunner(new DummyImmutableVal(0, 0));

			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curValue = target.Value;
					var newValue = new DummyImmutableVal(0, curValue.Bravo + 1);
					target.TrySwap(newValue, curValue);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => FastAssertTrue(cur.Bravo >= prev.Bravo)
			);
		}
	}
}