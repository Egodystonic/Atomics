// (c) Egodystonic Studios 2018


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.UnitTests.Common {
	abstract class CommonAtomicNumericTestSuite<T, TTarget> : CommonAtomicTestSuite<T, TTarget> where TTarget : INumericAtomic<T>, new() where T : IComparable<T>, IComparable, IEquatable<T> {
		protected abstract T Zero { get; }
		protected abstract T One { get; }
		protected abstract T Convert(int operand);
		protected abstract T Add(T lhs, T rhs);
		protected abstract T Sub(T lhs, T rhs);
		protected abstract T Mul(T lhs, T rhs);
		protected abstract T Div(T lhs, T rhs);

		[Test]
		public void GetAndSet() {
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
		}

		[Test]
		public void Exchange() {
			const int NumIterations = 300_000;

			var atomicInt = new AtomicInt(0);
			var runner = NewRunner(Zero);
			runner.GlobalSetUp = _ => { atomicInt.Set(0); };
			runner.AllThreadsTearDown = target => {
				Assert.AreEqual(Convert(NumIterations), target.Value);
			};

			// Test: Method always exhibits coherency for consecutive reads from external threads
			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					var newValue = Convert(atomicInt.Increment().NewValue);
					var prev = target.Exchange(newValue);
					Assert.AreEqual(prev, Sub(newValue, One));
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					Assert.LessOrEqual(prev, cur);
				}
			);
		}

		[Test]
		public void TryExchange() {
			const int NumIterations = 100_000;

			var runner = NewRunner(Convert(NumIterations));

			// Test: Return value of method is always consistent
			runner.ExecuteFreeThreadedTests(
				target => {
					var curValue = target.Value;
					var newValue = Add(curValue, curValue);
					var (wasSet, prevValue, setValue) = target.TryExchange(newValue, curValue);
					if (wasSet) {
						Assert.AreEqual(curValue, prevValue);
						Assert.AreEqual(newValue, setValue);
					}
					else {
						Assert.AreNotEqual(curValue, prevValue);
						Assert.AreEqual(setValue, prevValue);
					}
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			// Test: Method does what is expected and is safe from race conditions
			runner.AllThreadsTearDown = target => Assert.AreEqual(0, target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Equals(Zero)) return;
						var newValue = Sub(curValue, One);
						var (wasSet, prevValue, setValue) = target.TryExchange(newValue, curValue);
						if (wasSet) {
							Assert.AreEqual(curValue, prevValue);
							Assert.AreEqual(newValue, setValue);
						}
						else {
							Assert.AreNotEqual(curValue, prevValue);
							Assert.AreEqual(setValue, prevValue);
						}
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// Test: Method always exhibits coherency for consecutive reads from external threads
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curValue = target.Value;
					var newValue = Sub(curValue, One);
					target.TryExchange(newValue, curValue);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.LessOrEqual(cur, prev)
			);
		}

		[Test]
		public void PredicatedTryExchange() {
			const int NumIterations = 100_000;

			var runner = NewRunner(Zero);

			// Test: Return value of TryExchange is always consistent
			runner.ExecuteFreeThreadedTests(
				target => {
					var curValue = target.Value;
					var newValue = Add(curValue, One);
					var (wasSet, prevValue, _) = target.TryExchange(newValue, (c, n) => c.Equals(Sub(n, One)));
					if (wasSet) {
						Assert.AreEqual(curValue, prevValue);
						Assert.AreEqual(Add(prevValue, One), newValue);
					}
					else Assert.AreNotEqual(curValue, prevValue);
				},
				NumIterations
			);


			// Test: Method does what is expected and is safe from race conditions
			runner.AllThreadsTearDown = target => {
				Assert.AreEqual(NumIterations * -1, target.Value);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Equals(Convert(NumIterations * -1))) return;
						var newValue = Sub(curValue, One);
						var (wasSet, prevValue, _) = target.TryExchange(newValue, (c, n) => c.Equals(Add(n, One)));
						if (wasSet) Assert.AreEqual(curValue, prevValue);
						else Assert.AreNotEqual(curValue, prevValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// Test: Method always exhibits coherency for consecutive reads from external threads
			runner.ExecuteContinuousCoherencyTests(
				target => {
					checked {
						var curValue = target.Value;
						var newValue = Sub(curValue, Convert(3));
						target.TryExchange(newValue, (c, n) => c.Equals(Add(n, Convert(3))));
					}
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					Assert.LessOrEqual(cur, prev);
					Assert.AreEqual(cur, Mul(Div(cur, Convert(3)), Convert(3))); // For integer-based numerics, check that we haven't somehow become a non-multiple-of-three
				}
			);
		}

		[Test]
		public void MappedExchange() {
			const int NumIterations = 300_000;

			var runner = NewRunner(Zero);
			runner.AllThreadsTearDown = target => {
				Assert.AreEqual(Convert(NumIterations), target.Value);
			};

			// Test: Method always exhibits coherency for consecutive reads from external threads
			runner.ExecuteContinuousSingleWriterCoherencyTests(
				target => {
					var (prevValue, newValue) = target.Exchange(c => Add(c, One));
					Assert.AreEqual(prevValue, Sub(newValue, One));
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					Assert.LessOrEqual(prev, cur);
				}
			);
		}

		[Test]
		public void MappedTryExchange() {
			const int NumIterations = 100_000;

			var runner = NewRunner(Convert(NumIterations));

			// Test: Return value of method is always consistent
			runner.ExecuteFreeThreadedTests(
				target => {
					var curValue = target.Value;
					var (wasSet, prevValue, newValue) = target.TryExchange(c => Add(c, c), curValue);
					if (wasSet) {
						Assert.AreEqual(curValue, prevValue);
						Assert.AreEqual(Add(prevValue, prevValue), newValue);
					}
					else Assert.AreNotEqual(curValue, prevValue);
				},
				NumIterations
			);
			runner.AllThreadsTearDown = null;

			// Test: Method does what is expected and is safe from race conditions
			runner.AllThreadsTearDown = target => Assert.AreEqual(0, target.Value);
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Equals(Zero)) return;
						var (wasSet, prevValue, newValue) = target.TryExchange(c => Sub(c, One), curValue);
						if (wasSet) {
							Assert.AreEqual(curValue, prevValue);
							Assert.AreEqual(Sub(prevValue, One), newValue);
						}
						else Assert.AreNotEqual(curValue, prevValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// Test: Method always exhibits coherency for consecutive reads from external threads
			runner.ExecuteContinuousCoherencyTests(
				target => {
					var curValue = target.Value;
					target.TryExchange(c => Sub(c, One), curValue);
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => Assert.LessOrEqual(cur, prev)
			);
		}

		[Test]
		public void MappedPredicatedTryExchange() {
			const int NumIterations = 100_000;

			var runner = NewRunner(Zero);

			// Test: Return value of TryExchange is always consistent
			runner.ExecuteFreeThreadedTests(
				target => {
					var (wasSet, prevValue, newValue) = target.TryExchange(c => Add(c, One), (c, n) => c.Equals(Sub(n, One)));
					if (wasSet) Assert.AreEqual(Add(prevValue, One), newValue);
				},
				NumIterations
			);


			// Test: Method does what is expected and is safe from race conditions
			runner.AllThreadsTearDown = target => {
				Assert.AreEqual(NumIterations * -1, target.Value);
			};
			runner.ExecuteFreeThreadedTests(
				target => {
					while (true) {
						var curValue = target.Value;
						if (curValue.Equals(Convert(NumIterations * -1))) return;
						var (wasSet, prevValue, newValue) = target.TryExchange(c => Sub(c, One), (c, n) => c.Equals(Add(n, One)) && !c.Equals(Convert(NumIterations * -1)));
						if (wasSet) Assert.AreEqual(Sub(prevValue, One), newValue);
					}
				}
			);
			runner.AllThreadsTearDown = null;

			// Test: Method always exhibits coherency for consecutive reads from external threads
			runner.ExecuteContinuousCoherencyTests(
				target => {
					checked {
						target.TryExchange(c => Sub(c, Convert(3)), (c, n) => c.Equals(Add(n, Convert(3))));
					}
				},
				NumIterations,
				target => target.Value,
				(prev, cur) => {
					Assert.LessOrEqual(cur, prev);
					Assert.AreEqual(cur, Mul(Div(cur, Convert(3)), Convert(3))); // For integer-based numerics, check that we haven't somehow become a non-multiple-of-three
				}
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
	}
}