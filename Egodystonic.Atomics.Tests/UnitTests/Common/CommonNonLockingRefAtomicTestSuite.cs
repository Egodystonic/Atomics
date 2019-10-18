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
	sealed class CommonNonLockingRefAtomicTestSuite<TTarget> : CommonNonLockingAtomicTestSuite<DummyImmutableRef, TTarget> where TTarget : INonLockingAtomic<DummyImmutableRef>, new() {
		public CommonNonLockingRefAtomicTestSuite() : base(
			(a, b) => a == b, 
			new DummyImmutableRef("aaa", -1L), 
			new DummyImmutableRef("aaa", 0L), 
			new DummyImmutableRef("bbb", -1L), 
			new DummyImmutableRef("bbb", 0L)
		) { }

		public void Assert_Coherency_GetAndSetAndValue() {
			const int NumIterations = 1_000_000;
			var atomicLong = new LockFreeInt64(0L);
			var runner = NewRunner(new DummyImmutableRef(0L));

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => target.Set(new DummyImmutableRef(atomicLong.IncrementAndGet())),
				NumIterations,
				target => target.Get(),
				(prev, cur) => FastAssertTrue(prev.LongProp <= cur.LongProp)
			);

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => target.Value = new DummyImmutableRef(atomicLong.IncrementAndGet()),
				NumIterations,
				target => target.Value,
				(prev, cur) => FastAssertTrue(prev.LongProp <= cur.LongProp)
			);
		}

		public void Assert_Coherency_Swap() {
			const int NumIterations = 1_000_000;
			var runner = NewRunner(new DummyImmutableRef(0L));

			// (T)
			var atomicLong = new LockFreeInt64(0L);
			runner.GlobalSetUp = (_, __) => atomicLong.Set(0L);
			runner.AllThreadsTearDown = target => FastAssertEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					var newLongValue = atomicLong.IncrementAndGet();
					var prev = target.Swap(new DummyImmutableRef(newLongValue));
					FastAssertEqual(prev.LongProp, newLongValue - 1L);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => FastAssertTrue(cur.LongProp >= prev.LongProp)
			);
			runner.GlobalSetUp = null;
			runner.AllThreadsTearDown = null;
		}

		public void Assert_Coherency_TrySwap() {
			const int NumIterations = 1_000_000;

			var runner = NewRunner(new DummyImmutableRef(0L));

			runner.AllThreadsTearDown = target => FastAssertEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.LongProp == NumIterations) return;
						var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
						var prevValue = target.TrySwap(newValue, curValue);
						var wasSet = prevValue.Equals(curValue);
						var setValue = wasSet ? newValue : prevValue;
						if (wasSet) {
							FastAssertEqual(curValue, prevValue);
							FastAssertEqual(newValue, setValue);
						}
						else {
							FastAssertNotEqual(curValue, prevValue);
							FastAssertEqual(setValue, prevValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}
	}
}