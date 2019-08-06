using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;
using Egodystonic.Atomics.Tests.Harness;
using Egodystonic.Atomics.Tests.UnitTests.Common;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.UnitTests {
	[TestFixture]
	class CopyOnReadRefAPITest : CommonAtomicTestSuite<DummyEquatableRef, CopyOnReadRefEquatableWrapper> {
		#region Test Fields
		protected override DummyEquatableRef Alpha { get; } = new DummyEquatableRef("Alpha", 111L, null);
		protected override DummyEquatableRef Bravo { get; } = new DummyEquatableRef("Bravo", 222L, null);
		protected override DummyEquatableRef Charlie { get; } = new DummyEquatableRef("Charlie", 333L, null);
		protected override DummyEquatableRef Delta { get; } = new DummyEquatableRef("Delta", 444L, null);
		protected override bool AreEqual(DummyEquatableRef lhs, DummyEquatableRef rhs) => lhs == rhs;
		#endregion

		#region Test Setup
		[OneTimeSetUp]
		public void SetUpClass() { }

		[OneTimeTearDown]
		public void TearDownClass() { }

		[SetUp]
		public void SetUpTest() { }

		[TearDown]
		public void TearDownTest() { }
		#endregion

		#region Tests
		[Test]
		public void API_SpinWaitForExchange_NonEquatable() {
			var target = new CopyOnReadRef<DummyImmutableRef>(i => i == null ? null : new DummyImmutableRef(i.StringProp, i.LongProp, i.RefProp));

			var alpha = new DummyImmutableRef("Alpha", 111L, null);
			var bravo = new DummyImmutableRef("Bravo", 222L, null);
			var charlie = new DummyImmutableRef("Charlie", 333L, null);
			var delta = new DummyImmutableRef("Delta", 444L, null);

			// (T, T)
			target.Set(alpha);
			var exchRes = target.SpinWaitForExchange(bravo, alpha);
			Assert.True(AreValueEqual(alpha, exchRes.PreviousValue));
			Assert.True(AreValueEqual(bravo, exchRes.CurrentValue));
			var spinWaitTask = Task.Run(() => target.SpinWaitForExchange(charlie, delta));
			Thread.Sleep(100); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(delta);
			Assert.True(AreValueEqual(delta, spinWaitTask.Result.PreviousValue));
			Assert.True(AreValueEqual(charlie, spinWaitTask.Result.CurrentValue));

			// (Func<T, TContext, T>, T, TContext)
			target.Set(alpha);
			spinWaitTask = Task.Run(() => target.SpinWaitForExchange((t, ctx) => AreValueEqual(t, ctx) ? bravo : delta, charlie, charlie));
			Thread.Sleep(100); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(charlie);
			Assert.True(AreValueEqual(charlie, spinWaitTask.Result.PreviousValue));
			Assert.True(AreValueEqual(bravo, spinWaitTask.Result.CurrentValue));
			spinWaitTask = Task.Run(() => target.SpinWaitForExchange((t, ctx) => AreValueEqual(t, ctx) ? alpha : bravo, charlie, delta));
			Thread.Sleep(100); // Give the test time to fail
			Assert.False(spinWaitTask.IsCompleted);
			target.Set(delta);
			Assert.True(AreValueEqual(delta, spinWaitTask.Result.PreviousValue));
			Assert.True(AreValueEqual(bravo, spinWaitTask.Result.CurrentValue));
		}

		[Test]
		public void API_FastTryExchange_NonEquatable() {
			var target = new CopyOnReadRef<DummyImmutableRef>(i => i == null ? null : new DummyImmutableRef(i.StringProp, i.LongProp, i.RefProp));

			var alpha = new DummyImmutableRef("Alpha", 111L, null);
			var bravo = new DummyImmutableRef("Bravo", 222L, null);
			var charlie = new DummyImmutableRef("Charlie", 333L, null);
			var delta = new DummyImmutableRef("Delta", 444L, null);

			// (T, T)
			target.Set(alpha);
			var newValue = bravo;
			var prevValue = target.FastTryExchange(newValue, alpha);
			var wasSet = AreValueEqual(newValue, target.Value);
			var setValue = wasSet ? newValue : prevValue;
			Assert.AreEqual(true, wasSet);
			Assert.True(AreValueEqual(alpha, prevValue));
			Assert.True(AreValueEqual(bravo, setValue));

			newValue = delta;
			prevValue = target.FastTryExchange(newValue, charlie);
			wasSet = AreValueEqual(newValue, target.Value);
			setValue = wasSet ? newValue : prevValue;
			Assert.AreEqual(false, wasSet);
			Assert.True(AreValueEqual(bravo, prevValue));
			Assert.True(AreValueEqual(bravo, setValue));
		}

		[Test]
		public void API_FastTryExchangeRefOnly() {
			var target = new CopyOnReadRef<DummyEquatableRef>(i => i == null ? null : new DummyEquatableRef(i.StringProp, i.LongProp, i.RefProp));

			var alpha = new DummyEquatableRef("Alpha", 111L, null);
			var bravo = new DummyEquatableRef("Bravo", 222L, null);
			var charlie = new DummyEquatableRef("Charlie", 333L, null);

			// (T, T)
			target.Set(alpha);
			var newValue = bravo;
			var prevValue = target.FastTryExchangeRefOnly(newValue, alpha);
			var wasSet = AreValueEqual(newValue, target.Value);
			var setValue = wasSet ? newValue : prevValue;
			Assert.AreEqual(true, wasSet);
			Assert.True(alpha.Equals(prevValue));
			Assert.True(bravo.Equals(setValue));

			newValue = charlie;
			prevValue = target.FastTryExchangeRefOnly(newValue, new DummyEquatableRef(bravo.StringProp, bravo.LongProp, bravo.RefProp));
			wasSet = AreValueEqual(newValue, target.Value);
			setValue = wasSet ? newValue : prevValue;
			Assert.AreEqual(false, wasSet);
			Assert.True(bravo.Equals(prevValue));
			Assert.True(bravo.Equals(setValue));
			Assert.True(bravo.Equals(target.Value));
		}

		[Test]
		public void API_TryExchange_NonEquatable() {
			var target = new CopyOnReadRef<DummyImmutableRef>(i => i == null ? null : new DummyImmutableRef(i.StringProp, i.LongProp, i.RefProp));

			var alpha = new DummyImmutableRef("Alpha", 111L, null);
			var bravo = new DummyImmutableRef("Bravo", 222L, null);
			var charlie = new DummyImmutableRef("Charlie", 333L, null);
			var delta = new DummyImmutableRef("Delta", 444L, null);

			// (T, T)
			target.Set(alpha);
			var exchRes = target.TryExchange(bravo, alpha);
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.True(AreValueEqual(alpha, exchRes.PreviousValue));
			Assert.True(AreValueEqual(bravo, exchRes.CurrentValue));
			exchRes = target.TryExchange(delta, charlie);
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.True(AreValueEqual(bravo, exchRes.PreviousValue));
			Assert.True(AreValueEqual(bravo, exchRes.CurrentValue));

			// (Func<T, TContext, T>, T, TContext)
			target.Set(alpha);
			exchRes = target.TryExchange((t, ctx) => AreValueEqual(t, ctx) ? bravo : delta, alpha, alpha);
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.True(AreValueEqual(alpha, exchRes.PreviousValue));
			Assert.True(AreValueEqual(bravo, exchRes.CurrentValue));
			exchRes = target.TryExchange((t, ctx) => AreValueEqual(t, ctx) ? bravo : delta, charlie, charlie);
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.True(AreValueEqual(bravo, exchRes.PreviousValue));
			Assert.True(AreValueEqual(bravo, exchRes.CurrentValue));
		}

		bool AreValueEqual(DummyImmutableRef lhs, DummyImmutableRef rhs) {
			return lhs == null && rhs == null
				|| lhs.StringProp == rhs.StringProp && lhs.LongProp == rhs.LongProp && AreValueEqual(lhs.RefProp, rhs.RefProp);
		}

		bool AreValueEqual(DummyEquatableRef lhs, DummyEquatableRef rhs) {
			return lhs == null && rhs == null
				|| lhs.StringProp == rhs.StringProp && lhs.LongProp == rhs.LongProp && AreValueEqual(lhs.RefProp, rhs.RefProp);
		}
		#endregion Tests
	}

	[TestFixture]
	class CopyOnReadRefConcurrencyTest {
		#region Test Fields
		#endregion

		#region Test Setup
		[OneTimeSetUp]
		public void SetUpClass() { }

		[OneTimeTearDown]
		public void TearDownClass() { }

		[SetUp]
		public void SetUpTest() { }

		[TearDown]
		public void TearDownTest() { }
		#endregion

		#region Fast Assertions
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
		#endregion

		#region Tests
		static ConcurrentTestCaseRunner<CopyOnReadRef<DummyCopyCountingRef>> NewRunner() {
			return new ConcurrentTestCaseRunner<CopyOnReadRef<DummyCopyCountingRef>>(() => new CopyOnReadRef<DummyCopyCountingRef>(r => r?.Copy()));
		}
		static ConcurrentTestCaseRunner<CopyOnReadRef<DummyCopyCountingRef>> NewRunner(DummyCopyCountingRef initialValue) {
			return new ConcurrentTestCaseRunner<CopyOnReadRef<DummyCopyCountingRef>>(() => new CopyOnReadRef<DummyCopyCountingRef>(r => r?.Copy(), initialValue));
		}

		static ConcurrentTestCaseRunner<CopyOnReadRef<DummyImmutableRef>> NewImmutableRunner() {
			return new ConcurrentTestCaseRunner<CopyOnReadRef<DummyImmutableRef>>(() => new CopyOnReadRef<DummyImmutableRef>(r => r == null ? null : new DummyImmutableRef(r.StringProp, r.LongProp, r.RefProp)));
		}
		static ConcurrentTestCaseRunner<CopyOnReadRef<DummyImmutableRef>> NewImmutableRunner(DummyImmutableRef initialValue) {
			return new ConcurrentTestCaseRunner<CopyOnReadRef<DummyImmutableRef>>(() => new CopyOnReadRef<DummyImmutableRef>(r => r == null ? null : new DummyImmutableRef(r.StringProp, r.LongProp, r.RefProp), initialValue));
		}

		[Test]
		public void GetAndSetAndValue() {
			const int NumIterations = 1_000_000;
			var atomicLong = new AtomicInt64(0L);
			var runner = NewRunner(new DummyCopyCountingRef(0L));

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => target.Set(new DummyCopyCountingRef(atomicLong.Increment().CurrentValue)),
				NumIterations,
				target => target.Get(),
				(prev, cur) => AssertTrue(prev.LongProp <= cur.LongProp)
			);

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => target.Value = new DummyCopyCountingRef(atomicLong.Increment().CurrentValue),
				NumIterations,
				target => target.Value,
				(prev, cur) => AssertTrue(prev.LongProp <= cur.LongProp)
			);

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => target.Set(new DummyCopyCountingRef(atomicLong.Increment().CurrentValue)),
				NumIterations,
				target => target.Get(),
				(prev, cur) => {
					if (prev.LongProp == cur.LongProp) AssertIsCopy(prev, cur);
				}
			);

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => target.Value = new DummyCopyCountingRef(atomicLong.Increment().CurrentValue),
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					if (prev.LongProp == cur.LongProp) AssertIsCopy(prev, cur);
				}
			);
		}

		[Test]
		public void SpinWaitForValue() {
			const int NumIterations = 30_000;

			var runner = NewRunner(new DummyCopyCountingRef(0L));

			// (T)
			var valuesArr = Enumerable.Range(0, NumIterations + 1)
				.Select(i => new DummyCopyCountingRef((long) i))
				.ToArray();
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.LongProp == NumIterations) break;
						if ((curVal.LongProp & 1) == 0) {
							var targetVal = valuesArr[(int) (curVal.LongProp + 1L)];
							var spinWaitRes = target.SpinWaitForValue(targetVal);
							AssertAreEqual(curVal.LongProp + 1, spinWaitRes.LongProp);
							AssertIsCopy(targetVal, spinWaitRes);
						}
						else {
							target.Value = valuesArr[(int) (curVal.LongProp + 1L)];
						}
					}
				},
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.LongProp == NumIterations) break;
						if ((curVal.LongProp & 1) == 1) {
							var targetVal = valuesArr[(int) (curVal.LongProp + 1L)];
							var spinWaitRes = target.SpinWaitForValue(targetVal);
							AssertAreEqual(curVal.LongProp + 1, spinWaitRes.LongProp);
							AssertIsCopy(targetVal, spinWaitRes);
						}
						else {
							target.Value = valuesArr[(int) (curVal.LongProp + 1L)];
						}
					}
				}
			);

			// (Func<T, bool>)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteWriterReaderTests(
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.LongProp == NumIterations) break;
						var newVal = new DummyCopyCountingRef(curVal.LongProp + 1L);
						var tryExchRes = target.TryExchange(newVal, curVal);
						if (tryExchRes.ValueWasSet) {
							AssertIsCopy(curVal, tryExchRes.PreviousValue);
							AssertIsCopy(newVal, tryExchRes.CurrentValue);
						}
						else {
							AssertIsCopy(tryExchRes.PreviousValue);
							AssertIsCopy(tryExchRes.CurrentValue);
						}
					}
				},
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.LongProp == NumIterations) break;
						var spinWaitRes = target.SpinWaitForValue(c => c.LongProp > curVal.LongProp);
						AssertTrue(spinWaitRes.LongProp >= curVal.LongProp);
						AssertIsCopy(spinWaitRes);
					}
				}
			);

			// (Func<T, TContext, bool>, TContext)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteWriterReaderTests(
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.LongProp == NumIterations) break;
						var newVal = new DummyCopyCountingRef(curVal.LongProp + 1L);
						var tryExchRes = target.TryExchange(newVal, curVal);
						if (tryExchRes.ValueWasSet) {
							AssertIsCopy(curVal, tryExchRes.PreviousValue);
							AssertIsCopy(newVal, tryExchRes.CurrentValue);
						}
						else {
							AssertIsCopy(tryExchRes.PreviousValue);
							AssertIsCopy(tryExchRes.CurrentValue);
						}
					}
				},
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.LongProp == NumIterations) break;
						var spinWaitRes = target.SpinWaitForValue((c, ctx) => c.LongProp > ctx.LongProp, curVal);
						AssertTrue(spinWaitRes.LongProp >= curVal.LongProp);
						AssertIsCopy(spinWaitRes);
					}
				}
			);
		}

		[Test]
		public void Exchange() {
			const int NumIterations = 300_000;
			var runner = NewRunner(new DummyCopyCountingRef(0L));

			// (T)
			var atomicLong = new AtomicInt64(0L);
			runner.GlobalSetUp = (_, __) => atomicLong.Set(0L);
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					var newLongValue = atomicLong.Increment().CurrentValue;
					var prev = target.Exchange(new DummyCopyCountingRef(newLongValue)).PreviousValue;
					AssertAreEqual(prev.LongProp, newLongValue - 1L);
					AssertIsCopy(prev);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					AssertTrue(cur.LongProp >= prev.LongProp);
					if (cur.Equals(prev)) AssertIsCopy(cur, prev);
					else {
						AssertIsCopy(cur);
						AssertIsCopy(prev);
					}
				}
			);
			runner.GlobalSetUp = null;
			runner.AllThreadsTearDown = null;

			// (Func<T, T>)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					var (prevValue, CurrentValue) = target.Exchange(c => new DummyCopyCountingRef(c.LongProp + 1L));
					AssertAreEqual(prevValue.LongProp, CurrentValue.LongProp - 1L);
					AssertIsCopy(prevValue);
					AssertIsCopy(CurrentValue);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					AssertTrue(cur.LongProp >= prev.LongProp);
					if (cur.Equals(prev)) AssertIsCopy(cur, prev);
					else {
						AssertIsCopy(cur);
						AssertIsCopy(prev);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TContext, T>)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations * 10L, target.Value.LongProp);
			runner.ExecuteFreeThreadedTests(
				target => {
					var (prevValue, CurrentValue) = target.Exchange((c, ctx) => new DummyCopyCountingRef(c.LongProp + ctx), 10L);
					AssertAreEqual(prevValue.LongProp, CurrentValue.LongProp - 10L);
					AssertIsCopy(prevValue);
					AssertIsCopy(CurrentValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		[SuppressMessage("ReSharper", "AccessToModifiedClosure")] // Closure is always modified after use 
		public void SpinWaitForExchangeWithoutContext() {
			const int NumIterations = 30_000;

			var runner = NewRunner(new DummyCopyCountingRef("0", 0L));

			// (T, T)
			var valuesArr = Enumerable.Range(0, NumIterations + 1)
				.Select(i => new DummyCopyCountingRef(i.ToString(), (long) i))
				.ToArray();
			runner.GlobalSetUp = (target, _) => target.Value = valuesArr[0];
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.LongProp);
				AssertAreEqual(NumIterations.ToString(), target.Value.StringProp);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(valuesArr[nextVal + 1], valuesArr[nextVal]);
						AssertAreEqual(nextVal, exchRes.PreviousValue.LongProp);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.LongProp);
						AssertAreEqual(nextVal.ToString(), exchRes.PreviousValue.StringProp);
						AssertAreEqual((nextVal + 1).ToString(), exchRes.CurrentValue.StringProp);
						AssertIsCopy(valuesArr[nextVal], exchRes.PreviousValue);
						AssertIsCopy(valuesArr[nextVal + 1], exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(valuesArr[nextVal + 1], valuesArr[nextVal]);
						AssertAreEqual(nextVal, exchRes.PreviousValue.LongProp);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.LongProp);
						AssertAreEqual(nextVal.ToString(), exchRes.PreviousValue.StringProp);
						AssertAreEqual((nextVal + 1).ToString(), exchRes.CurrentValue.StringProp);
						AssertIsCopy(valuesArr[nextVal], exchRes.PreviousValue);
						AssertIsCopy(valuesArr[nextVal + 1], exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, T)
			valuesArr = Enumerable.Range(0, NumIterations + 1)
				.Select(i => new DummyCopyCountingRef((-i).ToString(), (long) i))
				.ToArray();
			runner.GlobalSetUp = (target, _) => target.Value = valuesArr[0];
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.LongProp);
				AssertAreEqual((-NumIterations).ToString(), target.Value.StringProp);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => valuesArr[c.LongProp + 1], valuesArr[nextVal]);
						AssertAreEqual(nextVal, exchRes.PreviousValue.LongProp);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.LongProp);
						AssertAreEqual((-nextVal).ToString(), exchRes.PreviousValue.StringProp);
						AssertAreEqual((-(nextVal + 1)).ToString(), exchRes.CurrentValue.StringProp);
						AssertIsCopy(valuesArr[nextVal], exchRes.PreviousValue);
						AssertIsCopy(valuesArr[nextVal + 1], exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => valuesArr[c.LongProp + 1], valuesArr[nextVal]);
						AssertAreEqual(nextVal, exchRes.PreviousValue.LongProp);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.LongProp);
						AssertAreEqual((-nextVal).ToString(), exchRes.PreviousValue.StringProp);
						AssertAreEqual((-(nextVal + 1)).ToString(), exchRes.CurrentValue.StringProp);
						AssertIsCopy(valuesArr[nextVal], exchRes.PreviousValue);
						AssertIsCopy(valuesArr[nextVal + 1], exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (T, Func<T, T, bool>)
			valuesArr = Enumerable.Range(0, NumIterations + 1)
				.Select(i => new DummyCopyCountingRef(i.ToString(), (long) i))
				.ToArray();
			runner.GlobalSetUp = (target, _) => target.Value = valuesArr[0];
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.LongProp);
				AssertAreEqual(NumIterations.ToString(), target.Value.StringProp);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(valuesArr[nextVal + 1], (c, n) => n.LongProp == c.LongProp + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.LongProp);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.LongProp);
						AssertAreEqual(nextVal.ToString(), exchRes.PreviousValue.StringProp);
						AssertAreEqual((nextVal + 1).ToString(), exchRes.CurrentValue.StringProp);
						AssertIsCopy(valuesArr[nextVal], exchRes.PreviousValue);
						AssertIsCopy(valuesArr[nextVal + 1], exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(valuesArr[nextVal + 1], (c, n) => n.LongProp == c.LongProp + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.LongProp);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.LongProp);
						AssertAreEqual(nextVal.ToString(), exchRes.PreviousValue.StringProp);
						AssertAreEqual((nextVal + 1).ToString(), exchRes.CurrentValue.StringProp);
						AssertIsCopy(valuesArr[nextVal], exchRes.PreviousValue);
						AssertIsCopy(valuesArr[nextVal + 1], exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, Func<T, T, bool>)
			valuesArr = Enumerable.Range(0, NumIterations + 1)
				.Select(i => new DummyCopyCountingRef((-i).ToString(), (long) i))
				.ToArray();
			runner.GlobalSetUp = (target, _) => target.Value = valuesArr[0];
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.LongProp);
				AssertAreEqual((-NumIterations).ToString(), target.Value.StringProp);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => valuesArr[c.LongProp + 1], (c, n) => n.LongProp == c.LongProp + 1 && c.LongProp == nextVal);
						AssertAreEqual(nextVal, exchRes.PreviousValue.LongProp);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.LongProp);
						AssertAreEqual((-nextVal).ToString(), exchRes.PreviousValue.StringProp);
						AssertAreEqual((-(nextVal + 1)).ToString(), exchRes.CurrentValue.StringProp);
						AssertIsCopy(valuesArr[nextVal], exchRes.PreviousValue);
						AssertIsCopy(valuesArr[nextVal + 1], exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => valuesArr[c.LongProp + 1], (c, n) => n.LongProp == c.LongProp + 1 && c.LongProp == nextVal);
						AssertAreEqual(nextVal, exchRes.PreviousValue.LongProp);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.LongProp);
						AssertAreEqual((-nextVal).ToString(), exchRes.PreviousValue.StringProp);
						AssertAreEqual((-(nextVal + 1)).ToString(), exchRes.CurrentValue.StringProp);
						AssertIsCopy(valuesArr[nextVal], exchRes.PreviousValue);
						AssertIsCopy(valuesArr[nextVal + 1], exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		[SuppressMessage("ReSharper", "AccessToModifiedClosure")] // Closure is always modified after use 
		public void SpinWaitForExchangeWithContext() {
			const int NumIterations = 10_000;

			var runner = NewRunner(new DummyCopyCountingRef("0", 0L));

			// (Func<T, TContext, T>, T)
			var valuesArr = Enumerable.Range(0, NumIterations + 1)
				.Select(i => new DummyCopyCountingRef((-i).ToString(), (long) i))
				.ToArray();
			runner.GlobalSetUp = (target, _) => target.Value = valuesArr[0];
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.LongProp);
				AssertAreEqual((-NumIterations).ToString(), target.Value.StringProp);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => ctx[c.LongProp + 1], valuesArr, valuesArr[nextVal]);
						AssertAreEqual(nextVal, exchRes.PreviousValue.LongProp);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.LongProp);
						AssertAreEqual((-nextVal).ToString(), exchRes.PreviousValue.StringProp);
						AssertAreEqual((-(nextVal + 1)).ToString(), exchRes.CurrentValue.StringProp);
						AssertIsCopy(valuesArr[nextVal], exchRes.PreviousValue);
						AssertIsCopy(valuesArr[nextVal + 1], exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => valuesArr[c.LongProp + 1], valuesArr, valuesArr[nextVal]);
						AssertAreEqual(nextVal, exchRes.PreviousValue.LongProp);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.LongProp);
						AssertAreEqual((-nextVal).ToString(), exchRes.PreviousValue.StringProp);
						AssertAreEqual((-(nextVal + 1)).ToString(), exchRes.CurrentValue.StringProp);
						AssertIsCopy(valuesArr[nextVal], exchRes.PreviousValue);
						AssertIsCopy(valuesArr[nextVal + 1], exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (T, Func<T, T, TContext, bool>)
			valuesArr = Enumerable.Range(0, NumIterations + 1)
				.Select(i => new DummyCopyCountingRef(i.ToString(), (long) i))
				.ToArray();
			runner.GlobalSetUp = (target, _) => target.Value = valuesArr[0];
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.LongProp);
				AssertAreEqual(NumIterations.ToString(), target.Value.StringProp);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(valuesArr[nextVal + 1], (c, n, ctx) => n.LongProp == c.LongProp + ctx, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.LongProp);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.LongProp);
						AssertAreEqual(nextVal.ToString(), exchRes.PreviousValue.StringProp);
						AssertAreEqual((nextVal + 1).ToString(), exchRes.CurrentValue.StringProp);
						AssertIsCopy(valuesArr[nextVal], exchRes.PreviousValue);
						AssertIsCopy(valuesArr[nextVal + 1], exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(valuesArr[nextVal + 1], (c, n, ctx) => n.LongProp == c.LongProp + ctx, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.LongProp);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.LongProp);
						AssertAreEqual(nextVal.ToString(), exchRes.PreviousValue.StringProp);
						AssertAreEqual((nextVal + 1).ToString(), exchRes.CurrentValue.StringProp);
						AssertIsCopy(valuesArr[nextVal], exchRes.PreviousValue);
						AssertIsCopy(valuesArr[nextVal + 1], exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;


			// (Func<T, TMapContext, T>, Func<T, T, TPredicateContext, bool>)
			valuesArr = Enumerable.Range(0, NumIterations + 1)
				.Select(i => new DummyCopyCountingRef((-i).ToString(), (long) i))
				.ToArray();
			runner.GlobalSetUp = (target, _) => target.Value = valuesArr[0];
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.LongProp);
				AssertAreEqual((-NumIterations).ToString(), target.Value.StringProp);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => ctx[c.LongProp + 1], valuesArr, (c, n, ctx) => n.LongProp == c.LongProp + ctx && c.LongProp == nextVal, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.LongProp);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.LongProp);
						AssertAreEqual((-nextVal).ToString(), exchRes.PreviousValue.StringProp);
						AssertAreEqual((-(nextVal + 1)).ToString(), exchRes.CurrentValue.StringProp);
						AssertIsCopy(valuesArr[nextVal], exchRes.PreviousValue);
						AssertIsCopy(valuesArr[nextVal + 1], exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => ctx[c.LongProp + 1], valuesArr, (c, n, ctx) => n.LongProp == c.LongProp + ctx && c.LongProp == nextVal, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.LongProp);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.LongProp);
						AssertAreEqual((-nextVal).ToString(), exchRes.PreviousValue.StringProp);
						AssertAreEqual((-(nextVal + 1)).ToString(), exchRes.CurrentValue.StringProp);
						AssertIsCopy(valuesArr[nextVal], exchRes.PreviousValue);
						AssertIsCopy(valuesArr[nextVal + 1], exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TContext, T>, Func<T, T, bool>)
			valuesArr = Enumerable.Range(0, NumIterations + 1)
				.Select(i => new DummyCopyCountingRef((-i).ToString(), (long) i))
				.ToArray();
			runner.GlobalSetUp = (target, _) => target.Value = valuesArr[0];
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.LongProp);
				AssertAreEqual((-NumIterations).ToString(), target.Value.StringProp);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => valuesArr[c.LongProp + ctx], 1, (c, n) => n.LongProp == c.LongProp + 1 && c.LongProp == nextVal);
						AssertAreEqual(nextVal, exchRes.PreviousValue.LongProp);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.LongProp);
						AssertAreEqual((-nextVal).ToString(), exchRes.PreviousValue.StringProp);
						AssertAreEqual((-(nextVal + 1)).ToString(), exchRes.CurrentValue.StringProp);
						AssertIsCopy(valuesArr[nextVal], exchRes.PreviousValue);
						AssertIsCopy(valuesArr[nextVal + 1], exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => valuesArr[c.LongProp + ctx], 1, (c, n) => n.LongProp == c.LongProp + 1 && c.LongProp == nextVal);
						AssertAreEqual(nextVal, exchRes.PreviousValue.LongProp);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.LongProp);
						AssertAreEqual((-nextVal).ToString(), exchRes.PreviousValue.StringProp);
						AssertAreEqual((-(nextVal + 1)).ToString(), exchRes.CurrentValue.StringProp);
						AssertIsCopy(valuesArr[nextVal], exchRes.PreviousValue);
						AssertIsCopy(valuesArr[nextVal + 1], exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, Func<T, T, TContext, bool>)
			valuesArr = Enumerable.Range(0, NumIterations + 1)
				.Select(i => new DummyCopyCountingRef((-i).ToString(), (long) i))
				.ToArray();
			runner.GlobalSetUp = (target, _) => target.Value = valuesArr[0];
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.LongProp);
				AssertAreEqual((-NumIterations).ToString(), target.Value.StringProp);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => valuesArr[c.LongProp + 1], (c, n, ctx) => n.LongProp == c.LongProp + ctx && c.LongProp == nextVal, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.LongProp);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.LongProp);
						AssertAreEqual((-nextVal).ToString(), exchRes.PreviousValue.StringProp);
						AssertAreEqual((-(nextVal + 1)).ToString(), exchRes.CurrentValue.StringProp);
						AssertIsCopy(valuesArr[nextVal], exchRes.PreviousValue);
						AssertIsCopy(valuesArr[nextVal + 1], exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => valuesArr[c.LongProp + 1], (c, n, ctx) => n.LongProp == c.LongProp + ctx && c.LongProp == nextVal, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.LongProp);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.LongProp);
						AssertAreEqual((-nextVal).ToString(), exchRes.PreviousValue.StringProp);
						AssertAreEqual((-(nextVal + 1)).ToString(), exchRes.CurrentValue.StringProp);
						AssertIsCopy(valuesArr[nextVal], exchRes.PreviousValue);
						AssertIsCopy(valuesArr[nextVal + 1], exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void FastTryExchangeRefOnly() {
			const int NumIterations = 30_000;

			var runner = NewImmutableRunner(new DummyImmutableRef("0", 0L));

			// (T, T)
			runner.ExecuteContinuousCoherencyTests(
				target => {
					while (true) {
						var curValue = target.GetWithoutCopy();
						if (curValue.LongProp == NumIterations) return;
						var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
						var prevValue = target.FastTryExchangeRefOnly(newValue, curValue);
						AssertTrue(prevValue.LongProp >= curValue.LongProp);
						if (prevValue.LongProp >= NumIterations) return;
					}
				},
				t => t.Value.LongProp,
				(prev, cur) => AssertTrue(prev <= cur),
				l => l >= NumIterations
			);
		}

		[Test]
		public void FastTryExchange() {
			const int NumIterations = 100_000;

			var runner = NewRunner(new DummyCopyCountingRef(0L));

			// (T, T)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.LongProp == NumIterations) return;
						var newValue = new DummyCopyCountingRef(curValue.LongProp + 1L);
						var prevValue = target.FastTryExchange(newValue, curValue);
						var wasSet = prevValue.Equals(curValue);
						var setValue = wasSet ? newValue : prevValue;
						if (wasSet) {
							AssertAreEqualObjects(curValue, prevValue);
							AssertAreEqualObjects(newValue, setValue);
							AssertIsCopy(curValue, prevValue);
						}
						else {
							AssertAreNotEqualObjects(curValue, prevValue);
							AssertAreEqualObjects(setValue, prevValue);
							AssertIsCopy(prevValue);
							AssertIsCopy(setValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void TryExchangeWithoutContext() {
			const int NumIterations = 100_000;

			var runner = NewRunner(new DummyCopyCountingRef(0L));

			// (T, T)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.LongProp == NumIterations) return;
						var newValue= new DummyCopyCountingRef(curValue.LongProp + 1L);
						var (wasSet, prevValue, setValue) = target.TryExchange(newValue, curValue);
						if (wasSet) {
							AssertAreEqualObjects(curValue, prevValue);
							AssertAreEqualObjects(newValue, setValue);
							AssertIsCopy(curValue, prevValue);
							AssertIsCopy(newValue, setValue);
						}
						else {
							AssertAreNotEqualObjects(curValue, prevValue);
							AssertAreEqualObjects(setValue, prevValue);
							AssertIsCopy(prevValue);
							AssertIsCopy(setValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// Func(T, Func<T, T, bool>)
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curValue = target.Value;
					var newValue = new DummyCopyCountingRef(curValue.LongProp + 1L);
					var tryExchRes = target.TryExchange(newValue, (c, n) => c.LongProp == n.LongProp - 1L);
					if (tryExchRes.ValueWasSet) {
						AssertIsCopy(curValue, tryExchRes.PreviousValue);
						AssertIsCopy(newValue, tryExchRes.CurrentValue);
					}
					else {
						AssertIsCopy(tryExchRes.PreviousValue);
						AssertIsCopy(tryExchRes.CurrentValue);
					}
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					AssertTrue(cur.LongProp >= prev.LongProp);
					if (cur.Equals(prev)) AssertIsCopy(cur, prev);
					else {
						AssertIsCopy(cur);
						AssertIsCopy(prev);
					}
				}
			);

			// (Func<T, T>, T)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.LongProp == NumIterations) return;
						var (wasSet, prevValue, CurrentValue) = target.TryExchange(c => new DummyCopyCountingRef(c.LongProp + 1L), curValue);
						if (wasSet) {
							AssertAreEqualObjects(curValue, prevValue);
							AssertAreEqualObjects(prevValue.LongProp + 1L, CurrentValue.LongProp);
							AssertIsCopy(curValue, prevValue);
							AssertIsCopy(CurrentValue);
						}
						else {
							AssertAreNotEqualObjects(curValue, prevValue);
							AssertIsCopy(prevValue);
							AssertIsCopy(CurrentValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, Func<T, T, bool>)
			runner.ExecuteFreeThreadedTests(
				target => {
					var (wasSet, prevValue, CurrentValue) = target.TryExchange(c => new DummyCopyCountingRef(c.LongProp + 1L), (c, n) => c.LongProp == n.LongProp - 1L);
					if (wasSet) AssertAreEqual(prevValue.LongProp + 1L, CurrentValue.LongProp);
					else AssertAreNotEqual(prevValue.LongProp + 1L, CurrentValue.LongProp);

					AssertIsCopy(prevValue);
					AssertIsCopy(CurrentValue);
				},
				NumIterations
			);
		}

		[Test]
		public void TryExchangeWithContext() {
			const int NumIterations = 100_000;

			var runner = NewRunner(new DummyCopyCountingRef(0L));

			// Func(T, Func<T, T, TContext, bool>)
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curValue = target.Value;
					var newValue = new DummyCopyCountingRef(curValue.LongProp + 1L);
					var tryExchRes = target.TryExchange(newValue, (c, n, ctx) => c.LongProp == n.LongProp - ctx, 1L);
					if (tryExchRes.ValueWasSet) {
						AssertIsCopy(curValue, tryExchRes.PreviousValue);
						AssertIsCopy(newValue, tryExchRes.CurrentValue);
					}
					else {
						AssertIsCopy(tryExchRes.PreviousValue);
						AssertIsCopy(tryExchRes.CurrentValue);
					}
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					AssertTrue(cur.LongProp >= prev.LongProp);
					if (cur.Equals(prev)) AssertIsCopy(cur, prev);
					else {
						AssertIsCopy(cur);
						AssertIsCopy(prev);
					}
				}
			);

			// (Func<T, TContext, T>, T)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.LongProp == NumIterations) return;
						var (wasSet, prevValue, CurrentValue) = target.TryExchange((c, ctx) => new DummyCopyCountingRef(c.LongProp + ctx), 1L, curValue);
						if (wasSet) {
							AssertAreEqualObjects(curValue, prevValue);
							AssertAreEqualObjects(prevValue.LongProp + 1L, CurrentValue.LongProp);
							AssertIsCopy(curValue, prevValue);
							AssertIsCopy(CurrentValue);
						}
						else {
							AssertAreNotEqualObjects(curValue, prevValue);
							AssertIsCopy(prevValue);
							AssertIsCopy(CurrentValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TContext, T>, Func<T, T, bool>)
			runner.ExecuteFreeThreadedTests(
				target => {
					var (wasSet, prevValue, CurrentValue) = target.TryExchange((c, ctx) => new DummyCopyCountingRef(c.LongProp + ctx), 1L, (c, n) => c.LongProp == n.LongProp - 1L);
					if (wasSet) AssertAreEqual(prevValue.LongProp + 1L, CurrentValue.LongProp);
					else AssertAreNotEqual(prevValue.LongProp + 1L, CurrentValue.LongProp);

					AssertIsCopy(prevValue);
					AssertIsCopy(CurrentValue);
				},
				NumIterations
			);

			// (Func<T, T>, Func<T, T, TContext, bool>)
			runner.ExecuteFreeThreadedTests(
				target => {
					var (wasSet, prevValue, CurrentValue) = target.TryExchange(c => new DummyCopyCountingRef(c.LongProp + 1L), (c, n, ctx) => c.LongProp == n.LongProp - ctx, 1L);
					if (wasSet) AssertAreEqual(prevValue.LongProp + 1L, CurrentValue.LongProp);
					else AssertAreNotEqual(prevValue.LongProp + 1L, CurrentValue.LongProp);

					AssertIsCopy(prevValue);
					AssertIsCopy(CurrentValue);
				},
				NumIterations
			);

			// (Func<T, TMapContext, T>, Func<T, T, TPredicateContext, bool>)
			runner.ExecuteFreeThreadedTests(
				target => {
					var (wasSet, prevValue, CurrentValue) = target.TryExchange((c, ctx) => new DummyCopyCountingRef(c.LongProp + (ctx - 1L)), 2L, (c, n, ctx) => c.LongProp == n.LongProp - ctx, 1L);
					if (wasSet) AssertAreEqual(prevValue.LongProp + 1L, CurrentValue.LongProp);
					else AssertAreNotEqual(prevValue.LongProp + 1L, CurrentValue.LongProp);

					AssertIsCopy(prevValue);
					AssertIsCopy(CurrentValue);
				},
				NumIterations
			);
		}

		[Test]
		public void SpinWaitForExchangeWithoutContext_NonEquatable() {
			const int NumIterations = 30_000;

			var runner = NewImmutableRunner(new DummyImmutableRef("0", 0L));

			// (T, T)
			var valuesArr = Enumerable.Range(0, NumIterations + 1)
				.Select(i => new DummyImmutableRef(i.ToString(), (long) i))
				.ToArray();
			runner.GlobalSetUp = (target, _) => target.Value = valuesArr[0];
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.LongProp);
				AssertAreEqual(NumIterations.ToString(), target.Value.StringProp);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(valuesArr[nextVal + 1], valuesArr[nextVal]);
						AssertAreEqual(nextVal, exchRes.PreviousValue.LongProp);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.LongProp);
						AssertAreEqual(nextVal.ToString(), exchRes.PreviousValue.StringProp);
						AssertAreEqual((nextVal + 1).ToString(), exchRes.CurrentValue.StringProp);
						AssertIsCopy(valuesArr[nextVal], exchRes.PreviousValue);
						AssertIsCopy(valuesArr[nextVal + 1], exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(valuesArr[nextVal + 1], valuesArr[nextVal]);
						AssertAreEqual(nextVal, exchRes.PreviousValue.LongProp);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.LongProp);
						AssertAreEqual(nextVal.ToString(), exchRes.PreviousValue.StringProp);
						AssertAreEqual((nextVal + 1).ToString(), exchRes.CurrentValue.StringProp);
						AssertIsCopy(valuesArr[nextVal], exchRes.PreviousValue);
						AssertIsCopy(valuesArr[nextVal + 1], exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void SpinWaitForExchangeWithContext_NonEquatable() {
			const int NumIterations = 30_000;

			var runner = NewImmutableRunner(new DummyImmutableRef("0", 0L));

			// (Func<T, TContext, T>, T)
			var valuesArr = Enumerable.Range(0, NumIterations + 1)
				.Select(i => new DummyImmutableRef((-i).ToString(), (long) i))
				.ToArray();
			runner.GlobalSetUp = (target, _) => target.Value = valuesArr[0];
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.LongProp);
				AssertAreEqual((-NumIterations).ToString(), target.Value.StringProp);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => ctx[c.LongProp + 1], valuesArr, valuesArr[nextVal]);
						AssertAreEqual(nextVal, exchRes.PreviousValue.LongProp);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.LongProp);
						AssertAreEqual((-nextVal).ToString(), exchRes.PreviousValue.StringProp);
						AssertAreEqual((-(nextVal + 1)).ToString(), exchRes.CurrentValue.StringProp);
						AssertIsCopy(valuesArr[nextVal], exchRes.PreviousValue);
						AssertIsCopy(valuesArr[nextVal + 1], exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => valuesArr[c.LongProp + 1], valuesArr, valuesArr[nextVal]);
						AssertAreEqual(nextVal, exchRes.PreviousValue.LongProp);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.LongProp);
						AssertAreEqual((-nextVal).ToString(), exchRes.PreviousValue.StringProp);
						AssertAreEqual((-(nextVal + 1)).ToString(), exchRes.CurrentValue.StringProp);
						AssertIsCopy(valuesArr[nextVal], exchRes.PreviousValue);
						AssertIsCopy(valuesArr[nextVal + 1], exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void FastTryExchangeRefOnly_NonEquatable() {
			const int NumIterations = 30_000;

			var runner = NewImmutableRunner(new DummyImmutableRef("0", 0L));

			// (T, T)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.GetWithoutCopy();
						if (curValue.LongProp == NumIterations) return;
						var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
						var prevValue = target.FastTryExchangeRefOnly(newValue, curValue);
						var wasSet = ReferenceEquals(prevValue, curValue);
						var setValue = wasSet ? newValue : prevValue;
						if (wasSet) {
							AssertIsCopy(curValue, prevValue);
							AssertIsCopy(newValue, setValue);
						}
						else {
							AssertAreNotEqualObjects(curValue, prevValue);
							AssertAreEqualObjects(setValue, prevValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void FastTryExchange_NonEquatable() {
			const int NumIterations = 30_000;

			var runner = NewImmutableRunner(new DummyImmutableRef("0", 0L));

			// (T, T)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.GetWithoutCopy();
						if (curValue.LongProp == NumIterations) return;
						var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
						var prevValue = target.FastTryExchange(newValue, curValue);
						var wasSet = ReferenceEquals(prevValue, curValue);
						var setValue = wasSet ? newValue : prevValue;
						if (wasSet) {
							AssertIsCopy(curValue, prevValue);
							AssertIsCopy(newValue, setValue);
						}
						else {
							AssertAreNotEqualObjects(curValue, prevValue);
							AssertAreEqualObjects(setValue, prevValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void TryExchangeWithoutContext_NonEquatable() {
			const int NumIterations = 30_000;

			var runner = NewImmutableRunner(new DummyImmutableRef("0", 0L));

			// (T, T)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.GetWithoutCopy();
						if (curValue.LongProp == NumIterations) return;
						var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
						var (wasSet, prevValue, setValue) = target.TryExchange(newValue, curValue);
						if (wasSet) {
							AssertIsCopy(curValue, prevValue);
							AssertIsCopy(newValue, setValue);
						}
						else {
							AssertAreNotEqualObjects(curValue, prevValue);
							AssertAreEqualObjects(setValue, prevValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void TryExchangeWithContext_NonEquatable() {
			const int NumIterations = 30_000;

			var runner = NewImmutableRunner(new DummyImmutableRef("0", 0L));

			// (Func<T, TContext, T>, T)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.GetWithoutCopy();
						if (curValue.LongProp == NumIterations) return;
						var (wasSet, prevValue, CurrentValue) = target.TryExchange((c, ctx) => new DummyImmutableRef(c.LongProp + ctx), 1L, curValue);
						if (wasSet) {
							AssertAreEqualObjects(prevValue.LongProp + 1L, CurrentValue.LongProp);
							AssertIsCopy(curValue, prevValue);
						}
						else {
							AssertAreNotEqualObjects(curValue, prevValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		void AssertIsCopy(DummyCopyCountingRef lhs, DummyCopyCountingRef rhs) {
			AssertAreEqual(lhs, rhs);
			AssertTrue(!ReferenceEquals(lhs, rhs));
		}

		void AssertIsCopy(DummyImmutableRef lhs, DummyImmutableRef rhs) {
			if (lhs != null && rhs != null) {
				AssertAreEqual(lhs.LongProp, rhs.LongProp);
				AssertAreEqual(lhs.StringProp, rhs.StringProp);
				AssertAreEqualObjects(lhs.RefProp, rhs.RefProp);
			}
			
			AssertTrue(!ReferenceEquals(lhs, rhs));
		}

		void AssertIsCopy(DummyEquatableRef lhs, DummyEquatableRef rhs) {
			if (lhs != null && rhs != null) {
				AssertAreEqual(lhs.LongProp, rhs.LongProp);
				AssertAreEqual(lhs.StringProp, rhs.StringProp);
				AssertAreEqualObjects(lhs.RefProp, rhs.RefProp);
			}

			AssertTrue(!ReferenceEquals(lhs, rhs));
		}

		void AssertIsCopy(DummyCopyCountingRef operand) {
			AssertAreNotEqual(0, operand.InstanceIndex);
		}
		#endregion Tests
	}
}