// (c) Egodystonic Studios 2018


using System;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.UnitTests.Common {
	abstract class CommonAtomicValTestSuite<TTarget> : CommonAtomicTestSuite<DummyImmutableVal, TTarget> where TTarget : IAtomic<DummyImmutableVal>, new() {
		[Test]
		public void GetAndSetAndValue() {
			const int NumIterations = 1_000_000;
			var atomicLong = new LockFreeInt64(0L);
			var runner = NewRunner(new DummyImmutableVal(0, 0));

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					unsafe {
						var newLongVal = atomicLong.Increment().CurrentValue;
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
						var newLongVal = atomicLong.Increment().CurrentValue;
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

		[Test]
		public void SpinWaitForValue() {
			const int NumIterations = 300_000;

			var runner = NewRunner(new DummyImmutableVal(0, 0));

			// (T)
			runner.AllThreadsTearDown = target => FastAssertEqual(NumIterations, target.Value.Alpha);
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.Alpha == NumIterations) break;
						if ((curVal.Alpha & 1) == 0) {
							AssertAreEqual(curVal.Alpha + 1, target.SpinWaitForValue(new DummyImmutableVal(curVal.Alpha + 1, 0)).Alpha);
						}
						else {
							target.Value = new DummyImmutableVal(curVal.Alpha + 1, 0);
						}
					}
				},
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.Alpha == NumIterations) break;
						if ((curVal.Alpha & 1) == 1) {
							AssertAreEqual(curVal.Alpha + 1, target.SpinWaitForValue(new DummyImmutableVal(curVal.Alpha + 1, 0)).Alpha);
						}
						else {
							target.Value = new DummyImmutableVal(curVal.Alpha + 1, 0);
						}
					}
				}
			);

			// (Func<T, bool>)
			runner.AllThreadsTearDown = target => FastAssertEqual(NumIterations, target.Value.Alpha);
			runner.ExecuteWriterReaderTests(
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.Alpha == NumIterations) break;
						target.TryExchange(new DummyImmutableVal(curVal.Alpha + 1, 0), curVal);
					}
				},
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.Alpha == NumIterations) break;
						FastAssertTrue(target.SpinWaitForValue(c => c.Alpha > curVal.Alpha).Alpha >= curVal.Alpha);
					}
				}
			);

			// (Func<T, TContext, bool>, TContext)
			runner.AllThreadsTearDown = target => FastAssertEqual(NumIterations, target.Value.Alpha);
			runner.ExecuteWriterReaderTests(
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.Alpha == NumIterations) break;
						target.TryExchange(new DummyImmutableVal(curVal.Alpha + 1, 0), curVal);
					}
				},
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.Alpha == NumIterations) break;
						FastAssertTrue(target.SpinWaitForValue((c, ctx) => c.Alpha > ctx.Alpha, curVal).Alpha >= curVal.Alpha);
					}
				}
			);
		}

		[Test]
		public void FastExchange() {
			const int NumIterations = 3_000_000;

			var atomicIntA = new LockFreeInt32(0);
			var atomicIntB = new LockFreeInt32(0);
			var runner = NewRunner(new DummyImmutableVal(0, 0));

			// (T)
			runner.GlobalSetUp = (_, __) => { atomicIntA.Set(0); atomicIntB.Set(0); };
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					var newA = atomicIntA.Increment().CurrentValue;
					var newB = atomicIntB.Increment().CurrentValue;
					var newValue = new DummyImmutableVal(newA, newB);
					var prev = target.FastExchange(newValue);
					AssertAreEqual(prev.Alpha, newA - 1);
					AssertAreEqual(prev.Bravo, newB - 1);
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

		[Test]
		public void Exchange() {
			const int NumIterations = 300_000;

			var atomicIntA = new LockFreeInt32(0);
			var atomicIntB = new LockFreeInt32(0);
			var runner = NewRunner(new DummyImmutableVal(0, 0));

			// (T)
			runner.GlobalSetUp = (_, __) => { atomicIntA.Set(0); atomicIntB.Set(0); };
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					var newA = atomicIntA.Increment().CurrentValue;
					var newB = atomicIntB.Increment().CurrentValue;
					var newValue = new DummyImmutableVal(newA, newB);
					var prev = target.Exchange(newValue).PreviousValue;
					AssertAreEqual(prev.Alpha, newA - 1);
					AssertAreEqual(prev.Bravo, newB - 1);
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

			// (Func<T, T>)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					var exchRes = target.Exchange(t => new DummyImmutableVal(t.Alpha + 1, t.Bravo + 1));
					AssertAreEqual(exchRes.PreviousValue.Alpha + 1, exchRes.CurrentValue.Alpha);
					AssertAreEqual(exchRes.PreviousValue.Bravo + 1, exchRes.CurrentValue.Bravo);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TContext, T>)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(NumIterations * 2, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					var exchRes = target.Exchange((t, ctx) => new DummyImmutableVal(t.Alpha + 1, t.Bravo + ctx), 2);
					AssertAreEqual(exchRes.PreviousValue.Alpha + 1, exchRes.CurrentValue.Alpha);
					AssertAreEqual(exchRes.PreviousValue.Bravo + 2, exchRes.CurrentValue.Bravo);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void SpinWaitForExchangeWithoutContext() {
			const int NumIterations = 100_000;

			var runner = NewRunner(new DummyImmutableVal(0, 0));

			// (T, T)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(new DummyImmutableVal(nextVal + 1, nextVal + 1), new DummyImmutableVal(nextVal, nextVal));
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(new DummyImmutableVal(nextVal + 1, nextVal + 1), new DummyImmutableVal(nextVal, nextVal));
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
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(-NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => new DummyImmutableVal(c.Alpha + 1, c.Bravo - 1), new DummyImmutableVal(nextVal, -nextVal));
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(-nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(-(nextVal + 1), exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => new DummyImmutableVal(c.Alpha + 1, c.Bravo - 1), new DummyImmutableVal(nextVal, -nextVal));
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
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(new DummyImmutableVal(nextVal + 1, nextVal + 1), (c, n) => n.Alpha == c.Alpha + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(new DummyImmutableVal(nextVal + 1, nextVal + 1), (c, n) => n.Alpha == c.Alpha + 1);
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
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => new DummyImmutableVal(nextVal + 1, c.Bravo + 1), (c, n) => n.Alpha == c.Alpha + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => new DummyImmutableVal(nextVal + 1, c.Bravo + 1), (c, n) => n.Alpha == c.Alpha + 1);
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
		public void SpinWaitForExchangeWithContext() {
			const int NumIterations = 100_000;

			var runner = NewRunner(new DummyImmutableVal(0, 0));

			// (Func<T, TContext, T>, T)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => new DummyImmutableVal(ctx, c.Bravo + 1), nextVal + 1, new DummyImmutableVal(nextVal, nextVal));
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => new DummyImmutableVal(ctx, c.Bravo + 1), nextVal + 1, new DummyImmutableVal(nextVal, nextVal));
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
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(new DummyImmutableVal(nextVal + 1, nextVal + 1), (c, n, ctx) => n.Alpha == c.Alpha + ctx, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(new DummyImmutableVal(nextVal + 1, nextVal + 1), (c, n, ctx) => n.Alpha == c.Alpha + ctx, 1);
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
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => new DummyImmutableVal(ctx, c.Bravo + 1), nextVal + 1, (c, n, ctx) => n.Alpha == c.Alpha + ctx, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => new DummyImmutableVal(ctx, c.Bravo + 1), nextVal + 1, (c, n, ctx) => n.Alpha == c.Alpha + ctx, 1);
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
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => new DummyImmutableVal(ctx, c.Bravo + 1), nextVal + 1, (c, n) => n.Alpha == c.Alpha + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => new DummyImmutableVal(ctx, c.Bravo + 1), nextVal + 1, (c, n) => n.Alpha == c.Alpha + 1);
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
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => new DummyImmutableVal(nextVal + 1, c.Bravo + 1), (c, n, ctx) => n.Alpha == c.Alpha + ctx, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.CurrentValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => new DummyImmutableVal(nextVal + 1, c.Bravo + 1), (c, n, ctx) => n.Alpha == c.Alpha + ctx, 1);
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
		public void FastTryExchange() {
			const int NumIterations = 2_000_000;

			var runner = NewRunner(new DummyImmutableVal(0, 0));

			// (T, T)
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curValue = target.Value;
					var newValue = new DummyImmutableVal(0, curValue.Bravo + 1);
					target.FastTryExchange(newValue, curValue);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => FastAssertTrue(cur.Bravo >= prev.Bravo)
			);
		}

		[Test]
		public void TryExchangeWithoutContext() {
			const int NumIterations = 200_000;

			var runner = NewRunner(new DummyImmutableVal(0, 0));

			// (T, T)
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curValue = target.Value;
					var newValue = new DummyImmutableVal(0, curValue.Bravo + 1);
					target.TryExchange(newValue, curValue);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => FastAssertTrue(cur.Bravo >= prev.Bravo)
			);

			// (T, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(-1 * NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var newValue = new DummyImmutableVal(curValue.Alpha + 1, curValue.Bravo - 1);
						var (wasSet, prevValue, setValue) = target.TryExchange(newValue, (c, n) => c.Alpha + 1 == n.Alpha && c.Bravo - 1 == n.Bravo);
						if (wasSet) {
							FastAssertEqual<DummyImmutableVal>(curValue, prevValue);
							FastAssertEqual<DummyImmutableVal>(newValue, setValue);
						}
						else {
							FastAssertNotEqual<DummyImmutableVal>(curValue, prevValue);
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
							? new DummyImmutableVal(c.Alpha, c.Bravo + 1)
							: new DummyImmutableVal(c.Alpha + 1, c.Bravo),
						curValue
					);

					if (wasSet) {
						FastAssertEqual<DummyImmutableVal>(curValue, prevValue);
						FastAssertEqual<DummyImmutableVal>(prevValue.Bravo < prevValue.Alpha ? new DummyImmutableVal(prevValue.Alpha, prevValue.Bravo + 1) : new DummyImmutableVal(prevValue.Alpha + 1, prevValue.Bravo), CurrentValue);
					}

					else FastAssertNotEqual<DummyImmutableVal>(curValue, prevValue);
				},
				NumIterations
			);

			// (Func<T, T>, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(-1 * NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var (wasSet, prevValue, CurrentValue) = target.TryExchange(c => new DummyImmutableVal(c.Alpha + 1, c.Bravo - 1), (c, n) => c.Alpha + 1 == n.Alpha && c.Bravo - 1 == n.Bravo && c.Alpha < NumIterations);
						if (wasSet) {
							FastAssertEqual<DummyImmutableVal>(new DummyImmutableVal(prevValue.Alpha + 1, prevValue.Bravo - 1), CurrentValue);
							FastAssertTrue(CurrentValue.Alpha <= NumIterations);
						}
						else AssertAreEqual(prevValue, CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void TryExchangeWithContext() {
			const int NumIterations = 300_000;

			var runner = NewRunner(new DummyImmutableVal(0, 0));

			// (T, Func<T, T, TContext, bool>)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(-1 * NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var newValue = new DummyImmutableVal(curValue.Alpha + 1, curValue.Bravo - 1);
						var (wasSet, prevValue, setValue) = target.TryExchange(newValue, (c, n, ctx) => c.Alpha + ctx == n.Alpha && c.Bravo - ctx == n.Bravo, 1);
						if (wasSet) {
							FastAssertEqual<DummyImmutableVal>(curValue, prevValue);
							FastAssertEqual<DummyImmutableVal>(newValue, setValue);
						}
						else {
							FastAssertNotEqual<DummyImmutableVal>(curValue, prevValue);
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
							? new DummyImmutableVal(c.Alpha, c.Bravo + ctx)
							: new DummyImmutableVal(c.Alpha + ctx, c.Bravo),
						1, curValue);

					if (wasSet) {
						FastAssertEqual<DummyImmutableVal>(curValue, prevValue);
						FastAssertEqual<DummyImmutableVal>(prevValue.Bravo < prevValue.Alpha ? new DummyImmutableVal(prevValue.Alpha, prevValue.Bravo + 1) : new DummyImmutableVal(prevValue.Alpha + 1, prevValue.Bravo), CurrentValue);
					}
					else FastAssertNotEqual<DummyImmutableVal>(curValue, prevValue);
				},
				NumIterations
			);

			// (Func<T, TContext, T>, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(-1 * NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var (wasSet, prevValue, CurrentValue) = target.TryExchange((c, ctx) => new DummyImmutableVal(c.Alpha + ctx, c.Bravo - ctx), 1, (c, n) => c.Alpha + 1 == n.Alpha && c.Bravo - 1 == n.Bravo && c.Alpha < NumIterations);
						if (wasSet) {
							FastAssertEqual<DummyImmutableVal>(new DummyImmutableVal(prevValue.Alpha + 1, prevValue.Bravo - 1), CurrentValue);
							FastAssertTrue(CurrentValue.Alpha <= NumIterations);
						}
						else AssertAreEqual(CurrentValue, prevValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, Func<T, T, TContext, bool>)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(-1 * NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var (wasSet, prevValue, CurrentValue) = target.TryExchange(c => new DummyImmutableVal(c.Alpha + 1, c.Bravo - 1), (c, n, ctx) => c.Alpha + ctx == n.Alpha && c.Bravo - ctx == n.Bravo && c.Alpha < NumIterations, 1);
						if (wasSet) {
							FastAssertEqual<DummyImmutableVal>(new DummyImmutableVal(prevValue.Alpha + 1, prevValue.Bravo - 1), CurrentValue);
							FastAssertTrue(CurrentValue.Alpha <= NumIterations);
						}
						else AssertAreEqual(CurrentValue, prevValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TMapContext, T>, Func<T, T, TPredicateContext, bool>)
			runner.AllThreadsTearDown = target => {
				FastAssertEqual(NumIterations, target.Value.Alpha);
				FastAssertEqual(-1 * NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var (wasSet, prevValue, CurrentValue) = target.TryExchange((c, ctx) => new DummyImmutableVal(c.Alpha + ctx, c.Bravo - ctx), 1, (c, n, ctx) => c.Alpha + 1 == n.Alpha && c.Bravo - 1 == n.Bravo && c.Alpha < ctx, NumIterations);
						if (wasSet) {
							FastAssertEqual<DummyImmutableVal>(new DummyImmutableVal(prevValue.Alpha + 1, prevValue.Bravo - 1), CurrentValue);
							FastAssertTrue(CurrentValue.Alpha <= NumIterations);
						}
						else AssertAreEqual(CurrentValue, prevValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}
	}
}