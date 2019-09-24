// (c) Egodystonic Studios 2018


using System;
using System.Threading;
using System.Threading.Tasks;
using Egodystonic.Atomics.Numerics;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.UnitTests.Common {
	abstract class CommonAtomicNumericTestSuite<T, TTarget> : CommonAtomicTestSuite<T, TTarget> where TTarget : INumericAtomic<T>, new() where T : IComparable<T>, IComparable, IEquatable<T> {
		protected abstract T Zero { get; }
		protected abstract T One { get; }
		protected abstract T Convert(int operand);
		protected abstract int ToInt(T operand);
		protected abstract T Add(T lhs, T rhs);
		protected abstract T Sub(T lhs, T rhs);
		protected abstract T Mul(T lhs, T rhs);
		protected abstract T Div(T lhs, T rhs);
		protected abstract bool GreaterThan(T lhs, T rhs);
		protected abstract bool GreaterThanOrEqualTo(T lhs, T rhs);
		protected abstract bool LessThan(T lhs, T rhs);
		protected abstract bool LessThanOrEqualTo(T lhs, T rhs);

		[Test]
		public void GetAndSetAndValue() {
			const int NumIterations = 1_000_000;
			var runner = NewRunner(Zero);

			var atomicInt = new LockFreeInt32();

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					target.Set(Convert(atomicInt.Increment().CurrentValue));
				},
				NumIterations,
				target => target.Get(),
				(prev, cur) => Assert.LessOrEqual(prev, cur)
			);

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					target.Value = Convert(atomicInt.Increment().CurrentValue);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.LessOrEqual(prev, cur)
			);
		}

		[Test]
		public void SpinWaitForValue() {
			const int NumIterations = 300_000;

			var runner = NewRunner(Zero);

			// (T)
			runner.AllThreadsTearDown = target => AssertAreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.Equals(Convert(NumIterations))) break;
						if ((ToInt(curVal) & 1) == 0) {
							AssertAreEqual(Add(curVal, One), target.SpinWaitForValue(Add(curVal, One)));
						}
						else {
							target.Value = Add(curVal, One);
						}
					}
				},
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.Equals(Convert(NumIterations))) break;
						if ((ToInt(curVal) & 1) == 1) {
							AssertAreEqual(Add(curVal, One), target.SpinWaitForValue(Add(curVal, One)));
						}
						else {
							target.Value = Add(curVal, One);
						}
					}
				}
			);

			// (Func<T, bool>)
			runner.AllThreadsTearDown = target => AssertAreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteWriterReaderTests(
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.Equals(Convert(NumIterations))) break;
						target.TryExchange(Add(curVal, One), curVal);
					}
				},
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.Equals(Convert(NumIterations))) break;
						AssertTrue(GreaterThanOrEqualTo(target.SpinWaitForValue(c => GreaterThan(c, curVal)), curVal));
					}
				}
			);

			// (Func<T, TContext, bool>, TContext)
			runner.AllThreadsTearDown = target => AssertAreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteWriterReaderTests(
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.Equals(Convert(NumIterations))) break;
						target.TryExchange(Add(curVal, One), curVal);
					}
				},
				target => {
					while (true) {
						var curVal = target.Value;
						if (curVal.Equals(Convert(NumIterations))) break;
						AssertTrue(GreaterThanOrEqualTo(target.SpinWaitForValue((c, ctx) => GreaterThan(c, curVal), curVal), curVal));
					}
				}
			);
		}

		[Test]
		public void FastExchange() {
			const int NumIterations = 3_000_000;

			var runner = NewRunner(Zero);

			// (T)
			var atomicInt = new LockFreeInt32(0);
			runner.GlobalSetUp = (_, __) => { atomicInt.Set(0); };
			runner.AllThreadsTearDown = target => {
				Assert.AreEqual(Convert(NumIterations), target.Value);
			};
			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					var newValue = Convert(atomicInt.Increment().CurrentValue);
					var prev = target.FastExchange(newValue);
					Assert.AreEqual(prev, Sub(newValue, One));
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					Assert.LessOrEqual(prev, cur);
				}
			);
			runner.GlobalSetUp = null;
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void Exchange() {
			const int NumIterations = 300_000;

			var runner = NewRunner(Zero);

			// (T)
			var atomicInt = new LockFreeInt32(0);
			runner.GlobalSetUp = (_, __) => { atomicInt.Set(0); };
			runner.AllThreadsTearDown = target => {
				Assert.AreEqual(Convert(NumIterations), target.Value);
			};
			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					var newValue = Convert(atomicInt.Increment().CurrentValue);
					var prev = target.Exchange(newValue).PreviousValue;
					Assert.AreEqual(prev, Sub(newValue, One));
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					Assert.LessOrEqual(prev, cur);
				}
			);
			runner.GlobalSetUp = null;
			runner.AllThreadsTearDown = null;

			// (Func<T, T>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(Convert(NumIterations), target.Value);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					var exchRes = target.Exchange(t => Add(t, One));
					AssertAreEqual(Add(exchRes.PreviousValue, One), exchRes.CurrentValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TContext, T>)
			runner.AllThreadsTearDown = target => {
				AssertAreEqual(Convert(NumIterations), target.Value);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					var exchRes = target.Exchange(Add, One);
					AssertAreEqual(Add(exchRes.PreviousValue, One), exchRes.CurrentValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void SpinWaitForExchangeWithoutContext() {
			const int NumIterations = 100_000;

			var runner = NewRunner(Zero);

			// (T, T)
			runner.AllThreadsTearDown = target => AssertAreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(Add(nextVal, One), nextVal);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(Add(nextVal, One), nextVal);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, T)
			runner.AllThreadsTearDown = target => AssertAreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(c => Add(c, One), nextVal);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(c => Add(c, One), nextVal);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (T, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => AssertAreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(Add(nextVal, One), (c, n) => n.Equals(Add(c, One)));
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(Add(nextVal, One), (c, n) => n.Equals(Add(c, One)));
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => AssertAreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(c => Add(c, One), (c, n) => n.Equals(Add(c, One)) && c.Equals(nextVal));
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(c => Add(c, One), (c, n) => n.Equals(Add(c, One)) && c.Equals(nextVal));
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void SpinWaitForExchangeWithContext() {
			const int NumIterations = 100_000;

			var runner = NewRunner(Zero);

			// (Func<T, TContext, T>, T)
			runner.AllThreadsTearDown = target => AssertAreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(Add, One, nextVal);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(Add, One, nextVal);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (T, Func<T, T, TContext, bool>)
			runner.AllThreadsTearDown = target => AssertAreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(Add(nextVal, One), (c, n, ctx) => n.Equals(Add(c, ctx)), One);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(Add(nextVal, One), (c, n, ctx) => n.Equals(Add(c, ctx)), One);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TMapContext, T>, Func<T, T, TPredicateContext, bool>)
			runner.AllThreadsTearDown = target => AssertAreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(Add, One, (c, n, ctx) => c.Equals(Sub(n, One)) && c.Equals(ctx), nextVal);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(Add, One, (c, n, ctx) => c.Equals(Sub(n, One)) && c.Equals(ctx), nextVal);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TContext, T>, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => AssertAreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(Add, One, (c, n) => n.Equals(Add(c, One)) && c.Equals(nextVal));
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(Add, One, (c, n) => n.Equals(Add(c, One)) && c.Equals(nextVal));
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, Func<T, T, TContext, bool>)
			runner.AllThreadsTearDown = target => AssertAreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						Console.WriteLine($"Waiting for {nextVal}");
						var exchRes = target.SpinWaitForExchange(c => Add(c, One), (c, n, ctx) => n.Equals(Add(c, ctx)) && c.Equals(nextVal), One);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.CurrentValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						Console.WriteLine($"Waiting for {nextVal}");
						var exchRes = target.SpinWaitForExchange(c => Add(c, One), (c, n, ctx) => n.Equals(Add(c, ctx)) && c.Equals(nextVal), One);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.CurrentValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void FastTryExchange() {
			const int NumIterations = 2_000_000;

			var runner = NewRunner(Zero);

			// (T, T)
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curValue = target.Value;
					var newValue = Add(curValue, One);
					target.FastTryExchange(newValue, curValue);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => AssertTrue(GreaterThanOrEqualTo(cur, prev))
			);
		}

		[Test]
		public void TryExchangeWithoutContext() {
			const int NumIterations = 200_000;

			var runner = NewRunner(Zero);

			// (T, T)
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curValue = target.Value;
					var newValue = Add(curValue, One);
					target.TryExchange(newValue, curValue);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => AssertTrue(GreaterThanOrEqualTo(cur, prev))
			);

			// (T, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => AssertAreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Equals(Convert(NumIterations))) return;
						var newValue = Add(curValue, One);
						var (wasSet, prevValue, setValue) = target.TryExchange(newValue, (c, n) => Equals(Add(c, One), n));
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
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curValue = target.Value;

					var (wasSet, prevValue, CurrentValue) = target.TryExchange(
						c => Add(c, One),
						curValue
					);

					if (wasSet) {
						AssertAreEqual(curValue, prevValue);
						AssertAreEqual(Add(curValue, One), CurrentValue);
					}
					else AssertAreNotEqual(curValue, prevValue);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => LessThanOrEqualTo(prev, cur)
			);

			// (Func<T, T>, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => AssertAreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Equals(Convert(NumIterations))) return;
						var (wasSet, prevValue, setValue) = target.TryExchange(c => Add(c, One), (c, n) => Equals(Add(c, One), n) && Equals(c, curValue));
						if (wasSet) {
							AssertAreEqual(curValue, prevValue);
							AssertAreEqual(Add(prevValue, One), setValue);
						}
						else {
							AssertAreNotEqual(curValue, prevValue);
							AssertAreEqual(setValue, prevValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void TryExchangeWithContext() {
			const int NumIterations = 300_000;

			var runner = NewRunner(Zero);

			// (T, Func<T, T, TContext, bool>)
			runner.AllThreadsTearDown = target => AssertAreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Equals(Convert(NumIterations))) return;
						var newValue = Add(curValue, One);
						var (wasSet, prevValue, setValue) = target.TryExchange(newValue, (c, n, ctx) => Equals(Add(c, ctx), n), One);
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
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curValue = target.Value;

					var (wasSet, prevValue, CurrentValue) = target.TryExchange(
						Add,
						One, curValue);

					if (wasSet) {
						AssertAreEqual(curValue, prevValue);
						AssertAreEqual(Add(curValue, One), CurrentValue);
					}
					else AssertAreNotEqual(curValue, prevValue);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => LessThanOrEqualTo(prev, cur)
			);

			// (Func<T, TContext, T>, Func<T, T, bool>)
			runner.AllThreadsTearDown = target => AssertAreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Equals(Convert(NumIterations))) return;
						var (wasSet, prevValue, setValue) = target.TryExchange(Add, One, (c, n) => Equals(Add(c, One), n) && LessThan(c, Convert(NumIterations)));
						if (wasSet) {
							AssertTrue(LessThanOrEqualTo(setValue, Convert(NumIterations)));
							AssertAreEqual(Add(prevValue, One), setValue);
						}
						else {
							AssertAreEqual(setValue, prevValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, T>, Func<T, T, TContext, bool>)
			runner.AllThreadsTearDown = target => AssertAreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Equals(Convert(NumIterations))) return;
						var (wasSet, prevValue, setValue) = target.TryExchange(c => Add(c, One), (c, n, ctx) => Equals(Add(c, ctx), n) && LessThan(c, Convert(NumIterations)), One);
						if (wasSet) {
							AssertTrue(LessThanOrEqualTo(setValue, Convert(NumIterations)));
							AssertAreEqual(Add(prevValue, One), setValue);
						}
						else {
							AssertAreEqual(setValue, prevValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TMapContext, T>, Func<T, T, TPredicateContext, bool>)
			runner.AllThreadsTearDown = target => AssertAreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Equals(Convert(NumIterations))) return;
						var (wasSet, prevValue, setValue) = target.TryExchange(Add, One, (c, n, ctx) => Equals(Add(c, One), n) && LessThan(c, Convert(ctx)), NumIterations);
						if (wasSet) {
							AssertTrue(LessThanOrEqualTo(setValue, Convert(NumIterations)));
							AssertAreEqual(Add(prevValue, One), setValue);
						}
						else {
							AssertAreEqual(setValue, prevValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void SpinWaitForMinimumValue() {
			const int NumIterations = 300_000;

			var runner = NewRunner(Zero);

			runner.AllThreadsTearDown = target => AssertAreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteSingleWriterTests(
				target => {
					var incRes = target.Increment();
					AssertAreEqual(Add(incRes.PreviousValue, One), incRes.CurrentValue);
				},
				target => {
					while (true) {
						var curVal = target.Value;
						if (ToInt(curVal) == NumIterations) break;

						var waitRes = target.SpinWaitForMinimumValue(Add(curVal, One));
						AssertTrue(GreaterThan(waitRes, curVal));
					}
				},
				NumIterations,
				iterateWriterFunc: true,
				iterateReaderFunc: false
			);
		}

		[Test]
		public void SpinWaitForMaximumValue() {
			const int NumIterations = 300_000;

			var runner = NewRunner(Convert(NumIterations));

			runner.AllThreadsTearDown = target => AssertAreEqual(Zero, target.Value);
			runner.ExecuteSingleWriterTests(
				target => {
					var incRes = target.Decrement();
					AssertAreEqual(Sub(incRes.PreviousValue, One), incRes.CurrentValue);
				},
				target => {
					while (true) {
						var curVal = target.Value;
						if (ToInt(curVal) == 0) break;

						var waitRes = target.SpinWaitForMaximumValue(Sub(curVal, One));
						AssertTrue(LessThan(waitRes, curVal));
					}
				},
				NumIterations,
				iterateWriterFunc: true,
				iterateReaderFunc: false
			);
		}

		[Test]
		public void SpinWaitForBoundedValue() {
			const int IncrementsPerTicket = 100;
			const int NumTickets = 300;
			const int TargetValue = IncrementsPerTicket * NumTickets;

			var runner = NewRunner(Zero);

			var ticketProvider = new LockFreeInt32();

			runner.GlobalSetUp = (_, __) => ticketProvider.Set(0);
			runner.AllThreadsTearDown = target => Assert.AreEqual(Convert(TargetValue), target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var ticket = ticketProvider.Increment().PreviousValue;
						if (ticket >= NumTickets) return;

						var lowerBoundInc = Convert(ticket * IncrementsPerTicket);
						var upperBoundEx = Convert((ticket + 1) * IncrementsPerTicket);
						var waitRes = target.SpinWaitForBoundedValue(lowerBoundInc, upperBoundEx);
						AssertTrue(LessThanOrEqualTo(waitRes, lowerBoundInc) && GreaterThan(upperBoundEx, waitRes));

						for (var i = 0; i < IncrementsPerTicket; ++i) target.Increment();
					}
				}
			);
		}

		[Test]
		public void SpinWaitForMinimumExchange() {
			const int NumIterations = 30;
			const int Target = 3_000;

			CancellationTokenSource readerCompletionSource = null;
			var runner = NewRunner(Zero);
			runner.GlobalSetUp = (_, __) => readerCompletionSource = new CancellationTokenSource();

			// (T, T)
			runner.ExecuteSingleReaderTests(
				target => {
					while (!readerCompletionSource.IsCancellationRequested) {
						target.Increment();
						Thread.Yield(); // To reduce contention, otherwise the read thread gets starved out
					}
				},
				target => {
					for (var i = 0; i < NumIterations; i++) {
						var exchRes = target.SpinWaitForMinimumExchange(Zero, Convert(Target));
						AssertAreEqual(Zero, exchRes.CurrentValue);
						AssertTrue(GreaterThanOrEqualTo(exchRes.PreviousValue, Convert(Target)));
					}
					readerCompletionSource.Cancel();
				}
			);

			// (Func<T, T>, T)
			runner.ExecuteSingleReaderTests(
				target => {
					while (!readerCompletionSource.IsCancellationRequested) {
						target.Increment();
						Thread.Yield(); // To reduce contention, otherwise the read thread gets starved out
					}
				},
				target => {
					for (var i = 0; i < NumIterations; i++) {
						var exchRes = target.SpinWaitForMinimumExchange(c => Sub(c, Convert(Target)), Convert(Target));
						AssertAreEqual(Sub(exchRes.PreviousValue, Convert(Target)), exchRes.CurrentValue);
						AssertTrue(GreaterThanOrEqualTo(exchRes.PreviousValue, Convert(Target)));
					}
					readerCompletionSource.Cancel();
				}
			);

			// (Func<T, TContext, T>, T)
			runner.ExecuteSingleReaderTests(
				target => {
					while (!readerCompletionSource.IsCancellationRequested) {
						target.Increment();
						Thread.Yield(); // To reduce contention, otherwise the read thread gets starved out
					}
				},
				target => {
					for (var i = 0; i < NumIterations; i++) {
						var exchRes = target.SpinWaitForMinimumExchange(Sub, Convert(Target), Convert(Target));
						AssertAreEqual(Sub(exchRes.PreviousValue, Convert(Target)), exchRes.CurrentValue);
						AssertTrue(GreaterThanOrEqualTo(exchRes.PreviousValue, Convert(Target)));
					}
					readerCompletionSource.Cancel();
				}
			);
		}

		[Test]
		public void SpinWaitForMaximumExchange() {
			const int NumIterations = 30;
			const int Target = -3_000;

			CancellationTokenSource readerCompletionSource = null;
			var runner = NewRunner(Zero);
			runner.GlobalSetUp = (_, __) => readerCompletionSource = new CancellationTokenSource();

			// (T, T)
			runner.ExecuteSingleReaderTests(
				target => {
					while (!readerCompletionSource.IsCancellationRequested) {
						target.Decrement();
						Thread.Yield(); // To reduce contention, otherwise the read thread gets starved out
					}
				},
				target => {
					for (var i = 0; i < NumIterations; i++) {
						var exchRes = target.SpinWaitForMaximumExchange(Zero, Convert(Target));
						AssertAreEqual(Zero, exchRes.CurrentValue);
						AssertTrue(LessThanOrEqualTo(exchRes.PreviousValue, Convert(Target)));
					}
					readerCompletionSource.Cancel();
				}
			);

			// (Func<T, T>, T)
			runner.ExecuteSingleReaderTests(
				target => {
					while (!readerCompletionSource.IsCancellationRequested) {
						target.Decrement();
						Thread.Yield(); // To reduce contention, otherwise the read thread gets starved out
					}
				},
				target => {
					for (var i = 0; i < NumIterations; i++) {
						var exchRes = target.SpinWaitForMaximumExchange(c => Sub(c, Convert(Target)), Convert(Target));
						AssertAreEqual(Sub(exchRes.PreviousValue, Convert(Target)), exchRes.CurrentValue);
						AssertTrue(LessThanOrEqualTo(exchRes.PreviousValue, Convert(Target)));
					}
					readerCompletionSource.Cancel();
				}
			);

			// (Func<T, TContext, T>, T)
			runner.ExecuteSingleReaderTests(
				target => {
					while (!readerCompletionSource.IsCancellationRequested) {
						target.Decrement();
						Thread.Yield(); // To reduce contention, otherwise the read thread gets starved out
					}
				},
				target => {
					for (var i = 0; i < NumIterations; i++) {
						var exchRes = target.SpinWaitForMaximumExchange(Sub, Convert(Target), Convert(Target));
						AssertAreEqual(Sub(exchRes.PreviousValue, Convert(Target)), exchRes.CurrentValue);
						AssertTrue(LessThanOrEqualTo(exchRes.PreviousValue, Convert(Target)));
					}
					readerCompletionSource.Cancel();
				}
			);
		}

		[Test]
		public void SpinWaitForBoundedExchange() {
			const int NumIterations = 30;
			const int IncrementSize = 3;
			const int Target = IncrementSize * 3_000;
			const int Range = IncrementSize - 1;

			CancellationTokenSource readerCompletionSource = null;
			var runner = NewRunner(Zero);
			runner.GlobalSetUp = (_, __) => readerCompletionSource = new CancellationTokenSource();

			// (T, T, T)
			runner.ExecuteSingleReaderTests(
				target => {
					while (!readerCompletionSource.IsCancellationRequested) {
						target.TryExchange(c => Add(c, Convert(IncrementSize)), (c, _) => LessThan(c, Convert(Target)));
						Thread.Yield(); // To reduce contention, otherwise the read thread gets starved out
					}
				},
				target => {
					for (var i = 0; i < NumIterations; i++) {
						var exchRes = target.SpinWaitForBoundedExchange(Zero, Convert(Target - Range), Convert(Target + Range));
						AssertAreEqual(Zero, exchRes.CurrentValue);
						AssertAreEqual(Convert(Target), exchRes.PreviousValue);
					}
					readerCompletionSource.Cancel();
				}
			);

			// (Func<T, T>, T, T)
			runner.ExecuteSingleReaderTests(
				target => {
					while (!readerCompletionSource.IsCancellationRequested) {
						target.TryExchange(c => Add(c, Convert(IncrementSize)), (c, _) => LessThan(c, Convert(Target)));
						Thread.Yield(); // To reduce contention, otherwise the read thread gets starved out
					}
				},
				target => {
					for (var i = 0; i < NumIterations; i++) {
						var exchRes = target.SpinWaitForBoundedExchange(c => Sub(c, Convert(Target)), Convert(Target - Range), Convert(Target + Range));
						AssertAreEqual(Sub(exchRes.PreviousValue, Convert(Target)), exchRes.CurrentValue);
						AssertAreEqual(Convert(Target), exchRes.PreviousValue);
					}
					readerCompletionSource.Cancel();
				}
			);

			// (Func<T, TContext, T>, T, T)
			runner.ExecuteSingleReaderTests(
				target => {
					while (!readerCompletionSource.IsCancellationRequested) {
						target.TryExchange(c => Add(c, Convert(IncrementSize)), (c, _) => LessThan(c, Convert(Target)));
						Thread.Yield(); // To reduce contention, otherwise the read thread gets starved out
					}
				},
				target => {
					for (var i = 0; i < NumIterations; i++) {
						var exchRes = target.SpinWaitForBoundedExchange((c, ctx) => Sub(c, ctx), Convert(Target - Range), Convert(Target + Range), Convert(Target));
						AssertAreEqual(Sub(exchRes.PreviousValue, Convert(Target)), exchRes.CurrentValue);
						AssertAreEqual(Convert(Target), exchRes.PreviousValue);
					}
					readerCompletionSource.Cancel();
				}
			);
		}

		[Test]
		public void TryMinimumExchange() {
			const int NumIterations = 300_000;
			const int MinValue = 5;

			var runner = NewRunner(Zero);

			// (T, T)
			runner.ExecuteWriterReaderTests(
				target => {
					target.Increment();
				},
				target => {
					var tryRes = target.TryMinimumExchange(Zero, Convert(MinValue));
					if (tryRes.ValueWasSet) {
						AssertAreEqual(Zero, tryRes.CurrentValue);
						AssertTrue(GreaterThanOrEqualTo(tryRes.PreviousValue, Convert(MinValue)));
					}
					else {
						AssertAreEqual(tryRes.PreviousValue, tryRes.CurrentValue);
						AssertTrue(LessThan(tryRes.PreviousValue, Convert(MinValue)));
					}
				},
				NumIterations
			);

			// (Func<T, T>, T)
			runner.ExecuteWriterReaderTests(
				target => {
					target.Increment();
				},
				target => {
					var tryRes = target.TryMinimumExchange(c => Div(c, Convert(2)), Convert(MinValue));
					if (tryRes.ValueWasSet) {
						AssertAreEqual(Div(tryRes.PreviousValue, Convert(2)), tryRes.CurrentValue);
						AssertTrue(GreaterThanOrEqualTo(tryRes.PreviousValue, Convert(MinValue)));
					}
					else {
						AssertAreEqual(tryRes.PreviousValue, tryRes.CurrentValue);
						AssertTrue(LessThan(tryRes.PreviousValue, Convert(MinValue)));
					}
				},
				NumIterations
			);

			// (Func<T, TContext, T>, T)
			runner.ExecuteWriterReaderTests(
				target => {
					target.Increment();
				},
				target => {
					var tryRes = target.TryMinimumExchange((c, ctx) => Div(c, Convert(ctx)), Convert(MinValue), 2);
					if (tryRes.ValueWasSet) {
						AssertAreEqual(Div(tryRes.PreviousValue, Convert(2)), tryRes.CurrentValue);
						AssertTrue(GreaterThanOrEqualTo(tryRes.PreviousValue, Convert(MinValue)));
					}
					else {
						AssertAreEqual(tryRes.PreviousValue, tryRes.CurrentValue);
						AssertTrue(LessThan(tryRes.PreviousValue, Convert(MinValue)));
					}
				},
				NumIterations
			);
		}

		[Test]
		public void TryMaximumExchange() {
			const int NumIterations = 300_000;
			const int MaxValue = -5;

			var runner = NewRunner(Zero);

			// (T, T)
			runner.ExecuteWriterReaderTests(
				target => {
					target.Decrement();
				},
				target => {
					var tryRes = target.TryMaximumExchange(Zero, Convert(MaxValue));
					if (tryRes.ValueWasSet) {
						AssertAreEqual(Zero, tryRes.CurrentValue);
						AssertTrue(LessThanOrEqualTo(tryRes.PreviousValue, Convert(MaxValue)));
					}
					else {
						AssertAreEqual(tryRes.PreviousValue, tryRes.CurrentValue);
						AssertTrue(GreaterThan(tryRes.PreviousValue, Convert(MaxValue)));
					}
				},
				NumIterations
			);

			// (Func<T, T>, T)
			runner.ExecuteWriterReaderTests(
				target => {
					target.Increment();
				},
				target => {
					var tryRes = target.TryMaximumExchange(c => Div(c, Convert(2)), Convert(MaxValue));
					if (tryRes.ValueWasSet) {
						AssertAreEqual(Div(tryRes.PreviousValue, Convert(2)), tryRes.CurrentValue);
						AssertTrue(LessThanOrEqualTo(tryRes.PreviousValue, Convert(MaxValue)));
					}
					else {
						AssertAreEqual(tryRes.PreviousValue, tryRes.CurrentValue);
						AssertTrue(GreaterThan(tryRes.PreviousValue, Convert(MaxValue)));
					}
				},
				NumIterations
			);

			// (Func<T, TContext, T>, T)
			runner.ExecuteWriterReaderTests(
				target => {
					target.Increment();
				},
				target => {
					var tryRes = target.TryMaximumExchange((c, ctx) => Div(c, Convert(ctx)), Convert(MaxValue), 2);
					if (tryRes.ValueWasSet) {
						AssertAreEqual(Div(tryRes.PreviousValue, Convert(2)), tryRes.CurrentValue);
						AssertTrue(LessThanOrEqualTo(tryRes.PreviousValue, Convert(MaxValue)));
					}
					else {
						AssertAreEqual(tryRes.PreviousValue, tryRes.CurrentValue);
						AssertTrue(GreaterThan(tryRes.PreviousValue, Convert(MaxValue)));
					}
				},
				NumIterations
			);
		}

		[Test]
		public void TryBoundedExchange() {
			const int NumIterations = 100_000;
			const int Modulus = 30;
			const int MinValue = (Modulus / 2) - 5;
			const int MaxValue = (Modulus / 2) + 5;

			var runner = NewRunner(Zero);

			// (T, T)
			runner.ExecuteWriterReaderTests(
				target => {
					target.Exchange(c => GreaterThanOrEqualTo(c, Convert(Modulus)) ? Zero : Add(c, One));
				},
				target => {
					var tryRes = target.TryBoundedExchange(Zero, Convert(MinValue), Convert(MaxValue));
					if (tryRes.ValueWasSet) {
						AssertAreEqual(Zero, tryRes.CurrentValue);
						AssertTrue(GreaterThanOrEqualTo(tryRes.PreviousValue, Convert(MinValue)));
						AssertTrue(LessThan(tryRes.PreviousValue, Convert(MaxValue)));
					}
					else {
						AssertAreEqual(tryRes.PreviousValue, tryRes.CurrentValue);
						AssertTrue(LessThan(tryRes.PreviousValue, Convert(MinValue)) || GreaterThanOrEqualTo(tryRes.PreviousValue, Convert(MaxValue)));
					}
				},
				NumIterations
			);

			// (Func<T, T>, T)
			runner.ExecuteWriterReaderTests(
				target => {
					target.Exchange(c => GreaterThanOrEqualTo(c, Convert(Modulus)) ? Zero : Add(c, One));
				},
				target => {
					var tryRes = target.TryBoundedExchange(c => Sub(c, Convert(MinValue)), Convert(MinValue), Convert(MaxValue));
					if (tryRes.ValueWasSet) {
						AssertAreEqual(Sub(tryRes.PreviousValue, Convert(MinValue)), tryRes.CurrentValue);
						AssertTrue(GreaterThanOrEqualTo(tryRes.PreviousValue, Convert(MinValue)));
						AssertTrue(LessThan(tryRes.PreviousValue, Convert(MaxValue)));
					}
					else {
						AssertAreEqual(tryRes.PreviousValue, tryRes.CurrentValue);
						AssertTrue(LessThan(tryRes.PreviousValue, Convert(MinValue)) || GreaterThanOrEqualTo(tryRes.PreviousValue, Convert(MaxValue)));
					}
				},
				NumIterations
			);

			// (Func<T, TContext, T>, T)
			runner.ExecuteWriterReaderTests(
				target => {
					target.Exchange(c => GreaterThanOrEqualTo(c, Convert(Modulus)) ? Zero : Add(c, One));
				},
				target => {
					var tryRes = target.TryBoundedExchange(Sub, Convert(MinValue), Convert(MaxValue), Convert(MinValue));
					if (tryRes.ValueWasSet) {
						AssertAreEqual(Sub(tryRes.PreviousValue, Convert(MinValue)), tryRes.CurrentValue);
						AssertTrue(GreaterThanOrEqualTo(tryRes.PreviousValue, Convert(MinValue)));
						AssertTrue(LessThan(tryRes.PreviousValue, Convert(MaxValue)));
					}
					else {
						AssertAreEqual(tryRes.PreviousValue, tryRes.CurrentValue);
						AssertTrue(LessThan(tryRes.PreviousValue, Convert(MinValue)) || GreaterThanOrEqualTo(tryRes.PreviousValue, Convert(MaxValue)));
					}
				},
				NumIterations
			);
		}

		[Test]
		public void FastIncrementDecrement() {
			const int NumIterations = 1_000_000;

			var runner = NewRunner(Zero);

			runner.AllThreadsTearDown = target => Assert.AreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curVal = target.Value;
					var newVal = target.FastIncrement();
					AssertTrue(GreaterThan(newVal, curVal));
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.GreaterOrEqual(cur, prev)
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => Assert.AreEqual(Convert(-NumIterations), target.Value);
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curVal = target.Value;
					var newVal = target.FastDecrement();
					AssertTrue(LessThan(newVal, curVal));
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.LessOrEqual(cur, prev)
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void IncrementDecrement() {
			const int NumIterations = 100_000;

			var runner = NewRunner(Zero);

			runner.AllThreadsTearDown = target => Assert.AreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					var (prevValue, CurrentValue) = target.Increment();
					Assert.AreEqual(Add(prevValue, One), CurrentValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => Assert.AreEqual(Convert(-NumIterations), target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					var (prevValue, CurrentValue) = target.Decrement();
					Assert.AreEqual(Sub(prevValue, One), CurrentValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => Assert.AreEqual(Zero, target.Value);
			runner.ExecuteWriterReaderTests(
				target => {
					var (prevValue, CurrentValue) = target.Increment();
					Assert.AreEqual(Add(prevValue, One), CurrentValue);
				},
				target => {
					var (prevValue, CurrentValue) = target.Decrement();
					Assert.AreEqual(Sub(prevValue, One), CurrentValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => Assert.AreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var (prevValue, CurrentValue) = target.Increment();
					Assert.AreEqual(Add(prevValue, One), CurrentValue);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.GreaterOrEqual(cur, prev)
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => Assert.AreEqual(Convert(-NumIterations), target.Value);
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var (prevValue, CurrentValue) = target.Decrement();
					Assert.AreEqual(Sub(prevValue, One), CurrentValue);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.LessOrEqual(cur, prev)
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void FastAddSub() {
			const int NumIterations = 100_000;

			var runner = NewRunner(Zero);

			runner.AllThreadsTearDown = target => Assert.AreEqual(Convert(NumIterations * 9), target.Value);
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curValue = target.Value;
					var newValue = target.FastAdd(Convert(9));
					AssertTrue(GreaterThan(newValue, curValue));
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.GreaterOrEqual(cur, prev)
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => Assert.AreEqual(Convert(-NumIterations * 9), target.Value);
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curValue = target.Value;
					var newValue = target.FastSubtract(Convert(9));
					AssertTrue(LessThan(newValue, curValue));
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.LessOrEqual(cur, prev)
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void AddSub() {
			const int NumIterations = 100_000;

			var runner = NewRunner(Zero);

			runner.AllThreadsTearDown = target => Assert.AreEqual(Convert(NumIterations * 7), target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					var (prevValue, CurrentValue) = target.Add(Convert(7));
					Assert.AreEqual(Add(prevValue, Convert(7)), CurrentValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => Assert.AreEqual(Convert(-NumIterations * 7), target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					var (prevValue, CurrentValue) = target.Subtract(Convert(7));
					Assert.AreEqual(Sub(prevValue, Convert(7)), CurrentValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => Assert.AreEqual(Zero, target.Value);
			runner.ExecuteWriterReaderTests(
				target => {
					var (prevValue, CurrentValue) = target.Add(Convert(3));
					Assert.AreEqual(Add(prevValue, Convert(3)), CurrentValue);
				},
				target => {
					var (prevValue, CurrentValue) = target.Subtract(Convert(3));
					Assert.AreEqual(Sub(prevValue, Convert(3)), CurrentValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => Assert.AreEqual(Convert(NumIterations * 9), target.Value);
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var (prevValue, CurrentValue) = target.Add(Convert(9));
					Assert.AreEqual(Add(prevValue, Convert(9)), CurrentValue);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.GreaterOrEqual(cur, prev)
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => Assert.AreEqual(Convert(-NumIterations * 9), target.Value);
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var (prevValue, CurrentValue) = target.Subtract(Convert(9));
					Assert.AreEqual(Sub(prevValue, Convert(9)), CurrentValue);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.LessOrEqual(cur, prev)
			);
			runner.AllThreadsTearDown = null;
		}

		[Test]
		public void MulDiv() {
			const int NumIterations = 300_000;

			var runner = NewRunner(Convert(32768));

			runner.ExecuteWriterReaderTests(
				target => {
					var (prevValue, CurrentValue) = target.MultiplyBy(Convert(7));
					Assert.AreEqual(Mul(prevValue, Convert(7)), CurrentValue);
				},
				target => {
					var (prevValue, CurrentValue) = target.DivideBy(Convert(3));
					Assert.AreEqual(Div(prevValue, Convert(3)), CurrentValue);
				},
				NumIterations
			);
		}

		// API Tests
		[Test]
		public void API_SpinWaitForMinimumValue() {
			var target = new TTarget();

			target.Set(Convert(100));
			var waitRes = target.SpinWaitForMinimumValue(Convert(90));
			Assert.AreEqual(Convert(100), waitRes);

			var spinWaitTask = Task.Run(() => target.SpinWaitForMinimumValue(Convert(200)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(199));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(200));
			Assert.AreEqual(Convert(200), spinWaitTask.Result);
			spinWaitTask = Task.Run(() => target.SpinWaitForMinimumValue(Convert(300)));
			Thread.Sleep(200); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(109003));
			Assert.AreEqual(Convert(109003), spinWaitTask.Result);
		}

		[Test]
		public void API_SpinWaitForMaximumValue() {
			var target = new TTarget();

			target.Set(Convert(-100));
			var waitRes = target.SpinWaitForMaximumValue(Convert(-90));
			Assert.AreEqual(Convert(-100), waitRes);

			var spinWaitTask = Task.Run(() => target.SpinWaitForMaximumValue(Convert(-200)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(-199));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(-200));
			Assert.AreEqual(Convert(-200), spinWaitTask.Result);
			spinWaitTask = Task.Run(() => target.SpinWaitForMaximumValue(Convert(-300)));
			Thread.Sleep(200); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(-109003));
			Assert.AreEqual(Convert(-109003), spinWaitTask.Result);
		}

		[Test]
		public void API_SpinWaitForBoundedValue() {
			var target = new TTarget();

			target.Set(Convert(100));
			var waitRes = target.SpinWaitForBoundedValue(Convert(90), Convert(110));
			Assert.AreEqual(Convert(100), waitRes);

			var spinWaitTask = Task.Run(() => target.SpinWaitForBoundedValue(Convert(200), Convert(300)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(190));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(200));
			Assert.AreEqual(Convert(200), spinWaitTask.Result);
			spinWaitTask = Task.Run(() => target.SpinWaitForBoundedValue(Convert(0), Convert(200)));
			Thread.Sleep(200); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(199));
			Assert.AreEqual(Convert(199), spinWaitTask.Result);
		}

		[Test]
		public void API_SpinWaitForMinimumExchange() {
			var target = new TTarget();

			target.Set(Convert(100));

			// (T, T)
			var exchRes = target.SpinWaitForMinimumExchange(Convert(190), Convert(90));
			Assert.AreEqual(Convert(100), exchRes.PreviousValue);
			Assert.AreEqual(Convert(190), exchRes.CurrentValue);
			var spinWaitTask = Task.Run(() => target.SpinWaitForMinimumExchange(Convert(50), Convert(200)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(200));
			Assert.AreEqual(Convert(200), spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Convert(50), spinWaitTask.Result.CurrentValue);

			// (Func<T, T>, T, T)
			exchRes = target.SpinWaitForMinimumExchange(t => Mul(t, Convert(2)), Convert(0));
			Assert.AreEqual(Convert(50), exchRes.PreviousValue);
			Assert.AreEqual(Convert(100), exchRes.CurrentValue);
			spinWaitTask = Task.Run(() => target.SpinWaitForMinimumExchange(t => Sub(t, Convert(10)), Convert(101)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(101));
			Assert.AreEqual(Convert(101), spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Convert(91), spinWaitTask.Result.CurrentValue);

			// (Func<T, TContext, T>, T, T)
			exchRes = target.SpinWaitForMinimumExchange(Add, Convert(91), Convert(109));
			Assert.AreEqual(Convert(91), exchRes.PreviousValue);
			Assert.AreEqual(Convert(200), exchRes.CurrentValue);
			spinWaitTask = Task.Run(() => target.SpinWaitForMinimumExchange(Div, Convert(300), Convert(10)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(300));
			Assert.AreEqual(Convert(300), spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Convert(30), spinWaitTask.Result.CurrentValue);
		}

		[Test]
		public void API_SpinWaitForMaximumExchange() {
			var target = new TTarget();

			target.Set(Convert(-100));

			// (T, T)
			var exchRes = target.SpinWaitForMaximumExchange(Convert(-190), Convert(-90));
			Assert.AreEqual(Convert(-100), exchRes.PreviousValue);
			Assert.AreEqual(Convert(-190), exchRes.CurrentValue);
			var spinWaitTask = Task.Run(() => target.SpinWaitForMaximumExchange(Convert(-50), Convert(-200)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(-200));
			Assert.AreEqual(Convert(-200), spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Convert(-50), spinWaitTask.Result.CurrentValue);

			// (Func<T, T>, T, T)
			exchRes = target.SpinWaitForMaximumExchange(t => Mul(t, Convert(2)), Convert(-0));
			Assert.AreEqual(Convert(-50), exchRes.PreviousValue);
			Assert.AreEqual(Convert(-100), exchRes.CurrentValue);
			spinWaitTask = Task.Run(() => target.SpinWaitForMaximumExchange(t => Sub(t, Convert(-10)), Convert(-101)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(-101));
			Assert.AreEqual(Convert(-101), spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Convert(-91), spinWaitTask.Result.CurrentValue);

			// (Func<T, TContext, T>, T, T)
			exchRes = target.SpinWaitForMaximumExchange(Add, Convert(-91), Convert(-109));
			Assert.AreEqual(Convert(-91), exchRes.PreviousValue);
			Assert.AreEqual(Convert(-200), exchRes.CurrentValue);
			spinWaitTask = Task.Run(() => target.SpinWaitForMaximumExchange(Div, Convert(-300), Convert(10)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(-300));
			Assert.AreEqual(Convert(-300), spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Convert(-30), spinWaitTask.Result.CurrentValue);
		}

		[Test]
		public void API_SpinWaitForBoundedExchange() {
			var target = new TTarget();

			target.Set(Convert(100));

			// (T, T, T)
			var exchRes = target.SpinWaitForBoundedExchange(Convert(190), Convert(90), Convert(110));
			Assert.AreEqual(Convert(100), exchRes.PreviousValue);
			Assert.AreEqual(Convert(190), exchRes.CurrentValue);
			var spinWaitTask = Task.Run(() => target.SpinWaitForBoundedExchange(Convert(50), Convert(200), Convert(300)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(200));
			Assert.AreEqual(Convert(200), spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Convert(50), spinWaitTask.Result.CurrentValue);

			// (Func<T, T>, T, T)
			exchRes = target.SpinWaitForBoundedExchange(t => Mul(t, Convert(2)), Convert(0), Convert(51));
			Assert.AreEqual(Convert(50), exchRes.PreviousValue);
			Assert.AreEqual(Convert(100), exchRes.CurrentValue);
			spinWaitTask = Task.Run(() => target.SpinWaitForBoundedExchange(t => Sub(t, Convert(10)), Convert(101), Convert(102)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(101));
			Assert.AreEqual(Convert(101), spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Convert(91), spinWaitTask.Result.CurrentValue);

			// (Func<T, TContext, T>, T, T)
			exchRes = target.SpinWaitForBoundedExchange(Add, Convert(91), Convert(92), Convert(109));
			Assert.AreEqual(Convert(91), exchRes.PreviousValue);
			Assert.AreEqual(Convert(200), exchRes.CurrentValue);
			spinWaitTask = Task.Run(() => target.SpinWaitForBoundedExchange(Div, Convert(300), Convert(400), Convert(10)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(300));
			Assert.AreEqual(Convert(300), spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Convert(30), spinWaitTask.Result.CurrentValue);
		}

		[Test]
		public void API_TryMinimumExchange() {
			var target = new TTarget();

			target.Set(Convert(100));

			// (T, T)
			var exchRes = target.TryMinimumExchange(Convert(190), Convert(90));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(100), exchRes.PreviousValue);
			Assert.AreEqual(Convert(190), exchRes.CurrentValue);
			exchRes = target.TryMinimumExchange(Convert(50), Convert(200));
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(190), exchRes.PreviousValue);
			Assert.AreEqual(Convert(190), exchRes.CurrentValue);
			target.Set(Convert(200));
			exchRes = target.TryMinimumExchange(Convert(50), Convert(200));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(200), exchRes.PreviousValue);
			Assert.AreEqual(Convert(50), exchRes.CurrentValue);

			// (Func<T, T>, T, T)
			exchRes = target.TryMinimumExchange(t => Mul(t, Convert(2)), Convert(0));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(50), exchRes.PreviousValue);
			Assert.AreEqual(Convert(100), exchRes.CurrentValue);
			exchRes = target.TryMinimumExchange(t => Sub(t, Convert(10)), Convert(101));
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(100), exchRes.PreviousValue);
			Assert.AreEqual(Convert(100), exchRes.CurrentValue);
			target.Set(Convert(101));
			exchRes = target.TryMinimumExchange(t => Sub(t, Convert(10)), Convert(101));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(101), exchRes.PreviousValue);
			Assert.AreEqual(Convert(91), exchRes.CurrentValue);

			// (Func<T, TContext, T>, T, T)
			exchRes = target.TryMinimumExchange(Add, Convert(91), Convert(109));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(91), exchRes.PreviousValue);
			Assert.AreEqual(Convert(200), exchRes.CurrentValue);
			exchRes = target.TryMinimumExchange(Div, Convert(300), Convert(10));
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(200), exchRes.PreviousValue);
			Assert.AreEqual(Convert(200), exchRes.CurrentValue);
			target.Set(Convert(300));
			exchRes = target.TryMinimumExchange(Div, Convert(300), Convert(10));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(300), exchRes.PreviousValue);
			Assert.AreEqual(Convert(30), exchRes.CurrentValue);
		}

		[Test]
		public void API_TryMaximumExchange() {
			var target = new TTarget();

			target.Set(Convert(-100));

			// (T, T)
			var exchRes = target.TryMaximumExchange(Convert(-190), Convert(-90));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(-100), exchRes.PreviousValue);
			Assert.AreEqual(Convert(-190), exchRes.CurrentValue);
			exchRes = target.TryMaximumExchange(Convert(-50), Convert(-200));
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(-190), exchRes.PreviousValue);
			Assert.AreEqual(Convert(-190), exchRes.CurrentValue);
			target.Set(Convert(-200));
			exchRes = target.TryMaximumExchange(Convert(-50), Convert(-200));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(-200), exchRes.PreviousValue);
			Assert.AreEqual(Convert(-50), exchRes.CurrentValue);

			// (Func<T, T>, T, T)
			exchRes = target.TryMaximumExchange(t => Mul(t, Convert(2)), Convert(-0));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(-50), exchRes.PreviousValue);
			Assert.AreEqual(Convert(-100), exchRes.CurrentValue);
			exchRes = target.TryMaximumExchange(t => Sub(t, Convert(-10)), Convert(-101));
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(-100), exchRes.PreviousValue);
			Assert.AreEqual(Convert(-100), exchRes.CurrentValue);
			target.Set(Convert(-101));
			exchRes = target.TryMaximumExchange(t => Sub(t, Convert(-10)), Convert(-101));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(-101), exchRes.PreviousValue);
			Assert.AreEqual(Convert(-91), exchRes.CurrentValue);

			// (Func<T, TContext, T>, T, T)
			exchRes = target.TryMaximumExchange(Add, Convert(-91), Convert(-109));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(-91), exchRes.PreviousValue);
			Assert.AreEqual(Convert(-200), exchRes.CurrentValue);
			exchRes = target.TryMaximumExchange(Div, Convert(-300), Convert(10));
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(-200), exchRes.PreviousValue);
			Assert.AreEqual(Convert(-200), exchRes.CurrentValue);
			target.Set(Convert(-300));
			exchRes = target.TryMaximumExchange(Div, Convert(-300), Convert(10));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(-300), exchRes.PreviousValue);
			Assert.AreEqual(Convert(-30), exchRes.CurrentValue);
		}

		[Test]
		public void API_TryBoundedExchange() {
			var target = new TTarget();

			target.Set(Convert(100));

			// (T, T, T)
			var exchRes = target.TryBoundedExchange(Convert(190), Convert(90), Convert(110));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(100), exchRes.PreviousValue);
			Assert.AreEqual(Convert(190), exchRes.CurrentValue);
			exchRes = target.TryBoundedExchange(Convert(50), Convert(200), Convert(300));
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(190), exchRes.PreviousValue);
			Assert.AreEqual(Convert(190), exchRes.CurrentValue);
			target.Set(Convert(200));
			exchRes = target.TryBoundedExchange(Convert(50), Convert(200), Convert(300));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(200), exchRes.PreviousValue);
			Assert.AreEqual(Convert(50), exchRes.CurrentValue);

			// (Func<T, T>, T, T)
			exchRes = target.TryBoundedExchange(t => Mul(t, Convert(2)), Convert(0), Convert(51));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(50), exchRes.PreviousValue);
			Assert.AreEqual(Convert(100), exchRes.CurrentValue);
			exchRes = target.TryBoundedExchange(t => Sub(t, Convert(10)), Convert(101), Convert(102));
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(100), exchRes.PreviousValue);
			Assert.AreEqual(Convert(100), exchRes.CurrentValue);
			target.Set(Convert(101));
			exchRes = target.TryBoundedExchange(t => Sub(t, Convert(10)), Convert(101), Convert(102));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(101), exchRes.PreviousValue);
			Assert.AreEqual(Convert(91), exchRes.CurrentValue);

			// (Func<T, TContext, T>, T, T)
			exchRes = target.TryBoundedExchange(Add, Convert(91), Convert(92), Convert(109));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(91), exchRes.PreviousValue);
			Assert.AreEqual(Convert(200), exchRes.CurrentValue);
			exchRes = target.TryBoundedExchange(Div, Convert(300), Convert(400), Convert(10));
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(200), exchRes.PreviousValue);
			Assert.AreEqual(Convert(200), exchRes.CurrentValue);
			target.Set(Convert(300));
			exchRes = target.TryBoundedExchange(Div, Convert(300), Convert(400), Convert(10));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(300), exchRes.PreviousValue);
			Assert.AreEqual(Convert(30), exchRes.CurrentValue);
		}

		[Test]
		public void API_FastIncrement() {
			var target = new TTarget();

			target.Set(Convert(100));
			var prevVal = target.Value;

			var incRes = target.FastIncrement();
			Assert.AreEqual(Convert(100), prevVal);
			Assert.AreEqual(Convert(101), incRes);
		}

		[Test]
		public void API_FastDecrement() {
			var target = new TTarget();

			target.Set(Convert(100));
			var prevVal = target.Value;

			var incRes = target.FastDecrement();
			Assert.AreEqual(Convert(100), prevVal);
			Assert.AreEqual(Convert(99), incRes);
		}

		[Test]
		public void API_FastAdd() {
			var target = new TTarget();

			target.Set(Convert(100));
			var prevVal = target.Value;

			var incRes = target.FastAdd(Convert(20));
			Assert.AreEqual(Convert(100), prevVal);
			Assert.AreEqual(Convert(120), incRes);
		}

		[Test]
		public void API_FastSubtract() {
			var target = new TTarget();

			target.Set(Convert(100));
			var prevVal = target.Value;

			var incRes = target.FastSubtract(Convert(20));
			Assert.AreEqual(Convert(100), prevVal);
			Assert.AreEqual(Convert(80), incRes);
		}

		[Test]
		public void API_Increment() {
			var target = new TTarget();

			target.Set(Convert(100));

			var incRes = target.Increment();
			Assert.AreEqual(Convert(100), incRes.PreviousValue);
			Assert.AreEqual(Convert(101), incRes.CurrentValue);
		}

		[Test]
		public void API_Decrement() {
			var target = new TTarget();

			target.Set(Convert(100));

			var incRes = target.Decrement();
			Assert.AreEqual(Convert(100), incRes.PreviousValue);
			Assert.AreEqual(Convert(99), incRes.CurrentValue);
		}

		[Test]
		public void API_Add() {
			var target = new TTarget();

			target.Set(Convert(100));

			var incRes = target.Add(Convert(20));
			Assert.AreEqual(Convert(100), incRes.PreviousValue);
			Assert.AreEqual(Convert(120), incRes.CurrentValue);
		}

		[Test]
		public void API_Subtract() {
			var target = new TTarget();

			target.Set(Convert(100));

			var incRes = target.Subtract(Convert(20));
			Assert.AreEqual(Convert(100), incRes.PreviousValue);
			Assert.AreEqual(Convert(80), incRes.CurrentValue);
		}

		[Test]
		public void API_MultiplyBy() {
			var target = new TTarget();

			target.Set(Convert(100));

			var incRes = target.MultiplyBy(Convert(20));
			Assert.AreEqual(Convert(100), incRes.PreviousValue);
			Assert.AreEqual(Convert(2000), incRes.CurrentValue);
		}

		[Test]
		public void API_DivideBy() {
			var target = new TTarget();

			target.Set(Convert(100));

			var incRes = target.DivideBy(Convert(20));
			Assert.AreEqual(Convert(100), incRes.PreviousValue);
			Assert.AreEqual(Convert(5), incRes.CurrentValue);
		}
	}
}