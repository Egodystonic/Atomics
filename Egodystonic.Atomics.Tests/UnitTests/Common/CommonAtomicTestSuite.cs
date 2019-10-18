// (c) Egodystonic Studios 2018


using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Egodystonic.Atomics.Tests.Harness;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.UnitTests.Common {
	abstract class CommonAtomicTestSuite<T, TTarget> where TTarget : IAtomic<T>, new() {
		readonly Func<T, T, bool> _equalityFunc;
		readonly ConcurrentTestCaseRunner.RunnerFactory<T, TTarget> _runnerFactory = new ConcurrentTestCaseRunner.RunnerFactory<T, TTarget>();

		protected T Alpha { get; }
		protected T Bravo { get; }
		protected T Charlie { get; }
		protected T Delta { get; }

		protected CommonAtomicTestSuite(Func<T, T, bool> equalityFunc, T alpha, T bravo, T charlie, T delta) {
			_equalityFunc = equalityFunc ?? throw new ArgumentNullException(nameof(equalityFunc));
			Alpha = alpha;
			Bravo = bravo;
			Charlie = charlie;
			Delta = delta;
		}

		public ConcurrentTestCaseRunner<TTarget> NewRunner() => _runnerFactory.NewRunner();
		public ConcurrentTestCaseRunner<TTarget> NewRunner(T initialValue) => _runnerFactory.NewRunner(initialValue);

		// These methods provided because NUnit is too slow a lot of the time
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastAssertEqual(int expected, int actual) {
			if (expected != actual) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe void FastAssertEqual<TTest>(TTest* expected, TTest* actual) where TTest : unmanaged {
			if (expected != actual) Assert.Fail($"Expected 0x{((IntPtr) expected).ToInt64():X} but was 0x{((IntPtr) actual).ToInt64():X}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastAssertEqual(long expected, long actual) {
			if (expected != actual) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastAssertEqual(uint expected, uint actual) {
			if (expected != actual) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastAssertEqual(ulong expected, ulong actual) {
			if (expected != actual) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastAssertEqual(float expected, float actual) {
			// ReSharper disable once CompareOfFloatsByEqualityOperator Exact comparison is expected here
			if (expected != actual) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastAssertEqual(double expected, double actual) {
			// ReSharper disable once CompareOfFloatsByEqualityOperator Exact comparison is expected here
			if (expected != actual) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastAssertEqual(float expected, float actual, float tolerance) {
			if (Math.Abs(expected - actual) > tolerance) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastAssertEqual(double expected, double actual, double tolerance) {
			if (Math.Abs(expected - actual) > tolerance) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastAssertEqual<TTest>(TTest expected, TTest actual) where TTest : IEquatable<TTest> {
			if (expected == null) {
				if (actual == null) return;
				Assert.Fail($"Expected <null> but was {actual}.");
			}
			if (!expected.Equals(actual)) Assert.Fail($"Expected {expected} but was {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastAssertEqual(object expected, object actual) {
			if (!Equals(expected, actual)) Assert.Fail($"Expected {expected} but was {actual}.");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastAssertNotEqual(int expected, int actual) {
			if (expected == actual) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe void FastAssertNotEqual<TTest>(TTest* expected, TTest* actual) where TTest : unmanaged {
			if (expected == actual) Assert.Fail($"Expected 0x{((IntPtr) expected).ToInt64():X} to not be equal to 0x{((IntPtr) actual).ToInt64():X}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastAssertNotEqual(long expected, long actual) {
			if (expected == actual) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastAssertNotEqual(uint expected, uint actual) {
			if (expected == actual) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastAssertNotEqual(ulong expected, ulong actual) {
			if (expected == actual) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastAssertNotEqual(float expected, float actual) {
			// ReSharper disable once CompareOfFloatsByEqualityOperator Exact comparison is expected here
			if (expected == actual) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastAssertNotEqual(double expected, double actual) {
			// ReSharper disable once CompareOfFloatsByEqualityOperator Exact comparison is expected here
			if (expected == actual) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastAssertNotEqual(float expected, float actual, float tolerance) {
			if (Math.Abs(expected - actual) <= tolerance) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastAssertNotEqual(double expected, double actual, double tolerance) {
			if (Math.Abs(expected - actual) <= tolerance) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastAssertNotEqual<TTest>(TTest expected, TTest actual) where TTest : IEquatable<TTest> {
			if (expected == null) {
				if (actual != null) return;
				Assert.Fail($"Expected <null> to not be equal to {actual}.");
			}
			if (expected.Equals(actual)) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastAssertNotEqual(object expected, object actual) {
			if (Equals(expected, actual)) Assert.Fail($"Expected {expected} to not be equal to {actual}.");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FastAssertTrue(bool condition) {
			if (!condition) Assert.Fail($"Condition was false.");
		}

		public bool AreEqual(T lhs, T rhs) => _equalityFunc(lhs, rhs);

		public void Assert_API_Value() {
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

		public void Assert_API_GetSet() {
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