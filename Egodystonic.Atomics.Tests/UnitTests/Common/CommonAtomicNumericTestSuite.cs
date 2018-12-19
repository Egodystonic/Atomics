// (c) Egodystonic Studios 2018


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;
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

			var atomicInt = new AtomicInt();

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					target.Set(Convert(atomicInt.Increment().NewValue));
				},
				NumIterations,
				target => target.Get(),
				(prev, cur) => Assert.LessOrEqual(prev, cur)
			);

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					target.Value = Convert(atomicInt.Increment().NewValue);
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
		public void Exchange() {
			const int NumIterations = 300_000;

			var runner = NewRunner(Zero);

			// (T)
			var atomicInt = new AtomicInt(0);
			runner.GlobalSetUp = (_, __) => { atomicInt.Set(0); };
			runner.AllThreadsTearDown = target => {
				Assert.AreEqual(Convert(NumIterations), target.Value);
			};
			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					var newValue = Convert(atomicInt.Increment().NewValue);
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
					AssertAreEqual(Add(exchRes.PreviousValue, One), exchRes.NewValue);
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
					AssertAreEqual(Add(exchRes.PreviousValue, One), exchRes.NewValue);
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
						AssertAreEqual(Add(nextVal, One), exchRes.NewValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(Add(nextVal, One), nextVal);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.NewValue);
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
						AssertAreEqual(Add(nextVal, One), exchRes.NewValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(c => Add(c, One), nextVal);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.NewValue);
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
						var exchRes = target.SpinWaitForExchange(Add(nextVal, One), (n, c) => n.Equals(Add(c, One)));
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.NewValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(Add(nextVal, One), (n, c) => n.Equals(Add(c, One)));
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.NewValue);
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
						var exchRes = target.SpinWaitForExchange(c => Add(c, One), (n, c) => n.Equals(Add(c, One)));
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.NewValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(c => Add(c, One), (n, c) => n.Equals(Add(c, One)));
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.NewValue);
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
						var exchRes = target.SpinWaitForExchange(Add, nextVal, One);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.NewValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(Add, nextVal, One);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.NewValue);
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
						AssertAreEqual(Add(nextVal, One), exchRes.NewValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(Add(nextVal, One), (c, n, ctx) => n.Equals(Add(c, ctx)), One);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.NewValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// (Func<T, TContext, T>, Func<T, T, TContext, bool>)
			runner.AllThreadsTearDown = target => AssertAreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteSingleWriterSingleReaderTests(
				target => {
					for (var i = 0; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(Add, (c, n, ctx) => n.Equals(Add(c, ctx)), One);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.NewValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(Add, (c, n, ctx) => n.Equals(Add(c, ctx)), One);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.NewValue);
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
						var exchRes = target.SpinWaitForExchange((c, ctx) => Add(Div(Add(c, ctx), Add(One, One)), One), (c, n, ctx) => n.Equals(Add(c, ctx)), nextVal, One);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.NewValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange((c, ctx) => Add(Div(Add(c, ctx), Add(One, One)), One), (c, n, ctx) => n.Equals(Add(c, ctx)), nextVal, One);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.NewValue);
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
						var exchRes = target.SpinWaitForExchange(Add, (c, n) => n.Equals(Add(c, One)), One);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.NewValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(Add, (c, n) => n.Equals(Add(c, One)), One);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.NewValue);
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
						var exchRes = target.SpinWaitForExchange(c => Add(c, One), (c, n, ctx) => n.Equals(Add(c, ctx)), One);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.NewValue);
					}
				},
				target => {
					for (var i = 1; i < NumIterations; i += 2) {
						var nextVal = Convert(i);
						var exchRes = target.SpinWaitForExchange(c => Add(c, One), (c, n, ctx) => n.Equals(Add(c, ctx)), One);
						AssertAreEqual(nextVal, exchRes.PreviousValue);
						AssertAreEqual(Add(nextVal, One), exchRes.NewValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;
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

					var (wasSet, prevValue, newValue) = target.TryExchange(
						c => Add(c, One),
						curValue
					);

					if (wasSet) {
						AssertAreEqual(curValue, prevValue);
						AssertAreEqual(Add(curValue, One), newValue);
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
						var (wasSet, prevValue, setValue) = target.TryExchange(c => Add(c, One), (c, n) => Equals(Add(c, One), n));
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

					var (wasSet, prevValue, newValue) = target.TryExchange(
						Add,
						curValue,
						One
					);

					if (wasSet) {
						AssertAreEqual(curValue, prevValue);
						AssertAreEqual(Add(curValue, One), newValue);
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
						var (wasSet, prevValue, setValue) = target.TryExchange(Add, (c, n) => Equals(Add(c, One), n) && LessThan(c, Convert(NumIterations)), One);
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

			// (Func<T, TContext, T>, Func<T, T, TContext, bool>)
			runner.AllThreadsTearDown = target => AssertAreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Equals(Convert(NumIterations))) return;
						var (wasSet, prevValue, setValue) = target.TryExchange(Add, (c, n, ctx) => Equals(Add(c, One), n) && LessThan(c, Convert(NumIterations)), One);
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
						var (wasSet, prevValue, setValue) = target.TryExchange(Add, (c, n, ctx) => Equals(Add(c, One), n) && LessThan(c, Convert(ctx)), One, NumIterations);
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
		public void SpinWaitForBoundedValue() {
			const int NumIterations = 300_000;
			const int Coefficient = 4;
			const int TicketChunking = NumIterations / 10_000;

			var runner = NewRunner(Zero);
			var ticketProvider = new AtomicInt(0);
			var ticketBarrier = new AtomicInt(0);

			// (T, T)
			runner.AllThreadsTearDown = target => AssertAreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteWriterReaderTests(
				target => {
					var ticketNumber = ticketProvider.Increment().NewValue;
					ticketBarrier.SpinWaitForMinimumValue(ticketNumber);
					target.Value = Convert(ticketNumber * Coefficient);
				},
				target => {
					while (true) {
						var barrierValue = ticketBarrier.Add(TicketChunking).NewValue;
						AssertAreEqual(
							Convert(barrierValue * Coefficient),
							target.SpinWaitForBoundedValue(Convert((barrierValue * Coefficient) - (Coefficient / 2)), Convert(barrierValue * Coefficient))
						);
						if (barrierValue == NumIterations * Coefficient) return;
					}
				},
				NumIterations,
				iterateWriterFunc: true,
				iterateReaderFunc: false
			);
		}

		[Test]
		public void IncrementDecrement() {
			const int NumIterations = 100_000;

			var runner = NewRunner(Zero);

			runner.AllThreadsTearDown = target => Assert.AreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					var (prevValue, newValue) = target.Increment();
					Assert.AreEqual(Add(prevValue, One), newValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => Assert.AreEqual(Convert(-NumIterations), target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					var (prevValue, newValue) = target.Decrement();
					Assert.AreEqual(Sub(prevValue, One), newValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => Assert.AreEqual(Zero, target.Value);
			runner.ExecuteWriterReaderTests(
				target => {
					var (prevValue, newValue) = target.Increment();
					Assert.AreEqual(Add(prevValue, One), newValue);
				},
				target => {
					var (prevValue, newValue) = target.Decrement();
					Assert.AreEqual(Sub(prevValue, One), newValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => Assert.AreEqual(Convert(NumIterations), target.Value);
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var (prevValue, newValue) = target.Increment();
					Assert.AreEqual(Add(prevValue, One), newValue);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.GreaterOrEqual(cur, prev)
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => Assert.AreEqual(Convert(-NumIterations), target.Value);
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var (prevValue, newValue) = target.Decrement();
					Assert.AreEqual(Sub(prevValue, One), newValue);
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
					var (prevValue, newValue) = target.Add(Convert(7));
					Assert.AreEqual(Add(prevValue, Convert(7)), newValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => Assert.AreEqual(Convert(-NumIterations * 7), target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					var (prevValue, newValue) = target.Subtract(Convert(7));
					Assert.AreEqual(Sub(prevValue, Convert(7)), newValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => Assert.AreEqual(Zero, target.Value);
			runner.ExecuteWriterReaderTests(
				target => {
					var (prevValue, newValue) = target.Add(Convert(3));
					Assert.AreEqual(Add(prevValue, Convert(3)), newValue);
				},
				target => {
					var (prevValue, newValue) = target.Subtract(Convert(3));
					Assert.AreEqual(Sub(prevValue, Convert(3)), newValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => Assert.AreEqual(Convert(NumIterations * 9), target.Value);
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var (prevValue, newValue) = target.Add(Convert(9));
					Assert.AreEqual(Add(prevValue, Convert(9)), newValue);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.GreaterOrEqual(cur, prev)
			);
			runner.AllThreadsTearDown = null;

			runner.AllThreadsTearDown = target => Assert.AreEqual(Convert(-NumIterations * 9), target.Value);
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var (prevValue, newValue) = target.Subtract(Convert(9));
					Assert.AreEqual(Sub(prevValue, Convert(9)), newValue);
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
					var (prevValue, newValue) = target.MultiplyBy(Convert(7));
					Assert.AreEqual(Mul(prevValue, Convert(7)), newValue);
				},
				target => {
					var (prevValue, newValue) = target.DivideBy(Convert(3));
					Assert.AreEqual(Div(prevValue, Convert(3)), newValue);
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
			Assert.AreEqual(Convert(190), exchRes.NewValue);
			var spinWaitTask = Task.Run(() => target.SpinWaitForMinimumExchange(Convert(50), Convert(200)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(200));
			Assert.AreEqual(Convert(200), spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Convert(50), spinWaitTask.Result.NewValue);

			// (Func<T, T>, T, T)
			exchRes = target.SpinWaitForMinimumExchange(t => Mul(t, Convert(2)), Convert(0));
			Assert.AreEqual(Convert(50), exchRes.PreviousValue);
			Assert.AreEqual(Convert(100), exchRes.NewValue);
			spinWaitTask = Task.Run(() => target.SpinWaitForMinimumExchange(t => Sub(t, Convert(10)), Convert(101)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(101));
			Assert.AreEqual(Convert(101), spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Convert(91), spinWaitTask.Result.NewValue);

			// (Func<T, TContext, T>, T, T)
			exchRes = target.SpinWaitForMinimumExchange(Add, Convert(91), Convert(109));
			Assert.AreEqual(Convert(91), exchRes.PreviousValue);
			Assert.AreEqual(Convert(200), exchRes.NewValue);
			spinWaitTask = Task.Run(() => target.SpinWaitForMinimumExchange(Div, Convert(300), Convert(10)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(300));
			Assert.AreEqual(Convert(300), spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Convert(30), spinWaitTask.Result.NewValue);
		}

		[Test]
		public void API_SpinWaitForMaximumExchange() {
			var target = new TTarget();

			target.Set(Convert(-100));

			// (T, T)
			var exchRes = target.SpinWaitForMaximumExchange(Convert(-190), Convert(-90));
			Assert.AreEqual(Convert(-100), exchRes.PreviousValue);
			Assert.AreEqual(Convert(-190), exchRes.NewValue);
			var spinWaitTask = Task.Run(() => target.SpinWaitForMaximumExchange(Convert(-50), Convert(-200)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(-200));
			Assert.AreEqual(Convert(-200), spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Convert(-50), spinWaitTask.Result.NewValue);

			// (Func<T, T>, T, T)
			exchRes = target.SpinWaitForMaximumExchange(t => Mul(t, Convert(2)), Convert(-0));
			Assert.AreEqual(Convert(-50), exchRes.PreviousValue);
			Assert.AreEqual(Convert(-100), exchRes.NewValue);
			spinWaitTask = Task.Run(() => target.SpinWaitForMaximumExchange(t => Sub(t, Convert(-10)), Convert(-101)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(-101));
			Assert.AreEqual(Convert(-101), spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Convert(-91), spinWaitTask.Result.NewValue);

			// (Func<T, TContext, T>, T, T)
			exchRes = target.SpinWaitForMaximumExchange(Add, Convert(-91), Convert(-109));
			Assert.AreEqual(Convert(-91), exchRes.PreviousValue);
			Assert.AreEqual(Convert(-200), exchRes.NewValue);
			spinWaitTask = Task.Run(() => target.SpinWaitForMaximumExchange(Div, Convert(-300), Convert(10)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(-300));
			Assert.AreEqual(Convert(-300), spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Convert(-30), spinWaitTask.Result.NewValue);
		}

		[Test]
		public void API_SpinWaitForBoundedExchange() {
			var target = new TTarget();

			target.Set(Convert(100));

			// (T, T, T)
			var exchRes = target.SpinWaitForBoundedExchange(Convert(190), Convert(90), Convert(110));
			Assert.AreEqual(Convert(100), exchRes.PreviousValue);
			Assert.AreEqual(Convert(190), exchRes.NewValue);
			var spinWaitTask = Task.Run(() => target.SpinWaitForBoundedExchange(Convert(50), Convert(200), Convert(300)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(200));
			Assert.AreEqual(Convert(200), spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Convert(50), spinWaitTask.Result.NewValue);

			// (Func<T, T>, T, T)
			exchRes = target.SpinWaitForBoundedExchange(t => Mul(t, Convert(2)), Convert(0), Convert(51));
			Assert.AreEqual(Convert(50), exchRes.PreviousValue);
			Assert.AreEqual(Convert(100), exchRes.NewValue);
			spinWaitTask = Task.Run(() => target.SpinWaitForBoundedExchange(t => Sub(t, Convert(10)), Convert(101), Convert(102)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(101));
			Assert.AreEqual(Convert(101), spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Convert(91), spinWaitTask.Result.NewValue);

			// (Func<T, TContext, T>, T, T)
			exchRes = target.SpinWaitForBoundedExchange(Add, Convert(91), Convert(92), Convert(109));
			Assert.AreEqual(Convert(91), exchRes.PreviousValue);
			Assert.AreEqual(Convert(200), exchRes.NewValue);
			spinWaitTask = Task.Run(() => target.SpinWaitForBoundedExchange(Div, Convert(300), Convert(400), Convert(10)));
			Thread.Sleep(100); // Give the test time to fail if it's gonna
			Assert.AreEqual(false, spinWaitTask.IsCompleted);
			target.Set(Convert(300));
			Assert.AreEqual(Convert(300), spinWaitTask.Result.PreviousValue);
			Assert.AreEqual(Convert(30), spinWaitTask.Result.NewValue);
		}

		[Test]
		public void API_TryMinimumExchange() {
			var target = new TTarget();

			target.Set(Convert(100));

			// (T, T)
			var exchRes = target.TryMinimumExchange(Convert(190), Convert(90));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(100), exchRes.PreviousValue);
			Assert.AreEqual(Convert(190), exchRes.NewValue);
			exchRes = target.TryMinimumExchange(Convert(50), Convert(200));
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(190), exchRes.PreviousValue);
			Assert.AreEqual(Convert(190), exchRes.NewValue);
			target.Set(Convert(200));
			exchRes = target.TryMinimumExchange(Convert(50), Convert(200));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(200), exchRes.PreviousValue);
			Assert.AreEqual(Convert(50), exchRes.NewValue);

			// (Func<T, T>, T, T)
			exchRes = target.TryMinimumExchange(t => Mul(t, Convert(2)), Convert(0));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(50), exchRes.PreviousValue);
			Assert.AreEqual(Convert(100), exchRes.NewValue);
			exchRes = target.TryMinimumExchange(t => Sub(t, Convert(10)), Convert(101));
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(100), exchRes.PreviousValue);
			Assert.AreEqual(Convert(100), exchRes.NewValue);
			target.Set(Convert(101));
			exchRes = target.TryMinimumExchange(t => Sub(t, Convert(10)), Convert(101));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(101), exchRes.PreviousValue);
			Assert.AreEqual(Convert(91), exchRes.NewValue);

			// (Func<T, TContext, T>, T, T)
			exchRes = target.TryMinimumExchange(Add, Convert(91), Convert(109));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(91), exchRes.PreviousValue);
			Assert.AreEqual(Convert(200), exchRes.NewValue);
			exchRes = target.TryMinimumExchange(Div, Convert(300), Convert(10));
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(200), exchRes.PreviousValue);
			Assert.AreEqual(Convert(200), exchRes.NewValue);
			target.Set(Convert(300));
			exchRes = target.TryMinimumExchange(Div, Convert(300), Convert(10));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(300), exchRes.PreviousValue);
			Assert.AreEqual(Convert(30), exchRes.NewValue);
		}

		[Test]
		public void API_TryMaximumExchange() {
			var target = new TTarget();

			target.Set(Convert(-100));

			// (T, T)
			var exchRes = target.TryMaximumExchange(Convert(-190), Convert(-90));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(-100), exchRes.PreviousValue);
			Assert.AreEqual(Convert(-190), exchRes.NewValue);
			exchRes = target.TryMaximumExchange(Convert(-50), Convert(-200));
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(-190), exchRes.PreviousValue);
			Assert.AreEqual(Convert(-190), exchRes.NewValue);
			target.Set(Convert(-200));
			exchRes = target.TryMaximumExchange(Convert(-50), Convert(-200));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(-200), exchRes.PreviousValue);
			Assert.AreEqual(Convert(-50), exchRes.NewValue);

			// (Func<T, T>, T, T)
			exchRes = target.TryMaximumExchange(t => Mul(t, Convert(2)), Convert(-0));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(-50), exchRes.PreviousValue);
			Assert.AreEqual(Convert(-100), exchRes.NewValue);
			exchRes = target.TryMaximumExchange(t => Sub(t, Convert(-10)), Convert(-101));
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(-100), exchRes.PreviousValue);
			Assert.AreEqual(Convert(-100), exchRes.NewValue);
			target.Set(Convert(-101));
			exchRes = target.TryMaximumExchange(t => Sub(t, Convert(-10)), Convert(-101));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(-101), exchRes.PreviousValue);
			Assert.AreEqual(Convert(-91), exchRes.NewValue);

			// (Func<T, TContext, T>, T, T)
			exchRes = target.TryMaximumExchange(Add, Convert(-91), Convert(-109));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(-91), exchRes.PreviousValue);
			Assert.AreEqual(Convert(-200), exchRes.NewValue);
			exchRes = target.TryMaximumExchange(Div, Convert(-300), Convert(10));
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(-200), exchRes.PreviousValue);
			Assert.AreEqual(Convert(-200), exchRes.NewValue);
			target.Set(Convert(-300));
			exchRes = target.TryMaximumExchange(Div, Convert(-300), Convert(10));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(-300), exchRes.PreviousValue);
			Assert.AreEqual(Convert(-30), exchRes.NewValue);
		}

		[Test]
		public void API_TryBoundedExchange() {
			var target = new TTarget();

			target.Set(Convert(100));

			// (T, T, T)
			var exchRes = target.TryBoundedExchange(Convert(190), Convert(90), Convert(110));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(100), exchRes.PreviousValue);
			Assert.AreEqual(Convert(190), exchRes.NewValue);
			exchRes = target.TryBoundedExchange(Convert(50), Convert(200), Convert(300));
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(190), exchRes.PreviousValue);
			Assert.AreEqual(Convert(190), exchRes.NewValue);
			target.Set(Convert(200));
			exchRes = target.TryBoundedExchange(Convert(50), Convert(200), Convert(300));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(200), exchRes.PreviousValue);
			Assert.AreEqual(Convert(50), exchRes.NewValue);

			// (Func<T, T>, T, T)
			exchRes = target.TryBoundedExchange(t => Mul(t, Convert(2)), Convert(0), Convert(51));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(50), exchRes.PreviousValue);
			Assert.AreEqual(Convert(100), exchRes.NewValue);
			exchRes = target.TryBoundedExchange(t => Sub(t, Convert(10)), Convert(101), Convert(102));
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(100), exchRes.PreviousValue);
			Assert.AreEqual(Convert(100), exchRes.NewValue);
			target.Set(Convert(101));
			exchRes = target.TryBoundedExchange(t => Sub(t, Convert(10)), Convert(101), Convert(102));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(101), exchRes.PreviousValue);
			Assert.AreEqual(Convert(91), exchRes.NewValue);

			// (Func<T, TContext, T>, T, T)
			exchRes = target.TryBoundedExchange(Add, Convert(91), Convert(92), Convert(109));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(91), exchRes.PreviousValue);
			Assert.AreEqual(Convert(200), exchRes.NewValue);
			exchRes = target.TryBoundedExchange(Div, Convert(300), Convert(400), Convert(10));
			Assert.AreEqual(false, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(200), exchRes.PreviousValue);
			Assert.AreEqual(Convert(200), exchRes.NewValue);
			target.Set(Convert(300));
			exchRes = target.TryBoundedExchange(Div, Convert(300), Convert(400), Convert(10));
			Assert.AreEqual(true, exchRes.ValueWasSet);
			Assert.AreEqual(Convert(300), exchRes.PreviousValue);
			Assert.AreEqual(Convert(30), exchRes.NewValue);
		}

		[Test]
		public void API_Increment() {
			var target = new TTarget();

			target.Set(Convert(100));

			var incRes = target.Increment();
			Assert.AreEqual(Convert(100), incRes.PreviousValue);
			Assert.AreEqual(Convert(101), incRes.NewValue);
		}

		[Test]
		public void API_Decrement() {
			var target = new TTarget();

			target.Set(Convert(100));

			var incRes = target.Decrement();
			Assert.AreEqual(Convert(100), incRes.PreviousValue);
			Assert.AreEqual(Convert(99), incRes.NewValue);
		}

		[Test]
		public void API_Add() {
			var target = new TTarget();

			target.Set(Convert(100));

			var incRes = target.Add(Convert(20));
			Assert.AreEqual(Convert(100), incRes.PreviousValue);
			Assert.AreEqual(Convert(120), incRes.NewValue);
		}

		[Test]
		public void API_Subtract() {
			var target = new TTarget();

			target.Set(Convert(100));

			var incRes = target.Subtract(Convert(20));
			Assert.AreEqual(Convert(100), incRes.PreviousValue);
			Assert.AreEqual(Convert(80), incRes.NewValue);
		}

		[Test]
		public void API_MultiplyBy() {
			var target = new TTarget();

			target.Set(Convert(100));

			var incRes = target.MultiplyBy(Convert(20));
			Assert.AreEqual(Convert(100), incRes.PreviousValue);
			Assert.AreEqual(Convert(2000), incRes.NewValue);
		}

		[Test]
		public void API_DivideBy() {
			var target = new TTarget();

			target.Set(Convert(100));

			var incRes = target.DivideBy(Convert(20));
			Assert.AreEqual(Convert(100), incRes.PreviousValue);
			Assert.AreEqual(Convert(5), incRes.NewValue);
		}
	}
}