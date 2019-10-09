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
		protected void FastAssertEqual(int expected, int actual) {
			if (expected != actual) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void FastAssertEqual<TTest>(TTest* expected, TTest* actual) where TTest : unmanaged {
			if (expected != actual) Assert.Fail($"Expected 0x{((IntPtr) expected).ToInt64():X} but was 0x{((IntPtr) actual).ToInt64():X}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FastAssertEqual(long expected, long actual) {
			if (expected != actual) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FastAssertEqual(uint expected, uint actual) {
			if (expected != actual) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FastAssertEqual(ulong expected, ulong actual) {
			if (expected != actual) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FastAssertEqual(float expected, float actual) {
			// ReSharper disable once CompareOfFloatsByEqualityOperator Exact comparison is expected here
			if (expected != actual) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FastAssertEqual(double expected, double actual) {
			// ReSharper disable once CompareOfFloatsByEqualityOperator Exact comparison is expected here
			if (expected != actual) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FastAssertEqual(float expected, float actual, float tolerance) {
			if (Math.Abs(expected - actual) > tolerance) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FastAssertEqual(double expected, double actual, double tolerance) {
			if (Math.Abs(expected - actual) > tolerance) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FastAssertEqual<TTest>(TTest expected, TTest actual) where TTest : IEquatable<TTest> {
			if (expected == null) {
				if (actual == null) return;
				Assert.Fail($"Expected <null> but was {actual}.");
			}
			if (!expected.Equals(actual)) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FastAssertEqual(object expected, object actual) {
			if (!Equals(expected, actual)) Assert.Fail($"Expected {expected} but was {actual}.");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FastAssertNotEqual(int expected, int actual) {
			if (expected == actual) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected unsafe void FastAssertNotEqual<TTest>(TTest* expected, TTest* actual) where TTest : unmanaged {
			if (expected == actual) Assert.Fail($"Expected 0x{((IntPtr) expected).ToInt64():X} to not be equal to 0x{((IntPtr) actual).ToInt64():X}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FastAssertNotEqual(long expected, long actual) {
			if (expected == actual) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FastAssertNotEqual(uint expected, uint actual) {
			if (expected == actual) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FastAssertNotEqual(ulong expected, ulong actual) {
			if (expected == actual) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FastAssertNotEqual(float expected, float actual) {
			// ReSharper disable once CompareOfFloatsByEqualityOperator Exact comparison is expected here
			if (expected == actual) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FastAssertNotEqual(double expected, double actual) {
			// ReSharper disable once CompareOfFloatsByEqualityOperator Exact comparison is expected here
			if (expected == actual) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FastAssertNotEqual(float expected, float actual, float tolerance) {
			if (Math.Abs(expected - actual) <= tolerance) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FastAssertNotEqual(double expected, double actual, double tolerance) {
			if (Math.Abs(expected - actual) <= tolerance) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FastAssertNotEqual<TTest>(TTest expected, TTest actual) where TTest : IEquatable<TTest> {
			if (expected == null) {
				if (actual != null) return;
				Assert.Fail($"Expected <null> to not be equal to {actual}.");
			}
			if (expected.Equals(actual)) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FastAssertNotEqual(object expected, object actual) {
			if (Equals(expected, actual)) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FastAssertTrue(bool condition) {
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
	}
}