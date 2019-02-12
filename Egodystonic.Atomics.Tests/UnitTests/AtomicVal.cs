using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;
using Egodystonic.Atomics.Tests.Harness;
using Egodystonic.Atomics.Tests.UnitTests.Common;
using NUnit.Framework;
using static Egodystonic.Atomics.Tests.Harness.ConcurrentTestCaseRunner;
using ImmutableVal = Egodystonic.Atomics.Tests.DummyObjects.DummyImmutableVal;
using EquatableVal = Egodystonic.Atomics.Tests.DummyObjects.DummyImmutableValAlphaOnlyEquatable;
using SixteenVal = Egodystonic.Atomics.Tests.DummyObjects.DummySixteenByteVal;

namespace Egodystonic.Atomics.Tests.UnitTests {
	[TestFixture]
	class AtomicValTest : CommonAtomicValTestSuite<AtomicVal<ImmutableVal>> {
		#region Test Fields
		RunnerFactory<SixteenVal, AtomicVal<SixteenVal>> _sixteenByteRunnerFactory;
		RunnerFactory<Dummy128ByteVal, AtomicVal<Dummy128ByteVal>> _oneTwentyEightByteRunnerFactory;

		protected override ImmutableVal Alpha { get; } = new ImmutableVal(1, 1);
		protected override ImmutableVal Bravo { get; } = new ImmutableVal(2, 2);
		protected override ImmutableVal Charlie { get; } = new ImmutableVal(3, 3);
		protected override ImmutableVal Delta { get; } = new ImmutableVal(4, 4);
		protected override bool AreEqual(ImmutableVal lhs, ImmutableVal rhs) => lhs == rhs;
		#endregion

		#region Test Setup
		[OneTimeSetUp]
		public void SetUpClass() {
			_sixteenByteRunnerFactory = new RunnerFactory<SixteenVal, AtomicVal<SixteenVal>>();
			_oneTwentyEightByteRunnerFactory = new RunnerFactory<Dummy128ByteVal, AtomicVal<Dummy128ByteVal>>();
		}

		[OneTimeTearDown]
		public void TearDownClass() { }

		[SetUp]
		public void SetUpTest() { }

		[TearDown]
		public void TearDownTest() { }
		#endregion

		#region Custom Equality Tests
		[Test]
		public void API_SpinWaitForValue_CustomEquality() {
			var target = new AtomicVal<EquatableVal>(new EquatableVal(0, 0));
			var task = Task.Run(() => target.SpinWaitForValue(new EquatableVal(1, 1)));
			target.Set(new EquatableVal(1, 100));
			Assert.AreEqual(new EquatableVal(1, 100), task.Result);
		}

		[Test]
		public void API_SpinWaitForExchange_CustomEquality() {
			var target = new AtomicVal<EquatableVal>(new EquatableVal(0, 0));
			var task = Task.Run(() => target.SpinWaitForExchange(new EquatableVal(1, 1), new EquatableVal(10, 10)));
			target.Set(new EquatableVal(10, 100));
			Assert.AreEqual((new EquatableVal(10, 100), new EquatableVal(1, 1)), task.Result);

			task = Task.Run(() => target.SpinWaitForExchange((c, ctx) => new EquatableVal(c.Alpha + ctx, c.Bravo + ctx), 1, new EquatableVal(100, 100)));
			target.Set(new EquatableVal(100, 1000));
			Assert.AreEqual((new EquatableVal(100, 1000), new EquatableVal(101, 1001)), task.Result);
		}

		[Test]
		public void API_TryExchange_CustomEquality() {
			var target = new AtomicVal<EquatableVal>(new EquatableVal(0, 0));
			Assert.AreEqual((false, new EquatableVal(0, 0), new EquatableVal(0, 0)), target.TryExchange(new EquatableVal(10, 10), new EquatableVal(1, 1)));
			target.Set(new EquatableVal(1, 0));
			Assert.AreEqual((true, new EquatableVal(1, 0), new EquatableVal(10, 10)), target.TryExchange(new EquatableVal(10, 10), new EquatableVal(1, 1)));

			target.Set(new EquatableVal(0, 0));
			Assert.AreEqual((false, new EquatableVal(0, 0), new EquatableVal(0, 0)), target.TryExchange((c, ctx) => new EquatableVal(10, c.Bravo + ctx), 10, new EquatableVal(1, 1)));
			target.Set(new EquatableVal(1, 0));
			Assert.AreEqual((true, new EquatableVal(1, 0), new EquatableVal(10, 10)), target.TryExchange((c, ctx) => new EquatableVal(10, c.Bravo + ctx), 10, new EquatableVal(1, 1)));
		}
		#endregion

		#region Oversized Value Type Test
		[Test]
		public void GetAndSetAndValue_Oversized() {
			const int NumIterations = 1_000_000;
			var atomicLong = new AtomicLong(0L);
			var runner = _sixteenByteRunnerFactory.NewRunner(new SixteenVal(0, 0));

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					unsafe {
						var newLongVal = atomicLong.Increment().CurrentValue;
						target.Set(*(SixteenVal*) &newLongVal);
					}
				},
				NumIterations,
				target => target.Get(),
				(prev, cur) => {
					unsafe {
						AssertTrue(*(long*) &prev <= *(long*) &cur);
					}
				}
			);

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					unsafe {
						var newLongVal = atomicLong.Increment().CurrentValue;
						target.Value = *(SixteenVal*) &newLongVal;
					}
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					unsafe {
						AssertTrue(*(long*) &prev <= *(long*) &cur);
					}
				}
			);
		}

		[Test]
		public void SpinWaitForValue_Oversized() {
			const int NumIterations = 300_000;

			var runner = _sixteenByteRunnerFactory.NewRunner(new SixteenVal(0, 0));

			// (T)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.Alpha);
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.Alpha == NumIterations) break;
						if ((curVal.Alpha & 1) == 0) {
							AssertAreEqual(curVal.Alpha + 1, target.SpinWaitForValue(new SixteenVal(curVal.Alpha + 1, 0)).Alpha);
						}
						else {
							target.Value = new SixteenVal(curVal.Alpha + 1, 0);
						}
					}
				},
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.Alpha == NumIterations) break;
						if ((curVal.Alpha & 1) == 1) {
							AssertAreEqual(curVal.Alpha + 1, target.SpinWaitForValue(new SixteenVal(curVal.Alpha + 1, 0)).Alpha);
						}
						else {
							target.Value = new SixteenVal(curVal.Alpha + 1, 0);
						}
					}
				}
			);

			// (Func<T, bool>)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.Alpha);
			runner.ExecuteWriterReaderTests(
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.Alpha == NumIterations) break;
						target.TryExchange(new SixteenVal(curVal.Alpha + 1, 0), curVal);
					}
				},
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.Alpha == NumIterations) break;
						AssertTrue(target.SpinWaitForValue(c => c.Alpha > curVal.Alpha).Alpha >= curVal.Alpha);
					}
				}
			);

			// (Func<T, TContext, bool>, TContext)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.Alpha);
			runner.ExecuteWriterReaderTests(
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.Alpha == NumIterations) break;
						target.TryExchange(new SixteenVal(curVal.Alpha + 1, 0), curVal);
					}
				},
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.Alpha == NumIterations) break;
						AssertTrue(target.SpinWaitForValue((c, ctx) => c.Alpha > ctx.Alpha, curVal).Alpha >= curVal.Alpha);
					}
				}
			);
		}

		[Test]
		public void Exchange_Oversized() {
			const int NumIterations = 300_000;

			var atomicIntA = new AtomicInt(0);
			var atomicIntB = new AtomicInt(0);
			var runner = _sixteenByteRunnerFactory.NewRunner(new SixteenVal(0, 0));

			// (T)
			runner.GlobalSetUp = (_, __) => { atomicIntA.Set(0); atomicIntB.Set(0); };
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					var newA = atomicIntA.Increment().CurrentValue;
					var newB = atomicIntB.Increment().CurrentValue;
					var newValue = new SixteenVal(newA, newB);
					var prev = target.Exchange(newValue).PreviousValue;
					AssertAreEqual(prev.Alpha, newA - 1);
					AssertAreEqual(prev.Bravo, newB - 1);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					AssertTrue(prev.Alpha <= cur.Alpha);
					AssertTrue(prev.Bravo <= cur.Bravo);
				}
			);
			runner.GlobalSetUp = null;
			runner.AllThreadsTearDown = null;

			// (Func<T, T>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					var exchRes = target.Exchange(t => new SixteenVal(t.Alpha + 1, t.Bravo + 1));
					AssertAreEqual(exchRes.PreviousValue.Alpha + 1, exchRes.CurrentValue.Alpha);
					AssertAreEqual(exchRes.PreviousValue.Bravo + 1, exchRes.CurrentValue.Bravo);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TContext, T>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations * 2, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					var exchRes = target.Exchange((t, ctx) => new SixteenVal(t.Alpha + 1, t.Bravo + ctx), 2);
					AssertAreEqual(exchRes.PreviousValue.Alpha + 1, exchRes.CurrentValue.Alpha);
					AssertAreEqual(exchRes.PreviousValue.Bravo + 2, exchRes.CurrentValue.Bravo);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void SpinWaitForExchangeWithoutContext_Oversized() {
			const int NumIterations = 100_000;

			var runner = _sixteenByteRunnerFactory.NewRunner(new SixteenVal(0, 0));

			// (T, T)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(new SixteenVal(nextVal + 1, nextVal + 1), new SixteenVal(nextVal, nextVal));
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(new SixteenVal(nextVal + 1, nextVal + 1), new SixteenVal(nextVal, nextVal));
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, T)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(-NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => new SixteenVal(c.Alpha + 1, c.Bravo - 1), new SixteenVal(nextVal, -nextVal));
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(-nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(-(nextVal + 1), exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => new SixteenVal(c.Alpha + 1, c.Bravo - 1), new SixteenVal(nextVal, -nextVal));
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(-nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(-(nextVal + 1), exchRes.CurrentValue.Bravo);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (T, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(new SixteenVal(nextVal + 1, nextVal + 1), (c, n) => n.Alpha == c.Alpha + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(new SixteenVal(nextVal + 1, nextVal + 1), (c, n) => n.Alpha == c.Alpha + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => new SixteenVal(nextVal + 1, c.Bravo + 1), (c, n) => n.Alpha == c.Alpha + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => new SixteenVal(nextVal + 1, c.Bravo + 1), (c, n) => n.Alpha == c.Alpha + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void SpinWaitForExchangeWithContext_Oversized() {
			const int NumIterations = 100_000;

			var runner = _sixteenByteRunnerFactory.NewRunner(new SixteenVal(0, 0));

			// (Func<T, TContext, T>, T)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => new SixteenVal(ctx, c.Bravo + 1), nextVal + 1, new SixteenVal(nextVal, nextVal));
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => new SixteenVal(ctx, c.Bravo + 1), nextVal + 1, new SixteenVal(nextVal, nextVal));
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (T, Func<T, T, TContext, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(new SixteenVal(nextVal + 1, nextVal + 1), (c, n, ctx) => n.Alpha == c.Alpha + ctx, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(new SixteenVal(nextVal + 1, nextVal + 1), (c, n, ctx) => n.Alpha == c.Alpha + ctx, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TMapContext, T>, Func<T, T, TPredicateContext, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => new SixteenVal(ctx, c.Bravo + 1), nextVal + 1, (c, n, ctx) => n.Alpha == c.Alpha + ctx, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => new SixteenVal(ctx, c.Bravo + 1), nextVal + 1, (c, n, ctx) => n.Alpha == c.Alpha + ctx, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TContext, T>, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => new SixteenVal(ctx, c.Bravo + 1), nextVal + 1, (c, n) => n.Alpha == c.Alpha + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => new SixteenVal(ctx, c.Bravo + 1), nextVal + 1, (c, n) => n.Alpha == c.Alpha + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, Func<T, T, TContext, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => new SixteenVal(nextVal + 1, c.Bravo + 1), (c, n, ctx) => n.Alpha == c.Alpha + ctx, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => new SixteenVal(nextVal + 1, c.Bravo + 1), (c, n, ctx) => n.Alpha == c.Alpha + ctx, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void TryExchangeWithoutContext_Oversized() {
			const int NumIterations = 200_000;

			var runner = _sixteenByteRunnerFactory.NewRunner(new SixteenVal(0, 0));

			// (T, T)
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curValue = target.Value;
					var newValue = new SixteenVal(0, curValue.Bravo + 1);
					target.TryExchange(newValue, curValue);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => AssertTrue(cur.Bravo >= prev.Bravo)
			);

			// (T, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(-1 * NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var newValue = new SixteenVal(curValue.Alpha + 1, curValue.Bravo - 1);
						var (wasSet, prevValue, setValue) = target.TryExchange(newValue, (c, n) => c.Alpha + 1 == n.Alpha && c.Bravo - 1 == n.Bravo);
						if (wasSet) {
							AssertAreEqual(curValue, prevValue);
							AssertAreEqual(newValue, setValue);
						}
						else {
							AssertAreNotEqual(curValue, prevValue);
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

					var (wasSet, prevValue, CurrentValue) = target.TryExchange(
						c => c.Bravo < c.Alpha
							? new SixteenVal(c.Alpha, c.Bravo + 1)
							: new SixteenVal(c.Alpha + 1, c.Bravo),
						curValue
					);

					if (wasSet) {
						AssertAreEqual(curValue, prevValue);
						AssertAreEqual(prevValue.Bravo < prevValue.Alpha ? new SixteenVal(prevValue.Alpha, prevValue.Bravo + 1) : new SixteenVal(prevValue.Alpha + 1, prevValue.Bravo), CurrentValue);
					}

					else AssertAreNotEqual(curValue, prevValue);
				},
				NumIterations
			);

			// (Func<T, T>, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(-1 * NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var (wasSet, prevValue, CurrentValue) = target.TryExchange(c => new SixteenVal(c.Alpha + 1, c.Bravo - 1), (c, n) => c.Alpha + 1 == n.Alpha && c.Bravo - 1 == n.Bravo && c.Alpha < NumIterations);
						if (wasSet) {
							AssertAreEqual(new SixteenVal(prevValue.Alpha + 1, prevValue.Bravo - 1), CurrentValue);
							AssertTrue(CurrentValue.Alpha <= NumIterations);
						}
						else AssertAreEqual(prevValue, CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void TryExchangeWithContext_Oversized() {
			const int NumIterations = 300_000;

			var runner = _sixteenByteRunnerFactory.NewRunner(new SixteenVal(0, 0));

			// (T, Func<T, T, TContext, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(-1 * NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var newValue = new SixteenVal(curValue.Alpha + 1, curValue.Bravo - 1);
						var (wasSet, prevValue, setValue) = target.TryExchange(newValue, (c, n, ctx) => c.Alpha + ctx == n.Alpha && c.Bravo - ctx == n.Bravo, 1);
						if (wasSet) {
							AssertAreEqual(curValue, prevValue);
							AssertAreEqual(newValue, setValue);
						}
						else {
							AssertAreNotEqual(curValue, prevValue);
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

					var (wasSet, prevValue, CurrentValue) = target.TryExchange(
						(c, ctx) => c.Bravo < c.Alpha
							? new SixteenVal(c.Alpha, c.Bravo + ctx)
							: new SixteenVal(c.Alpha + ctx, c.Bravo),
						1, curValue);

					if (wasSet) {
						AssertAreEqual(curValue, prevValue);
						AssertAreEqual(prevValue.Bravo < prevValue.Alpha ? new SixteenVal(prevValue.Alpha, prevValue.Bravo + 1) : new SixteenVal(prevValue.Alpha + 1, prevValue.Bravo), CurrentValue);
					}
					else AssertAreNotEqual(curValue, prevValue);
				},
				NumIterations
			);

			// (Func<T, TContext, T>, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(-1 * NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var (wasSet, prevValue, CurrentValue) = target.TryExchange((c, ctx) => new SixteenVal(c.Alpha + ctx, c.Bravo - ctx), 1, (c, n) => c.Alpha + 1 == n.Alpha && c.Bravo - 1 == n.Bravo && c.Alpha < NumIterations);
						if (wasSet) {
							AssertAreEqual(new SixteenVal(prevValue.Alpha + 1, prevValue.Bravo - 1), CurrentValue);
							AssertTrue(CurrentValue.Alpha <= NumIterations);
						}
						else AssertAreEqual(CurrentValue, prevValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, Func<T, T, TContext, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(-1 * NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var (wasSet, prevValue, CurrentValue) = target.TryExchange(c => new SixteenVal(c.Alpha + 1, c.Bravo - 1), (c, n, ctx) => c.Alpha + ctx == n.Alpha && c.Bravo - ctx == n.Bravo && c.Alpha < NumIterations, 1);
						if (wasSet) {
							AssertAreEqual(new SixteenVal(prevValue.Alpha + 1, prevValue.Bravo - 1), CurrentValue);
							AssertTrue(CurrentValue.Alpha <= NumIterations);
						}
						else AssertAreEqual(CurrentValue, prevValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TMapContext, T>, Func<T, T, TPredicateContext, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(-1 * NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var (wasSet, prevValue, CurrentValue) = target.TryExchange((c, ctx) => new SixteenVal(c.Alpha + ctx, c.Bravo - ctx), 1, (c, n, ctx) => c.Alpha + 1 == n.Alpha && c.Bravo - 1 == n.Bravo && c.Alpha < ctx, NumIterations);
						if (wasSet) {
							AssertAreEqual(new SixteenVal(prevValue.Alpha + 1, prevValue.Bravo - 1), CurrentValue);
							AssertTrue(CurrentValue.Alpha <= NumIterations);
						}
						else AssertAreEqual(CurrentValue, prevValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}
		#endregion

		#region Very Oversized Value Type Test
		[Test]
		public void GetAndSetAndValue_VeryOversized() {
			const int NumIterations = 1_000_000;
			var atomicLong = new AtomicLong(0L);
			var runner = _oneTwentyEightByteRunnerFactory.NewRunner(new Dummy128ByteVal(0, 0));

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					unsafe {
						var newLongVal = atomicLong.Increment().CurrentValue;
						target.Set(*(Dummy128ByteVal*) &newLongVal);
					}
				},
				NumIterations,
				target => target.Get(),
				(prev, cur) => {
					unsafe {
						AssertTrue(*(long*) &prev <= *(long*) &cur);
					}
				}
			);

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					unsafe {
						var newLongVal = atomicLong.Increment().CurrentValue;
						target.Value = *(Dummy128ByteVal*) &newLongVal;
					}
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					unsafe {
						AssertTrue(*(long*) &prev <= *(long*) &cur);
					}
				}
			);
		}

		[Test]
		public void SpinWaitForValue_VeryOversized() {
			const int NumIterations = 300_000;

			var runner = _oneTwentyEightByteRunnerFactory.NewRunner(new Dummy128ByteVal(0, 0));

			// (T)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.Alpha);
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.Alpha == NumIterations) break;
						if ((curVal.Alpha & 1) == 0) {
							AssertAreEqual(curVal.Alpha + 1, target.SpinWaitForValue(new Dummy128ByteVal(curVal.Alpha + 1, 0)).Alpha);
						}
						else {
							target.Value = new Dummy128ByteVal(curVal.Alpha + 1, 0);
						}
					}
				},
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.Alpha == NumIterations) break;
						if ((curVal.Alpha & 1) == 1) {
							AssertAreEqual(curVal.Alpha + 1, target.SpinWaitForValue(new Dummy128ByteVal(curVal.Alpha + 1, 0)).Alpha);
						}
						else {
							target.Value = new Dummy128ByteVal(curVal.Alpha + 1, 0);
						}
					}
				}
			);

			// (Func<T, bool>)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.Alpha);
			runner.ExecuteWriterReaderTests(
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.Alpha == NumIterations) break;
						target.TryExchange(new Dummy128ByteVal(curVal.Alpha + 1, 0), curVal);
					}
				},
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.Alpha == NumIterations) break;
						AssertTrue(target.SpinWaitForValue(c => c.Alpha > curVal.Alpha).Alpha >= curVal.Alpha);
					}
				}
			);

			// (Func<T, TContext, bool>, TContext)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.Alpha);
			runner.ExecuteWriterReaderTests(
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.Alpha == NumIterations) break;
						target.TryExchange(new Dummy128ByteVal(curVal.Alpha + 1, 0), curVal);
					}
				},
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.Alpha == NumIterations) break;
						AssertTrue(target.SpinWaitForValue((c, ctx) => c.Alpha > ctx.Alpha, curVal).Alpha >= curVal.Alpha);
					}
				}
			);
		}

		[Test]
		public void Exchange_VeryOversized() {
			const int NumIterations = 300_000;

			var atomicIntA = new AtomicInt(0);
			var atomicIntB = new AtomicInt(0);
			var runner = _oneTwentyEightByteRunnerFactory.NewRunner(new Dummy128ByteVal(0, 0));

			// (T)
			runner.GlobalSetUp = (_, __) => { atomicIntA.Set(0); atomicIntB.Set(0); };
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					var newA = atomicIntA.Increment().CurrentValue;
					var newB = atomicIntB.Increment().CurrentValue;
					var newValue = new Dummy128ByteVal(newA, newB);
					var prev = target.Exchange(newValue).PreviousValue;
					AssertAreEqual(prev.Alpha, newA - 1);
					AssertAreEqual(prev.Bravo, newB - 1);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					AssertTrue(prev.Alpha <= cur.Alpha);
					AssertTrue(prev.Bravo <= cur.Bravo);
				}
			);
			runner.GlobalSetUp = null;
			runner.AllThreadsTearDown = null;

			// (Func<T, T>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					var exchRes = target.Exchange(t => new Dummy128ByteVal(t.Alpha + 1, t.Bravo + 1));
					AssertAreEqual(exchRes.PreviousValue.Alpha + 1, exchRes.CurrentValue.Alpha);
					AssertAreEqual(exchRes.PreviousValue.Bravo + 1, exchRes.CurrentValue.Bravo);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TContext, T>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations * 2, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					var exchRes = target.Exchange((t, ctx) => new Dummy128ByteVal(t.Alpha + 1, t.Bravo + ctx), 2);
					AssertAreEqual(exchRes.PreviousValue.Alpha + 1, exchRes.CurrentValue.Alpha);
					AssertAreEqual(exchRes.PreviousValue.Bravo + 2, exchRes.CurrentValue.Bravo);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void SpinWaitForExchangeWithoutContext_VeryOversized() {
			const int NumIterations = 100_000;

			var runner = _oneTwentyEightByteRunnerFactory.NewRunner(new Dummy128ByteVal(0, 0));

			// (T, T)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(new Dummy128ByteVal(nextVal + 1, nextVal + 1), new Dummy128ByteVal(nextVal, nextVal));
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(new Dummy128ByteVal(nextVal + 1, nextVal + 1), new Dummy128ByteVal(nextVal, nextVal));
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, T)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(-NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => new Dummy128ByteVal(c.Alpha + 1, c.Bravo - 1), new Dummy128ByteVal(nextVal, -nextVal));
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(-nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(-(nextVal + 1), exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => new Dummy128ByteVal(c.Alpha + 1, c.Bravo - 1), new Dummy128ByteVal(nextVal, -nextVal));
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(-nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(-(nextVal + 1), exchRes.CurrentValue.Bravo);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (T, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(new Dummy128ByteVal(nextVal + 1, nextVal + 1), (c, n) => n.Alpha == c.Alpha + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(new Dummy128ByteVal(nextVal + 1, nextVal + 1), (c, n) => n.Alpha == c.Alpha + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => new Dummy128ByteVal(nextVal + 1, c.Bravo + 1), (c, n) => n.Alpha == c.Alpha + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => new Dummy128ByteVal(nextVal + 1, c.Bravo + 1), (c, n) => n.Alpha == c.Alpha + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void SpinWaitForExchangeWithContext_VeryOversized() {
			const int NumIterations = 100_000;

			var runner = _oneTwentyEightByteRunnerFactory.NewRunner(new Dummy128ByteVal(0, 0));

			// (Func<T, TContext, T>, T)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => new Dummy128ByteVal(ctx, c.Bravo + 1), nextVal + 1, new Dummy128ByteVal(nextVal, nextVal));
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => new Dummy128ByteVal(ctx, c.Bravo + 1), nextVal + 1, new Dummy128ByteVal(nextVal, nextVal));
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (T, Func<T, T, TContext, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(new Dummy128ByteVal(nextVal + 1, nextVal + 1), (c, n, ctx) => n.Alpha == c.Alpha + ctx, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(new Dummy128ByteVal(nextVal + 1, nextVal + 1), (c, n, ctx) => n.Alpha == c.Alpha + ctx, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TMapContext, T>, Func<T, T, TPredicateContext, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => new Dummy128ByteVal(ctx, c.Bravo + 1), nextVal + 1, (c, n, ctx) => n.Alpha == c.Alpha + ctx, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => new Dummy128ByteVal(ctx, c.Bravo + 1), nextVal + 1, (c, n, ctx) => n.Alpha == c.Alpha + ctx, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TContext, T>, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => new Dummy128ByteVal(ctx, c.Bravo + 1), nextVal + 1, (c, n) => n.Alpha == c.Alpha + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => new Dummy128ByteVal(ctx, c.Bravo + 1), nextVal + 1, (c, n) => n.Alpha == c.Alpha + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, Func<T, T, TContext, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => new Dummy128ByteVal(nextVal + 1, c.Bravo + 1), (c, n, ctx) => n.Alpha == c.Alpha + ctx, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => new Dummy128ByteVal(nextVal + 1, c.Bravo + 1), (c, n, ctx) => n.Alpha == c.Alpha + ctx, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void TryExchangeWithoutContext_VeryOversized() {
			const int NumIterations = 200_000;

			var runner = _oneTwentyEightByteRunnerFactory.NewRunner(new Dummy128ByteVal(0, 0));

			// (T, T)
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curValue = target.Value;
					var newValue = new Dummy128ByteVal(0, curValue.Bravo + 1);
					target.TryExchange(newValue, curValue);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => AssertTrue(cur.Bravo >= prev.Bravo)
			);

			// (T, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(-1 * NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var newValue = new Dummy128ByteVal(curValue.Alpha + 1, curValue.Bravo - 1);
						var (wasSet, prevValue, setValue) = target.TryExchange(newValue, (c, n) => c.Alpha + 1 == n.Alpha && c.Bravo - 1 == n.Bravo);
						if (wasSet) {
							AssertAreEqual(curValue, prevValue);
							AssertAreEqual(newValue, setValue);
						}
						else {
							AssertAreNotEqual(curValue, prevValue);
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

					var (wasSet, prevValue, CurrentValue) = target.TryExchange(
						c => c.Bravo < c.Alpha
							? new Dummy128ByteVal(c.Alpha, c.Bravo + 1)
							: new Dummy128ByteVal(c.Alpha + 1, c.Bravo),
						curValue
					);

					if (wasSet) {
						AssertAreEqual(curValue, prevValue);
						AssertAreEqual(prevValue.Bravo < prevValue.Alpha ? new Dummy128ByteVal(prevValue.Alpha, prevValue.Bravo + 1) : new Dummy128ByteVal(prevValue.Alpha + 1, prevValue.Bravo), CurrentValue);
					}

					else AssertAreNotEqual(curValue, prevValue);
				},
				NumIterations
			);

			// (Func<T, T>, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(-1 * NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var (wasSet, prevValue, CurrentValue) = target.TryExchange(c => new Dummy128ByteVal(c.Alpha + 1, c.Bravo - 1), (c, n) => c.Alpha + 1 == n.Alpha && c.Bravo - 1 == n.Bravo && c.Alpha < NumIterations);
						if (wasSet) {
							AssertAreEqual(new Dummy128ByteVal(prevValue.Alpha + 1, prevValue.Bravo - 1), CurrentValue);
							AssertTrue(CurrentValue.Alpha <= NumIterations);
						}
						else AssertAreEqual(prevValue, CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void TryExchangeWithContext_VeryOversized() {
			const int NumIterations = 300_000;

			var runner = _oneTwentyEightByteRunnerFactory.NewRunner(new Dummy128ByteVal(0, 0));

			// (T, Func<T, T, TContext, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(-1 * NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var newValue = new Dummy128ByteVal(curValue.Alpha + 1, curValue.Bravo - 1);
						var (wasSet, prevValue, setValue) = target.TryExchange(newValue, (c, n, ctx) => c.Alpha + ctx == n.Alpha && c.Bravo - ctx == n.Bravo, 1);
						if (wasSet) {
							AssertAreEqual(curValue, prevValue);
							AssertAreEqual(newValue, setValue);
						}
						else {
							AssertAreNotEqual(curValue, prevValue);
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

					var (wasSet, prevValue, CurrentValue) = target.TryExchange(
						(c, ctx) => c.Bravo < c.Alpha
							? new Dummy128ByteVal(c.Alpha, c.Bravo + ctx)
							: new Dummy128ByteVal(c.Alpha + ctx, c.Bravo),
						1, curValue);

					if (wasSet) {
						AssertAreEqual(curValue, prevValue);
						AssertAreEqual(prevValue.Bravo < prevValue.Alpha ? new Dummy128ByteVal(prevValue.Alpha, prevValue.Bravo + 1) : new Dummy128ByteVal(prevValue.Alpha + 1, prevValue.Bravo), CurrentValue);
					}
					else AssertAreNotEqual(curValue, prevValue);
				},
				NumIterations
			);

			// (Func<T, TContext, T>, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(-1 * NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var (wasSet, prevValue, CurrentValue) = target.TryExchange((c, ctx) => new Dummy128ByteVal(c.Alpha + ctx, c.Bravo - ctx), 1, (c, n) => c.Alpha + 1 == n.Alpha && c.Bravo - 1 == n.Bravo && c.Alpha < NumIterations);
						if (wasSet) {
							AssertAreEqual(new Dummy128ByteVal(prevValue.Alpha + 1, prevValue.Bravo - 1), CurrentValue);
							AssertTrue(CurrentValue.Alpha <= NumIterations);
						}
						else AssertAreEqual(CurrentValue, prevValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, Func<T, T, TContext, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(-1 * NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var (wasSet, prevValue, CurrentValue) = target.TryExchange(c => new Dummy128ByteVal(c.Alpha + 1, c.Bravo - 1), (c, n, ctx) => c.Alpha + ctx == n.Alpha && c.Bravo - ctx == n.Bravo && c.Alpha < NumIterations, 1);
						if (wasSet) {
							AssertAreEqual(new Dummy128ByteVal(prevValue.Alpha + 1, prevValue.Bravo - 1), CurrentValue);
							AssertTrue(CurrentValue.Alpha <= NumIterations);
						}
						else AssertAreEqual(CurrentValue, prevValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TMapContext, T>, Func<T, T, TPredicateContext, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(-1 * NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var (wasSet, prevValue, CurrentValue) = target.TryExchange((c, ctx) => new Dummy128ByteVal(c.Alpha + ctx, c.Bravo - ctx), 1, (c, n, ctx) => c.Alpha + 1 == n.Alpha && c.Bravo - 1 == n.Bravo && c.Alpha < ctx, NumIterations);
						if (wasSet) {
							AssertAreEqual(new Dummy128ByteVal(prevValue.Alpha + 1, prevValue.Bravo - 1), CurrentValue);
							AssertTrue(CurrentValue.Alpha <= NumIterations);
						}
						else AssertAreEqual(CurrentValue, prevValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}
		#endregion
	}
}