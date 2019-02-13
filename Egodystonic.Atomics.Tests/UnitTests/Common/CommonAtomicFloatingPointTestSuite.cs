// (c) Egodystonic Studios 2018

using System;
using System.Threading;
using System.Threading.Tasks;
using Egodystonic.Atomics.Numerics;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.UnitTests.Common {
	abstract class CommonAtomicFloatingPointTestSuite<T, TTarget> : CommonAtomicNumericTestSuite<T, TTarget> where TTarget : IFloatingPointAtomic<T>, new() where T : IComparable<T>, IComparable, IEquatable<T> {
		const double TestTolerance = 0.01d;

		protected abstract T Convert(float operand);
		protected abstract double AsDouble(T operand);
		protected abstract T Abs(T operand);

		void AssertEqualWithTolerance(T lhs, T rhs) => Assert.AreEqual(AsDouble(lhs), AsDouble(rhs), TestTolerance);

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
						var (wasSet, prevValue, setValue) = target.TryExchangeWithMaxDelta(newValue, Sub(curValue, Convert(0.25f)), Convert(0.5f));
						if (wasSet) {
							Assert.AreEqual(curValue, prevValue);
							Assert.AreEqual(newValue, setValue);
						}
						else {
							Assert.AreNotEqual(curValue, prevValue);
							Assert.AreEqual(setValue, prevValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// Test: Method always exhibits coherency for consecutive reads from external threads
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curValue = target.Value;
					var newValue = Sub(curValue, One);
					target.TryExchangeWithMaxDelta(newValue, Sub(curValue, Convert(0.25f)), Convert(0.5f));
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.LessOrEqual(cur, prev)
			);

			var testValue = new TTarget { Value = Convert(100f) };
			testValue.Value = Convert(100f);
			Assert.False(testValue.TryExchangeWithMaxDelta(Zero, Convert(101f), Convert(0.5f)).ValueWasSet);
			testValue.Value = Convert(100f);
			Assert.True(testValue.TryExchangeWithMaxDelta(Zero, Convert(101f), Convert(1f)).ValueWasSet);
			testValue.Value = Convert(100f);
			Assert.True(testValue.TryExchangeWithMaxDelta(Zero, Convert(99f), Convert(1f)).ValueWasSet);
			testValue.Value = Convert(100f);
			Assert.False(testValue.TryExchangeWithMaxDelta(Zero, Convert(99f), Convert(0.5f)).ValueWasSet);
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
						var (wasSet, prevValue, CurrentValue) = target.TryExchangeWithMaxDelta(c => Sub(c, One), Add(curValue, Convert(0.25f)), Convert(0.5f));
						if (wasSet) {
							Assert.AreEqual(Add(CurrentValue, One), prevValue);
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
					target.TryExchangeWithMaxDelta(c => Sub(c, One), Sub(curValue, One), Convert(1.5f));
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.LessOrEqual(cur, prev)
			);

			var testValue = new TTarget { Value = Convert(100f) };
			Assert.False(testValue.TryExchangeWithMaxDelta(c => Zero, Convert(101f), Convert(0.5f)).ValueWasSet);
			testValue.Value = Convert(100f);
			Assert.True(testValue.TryExchangeWithMaxDelta(c => Zero, Convert(101f), Convert(1f)).ValueWasSet);
			testValue.Value = Convert(100f);
			Assert.True(testValue.TryExchangeWithMaxDelta(c => Zero, Convert(99f), Convert(1f)).ValueWasSet);
			testValue.Value = Convert(100f);
			Assert.False(testValue.TryExchangeWithMaxDelta(c => Zero, Convert(99f), Convert(0.5f)).ValueWasSet);
		}

		// API Tests
		[Test]
		public void API_SpinWaitForValueWithMaxDelta() {
			var target = new TTarget();

			target.Set(Convert(100f));
			var waitRes = target.SpinWaitForValueWithMaxDelta(Convert(100f), Convert(0f));
			AssertEqualWithTolerance(Convert(100f), waitRes);

			var spinWaitTask = Task.Run(() => target.SpinWaitForValueWithMaxDelta(Convert(200f), Convert(99.99f)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(100.0001f));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(100.1f));
			AssertEqualWithTolerance(Convert(100.1f), spinWaitTask.Result);
			spinWaitTask = Task.Run(() => target.SpinWaitForValueWithMaxDelta(Convert(0f), Convert(1f)));
			Thread.Sleep(200); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(-1f));
			AssertEqualWithTolerance(Convert(-1f), spinWaitTask.Result);
		}

		[Test]
		public void API_SpinWaitForExchangeWithMaxDelta() {
			var target = new TTarget();

			target.Set(Convert(1f));

			// (T, T, T)
			var exchRes = target.SpinWaitForExchangeWithMaxDelta(Convert(2f), Convert(1f), Convert(0f));
			AssertEqualWithTolerance(exchRes.PreviousValue, Convert(1f));
			AssertEqualWithTolerance(exchRes.CurrentValue, Convert(2f));
			var spinWaitTask = Task.Run(() => target.SpinWaitForExchangeWithMaxDelta(Convert(3f), Convert(2.5f), Convert(0.1f)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(2.45f));
			AssertEqualWithTolerance(spinWaitTask.Result.PreviousValue, Convert(2.45f));
			AssertEqualWithTolerance(spinWaitTask.Result.CurrentValue, Convert(3f));

			// (Func<T, T>, T, T)
			exchRes = target.SpinWaitForExchangeWithMaxDelta(t => Mul(t, Convert(2f)), Convert(4f), Convert(1f));
			AssertEqualWithTolerance(exchRes.PreviousValue, Convert(3f));
			AssertEqualWithTolerance(exchRes.CurrentValue, Convert(6f));
			spinWaitTask = Task.Run(() => target.SpinWaitForExchangeWithMaxDelta(t => Sub(t, Convert(1.1f)), Convert(6.1f), Convert(0f)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(6.1f));
			AssertEqualWithTolerance(spinWaitTask.Result.PreviousValue, Convert(6.1f));
			AssertEqualWithTolerance(spinWaitTask.Result.CurrentValue, Convert(5f));

			// (Func<T, TContext, T>, T, T)
			exchRes = target.SpinWaitForExchangeWithMaxDelta(Add, Convert(100f), Convert(100f), Convert(5f));
			AssertEqualWithTolerance(exchRes.PreviousValue, Convert(5f));
			AssertEqualWithTolerance(exchRes.CurrentValue, Convert(10f));
			spinWaitTask = Task.Run(() => target.SpinWaitForExchangeWithMaxDelta(Div, Convert(20.1f), Convert(0.11f), Convert(10f)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(20f));
			AssertEqualWithTolerance(spinWaitTask.Result.PreviousValue, Convert(20f));
			AssertEqualWithTolerance(spinWaitTask.Result.CurrentValue, Convert(2f));
		}

		[Test]
		public void API_TryExchangeWithMaxDelta() {
			var target = new TTarget();

			target.Set(Convert(1f));

			// (T, T, T)
			var exchRes = target.TryExchangeWithMaxDelta(Convert(2f), Convert(1f), Convert(0f));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			AssertEqualWithTolerance(Convert(1f), exchRes.PreviousValue);
			AssertEqualWithTolerance(Convert(2f), exchRes.CurrentValue);
			exchRes = target.TryExchangeWithMaxDelta(Convert(3f), Convert(2.5f), Convert(0.1f));
			Assert.AreEqual(false, exchRes.ValueWasSet);
			AssertEqualWithTolerance(Convert(2f), exchRes.PreviousValue);
			AssertEqualWithTolerance(Convert(2f), exchRes.CurrentValue);
			target.Set(Convert(2.45f));
			exchRes = target.TryExchangeWithMaxDelta(Convert(3f), Convert(2.5f), Convert(0.1f));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			AssertEqualWithTolerance(Convert(2.45f), exchRes.PreviousValue);
			AssertEqualWithTolerance(Convert(3f), exchRes.CurrentValue);

			// (Func<T, T>, T, T)
			exchRes = target.TryExchangeWithMaxDelta(t => Mul(t, Convert(2f)), Convert(4f), Convert(1f));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			AssertEqualWithTolerance(Convert(3f), exchRes.PreviousValue);
			AssertEqualWithTolerance(Convert(6f), exchRes.CurrentValue);
			exchRes = target.TryExchangeWithMaxDelta(t => Sub(t, Convert(1.1f)), Convert(6.1f), Convert(0f));
			Assert.AreEqual(false, exchRes.ValueWasSet);
			AssertEqualWithTolerance(exchRes.PreviousValue, Convert(6f));
			AssertEqualWithTolerance(exchRes.CurrentValue, Convert(6f));
			target.Set(Convert(6.1f));
			exchRes = target.TryExchangeWithMaxDelta(t => Sub(t, Convert(1.1f)), Convert(6.1f), Convert(0f));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			AssertEqualWithTolerance(Convert(6.1f), exchRes.PreviousValue);
			AssertEqualWithTolerance(Convert(5f), exchRes.CurrentValue);

			// (Func<T, TContext, T>, T, T)
			exchRes = target.TryExchangeWithMaxDelta(Add, Convert(100f), Convert(100f), Convert(5f));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			AssertEqualWithTolerance(Convert(5f), exchRes.PreviousValue);
			AssertEqualWithTolerance(Convert(10f), exchRes.CurrentValue);
			exchRes = target.TryExchangeWithMaxDelta(Div, Convert(20.1f), Convert(0.1f), Convert(10f));
			Assert.AreEqual(false, exchRes.ValueWasSet);
			AssertEqualWithTolerance(Convert(10f), exchRes.PreviousValue);
			AssertEqualWithTolerance(Convert(10f), exchRes.CurrentValue);
			target.Set(Convert(20f));
			exchRes = target.TryExchangeWithMaxDelta(Div, Convert(20.1f), Convert(0.11f), Convert(10f));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			AssertEqualWithTolerance(Convert(20f), exchRes.PreviousValue);
			AssertEqualWithTolerance(Convert(2f), exchRes.CurrentValue);
		}
	}
}