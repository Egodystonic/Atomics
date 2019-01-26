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

		public int SpinWaitForBoundedValue(int lowerBoundInclusive, int upperBoundExclusive) {
			var spinner = new SpinWait();
			while (true) {
				var curVal = Get();
				if (curVal >= lowerBoundInclusive && curVal <= upperBoundExclusive) return curVal;
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForBoundedExchange(int CurrentValue, int lowerBoundInclusive, int upperBoundExclusive) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue > upperBoundExclusive) {
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
				if (curValue < lowerBoundInclusive || curValue > upperBoundExclusive) {
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
				if (curValue < lowerBoundInclusive || curValue > upperBoundExclusive) {
					spinner.SpinOnce();
					continue;
				}

				var CurrentValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		
		public (int PreviousValue, int CurrentValue) Increment() {
			var prevVal = Interlocked.Increment(ref _value);
			return (prevVal, prevVal + 1);
		}

		
		public (int PreviousValue, int CurrentValue) Decrement() {
			var prevVal = Interlocked.Decrement(ref _value);
			return (prevVal, prevVal - 1);
		}

		
		public (int PreviousValue, int CurrentValue) Add(int operand) {
			var prevVal = Interlocked.Add(ref _value, operand);
			return (prevVal, prevVal + operand);
		}

		
		public (int PreviousValue, int CurrentValue) Subtract(int operand) {
			var prevVal = Interlocked.Add(ref _value, -operand);
			return (prevVal, prevVal - operand);
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