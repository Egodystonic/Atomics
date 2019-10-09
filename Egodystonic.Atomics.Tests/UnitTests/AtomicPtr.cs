using System;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;
using Egodystonic.Atomics.Tests.Harness;
using Egodystonic.Atomics.Tests.UnitTests.Common;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.UnitTests {
	[TestFixture]
	unsafe class AtomicPtrTest : CommonAtomicValTestSuite<AtomicPtrAsDummyImmutableValWrapper> {
		#region Test Fields
		protected override DummyImmutableVal Alpha { get; } = new DummyImmutableVal(1, 1);
		protected override DummyImmutableVal Bravo { get; } = new DummyImmutableVal(2, 2);
		protected override DummyImmutableVal Charlie { get; } = new DummyImmutableVal(3, 3);
		protected override DummyImmutableVal Delta { get; } = new DummyImmutableVal(4, 4);
		protected override bool AreEqual(DummyImmutableVal lhs, DummyImmutableVal rhs) => lhs == rhs;
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
		public void IncrementDecrement() {
			const int NumIterations = 100_000;
			var initialValue = (long*) 0x12345678;

			var runner = new ConcurrentTestCaseRunner<AtomicPtr<long>>(() => new AtomicPtr<long>(initialValue));

			runner.AllThreadsTearDown = target => FastAssertEqual(initialValue + NumIterations, target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					var incRes = target.Increment();
					FastAssertEqual(((IntPtr) incRes.PreviousValue) + sizeof(long), (IntPtr) incRes.CurrentValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => FastAssertEqual(initialValue - NumIterations, target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					var decRes = target.Decrement();
					FastAssertEqual(((IntPtr) decRes.PreviousValue) - sizeof(long), (IntPtr) decRes.CurrentValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => FastAssertEqual(initialValue, target.Value);
			runner.ExecuteWriterReaderTests(
				target => {
					var incRes = target.Increment();
					FastAssertEqual(((IntPtr) incRes.PreviousValue) + sizeof(long), (IntPtr) incRes.CurrentValue);
				},
				target => {
					var decRes = target.Decrement();
					FastAssertEqual(((IntPtr) decRes.PreviousValue) - sizeof(long), (IntPtr) decRes.CurrentValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => FastAssertEqual(initialValue + NumIterations, target.Value);
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var incRes = target.Increment();
					FastAssertEqual(((IntPtr) incRes.PreviousValue) + sizeof(long), (IntPtr) incRes.CurrentValue);
				},
				NumIterations,
				target => target.GetAsIntPtr(),
				(prev, cur) => FastAssertTrue(cur.ToInt64() >= prev.ToInt64())
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => FastAssertEqual(initialValue - NumIterations, target.Value);
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var decRes = target.Decrement();
					FastAssertEqual(((IntPtr) decRes.PreviousValue) - sizeof(long), (IntPtr) decRes.CurrentValue);
				},
				NumIterations,
				target => target.GetAsIntPtr(),
				(prev, cur) => FastAssertTrue(cur.ToInt64() <= prev.ToInt64())
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void AddSub() {
			const int NumIterations = 100_000;
			var initialValue = (long*) 0x12345678;

			var runner = new ConcurrentTestCaseRunner<AtomicPtr<long>>(() => new AtomicPtr<long>(initialValue));

			runner.AllThreadsTearDown = target => FastAssertEqual(initialValue + NumIterations * 7, target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					var addRes = target.Add(7);
					FastAssertEqual((IntPtr) addRes.PreviousValue + sizeof(long) * 7, (IntPtr) addRes.CurrentValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => FastAssertEqual(initialValue - NumIterations * 7, target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					var subRes = target.Subtract(7);
					FastAssertEqual((IntPtr) subRes.PreviousValue - sizeof(long) * 7, (IntPtr) subRes.CurrentValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => FastAssertEqual(initialValue, target.Value);
			runner.ExecuteWriterReaderTests(
				target => {
					var (prevValue, CurrentValue) = target.Add(3L);
					AssertAreEqual(prevValue + 3, CurrentValue);
				},
				target => {
					var (prevValue, CurrentValue) = target.Subtract(3L);
					AssertAreEqual(prevValue - 3, CurrentValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => FastAssertEqual(initialValue + NumIterations * 9, target.Value);
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var (prevValue, CurrentValue) = target.Add(new IntPtr(9));
					AssertAreEqual(prevValue + 9, CurrentValue);
				},
				NumIterations,
				target => (IntPtr) target.Value,
				(prev, cur) => FastAssertTrue(cur.ToInt64() >= prev.ToInt64())
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => FastAssertEqual(initialValue - NumIterations * 9, target.Value);
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var (prevValue, CurrentValue) = target.Subtract(new IntPtr(9));
					AssertAreEqual(prevValue - 9, CurrentValue);
				},
				NumIterations,
				target => (IntPtr) target.Value,
				(prev, cur) => FastAssertTrue(cur.ToInt64() <= prev.ToInt64())
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void SpinWaitForValueTyped() {
			const int NumIterations = 300_000;
			var initialValue = (long*) 0x12345678;

			var runner = new ConcurrentTestCaseRunner<AtomicPtr<long>>(() => new AtomicPtr<long>(initialValue));
			runner.AllThreadsTearDown = target => FastAssertEqual(initialValue + NumIterations, target.Value);

			// (T)
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal == initialValue + NumIterations) break;
						if (((int) curVal & 0x08) == 0) {
							FastAssertEqual(curVal + 1, target.SpinWaitForValue(curVal + 1));
						}
						else {
							target.Value = curVal + 1;
						}
					}
				},
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal == initialValue + NumIterations) break;
						if (((int) curVal & 0x08) == 0x08) {
							FastAssertEqual(curVal + 1, target.SpinWaitForValue(curVal + 1));
						}
						else {
							target.Value = curVal + 1;
						}
					}
				}
			);

			// (Func<T, bool>)
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal == initialValue + NumIterations) break;
						if (((int) curVal & 0x08) == 0) {
							FastAssertEqual(curVal + 1, target.SpinWaitForValue(c => c  - 1 == curVal));
						}
						else {
							target.Value = curVal + 1;
						}
					}
				},
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal == initialValue + NumIterations) break;
						if (((int) curVal & 0x08) == 0x08) {
							FastAssertEqual(curVal + 1, target.SpinWaitForValue(c => c - 1 == curVal));
						}
						else {
							target.Value = curVal + 1;
						}
					}
				}
			);

			// (Func<T, TContext, bool>, TContext)
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal == initialValue + NumIterations) break;
						if (((int) curVal & 0x08) == 0) {
							FastAssertEqual(curVal + 1, target.SpinWaitForValue((c, ctx) => c == (long*) ctx + 1, (int) curVal));
						}
						else {
							target.Value = curVal + 1;
						}
					}
				},
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal == initialValue + NumIterations) break;
						if (((int) curVal & 0x08) == 0x08) {
							FastAssertEqual(curVal + 1, target.SpinWaitForValue((c, ctx) => c == (long*) ctx + 1, (int) curVal));
						}
						else {
							target.Value = curVal + 1;
						}
					}
				}
			);
		}

		[Test]
		public void ExchangeTyped() {
			const int NumIterations = 300_000;
			var initialValue = (long*) 0x12345678;

			var runner = new ConcurrentTestCaseRunner<AtomicPtr<long>>(() => new AtomicPtr<long>(initialValue));
			var atomicInt = new LockFreeInt32(0);

			// (T)
			runner.GlobalSetUp = (_, __) => { atomicInt.Set(0); };
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(initialValue + NumIterations, target.Value);
			};
			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					var newInt = atomicInt.Increment().CurrentValue;
					var newValue = initialValue + newInt;
					var prev = target.Exchange(newValue).PreviousValue;
					FastAssertEqual(prev, newValue - 1);
				},
				NumIterations,
				target => (IntPtr) target.Value,
				(prev, cur) => {
					FastAssertTrue(prev.ToInt64() <= cur.ToInt64());
				}
			);
			runner.GlobalSetUp = null;
			runner.AllThreadsTearDown = null;

			// (Func<T, T>)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(initialValue + NumIterations, target.Value);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					var exchRes = target.Exchange(t => t + 1);
					AssertAreEqual(exchRes.PreviousValue + 1, exchRes.CurrentValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TContext, T>)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(initialValue + NumIterations, target.Value);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					var exchRes = target.Exchange((t, ctx) => t + ctx, 1);
					AssertAreEqual(exchRes.PreviousValue + 1, exchRes.CurrentValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void SpinWaitForExchangeWithoutContextTyped() {
			const int NumIterations = 100_000;
			var initialValue = (long*) 0x12345678;

			var runner = new ConcurrentTestCaseRunner<AtomicPtr<long>>(() => new AtomicPtr<long>(initialValue));

			// (T, T)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(initialValue + NumIterations, target.Value);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(initialValue + nextVal + 1, initialValue + nextVal);
						FastAssertEqual(initialValue + nextVal, exchRes.PreviousValue);
						FastAssertEqual(initialValue + nextVal + 1, exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(initialValue + nextVal + 1, initialValue + nextVal);
						FastAssertEqual(initialValue + nextVal, exchRes.PreviousValue);
						FastAssertEqual(initialValue + nextVal + 1, exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, T)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(initialValue - NumIterations, target.Value);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => c - 1, initialValue - nextVal);
						FastAssertEqual(initialValue - nextVal, exchRes.PreviousValue);
						FastAssertEqual(initialValue - (nextVal + 1), exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => c - 1, initialValue - nextVal);
						FastAssertEqual(initialValue - nextVal, exchRes.PreviousValue);
						FastAssertEqual(initialValue - (nextVal + 1), exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (T, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(initialValue + NumIterations, target.Value);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(initialValue + nextVal + 1, (c, n) => n == c + 1);
						FastAssertEqual(initialValue + nextVal, exchRes.PreviousValue);
						FastAssertEqual(initialValue + nextVal + 1, exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(initialValue + nextVal + 1, (c, n) => n == c + 1);
						FastAssertEqual(initialValue + nextVal, exchRes.PreviousValue);
						FastAssertEqual(initialValue + nextVal + 1, exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(initialValue + NumIterations, target.Value);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => c + 1, (c, n) => n == c + 1 && c == initialValue + nextVal);
						FastAssertEqual(initialValue + nextVal, exchRes.PreviousValue);
						FastAssertEqual(initialValue + nextVal + 1, exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => c + 1, (c, n) => n == c + 1 && c == initialValue + nextVal);
						FastAssertEqual(initialValue + nextVal, exchRes.PreviousValue);
						FastAssertEqual(initialValue + nextVal + 1, exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void SpinWaitForExchangeWithContextTyped() {
			const int NumIterations = 30_000;
			var initialValue = (long*) 0x12345678;

			var runner = new ConcurrentTestCaseRunner<AtomicPtr<long>>(() => new AtomicPtr<long>(initialValue));

			// (Func<T, TContext, T>, T)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(initialValue - NumIterations, target.Value);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => c - ctx, initialValue - nextVal, 1);
						FastAssertEqual(initialValue - nextVal, exchRes.PreviousValue);
						FastAssertEqual(initialValue - (nextVal + 1), exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => c - ctx, initialValue - nextVal, 1);
						FastAssertEqual(initialValue - nextVal, exchRes.PreviousValue);
						FastAssertEqual(initialValue - (nextVal + 1), exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (T, Func<T, T, TContext, bool>)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(initialValue + NumIterations, target.Value);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(initialValue + nextVal + 1, (c, n, ctx) => n == c + ctx, 1);
						FastAssertEqual(initialValue + nextVal, exchRes.PreviousValue);
						FastAssertEqual(initialValue + nextVal + 1, exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(initialValue + nextVal + 1, (c, n, ctx) => n == c + ctx, 1);
						FastAssertEqual(initialValue + nextVal, exchRes.PreviousValue);
						FastAssertEqual(initialValue + nextVal + 1, exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TContext, T>, Func<T, T, TContext, bool>)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(initialValue + NumIterations, target.Value);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => c + ctx, (c, n, ctx) => n == c + ctx && c == initialValue + nextVal, 1);
						FastAssertEqual(initialValue + nextVal, exchRes.PreviousValue);
						FastAssertEqual(initialValue + nextVal + 1, exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => c + ctx, (c, n, ctx) => n == c + ctx && c == initialValue + nextVal, 1);
						FastAssertEqual(initialValue + nextVal, exchRes.PreviousValue);
						FastAssertEqual(initialValue + nextVal + 1, exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TMapContext, T>, Func<T, T, TPredicateContext, bool>)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(initialValue + NumIterations, target.Value);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => c + ctx, (c, n, ctx) => n == c + (ctx - 1) && c == initialValue + nextVal, 1, 2);
						FastAssertEqual(initialValue + nextVal, exchRes.PreviousValue);
						FastAssertEqual(initialValue + nextVal + 1, exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => c + ctx, (c, n, ctx) => n == c + (ctx - 1) && c == initialValue + nextVal, 1, 2);
						FastAssertEqual(initialValue + nextVal, exchRes.PreviousValue);
						FastAssertEqual(initialValue + nextVal + 1, exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TContext, T>, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(initialValue + NumIterations, target.Value);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => c + ctx, (c, n) => n == c + 1 && c == initialValue + nextVal, 1);
						FastAssertEqual(initialValue + nextVal, exchRes.PreviousValue);
						FastAssertEqual(initialValue + nextVal + 1, exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => c + ctx, (c, n) => n == c + 1 && c == initialValue + nextVal, 1);
						FastAssertEqual(initialValue + nextVal, exchRes.PreviousValue);
						FastAssertEqual(initialValue + nextVal + 1, exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, Func<T, T, TContext, bool>)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(initialValue + NumIterations, target.Value);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => c + 1, (c, n, ctx) => n == c + ctx && c == initialValue + nextVal, 1);
						FastAssertEqual(initialValue + nextVal, exchRes.PreviousValue);
						FastAssertEqual(initialValue + nextVal + 1, exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => c + 1, (c, n, ctx) => n == c + ctx && c == initialValue + nextVal, 1);
						FastAssertEqual(initialValue + nextVal, exchRes.PreviousValue);
						FastAssertEqual(initialValue + nextVal + 1, exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void TryExchangeWithoutContextTyped() {
			const int NumIterations = 200_000;
			var initialValue = (DummySixteenByteVal*) 0x12345678;

			var runner = new ConcurrentTestCaseRunner<AtomicPtr<DummySixteenByteVal>>(() => new AtomicPtr<DummySixteenByteVal>(initialValue));

			// (T, T)
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curValue = target.Value;
					var newValue = curValue + 1;
					target.TryExchange(newValue, curValue);
				},
				NumIterations,
				target => (IntPtr) target.Value,
				(prev, cur) => FastAssertTrue(cur.ToInt64() >= prev.ToInt64())
			);

			// (T, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(initialValue + NumIterations, target.Value);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue == initialValue + NumIterations) return;
						var newValue = curValue + 1;
						var (wasSet, prevValue, setValue) = target.TryExchange(newValue, (c, n) => c + 1 == n);
						if (wasSet) {
							FastAssertEqual(curValue, prevValue);
							FastAssertEqual(newValue, setValue);
						}
						else {
							FastAssertNotEqual(curValue, prevValue);
							AssertAreEqual(setValue, prevValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, T)
			runner.ExecuteFreeThreadedTests(
				target => {
					var curValue = target.Value;

					var (wasSet, prevValue, CurrentValue) = target.TryExchange(c => c + 1, curValue);

					if (wasSet) {
						FastAssertEqual(curValue, prevValue);
						FastAssertEqual(curValue + 1, CurrentValue);
					}
					else FastAssertNotEqual(curValue, prevValue);
				},
				NumIterations
			);

			// (Func<T, T>, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(initialValue + NumIterations, target.Value);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue == initialValue + NumIterations) return;
						var (wasSet, prevValue, CurrentValue) = target.TryExchange(c => c + 1, (c, n) => c + 1 == n && c < initialValue + NumIterations);
						if (wasSet) {
							AssertAreEqual(prevValue + 1, CurrentValue);
							FastAssertTrue(CurrentValue <= initialValue + NumIterations);
						}
						else AssertAreEqual(prevValue, CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void TryExchangeWithContextTyped() {
			const int NumIterations = 300_000;
			var initialValue = (DummySixteenByteVal*) 0x12345678;

			var runner = new ConcurrentTestCaseRunner<AtomicPtr<DummySixteenByteVal>>(() => new AtomicPtr<DummySixteenByteVal>(initialValue));

			// (T, Func<T, T, TContext, bool>)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(initialValue + NumIterations, target.Value);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue == initialValue + NumIterations) return;
						var newValue = curValue + 1;
						var (wasSet, prevValue, setValue) = target.TryExchange(newValue, (c, n, ctx) => c + ctx == n, 1);
						if (wasSet) {
							FastAssertEqual(curValue, prevValue);
							FastAssertEqual(newValue, setValue);
						}
						else {
							FastAssertNotEqual(curValue, prevValue);
							AssertAreEqual(setValue, prevValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TContext, T>, T)
			runner.ExecuteFreeThreadedTests(
				target => {
					var curValue = target.Value;

					var (wasSet, prevValue, CurrentValue) = target.TryExchange((c, ctx) => c + ctx, curValue, 1);

					if (wasSet) {
						FastAssertEqual(curValue, prevValue);
						FastAssertEqual(curValue + 1, CurrentValue);
					}
					else FastAssertNotEqual(curValue, prevValue);
				},
				NumIterations
			);

			// (Func<T, TContext, T>, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(initialValue + NumIterations, target.Value);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue == initialValue + NumIterations) return;
						var newValue = curValue + 1;
						var (wasSet, prevValue, setValue) = target.TryExchange((c, ctx) => c + ctx, (c, n) => n == newValue, 1);
						if (wasSet) {
							FastAssertEqual(curValue, prevValue);
							FastAssertEqual(newValue, setValue);
						}
						else {
							FastAssertNotEqual(curValue, prevValue);
							AssertAreEqual(setValue, prevValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, Func<T, T, TContext, bool>)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(initialValue + NumIterations, target.Value);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue == initialValue + NumIterations) return;
						var newValue = curValue + 1;
						var (wasSet, prevValue, setValue) = target.TryExchange(c => c + 1, (c, n, ctx) => n == (DummySixteenByteVal*) ctx, ((IntPtr) newValue).ToInt64());
						if (wasSet) {
							FastAssertEqual(curValue, prevValue);
							FastAssertEqual(newValue, setValue);
						}
						else {
							FastAssertNotEqual(curValue, prevValue);
							AssertAreEqual(setValue, prevValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TContext, T>, Func<T, T, TContext, bool>)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(initialValue + NumIterations, target.Value);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue == initialValue + NumIterations) return;
						var newValue = curValue + 1;
						var (wasSet, prevValue, setValue) = target.TryExchange((c, ctx) => c + ctx, (c, n, ctx) => n == curValue + ctx, 1);
						if (wasSet) {
							FastAssertEqual(curValue, prevValue);
							FastAssertEqual(newValue, setValue);
						}
						else {
							FastAssertNotEqual(curValue, prevValue);
							AssertAreEqual(setValue, prevValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TMapContext, T>, Func<T, T, TPredicateContext, bool>)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(initialValue + NumIterations, target.Value);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue == initialValue + NumIterations) return;
						var newValue = curValue + 1;
						var (wasSet, prevValue, setValue) = target.TryExchange((c, ctx) => c + ctx, (c, n, ctx) => n == (DummySixteenByteVal*) ctx, 1, ((IntPtr) newValue).ToInt64());
						if (wasSet) {
							FastAssertEqual(curValue, prevValue);
							FastAssertEqual(newValue, setValue);
						}
						else {
							FastAssertNotEqual(curValue, prevValue);
							AssertAreEqual(setValue, prevValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void API_IncrementDecrement() {
			const int InitialPtr = 0x12345000;

			var target = new AtomicPtr<DummySixteenByteVal>(new IntPtr(InitialPtr));
			var incRes = target.Increment();
			FastAssertEqual((DummySixteenByteVal*) (InitialPtr + sizeof(DummySixteenByteVal)), incRes.CurrentValue);
			FastAssertEqual((DummySixteenByteVal*) InitialPtr, incRes.PreviousValue);
			FastAssertEqual<IntPtr>((IntPtr) (InitialPtr + sizeof(DummySixteenByteVal)), incRes.AsUntyped.CurrentValue);
			FastAssertEqual<IntPtr>((IntPtr) InitialPtr, incRes.AsUntyped.PreviousValue);

			var decRes = target.Decrement();
			FastAssertEqual((DummySixteenByteVal*) (InitialPtr + sizeof(DummySixteenByteVal)), decRes.PreviousValue);
			FastAssertEqual((DummySixteenByteVal*) InitialPtr, decRes.CurrentValue);
			FastAssertEqual<IntPtr>((IntPtr) (InitialPtr + sizeof(DummySixteenByteVal)), decRes.AsUntyped.PreviousValue);
			FastAssertEqual<IntPtr>((IntPtr) InitialPtr, decRes.AsUntyped.CurrentValue);
		}

		[Test]
		public void API_AddSubtract() {
			const int InitialPtr = 0x12345000;

			var target = new AtomicPtr<DummySixteenByteVal>(new IntPtr(InitialPtr));

			AssertResult(target.Add(10), InitialPtr, InitialPtr + sizeof(DummySixteenByteVal) * 10);
			AssertResult(target.Subtract(10), InitialPtr + sizeof(DummySixteenByteVal) * 10, InitialPtr);
			AssertResult(target.Add(10L), InitialPtr, InitialPtr + sizeof(DummySixteenByteVal) * 10);
			AssertResult(target.Subtract(10L), InitialPtr + sizeof(DummySixteenByteVal) * 10, InitialPtr);
			AssertResult(target.Add(new IntPtr(10L)), InitialPtr, InitialPtr + sizeof(DummySixteenByteVal) * 10);
			AssertResult(target.Subtract(new IntPtr(10L)), InitialPtr + sizeof(DummySixteenByteVal) * 10, InitialPtr);

			void AssertResult(AtomicPtr<DummySixteenByteVal>.TypedPtrExchangeRes result, int expectedPreviousAddr, int expectedNewAddr) {
				FastAssertEqual((DummySixteenByteVal*) expectedPreviousAddr, result.PreviousValue);
				FastAssertEqual((DummySixteenByteVal*) expectedNewAddr, result.CurrentValue);
				FastAssertEqual<IntPtr>((IntPtr) expectedPreviousAddr, result.AsUntyped.PreviousValue);
				FastAssertEqual<IntPtr>((IntPtr) expectedNewAddr, result.AsUntyped.CurrentValue);
			}
		}
		#endregion Tests
	}
}