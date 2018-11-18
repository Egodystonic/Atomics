// (c) Egodystonic Studios 2018

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.UnitTests.Common {
	abstract class CommonAtomicFloatingPointTestSuite<T, TTarget> : CommonAtomicNumericTestSuite<T, TTarget> where TTarget : IFloatingPointAtomic<T>, new() where T : IComparable<T>, IComparable, IEquatable<T> {
		protected abstract T Convert(float operand);
		protected abstract T Abs(T operand);

		[Test]
		public void TryExchangeFloatingPointDelta() {
			const int NumIterations = 100_000;

			var runner = NewRunner(Convert(NumIterations));

			// Test: Method does what is expected and is safe from race conditions
			runner.AllThreadsTearDown = target => Assert.AreEqual(0, target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Equals(Zero)) return;
						var newValue = Sub(curValue, One);
						var (wasSet, prevValue) = target.TryExchange(newValue, Sub(curValue, Convert(0.25f)), Convert(0.5f));
						if (wasSet) Assert.AreEqual(prevValue, curValue);
						else Assert.AreNotEqual(prevValue, curValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// Test: Method always exhibits coherency for consecutive reads from external threads
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curValue = target.Value;
					var newValue = Sub(curValue, One);
					target.TryExchange(newValue, Sub(curValue, Convert(0.25f)), Convert(0.5f));
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.LessOrEqual(cur, prev)
			);

			var testValue = new TTarget { Value = Convert(100f) };
			testValue.Value = Convert(100f);
			Assert.False(testValue.TryExchange(Zero, Convert(101f), Convert(0.5f)).ValueWasSet);
			testValue.Value = Convert(100f);
			Assert.True(testValue.TryExchange(Zero, Convert(101f), Convert(1f)).ValueWasSet);
			testValue.Value = Convert(100f);
			Assert.True(testValue.TryExchange(Zero, Convert(99f), Convert(1f)).ValueWasSet);
			testValue.Value = Convert(100f);
			Assert.False(testValue.TryExchange(Zero, Convert(99f), Convert(0.5f)).ValueWasSet);
		}

		[Test]
		public void MappedTryExchangeFloatingPointDelta() {
			const int NumIterations = 100_000;

			var runner = NewRunner(Convert(NumIterations));

			// Test: Method does what is expected and is safe from race conditions
			runner.AllThreadsTearDown = target => Assert.AreEqual(0, target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Equals(Zero)) return;
						var (wasSet, prevValue, newValue) = target.TryExchange(c => Sub(c, One), Add(curValue, Convert(0.25f)), Convert(0.5f));
						if (wasSet) {
							Assert.AreEqual(Add(newValue, One), prevValue);
							Assert.AreEqual(prevValue, curValue);
						}
						else Assert.AreNotEqual(prevValue, curValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// Test: Method always exhibits coherency for consecutive reads from external threads
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curValue = target.Value;
					target.TryExchange(c => Sub(c, One), Sub(curValue, One), Convert(1.5f));
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.LessOrEqual(cur, prev)
			);

			var testValue = new TTarget { Value = Convert(100f) };
			Assert.False(testValue.TryExchange(c => Zero, Convert(101f), Convert(0.5f)).ValueWasSet);
			testValue.Value = Convert(100f);
			Assert.True(testValue.TryExchange(c => Zero, Convert(101f), Convert(1f)).ValueWasSet);
			testValue.Value = Convert(100f);
			Assert.True(testValue.TryExchange(c => Zero, Convert(99f), Convert(1f)).ValueWasSet);
			testValue.Value = Convert(100f);
			Assert.False(testValue.TryExchange(c => Zero, Convert(99f), Convert(0.5f)).ValueWasSet);
		}
	}
}