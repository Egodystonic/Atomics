// (c) Egodystonic Studios 2018


using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Egodystonic.Atomics.Tests.Harness;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.UnitTests.Common {
	abstract class CommonAtomicTestSuite<T, TTarget> where TTarget : IAtomic<T>, new() {
		readonly ConcurrentTestCaseRunner.RunnerFactory<T, TTarget> _runnerFactory = new ConcurrentTestCaseRunner.RunnerFactory<T, TTarget>();

		protected ConcurrentTestCaseRunner<TTarget> NewRunner() => _runnerFactory.NewRunner();
		protected ConcurrentTestCaseRunner<TTarget> NewRunner(T initialValue) => _runnerFactory.NewRunner(initialValue);

		// These methods provided because NUnit is too slow a lot of the time
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void AssertAreEqual(int expected, int actual) {
			if (expected != actual) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void AssertAreEqual<TTest>(TTest* expected, TTest* actual) where TTest : unmanaged {
			if (expected != actual) Assert.Fail($"Expected 0x{((IntPtr) expected).ToInt64():X} but was 0x{((IntPtr) actual).ToInt64():X}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void AssertAreEqual(long expected, long actual) {
			if (expected != actual) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void AssertAreEqual(uint expected, uint actual) {
			if (expected != actual) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void AssertAreEqual(ulong expected, ulong actual) {
			if (expected != actual) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void AssertAreEqual(float expected, float actual) {
			// ReSharper disable once CompareOfFloatsByEqualityOperator Exact comparison is expected here
			if (expected != actual) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void AssertAreEqual(double expected, double actual) {
			// ReSharper disable once CompareOfFloatsByEqualityOperator Exact comparison is expected here
			if (expected != actual) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void AssertAreEqual(float expected, float actual, float tolerance) {
			if (Math.Abs(expected - actual) > tolerance) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void AssertAreEqual(double expected, double actual, double tolerance) {
			if (Math.Abs(expected - actual) > tolerance) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void AssertAreEqual<TTest>(TTest expected, TTest actual) where TTest : IEquatable<TTest> {
			if (expected == null) {
				if (actual == null) return;
				Assert.Fail($"Expected <null> but was {actual}.");
			}
			if (!expected.Equals(actual)) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void AssertAreEqualObjects(object expected, object actual) {
			if (!Equals(expected, actual)) Assert.Fail($"Expected {expected} but was {actual}.");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void AssertAreNotEqual(int expected, int actual) {
			if (expected == actual) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void AssertAreNotEqual<TTest>(TTest* expected, TTest* actual) where TTest : unmanaged {
			if (expected == actual) Assert.Fail($"Expected 0x{((IntPtr) expected).ToInt64():X} to not be equal to 0x{((IntPtr) actual).ToInt64():X}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void AssertAreNotEqual(long expected, long actual) {
			if (expected == actual) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void AssertAreNotEqual(uint expected, uint actual) {
			if (expected == actual) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void AssertAreNotEqual(ulong expected, ulong actual) {
			if (expected == actual) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void AssertAreNotEqual(float expected, float actual) {
			// ReSharper disable once CompareOfFloatsByEqualityOperator Exact comparison is expected here
			if (expected == actual) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void AssertAreNotEqual(double expected, double actual) {
			// ReSharper disable once CompareOfFloatsByEqualityOperator Exact comparison is expected here
			if (expected == actual) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void AssertAreNotEqual(float expected, float actual, float tolerance) {
			if (Math.Abs(expected - actual) <= tolerance) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void AssertAreNotEqual(double expected, double actual, double tolerance) {
			if (Math.Abs(expected - actual) <= tolerance) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void AssertAreNotEqual<TTest>(TTest expected, TTest actual) where TTest : IEquatable<TTest> {
			if (expected == null) {
				if (actual != null) return;
				Assert.Fail($"Expected <null> to not be equal to {actual}.");
			}
			if (expected.Equals(actual)) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void AssertAreNotEqualObjects(object expected, object actual) {
			if (Equals(expected, actual)) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void AssertTrue(bool condition) {
			if (!condition) Assert.Fail($"Condition was false.");
		}


		// These tests just check the API for TTarget. I.e. return value, parameters, etc.
		protected abstract T Alpha { get; }
		protected abstract T Bravo { get; }
		protected abstract T Charlie { get; }
		protected abstract T Delta { get; }

		protected abstract bool AreEqual(T lhs, T rhs);

		[Test]
		public void API_Value() {
			var target = new TTarget();

			target.Value = Alpha;
			Assert.AreEqual(Alpha, target.Value);

			target.Value = Bravo;
			Assert.AreEqual(Bravo, target.Value);

			target.Value = Charlie;
			Assert.AreEqual(Charlie, target.Value);

			target.Value = Delta;
			Assert.AreEqual(Delta, target.Value);
		}

		[Test]
		public void API_GetSet() {
			var target = new TTarget();

			target.Set(Alpha);
			Assert.AreEqual(Alpha, target.Get());

			target.Set(Bravo);
			Assert.AreEqual(Bravo, target.Get());

			target.Set(Charlie);
			Assert.AreEqual(Charlie, target.Get());

			target.Set(Delta);
			Assert.AreEqual(Delta, target.Get());
		}

		[Test]
		public void API_GetSetUnsafe() {
			var target = new TTarget();

			target.SetUnsafe(Alpha);
			Assert.AreEqual(Alpha, target.GetUnsafe());

			target.SetUnsafe(Bravo);
			Assert.AreEqual(Bravo, target.GetUnsafe());

			target.SetUnsafe(Charlie);
			Assert.AreEqual(Charlie, target.GetUnsafe());

			target.SetUnsafe(Delta);
			Assert.AreEqual(Delta, target.GetUnsafe());
		}

		[Test]
		public void API_SpinWaitForValue() {
			var target = new TTarget();

			target.Set(Alpha);

			var spinWaitTask = Task.Run(() => target.SpinWaitForValue(Bravo));
			Thread.Sleep(200); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(Bravo);
			Assert.AreEqual(Bravo, spinWaitTask.Result);

			spinWaitTask = Task.Run(() => target.SpinWaitForValue(t => AreEqual(t, Charlie)));
			Thread.Sleep(200); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(Charlie);
			Assert.AreEqual(Charlie, spinWaitTask.Result);

			spinWaitTask = Task.Run(() => target.SpinWaitForValue(AreEqual, Delta));
			Thread.Sleep(200); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(Delta);
			Assert.AreEqual(Delta, spinWaitTask.Result);
		}

		[Test]
		public void API_Exchange() {
			var target = new TTarget();

			target.Set(Alpha);

			var exchRes = target.Exchange(Bravo);
			Assert.AreEqual(Alpha, exchRes.PreviousValue);
			Assert.AreEqual(Bravo, exchRes.NewValue);

			exchRes = target.Exchange(t => AreEqual(t, Bravo) ? Charlie : Delta);
			Assert.AreEqual(Bravo, exchRes.PreviousValue);
			Assert.AreEqual(Charlie, exchRes.NewValue);

			exchRes = target.Exchange(t => AreEqual(t, Bravo) ? Charlie : Delta);
			Assert.AreEqual(Charlie, exchRes.PreviousValue);
			Assert.AreEqual(Delta, exchRes.NewValue);

			exchRes = target.Exchange((t, ctx) => AreEqual(t, ctx) ? Alpha : Bravo, Delta);
			Assert.AreEqual(Delta, exchRes.PreviousValue);
			Assert.AreEqual(Alpha, exchRes.NewValue);

			exchRes = target.Exchange((t, ctx) => AreEqual(t, ctx) ? Alpha : Bravo, Delta);
			Assert.AreEqual(Alpha, exchRes.PreviousValue);
			Assert.AreEqual(Bravo, exchRes.NewValue);
		}

		[Test]
		public void API_SpinWaitForExchange() {
			var target = new TTarget();

			// (T, T)
			target.Set(Alpha);
			var exchRes = target.SpinWaitForExchange(Bravo, Alpha);
			Assert.AreEqual(Alpha, exchRes.PreviousValue);
			Assert.AreEqual(Bravo, exchRes.NewValue);
			var spinWaitTask = Task.Run(() => target.SpinWaitForExchange(Charlie, Delta));
			Thread.Sleep(100); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(Delta);
			Assert.AreEqual(Delta, spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Charlie, spinWaitTask.Result.NewValue);

			// (Func<T, T>, T)
			target.Set(Alpha);
			spinWaitTask = Task.Run(() => target.SpinWaitForExchange(t => AreEqual(t, Charlie) ? Bravo : Delta, Charlie));
			Thread.Sleep(100); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(Charlie);
			Assert.AreEqual(Charlie, spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Bravo, spinWaitTask.Result.NewValue);
			spinWaitTask = Task.Run(() => target.SpinWaitForExchange(t => AreEqual(t, Charlie) ? Alpha : Bravo, Delta));
			Thread.Sleep(100); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(Delta);
			Assert.AreEqual(Delta, spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Bravo, spinWaitTask.Result.NewValue);
			
			// (Func<T, TContext, T>, T, TContext)
			target.Set(Alpha);
			spinWaitTask = Task.Run(() => target.SpinWaitForExchange((t, ctx) => AreEqual(t, ctx) ? Bravo : Delta, Charlie, Charlie));
			Thread.Sleep(100); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(Charlie);
			Assert.AreEqual(Charlie, spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Bravo, spinWaitTask.Result.NewValue);
			spinWaitTask = Task.Run(() => target.SpinWaitForExchange((t, ctx) => AreEqual(t, ctx) ? Alpha : Bravo, Delta, Charlie));
			Thread.Sleep(100); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(Delta);
			Assert.AreEqual(Delta, spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Bravo, spinWaitTask.Result.NewValue);
			
			// (T, Func<T, T, bool>)
			target.Set(Alpha);
			spinWaitTask = Task.Run(() => target.SpinWaitForExchange(Bravo, (c, n) => AreEqual(c, Charlie) && AreEqual(n, Bravo)));
			Thread.Sleep(100); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(Charlie);
			Assert.AreEqual(Charlie, spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Bravo, spinWaitTask.Result.NewValue);
			spinWaitTask = Task.Run(() => target.SpinWaitForExchange(Bravo, (c, n) => AreEqual(c, Delta) && AreEqual(n, Bravo)));
			Thread.Sleep(100); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(Delta);
			Assert.AreEqual(Delta, spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Bravo, spinWaitTask.Result.NewValue);

			// (T, Func<T, T, TContext, bool>)
			target.Set(Alpha);
			spinWaitTask = Task.Run(() => target.SpinWaitForExchange(Bravo, (c, n, ctx) => AreEqual(c, ctx) && AreEqual(n, Bravo), Charlie));
			Thread.Sleep(100); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(Charlie);
			Assert.AreEqual(Charlie, spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Bravo, spinWaitTask.Result.NewValue);
			spinWaitTask = Task.Run(() => target.SpinWaitForExchange(Bravo, (c, n, ctx) => AreEqual(c, Delta) && AreEqual(n, ctx), Bravo));
			Thread.Sleep(100); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(Delta);
			Assert.AreEqual(Delta, spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Bravo, spinWaitTask.Result.NewValue);

			// (Func<T, T>, Func<T, T, bool>)
			target.Set(Alpha);
			spinWaitTask = Task.Run(() => target.SpinWaitForExchange(t => AreEqual(t, Charlie) ? Bravo : Delta, (c, n) => AreEqual(c, Charlie) && AreEqual(n, Bravo)));
			Thread.Sleep(100); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(Charlie);
			Assert.AreEqual(Charlie, spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Bravo, spinWaitTask.Result.NewValue);
			spinWaitTask = Task.Run(() => target.SpinWaitForExchange(t => AreEqual(t, Charlie) ? Alpha : Bravo, (c, n) => AreEqual(c, Delta) && AreEqual(n, Bravo)));
			Thread.Sleep(100); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(Delta);
			Assert.AreEqual(Delta, spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Bravo, spinWaitTask.Result.NewValue);

			// (Func<T, TContext, T>, Func<T, T, bool>)
			target.Set(Alpha);
			spinWaitTask = Task.Run(() => target.SpinWaitForExchange((t, ctx) => AreEqual(t, ctx) ? Bravo : Delta, (c, n) => AreEqual(c, Charlie) && AreEqual(n, Bravo), Charlie));
			Thread.Sleep(100); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(Charlie);
			Assert.AreEqual(Charlie, spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Bravo, spinWaitTask.Result.NewValue);
			spinWaitTask = Task.Run(() => target.SpinWaitForExchange((t, ctx) => AreEqual(t, ctx) ? Alpha : Bravo, (c, n) => AreEqual(c, Delta) && AreEqual(n, Bravo), Charlie));
			Thread.Sleep(100); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(Delta);
			Assert.AreEqual(Delta, spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Bravo, spinWaitTask.Result.NewValue);

			// (Func<T, T>, Func<T, T, TContext, bool>)
			target.Set(Alpha);
			spinWaitTask = Task.Run(() => target.SpinWaitForExchange(t => AreEqual(t, Charlie) ? Bravo : Delta, (c, n, ctx) => AreEqual(c, ctx) && AreEqual(n, Bravo), Charlie));
			Thread.Sleep(100); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(Charlie);
			Assert.AreEqual(Charlie, spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Bravo, spinWaitTask.Result.NewValue);
			spinWaitTask = Task.Run(() => target.SpinWaitForExchange(t => AreEqual(t, Charlie) ? Alpha : Bravo, (c, n, ctx) => AreEqual(c, Delta) && AreEqual(n, ctx), Bravo));
			Thread.Sleep(100); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(Delta);
			Assert.AreEqual(Delta, spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Bravo, spinWaitTask.Result.NewValue);
			
			// (Func<T, TContext, T>, Func<T, T, TContext, bool>)
			target.Set(Alpha);
			spinWaitTask = Task.Run(() => target.SpinWaitForExchange((t, ctx) => AreEqual(t, ctx) ? Bravo : Delta, (c, n, ctx) => AreEqual(c, ctx) && AreEqual(n, Bravo), Charlie));
			Thread.Sleep(100); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(Charlie);
			Assert.AreEqual(Charlie, spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Bravo, spinWaitTask.Result.NewValue);
			spinWaitTask = Task.Run(() => target.SpinWaitForExchange((t, ctx) => !AreEqual(t, ctx) ? Bravo : Alpha, (c, n, ctx) => AreEqual(c, Delta) && AreEqual(n, ctx), Bravo));
			Thread.Sleep(100); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(Delta);
			Assert.AreEqual(Delta, spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Bravo, spinWaitTask.Result.NewValue);
			
			// (Func<T, TMapContext, T>, Func<T, T, TPredicateContext, bool>)
			target.Set(Alpha);
			spinWaitTask = Task.Run(() => target.SpinWaitForExchange((t, ctx) => AreEqual(t, ctx) ? Bravo : Delta, (c, n, ctx) => AreEqual(c, ctx) && AreEqual(n, Bravo), Charlie, Charlie));
			Thread.Sleep(100); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(Charlie);
			Assert.AreEqual(Charlie, spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Bravo, spinWaitTask.Result.NewValue);
			spinWaitTask = Task.Run(() => target.SpinWaitForExchange((t, ctx) => AreEqual(t, ctx) ? Alpha : Bravo, (c, n, ctx) => AreEqual(c, Delta) && AreEqual(n, ctx), Charlie, Bravo));
			Thread.Sleep(100); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(Delta);
			Assert.AreEqual(Delta, spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Bravo, spinWaitTask.Result.NewValue);
		}

		[Test]
		public void API_TryExchange() {
			var target = new TTarget();

			// (T, T)
			target.Set(Alpha);
			var exchRes = target.TryExchange(Bravo, Alpha);
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Alpha, exchRes.PreviousValue);
			Assert.AreEqual(Bravo, exchRes.NewValue);
			exchRes = target.TryExchange(Delta, Charlie);
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Bravo, exchRes.PreviousValue);
			Assert.AreEqual(Bravo, exchRes.NewValue);

			// (Func<T, T>, T)
			target.Set(Alpha);
			exchRes = target.TryExchange(t => AreEqual(t, Alpha) ? Bravo : Delta, Alpha);
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Alpha, exchRes.PreviousValue);
			Assert.AreEqual(Bravo, exchRes.NewValue);
			exchRes = target.TryExchange(t => AreEqual(t, Charlie) ? Bravo : Delta, Charlie);
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Bravo, exchRes.PreviousValue);
			Assert.AreEqual(Bravo, exchRes.NewValue);

			// (Func<T, TContext, T>, T, TContext)
			target.Set(Alpha);
			exchRes = target.TryExchange((t, ctx) => AreEqual(t, ctx) ? Bravo : Delta, Alpha, Alpha);
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Alpha, exchRes.PreviousValue);
			Assert.AreEqual(Bravo, exchRes.NewValue);
			exchRes = target.TryExchange((t, ctx) => AreEqual(t, ctx) ? Bravo : Delta, Charlie, Charlie);
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Bravo, exchRes.PreviousValue);
			Assert.AreEqual(Bravo, exchRes.NewValue);

			// (T, Func<T, T, bool>)
			target.Set(Alpha);
			exchRes = target.TryExchange(Bravo, (c, n) => AreEqual(c, Alpha) && AreEqual(n, Bravo));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Alpha, exchRes.PreviousValue);
			Assert.AreEqual(Bravo, exchRes.NewValue);
			exchRes = target.TryExchange(Delta, (c, n) => AreEqual(c, Bravo) && AreEqual(n, Bravo));
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Bravo, exchRes.PreviousValue);
			Assert.AreEqual(Bravo, exchRes.NewValue);

			// (T, Func<T, T, TContext, bool>)
			target.Set(Alpha);
			exchRes = target.TryExchange(Bravo, (c, n, ctx) => AreEqual(c, Alpha) && AreEqual(n, ctx), Bravo);
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Alpha, exchRes.PreviousValue);
			Assert.AreEqual(Bravo, exchRes.NewValue);
			exchRes = target.TryExchange(Delta, (c, n, ctx) => AreEqual(c, Bravo) && AreEqual(n, ctx), Bravo);
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Bravo, exchRes.PreviousValue);
			Assert.AreEqual(Bravo, exchRes.NewValue);

			// (Func<T, T>, Func<T, T, bool>)
			target.Set(Alpha);
			exchRes = target.TryExchange(t => AreEqual(t, Alpha) ? Bravo : Delta, (c, n) => AreEqual(c, Alpha) && AreEqual(n, Bravo));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Alpha, exchRes.PreviousValue);
			Assert.AreEqual(Bravo, exchRes.NewValue);
			exchRes = target.TryExchange(t => AreEqual(t, Charlie) ? Bravo : Delta, (c, n) => AreEqual(c, Bravo) && AreEqual(n, Bravo));
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Bravo, exchRes.PreviousValue);
			Assert.AreEqual(Bravo, exchRes.NewValue);

			// (Func<T, TContext, T>, Func<T, T, bool>)
			target.Set(Alpha);
			exchRes = target.TryExchange((t, ctx) => AreEqual(t, ctx) ? Bravo : Delta, (c, n) => AreEqual(c, Alpha) && AreEqual(n, Bravo), Alpha);
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Alpha, exchRes.PreviousValue);
			Assert.AreEqual(Bravo, exchRes.NewValue);
			exchRes = target.TryExchange((t, ctx) => AreEqual(t, ctx) ? Bravo : Delta, (c, n) => AreEqual(c, Bravo) && AreEqual(n, Bravo), Charlie);
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Bravo, exchRes.PreviousValue);
			Assert.AreEqual(Bravo, exchRes.NewValue);

			// (Func<T, T>, Func<T, T, TContext, bool>)
			target.Set(Alpha);
			exchRes = target.TryExchange(t => AreEqual(t, Alpha) ? Bravo : Delta, (c, n, ctx) => AreEqual(c, Alpha) && AreEqual(n, ctx), Bravo);
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Alpha, exchRes.PreviousValue);
			Assert.AreEqual(Bravo, exchRes.NewValue);
			exchRes = target.TryExchange(t => AreEqual(t, Charlie) ? Bravo : Delta, (c, n, ctx) => AreEqual(c, Bravo) && AreEqual(n, ctx), Bravo);
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Bravo, exchRes.PreviousValue);
			Assert.AreEqual(Bravo, exchRes.NewValue);

			// (Func<T, TContext, T>, Func<T, T, TContext, bool>)
			target.Set(Alpha);
			exchRes = target.TryExchange((t, ctx) => AreEqual(t, ctx) ? Bravo : Delta, (c, n, ctx) => AreEqual(c, Alpha) && !AreEqual(n, ctx), Alpha);
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Alpha, exchRes.PreviousValue);
			Assert.AreEqual(Bravo, exchRes.NewValue);
			exchRes = target.TryExchange((t, ctx) => AreEqual(t, ctx) ? Bravo : Delta, (c, n, ctx) => !AreEqual(c, Bravo) && AreEqual(n, ctx), Bravo);
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Bravo, exchRes.PreviousValue);
			Assert.AreEqual(Bravo, exchRes.NewValue);

			// (Func<T, TMapContext, T>, Func<T, T, TPredicateContext, bool>)
			target.Set(Alpha);
			exchRes = target.TryExchange((t, ctx) => AreEqual(t, ctx) ? Bravo : Delta, (c, n, ctx) => AreEqual(c, Alpha) && AreEqual(n, ctx), Alpha, Bravo);
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Alpha, exchRes.PreviousValue);
			Assert.AreEqual(Bravo, exchRes.NewValue);
			exchRes = target.TryExchange((t, ctx) => AreEqual(t, ctx) ? Bravo : Delta, (c, n, ctx) => AreEqual(c, Bravo) && AreEqual(n, ctx), Charlie, Bravo);
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Bravo, exchRes.PreviousValue);
			Assert.AreEqual(Bravo, exchRes.NewValue);
		}
	}
}