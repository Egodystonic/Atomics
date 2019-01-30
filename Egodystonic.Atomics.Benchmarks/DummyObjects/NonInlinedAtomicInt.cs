using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Egodystonic.Atomics.Numerics;

namespace Egodystonic.Atomics.Benchmarks.DummyObjects {
	public sealed class NonInlinedAtomicInt : INumericAtomic<int> {
		int _value;

		public int Value {
			
			get => Get();
			
			set => Set(value);
		}

		public NonInlinedAtomicInt() : this(default) { }
		public NonInlinedAtomicInt(int initialValue) => Set(initialValue);

		
		public int Get() => Volatile.Read(ref _value);

		
		public int GetUnsafe() => _value;

		
		public void Set(int CurrentValue) => Volatile.Write(ref _value, CurrentValue);

		
		public void SetUnsafe(int CurrentValue) => _value = CurrentValue;

		
		public (int PreviousValue, int CurrentValue) Exchange(int CurrentValue) => (Interlocked.Exchange(ref _value, CurrentValue), CurrentValue);

		
		public int SpinWaitForValue(int targetValue) {
			var spinner = new SpinWait();
			while (Get() != targetValue) spinner.SpinOnce();
			return targetValue;
		}

		public (int PreviousValue, int CurrentValue) Exchange<TContext>(Func<int, TContext, int> mapFunc, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var CurrentValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForExchange(int CurrentValue, int comparand) {
			var spinner = new SpinWait();

			while (true) {
				if (Interlocked.CompareExchange(ref _value, CurrentValue, comparand) == comparand) return (comparand, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForExchange<TContext>(Func<int, TContext, int> mapFunc, TContext context, int comparand) {
			var spinner = new SpinWait();
			var CurrentValue = mapFunc(comparand, context); // curValue will always be comparand when this method returns

			while (true) {
				if (Interlocked.CompareExchange(ref _value, CurrentValue, comparand) == comparand) return (comparand, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<int, TMapContext, int> mapFunc, TMapContext mapContext, Func<int, int, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var CurrentValue = mapFunc(curValue, mapContext);
				if (!predicate(curValue, CurrentValue, predicateContext)) {
					spinner.SpinOnce();
					continue;
				}

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		
		public (bool ValueWasSet, int PreviousValue, int CurrentValue) TryExchange(int CurrentValue, int comparand) {
			var oldValue = Interlocked.CompareExchange(ref _value, CurrentValue, comparand);
			var wasSet = oldValue == comparand;
			return (wasSet, oldValue, wasSet ? CurrentValue : oldValue);
		}

		public (bool ValueWasSet, int PreviousValue, int CurrentValue) TryExchange<TContext>(Func<int, TContext, int> mapFunc, TContext context, int comparand) {
			var CurrentValue = mapFunc(comparand, context); // Comparand will always be curValue if the interlocked call passes
			var prevValue = Interlocked.CompareExchange(ref _value, CurrentValue, comparand);
			if (prevValue == comparand) return (true, prevValue, CurrentValue);
			else return (false, prevValue, prevValue);
		}

		public (bool ValueWasSet, int PreviousValue, int CurrentValue) TryExchange<TMapContext, TPredicateContext>(Func<int, TMapContext, int> mapFunc, TMapContext mapContext, Func<int, int, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var CurrentValue = mapFunc(curValue, mapContext);
				if (!predicate(curValue, CurrentValue, predicateContext)) return (false, curValue, curValue);

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);

				spinner.SpinOnce();
			}
		}

		// ============================ Numeric API ============================

		public int SpinWaitForMinimumValue(int minValue) {
			var spinner = new SpinWait();
			while (true) {
				var curVal = Get();
				if (curVal >= minValue) return curVal;
				spinner.SpinOnce();
			}
		}

		public int SpinWaitForMaximumValue(int maxValue) {
			var spinner = new SpinWait();
			while (true) {
				var curVal = Get();
				if (curVal <= maxValue) return curVal;
				spinner.SpinOnce();
			}
		}

		public int SpinWaitForBoundedValue(int lowerBoundInclusive, int upperBoundExclusive) {
			var spinner = new SpinWait();
			while (true) {
				var curVal = Get();
				if (curVal >= lowerBoundInclusive && curVal < upperBoundExclusive) return curVal;
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForMinimumExchange(int CurrentValue, int minValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) {
					spinner.SpinOnce();
					continue;
				}

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForMinimumExchange(Func<int, int> mapFunc, int minValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) {
					spinner.SpinOnce();
					continue;
				}

				var CurrentValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForMinimumExchange<TContext>(Func<int, TContext, int> mapFunc, int minValue, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) {
					spinner.SpinOnce();
					continue;
				}

				var CurrentValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForMaximumExchange(int CurrentValue, int maxValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) {
					spinner.SpinOnce();
					continue;
				}

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForMaximumExchange(Func<int, int> mapFunc, int maxValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) {
					spinner.SpinOnce();
					continue;
				}

				var CurrentValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForMaximumExchange<TContext>(Func<int, TContext, int> mapFunc, int maxValue, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) {
					spinner.SpinOnce();
					continue;
				}

				var CurrentValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForBoundedExchange(int CurrentValue, int lowerBoundInclusive, int upperBoundExclusive) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) {
					spinner.SpinOnce();
					continue;
				}

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForBoundedExchange(Func<int, int> mapFunc, int lowerBoundInclusive, int upperBoundExclusive) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) {
					spinner.SpinOnce();
					continue;
				}

				var CurrentValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForBoundedExchange<TContext>(Func<int, TContext, int> mapFunc, int lowerBoundInclusive, int upperBoundExclusive, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) {
					spinner.SpinOnce();
					continue;
				}

				var CurrentValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int CurrentValue) TryMinimumExchange(int CurrentValue, int minValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) return (false, curValue, curValue);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int CurrentValue) TryMinimumExchange(Func<int, int> mapFunc, int minValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) return (false, curValue, curValue);
				var CurrentValue = mapFunc(curValue);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int CurrentValue) TryMinimumExchange<TContext>(Func<int, TContext, int> mapFunc, int minValue, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) return (false, curValue, curValue);
				var CurrentValue = mapFunc(curValue, context);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int CurrentValue) TryMaximumExchange(int CurrentValue, int maxValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) return (false, curValue, curValue);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int CurrentValue) TryMaximumExchange(Func<int, int> mapFunc, int maxValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) return (false, curValue, curValue);
				var CurrentValue = mapFunc(curValue);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int CurrentValue) TryMaximumExchange<TContext>(Func<int, TContext, int> mapFunc, int maxValue, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) return (false, curValue, curValue);
				var CurrentValue = mapFunc(curValue, context);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int CurrentValue) TryBoundedExchange(int CurrentValue, int lowerBoundInclusive, int upperBoundExclusive) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) return (false, curValue, curValue);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int CurrentValue) TryBoundedExchange(Func<int, int> mapFunc, int lowerBoundInclusive, int upperBoundExclusive) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) return (false, curValue, curValue);
				var CurrentValue = mapFunc(curValue);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int CurrentValue) TryBoundedExchange<TContext>(Func<int, TContext, int> mapFunc, int lowerBoundInclusive, int upperBoundExclusive, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) return (false, curValue, curValue);
				var CurrentValue = mapFunc(curValue, context);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		
		public (int PreviousValue, int CurrentValue) Increment() {
			var newVal = Interlocked.Increment(ref _value);
			return (newVal - 1, newVal);
		}

		
		public (int PreviousValue, int CurrentValue) Decrement() {
			var newVal = Interlocked.Decrement(ref _value);
			return (newVal + 1, newVal);
		}

		
		public (int PreviousValue, int CurrentValue) Add(int operand) {
			var newVal = Interlocked.Add(ref _value, operand);
			return (newVal - operand, newVal);
		}

		
		public (int PreviousValue, int CurrentValue) Subtract(int operand) {
			var newVal = Interlocked.Add(ref _value, -operand);
			return (newVal + operand, newVal);
		}

		public (int PreviousValue, int CurrentValue) MultiplyBy(int operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var CurrentValue = curValue * operand;

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) DivideBy(int operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var CurrentValue = curValue / operand;

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		
		public static implicit operator int(NonInlinedAtomicInt operand) => operand.Get();
	}
}