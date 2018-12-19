﻿// ReSharper disable CompareOfFloatsByEqualityOperator Direct comparison is correct behaviour here; we're using as a bitwise equality check, not interpreting sameness/value
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Egodystonic.Atomics.Numerics {
	public sealed class AtomicInt : INumericAtomic<int> {
		int _value;

		public int Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Set(value);
		}

		public AtomicInt() : this(default) { }
		public AtomicInt(int initialValue) => Set(initialValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Get() => Volatile.Read(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetUnsafe() => _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(int newValue) => Volatile.Write(ref _value, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(int newValue) => _value = newValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int PreviousValue, int NewValue) Exchange(int newValue) => (Interlocked.Exchange(ref _value, newValue), newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int SpinWaitForValue(int targetValue) {
			var spinner = new SpinWait();
			while (Get() != targetValue) spinner.SpinOnce();
			return targetValue;
		}

		public (int PreviousValue, int NewValue) Exchange<TContext>(Func<int, TContext, int> mapFunc, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int NewValue) SpinWaitForExchange(int newValue, int comparand) {
			var spinner = new SpinWait();

			while (true) {
				if (Interlocked.CompareExchange(ref _value, newValue, comparand) == comparand) return (comparand, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int NewValue) SpinWaitForExchange<TContext>(Func<int, TContext, int> mapFunc, int comparand, TContext context) {
			var spinner = new SpinWait();
			var newValue = mapFunc(comparand, context); // curValue will always be comparand when this method returns

			while (true) {
				if (Interlocked.CompareExchange(ref _value, newValue, comparand) == comparand) return (comparand, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int NewValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<int, TMapContext, int> mapFunc, Func<int, int, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = mapFunc(curValue, mapContext);
				if (!predicate(curValue, newValue, predicateContext)) {
					spinner.SpinOnce();
					continue;
				}

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, int PreviousValue, int NewValue) TryExchange(int newValue, int comparand) {
			var oldValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			var wasSet = oldValue == comparand;
			return (wasSet, oldValue, wasSet ? newValue : oldValue);
		}

		public (bool ValueWasSet, int PreviousValue, int NewValue) TryExchange<TContext>(Func<int, TContext, int> mapFunc, int comparand, TContext context) {
			var newValue = mapFunc(comparand, context); // Comparand will always be curValue if the interlocked call passes
			var prevValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			if (prevValue == comparand) return (true, prevValue, newValue);
			else return (false, prevValue, prevValue);
		}

		public (bool ValueWasSet, int PreviousValue, int NewValue) TryExchange<TMapContext, TPredicateContext>(Func<int, TMapContext, int> mapFunc, Func<int, int, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = mapFunc(curValue, mapContext);
				if (!predicate(curValue, newValue, predicateContext)) return (false, curValue, curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);

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

		public (int PreviousValue, int NewValue) SpinWaitForMinimumExchange(int newValue, int minValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) {
					spinner.SpinOnce();
					continue;
				}

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int NewValue) SpinWaitForMinimumExchange(Func<int, int> mapFunc, int minValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) {
					spinner.SpinOnce();
					continue;
				}

				var newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int NewValue) SpinWaitForMinimumExchange<TContext>(Func<int, TContext, int> mapFunc, int minValue, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) {
					spinner.SpinOnce();
					continue;
				}

				var newValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int NewValue) SpinWaitForMaximumExchange(int newValue, int maxValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) {
					spinner.SpinOnce();
					continue;
				}

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int NewValue) SpinWaitForMaximumExchange(Func<int, int> mapFunc, int maxValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) {
					spinner.SpinOnce();
					continue;
				}

				var newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int NewValue) SpinWaitForMaximumExchange<TContext>(Func<int, TContext, int> mapFunc, int maxValue, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) {
					spinner.SpinOnce();
					continue;
				}

				var newValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int NewValue) SpinWaitForBoundedExchange(int newValue, int lowerBoundInclusive, int upperBoundExclusive) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) {
					spinner.SpinOnce();
					continue;
				}

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int NewValue) SpinWaitForBoundedExchange(Func<int, int> mapFunc, int lowerBoundInclusive, int upperBoundExclusive) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) {
					spinner.SpinOnce();
					continue;
				}

				var newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int NewValue) SpinWaitForBoundedExchange<TContext>(Func<int, TContext, int> mapFunc, int lowerBoundInclusive, int upperBoundExclusive, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) {
					spinner.SpinOnce();
					continue;
				}

				var newValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int NewValue) TryMinimumExchange(int newValue, int minValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) return (false, curValue, curValue);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int NewValue) TryMinimumExchange(Func<int, int> mapFunc, int minValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) return (false, curValue, curValue);
				var newValue = mapFunc(curValue);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int NewValue) TryMinimumExchange<TContext>(Func<int, TContext, int> mapFunc, int minValue, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) return (false, curValue, curValue);
				var newValue = mapFunc(curValue, context);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int NewValue) TryMaximumExchange(int newValue, int maxValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) return (false, curValue, curValue);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int NewValue) TryMaximumExchange(Func<int, int> mapFunc, int maxValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) return (false, curValue, curValue);
				var newValue = mapFunc(curValue);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int NewValue) TryMaximumExchange<TContext>(Func<int, TContext, int> mapFunc, int maxValue, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) return (false, curValue, curValue);
				var newValue = mapFunc(curValue, context);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int NewValue) TryBoundedExchange(int newValue, int lowerBoundInclusive, int upperBoundExclusive) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) return (false, curValue, curValue);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int NewValue) TryBoundedExchange(Func<int, int> mapFunc, int lowerBoundInclusive, int upperBoundExclusive) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) return (false, curValue, curValue);
				var newValue = mapFunc(curValue);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int NewValue) TryBoundedExchange<TContext>(Func<int, TContext, int> mapFunc, int lowerBoundInclusive, int upperBoundExclusive, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) return (false, curValue, curValue);
				var newValue = mapFunc(curValue, context);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int PreviousValue, int NewValue) Increment() {
			var newVal = Interlocked.Increment(ref _value);
			return (newVal - 1, newVal);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int PreviousValue, int NewValue) Decrement() {
			var newVal = Interlocked.Decrement(ref _value);
			return (newVal + 1, newVal);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int PreviousValue, int NewValue) Add(int operand) {
			var newVal = Interlocked.Add(ref _value, operand);
			return (newVal - operand, newVal);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int PreviousValue, int NewValue) Subtract(int operand) {
			var newVal = Interlocked.Add(ref _value, -operand);
			return (newVal + operand, newVal);
		}

		public (int PreviousValue, int NewValue) MultiplyBy(int operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = curValue * operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int NewValue) DivideBy(int operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = curValue / operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator int(AtomicInt operand) => operand.Get();
	}
}