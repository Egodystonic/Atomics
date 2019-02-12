// (c) Egodystonic Studios 2018


using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.UnitTests.Common {
	abstract class CommonAtomicRefTestSuite<TTarget> : CommonAtomicTestSuite<DummyImmutableRef, TTarget> where TTarget : IAtomic<DummyImmutableRef>, new() {
		[Test]
		public void GetAndSetAndValue() {
			const int NumIterations = 1_000_000;
			var atomicLong = new AtomicLong(0L);
			var runner = NewRunner(new DummyImmutableRef(0L));

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => target.Set(new DummyImmutableRef(atomicLong.Increment().CurrentValue)),
				NumIterations,
				target => target.Get(),
				(prev, cur) => AssertTrue(prev.LongProp <= cur.LongProp)
			);

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => target.Value = new DummyImmutableRef(atomicLong.Increment().CurrentValue),
				NumIterations,
				target => target.Value,
				(prev, cur) => AssertTrue(prev.LongProp <= cur.LongProp)
			);
		}

		[Test]
		public void SpinWaitForValue() {
			const int NumIterations = 300_000;

			var runner = NewRunner(new DummyImmutableRef(0L));

			// (T)
			var valuesArr = Enumerable.Range(0, NumIterations + 1)
				.Select(i => new DummyImmutableRef(i))
				.ToArray();
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.LongProp == NumIterations) break;
						if ((curVal.LongProp & 1) == 0) {
							AssertAreEqual(curVal.LongProp + 1, target.SpinWaitForValue(valuesArr[(int) (curVal.LongProp + 1L)]).LongProp);
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
							AssertAreEqual(curVal.LongProp + 1, target.SpinWaitForValue(valuesArr[(int) (curVal.LongProp + 1L)]).LongProp);
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
						target.TryExchange(new DummyImmutableRef(curVal.LongProp + 1L), curVal);
					}
				},
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.LongProp == NumIterations) break;
						AssertTrue(target.SpinWaitForValue(c => c.LongProp > curVal.LongProp).LongProp >= curVal.LongProp);
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
						target.TryExchange(new DummyImmutableRef(curVal.LongProp + 1L), curVal);
					}
				},
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.LongProp == NumIterations) break;
						AssertTrue(target.SpinWaitForValue((c, ctx) => c.LongProp > ctx.LongProp, curVal).LongProp >= curVal.LongProp);
					}
				}
			);
		}

		[Test]
		public void FastExchange() {
			const int NumIterations = 1_000_000;
			var runner = NewRunner(new DummyImmutableRef(0L));

			// (T)
			var atomicLong = new AtomicLong(0L);
			runner.GlobalSetUp = (_, __) => atomicLong.Set(0L);
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					var newLongValue = atomicLong.Increment().CurrentValue;
					var prev = target.FastExchange(new DummyImmutableRef(newLongValue));
					AssertAreEqual(prev.LongProp, newLongValue - 1L);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => AssertTrue(cur.LongProp >= prev.LongProp)
			);
			runner.GlobalSetUp = null;
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void Exchange() {
			const int NumIterations = 300_000;
			var runner = NewRunner(new DummyImmutableRef(0L));

			// (T)
			var atomicLong = new AtomicLong(0L);
			runner.GlobalSetUp = (_, __) => atomicLong.Set(0L);
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					var newLongValue = atomicLong.Increment().CurrentValue;
					var prev = target.Exchange(new DummyImmutableRef(newLongValue)).PreviousValue;
					AssertAreEqual(prev.LongProp, newLongValue - 1L);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => AssertTrue(cur.LongProp >= prev.LongProp)
			);
			runner.GlobalSetUp = null;
			runner.AllThreadsTearDown = null;

			// (Func<T, T>)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					var (prevValue, CurrentValue) = target.Exchange(c => new DummyImmutableRef(c.LongProp + 1L));
					AssertAreEqual(prevValue.LongProp, CurrentValue.LongProp - 1L);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => AssertTrue(cur.LongProp >= prev.LongProp)
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TContext, T>)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations * 10L, target.Value.LongProp);
			runner.ExecuteFreeThreadedTests(
				target => {
					var (prevValue, CurrentValue) = target.Exchange((c, ctx) => new DummyImmutableRef(c.LongProp + ctx), 10L);
					AssertAreEqual(prevValue.LongProp, CurrentValue.LongProp - 10L);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		[SuppressMessage("ReSharper", "AccessToModifiedClosure")] // Closure is always modified after use 
		public void SpinWaitForExchangeWithoutContext() {
			const int NumIterations = 30_000;

			var runner = NewRunner(new DummyImmutableRef("0", 0L));

			// (T, T)
			var valuesArr = Enumerable.Range(0, NumIterations + 1)
				.Select(i => new DummyImmutableRef(i.ToString(), i))
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
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, T)
			valuesArr = Enumerable.Range(0, NumIterations + 1)
				.Select(i => new DummyImmutableRef((-i).ToString(), i))
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
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (T, Func<T, T, bool>)
			valuesArr = Enumerable.Range(0, NumIterations + 1)
				.Select(i => new DummyImmutableRef(i.ToString(), i))
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
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, Func<T, T, bool>)
			valuesArr = Enumerable.Range(0, NumIterations + 1)
				.Select(i => new DummyImmutableRef((-i).ToString(), i))
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
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		[SuppressMessage("ReSharper", "AccessToModifiedClosure")] // Closure is always modified after use 
		public void SpinWaitForExchangeWithContext() {
			const int NumIterations = 10_000;

			var runner = NewRunner(new DummyImmutableRef("0", 0L));

			// (Func<T, TContext, T>, T)
			var valuesArr = Enumerable.Range(0, NumIterations + 1)
				.Select(i => new DummyImmutableRef((-i).ToString(), i))
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
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (T, Func<T, T, TContext, bool>)
			valuesArr = Enumerable.Range(0, NumIterations + 1)
				.Select(i => new DummyImmutableRef(i.ToString(), i))
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
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TMapContext, T>, Func<T, T, TPredicateContext, bool>)
			valuesArr = Enumerable.Range(0, NumIterations + 1)
				.Select(i => new DummyImmutableRef((-i).ToString(), i))
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
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TContext, T>, Func<T, T, bool>)
			valuesArr = Enumerable.Range(0, NumIterations + 1)
				.Select(i => new DummyImmutableRef((-i).ToString(), i))
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
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, Func<T, T, TContext, bool>)
			valuesArr = Enumerable.Range(0, NumIterations + 1)
				.Select(i => new DummyImmutableRef((-i).ToString(), i))
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
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void FastTryExchange() {
			const int NumIterations = 1_000_000;

			var runner = NewRunner(new DummyImmutableRef(0L));

			// (T, T)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.LongProp == NumIterations) return;
						var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
						var prevValue = target.FastTryExchange(newValue, curValue);
						var wasSet = prevValue.Equals(curValue);
						var setValue = wasSet ? newValue : prevValue;
						if (wasSet) {
							AssertAreEqualObjects(curValue, prevValue);
							AssertAreEqualObjects(newValue, setValue);
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
		public void TryExchangeWithoutContext() {
			const int NumIterations = 100_000;

			var runner = NewRunner(new DummyImmutableRef(0L));

			// (T, T)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.LongProp == NumIterations) return;
						var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
						var (wasSet, prevValue, setValue) = target.TryExchange(newValue, curValue);
						if (wasSet) {
							AssertAreEqualObjects(curValue, prevValue);
							AssertAreEqualObjects(newValue, setValue);
						}
						else {
							AssertAreNotEqualObjects(curValue, prevValue);
							AssertAreEqualObjects(setValue, prevValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// Func(T, Func<T, T, bool>)
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curValue = target.Value;
					var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
					target.TryExchange(newValue, (c, n) => c.LongProp == n.LongProp - 1L);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => AssertTrue(cur.LongProp >= prev.LongProp)
			);

			// (Func<T, T>, T)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.LongProp == NumIterations) return;
						var (wasSet, prevValue, CurrentValue) = target.TryExchange(c => new DummyImmutableRef(c.LongProp + 1L), curValue);
						if (wasSet) {
							AssertAreEqualObjects(curValue, prevValue);
							AssertAreEqualObjects(prevValue.LongProp + 1L, CurrentValue.LongProp);
						}
						else AssertAreNotEqualObjects(curValue, prevValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, Func<T, T, bool>)
			runner.ExecuteFreeThreadedTests(
				target => {
					var (wasSet, prevValue, CurrentValue) = target.TryExchange(c => new DummyImmutableRef(c.LongProp + 1L), (c, n) => c.LongProp == n.LongProp - 1L);
					if (wasSet) AssertAreEqual(prevValue.LongProp + 1L, CurrentValue.LongProp);
					else AssertAreNotEqual(prevValue.LongProp + 1L, CurrentValue.LongProp);
				},
				NumIterations
			);
		}

		[Test]
		public void TryExchangeWithContext() {
			const int NumIterations = 100_000;

			var runner = NewRunner(new DummyImmutableRef(0L));

			// Func(T, Func<T, T, TContext, bool>)
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curValue = target.Value;
					var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
					target.TryExchange(newValue, (c, n, ctx) => c.LongProp == n.LongProp - ctx, 1L);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => AssertTrue(cur.LongProp >= prev.LongProp)
			);

			// (Func<T, TContext, T>, T)
			runner.AllThreadsTearDown = target => AssertAreEqual(NumIterations, target.Value.LongProp);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.LongProp == NumIterations) return;
						var (wasSet, prevValue, CurrentValue) = target.TryExchange((c, ctx) => new DummyImmutableRef(c.LongProp + ctx), 1L, curValue);
						if (wasSet) {
							AssertAreEqualObjects(curValue, prevValue);
							AssertAreEqualObjects(prevValue.LongProp + 1L, CurrentValue.LongProp);
						}
						else AssertAreNotEqualObjects(curValue, prevValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TContext, T>, Func<T, T, bool>)
			runner.ExecuteFreeThreadedTests(
				target => {
					var (wasSet, prevValue, CurrentValue) = target.TryExchange((c, ctx) => new DummyImmutableRef(c.LongProp + ctx), 1L, (c, n) => c.LongProp == n.LongProp - 1L);
					if (wasSet) AssertAreEqual(prevValue.LongProp + 1L, CurrentValue.LongProp);
					else AssertAreNotEqual(prevValue.LongProp + 1L, CurrentValue.LongProp);
				},
				NumIterations
			);

			// (Func<T, T>, Func<T, T, TContext, bool>)
			runner.ExecuteFreeThreadedTests(
				target => {
					var (wasSet, prevValue, CurrentValue) = target.TryExchange(c => new DummyImmutableRef(c.LongProp + 1L), (c, n, ctx) => c.LongProp == n.LongProp - ctx, 1L);
					if (wasSet) AssertAreEqual(prevValue.LongProp + 1L, CurrentValue.LongProp);
					else AssertAreNotEqual(prevValue.LongProp + 1L, CurrentValue.LongProp);
				},
				NumIterations
			);

			// (Func<T, TMapContext, T>, Func<T, T, TPredicateContext, bool>)
			runner.ExecuteFreeThreadedTests(
				target => {
					var (wasSet, prevValue, CurrentValue) = target.TryExchange((c, ctx) => new DummyImmutableRef(c.LongProp + (ctx - 1L)), 2L, (c, n, ctx) => c.LongProp == n.LongProp - ctx, 1L);
					if (wasSet) AssertAreEqual(prevValue.LongProp + 1L, CurrentValue.LongProp);
					else AssertAreNotEqual(prevValue.LongProp + 1L, CurrentValue.LongProp);
				},
				NumIterations
			);
		}
	}
}