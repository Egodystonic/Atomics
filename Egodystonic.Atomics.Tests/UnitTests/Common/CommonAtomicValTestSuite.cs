// (c) Egodystonic Studios 2018


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;
using Egodystonic.Atomics.Tests.Harness;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.UnitTests.Common {
	abstract class CommonAtomicValTestSuite<TTarget> : CommonAtomicTestSuite<DummyImmutableVal, TTarget> where TTarget : IAtomic<DummyImmutableVal>, new() {
		[Test]
		public void GetAndSetAndValue() {
			const int NumIterations = 1_000_000;
			var atomicLong = new AtomicLong(0L);
			var runner = NewRunner(new DummyImmutableVal(0, 0));

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					unsafe {
						var newLongVal = atomicLong.Increment().NewValue;
						target.Set(*(DummyImmutableVal*) &newLongVal);
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
						var newLongVal = atomicLong.Increment().NewValue;
						target.Value = *(DummyImmutableVal*) &newLongVal;
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
		public void SpinWaitForValue() {
			const int NumIterations = 300_000;

			var runner = NewRunner(new DummyImmutableVal(0, 0));

			// (T)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.Alpha);
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
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.Alpha);
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
						target.TryExchange(new DummyImmutableVal(curVal.Alpha + 1, 0), curVal);
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
		public void Exchange() {
			const int NumIterations = 300_000;

			var atomicIntA = new AtomicInt(0);
			var atomicIntB = new AtomicInt(0);
			var runner = NewRunner(new DummyImmutableVal(0, 0));

			// (T)
			runner.GlobalSetUp = (_, __) => { atomicIntA.Set(0); atomicIntB.Set(0); };
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					var newA = atomicIntA.Increment().NewValue;
					var newB = atomicIntB.Increment().NewValue;
					var newValue = new DummyImmutableVal(newA, newB);
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
					var exchRes = target.Exchange(t => new DummyImmutableVal(t.Alpha + 1, t.Bravo + 1));
					AssertAreEqual(exchRes.PreviousValue.Alpha + 1, exchRes.NewValue.Alpha);
					AssertAreEqual(exchRes.PreviousValue.Bravo + 1, exchRes.NewValue.Bravo);
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
					var exchRes = target.Exchange((t, ctx) => new DummyImmutableVal(t.Alpha + 1, t.Bravo + ctx), 2);
					AssertAreEqual(exchRes.PreviousValue.Alpha + 1, exchRes.NewValue.Alpha);
					AssertAreEqual(exchRes.PreviousValue.Bravo + 2, exchRes.NewValue.Bravo);
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
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(new DummyImmutableVal(nextVal + 1, nextVal + 1), new DummyImmutableVal(nextVal, nextVal));
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(new DummyImmutableVal(nextVal + 1, nextVal + 1), new DummyImmutableVal(nextVal, nextVal));
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Bravo);
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
						var exchRes = target.SpinWaitForExchange(c => new DummyImmutableVal(c.Alpha + 1, c.Bravo - 1), new DummyImmutableVal(nextVal, -nextVal));
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Alpha);
						AssertAreEqual(-nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(-(nextVal + 1), exchRes.NewValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => new DummyImmutableVal(c.Alpha + 1, c.Bravo - 1), new DummyImmutableVal(nextVal, -nextVal));
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Alpha);
						AssertAreEqual(-nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(-(nextVal + 1), exchRes.NewValue.Bravo);
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
						var exchRes = target.SpinWaitForExchange(new DummyImmutableVal(nextVal + 1, nextVal + 1), (c, n) => n.Alpha == c.Alpha + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(new DummyImmutableVal(nextVal + 1, nextVal + 1), (c, n) => n.Alpha == c.Alpha + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Bravo);
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
						var exchRes = target.SpinWaitForExchange(c => new DummyImmutableVal(nextVal + 1, c.Bravo + 1), (c, n) => n.Alpha == c.Alpha + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => new DummyImmutableVal(nextVal + 1, c.Bravo + 1), (c, n) => n.Alpha == c.Alpha + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Bravo);
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
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => new DummyImmutableVal(ctx, c.Bravo + 1), new DummyImmutableVal(nextVal, nextVal), nextVal + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => new DummyImmutableVal(ctx, c.Bravo + 1), new DummyImmutableVal(nextVal, nextVal), nextVal + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Bravo);
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
						var exchRes = target.SpinWaitForExchange(new DummyImmutableVal(nextVal + 1, nextVal + 1), (c, n, ctx) => n.Alpha == c.Alpha + ctx, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(new DummyImmutableVal(nextVal + 1, nextVal + 1), (c, n, ctx) => n.Alpha == c.Alpha + ctx, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Bravo);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TContext, T>, Func<T, T, TContext, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(NumIterations, target.Value.Bravo);
			};
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => new DummyImmutableVal(ctx, c.Bravo + 1), (c, n, ctx) => n.Alpha == c.Alpha + 1 && n.Bravo == ctx, nextVal + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => new DummyImmutableVal(ctx, c.Bravo + 1), (c, n, ctx) => n.Alpha == c.Alpha + 1 && n.Bravo == ctx, nextVal + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Bravo);
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
						var exchRes = target.SpinWaitForExchange((c, ctx) => new DummyImmutableVal(ctx, c.Bravo + 1), (c, n, ctx) => n.Alpha == c.Alpha + ctx, nextVal + 1, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => new DummyImmutableVal(ctx, c.Bravo + 1), (c, n, ctx) => n.Alpha == c.Alpha + ctx, nextVal + 1, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Bravo);
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
						var exchRes = target.SpinWaitForExchange((c, ctx) => new DummyImmutableVal(ctx, c.Bravo + 1), (c, n) => n.Alpha == c.Alpha + 1, nextVal + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange((c, ctx) => new DummyImmutableVal(ctx, c.Bravo + 1), (c, n) => n.Alpha == c.Alpha + 1, nextVal + 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Bravo);
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
						var exchRes = target.SpinWaitForExchange(c => new DummyImmutableVal(nextVal + 1, c.Bravo + 1), (c, n, ctx) => n.Alpha == c.Alpha + ctx, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Bravo);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = i;
						var exchRes = target.SpinWaitForExchange(c => new DummyImmutableVal(nextVal + 1, c.Bravo + 1), (c, n, ctx) => n.Alpha == c.Alpha + ctx, 1);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Alpha);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Alpha);
						AssertAreEqual(nextVal, exchRes.PreviousValue.Bravo);
						AssertAreEqual(nextVal + 1, exchRes.NewValue.Bravo);
					}
				}
			);
			runner.AllThreadsTearDown = null;
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
						var newValue = new DummyImmutableVal(curValue.Alpha + 1, curValue.Bravo - 1);
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

					var (wasSet, prevValue, newValue) = target.TryExchange(
						c => c.Bravo < c.Alpha
							? new DummyImmutableVal(c.Alpha, c.Bravo + 1)
							: new DummyImmutableVal(c.Alpha + 1, c.Bravo),
						curValue
					);

					if (wasSet) {
						AssertAreEqual(curValue, prevValue);
						AssertAreEqual(prevValue.Bravo < prevValue.Alpha ? new DummyImmutableVal(prevValue.Alpha, prevValue.Bravo + 1) : new DummyImmutableVal(prevValue.Alpha + 1, prevValue.Bravo), newValue);
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
						var (wasSet, prevValue, newValue) = target.TryExchange(c => new DummyImmutableVal(c.Alpha + 1, c.Bravo - 1), (c, n) => c.Alpha + 1 == n.Alpha && c.Bravo - 1 == n.Bravo && c.Alpha < NumIterations);
						if (wasSet) {
							AssertAreEqual(new DummyImmutableVal(prevValue.Alpha + 1, prevValue.Bravo - 1), newValue);
							AssertTrue(newValue.Alpha <= NumIterations);
						}
						else AssertAreEqual(prevValue, newValue);
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
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(-1 * NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var newValue = new DummyImmutableVal(curValue.Alpha + 1, curValue.Bravo - 1);
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

					var (wasSet, prevValue, newValue) = target.TryExchange(
						(c, ctx) => c.Bravo < c.Alpha
							? new DummyImmutableVal(c.Alpha, c.Bravo + ctx)
							: new DummyImmutableVal(c.Alpha + ctx, c.Bravo),
						curValue,
						1
					);

					if (wasSet) {
						AssertAreEqual(curValue, prevValue);
						AssertAreEqual(prevValue.Bravo < prevValue.Alpha ? new DummyImmutableVal(prevValue.Alpha, prevValue.Bravo + 1) : new DummyImmutableVal(prevValue.Alpha + 1, prevValue.Bravo), newValue);
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
						var (wasSet, prevValue, newValue) = target.TryExchange((c, ctx) => new DummyImmutableVal(c.Alpha + ctx, c.Bravo - ctx), (c, n) => c.Alpha + 1 == n.Alpha && c.Bravo - 1 == n.Bravo && c.Alpha < NumIterations, 1);
						if (wasSet) {
							AssertAreEqual(new DummyImmutableVal(prevValue.Alpha + 1, prevValue.Bravo - 1), newValue);
							AssertTrue(newValue.Alpha <= NumIterations);
						}
						else AssertAreEqual(newValue, prevValue);
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
						var (wasSet, prevValue, newValue) = target.TryExchange(c => new DummyImmutableVal(c.Alpha + 1, c.Bravo - 1), (c, n, ctx) => c.Alpha + ctx == n.Alpha && c.Bravo - ctx == n.Bravo && c.Alpha < NumIterations, 1);
						if (wasSet) {
							AssertAreEqual(new DummyImmutableVal(prevValue.Alpha + 1, prevValue.Bravo - 1), newValue);
							AssertTrue(newValue.Alpha <= NumIterations);
						}
						else AssertAreEqual(newValue, prevValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TContext, T>, Func<T, T, TContext, bool>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(NumIterations, target.Value.Alpha);
				AssertAreEqual(-1 * NumIterations, target.Value.Bravo);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Alpha == NumIterations) return;
						var (wasSet, prevValue, newValue) = target.TryExchange((c, ctx) => new DummyImmutableVal(c.Alpha + ctx, c.Bravo - ctx), (c, n, ctx) => c.Alpha + ctx == n.Alpha && c.Bravo - ctx == n.Bravo && c.Alpha < NumIterations, 1);
						if (wasSet) {
							AssertAreEqual(new DummyImmutableVal(prevValue.Alpha + 1, prevValue.Bravo - 1), newValue);
							AssertTrue(newValue.Alpha <= NumIterations);
						}
						else AssertAreEqual(newValue, prevValue);
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
						var (wasSet, prevValue, newValue) = target.TryExchange((c, ctx) => new DummyImmutableVal(c.Alpha + ctx, c.Bravo - ctx), (c, n, ctx) => c.Alpha + 1 == n.Alpha && c.Bravo - 1 == n.Bravo && c.Alpha < ctx, 1, NumIterations);
						if (wasSet) {
							AssertAreEqual(new DummyImmutableVal(prevValue.Alpha + 1, prevValue.Bravo - 1), newValue);
							AssertTrue(newValue.Alpha <= NumIterations);
						}
						else AssertAreEqual(newValue, prevValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}
	}
}