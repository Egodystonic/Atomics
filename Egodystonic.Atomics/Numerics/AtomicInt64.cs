// (c) Egodystonic Studios 2018
// Author: Ben Bowen
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Egodystonic.Atomics.Numerics {
	public sealed class AtomicInt64 : INumericAtomic<long>, IFormattable {
		long _value;

		public long Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Set(value);
		}

		public AtomicInt64() : this(default) { }
		public AtomicInt64(long initialValue) => Set(initialValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Get() {
			if (IntPtr.Size == sizeof(long)) return Volatile.Read(ref _value);
			else return Interlocked.Read(ref _value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long GetUnsafe() => _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(long newValue) {
			if (IntPtr.Size == sizeof(long)) Volatile.Write(ref _value, newValue);
			else Interlocked.Exchange(ref _value, newValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(long newValue) => _value = newValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long FastExchange(long newValue) => Interlocked.Exchange(ref _value, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long PreviousValue, long CurrentValue) Exchange(long newValue) => (Interlocked.Exchange(ref _value, newValue), newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long SpinWaitForValue(long targetValue) {
			var spinner = new SpinWait();
			while (Get() != targetValue) spinner.SpinOnce();
			return targetValue;
		}

		public (long PreviousValue, long CurrentValue) Exchange<TContext>(Func<long, TContext, long> mapFunc, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (long PreviousValue, long CurrentValue) SpinWaitForExchange(long newValue, long comparand) {
			var spinner = new SpinWait();

			while (true) {
				if (Interlocked.CompareExchange(ref _value, newValue, comparand) == comparand) return (comparand, newValue);
				spinner.SpinOnce();
			}
		}

		public (long PreviousValue, long CurrentValue) SpinWaitForExchange<TContext>(Func<long, TContext, long> mapFunc, TContext context, long comparand) {
			var spinner = new SpinWait();
			var newValue = mapFunc(comparand, context); // curValue will always be comparand when this method returns

			while (true) {
				if (Interlocked.CompareExchange(ref _value, newValue, comparand) == comparand) return (comparand, newValue);
				spinner.SpinOnce();
			}
		}

		public (long PreviousValue, long CurrentValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<long, TMapContext, long> mapFunc, TMapContext mapContext, Func<long, long, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
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
		public long FastTryExchange(long newValue, long comparand) => Interlocked.CompareExchange(ref _value, newValue, comparand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, long PreviousValue, long CurrentValue) TryExchange(long newValue, long comparand) {
			var oldValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			var wasSet = oldValue == comparand;
			return (wasSet, oldValue, wasSet ? newValue : oldValue);
		}

		public (bool ValueWasSet, long PreviousValue, long CurrentValue) TryExchange<TContext>(Func<long, TContext, long> mapFunc, TContext context, long comparand) {
			var newValue = mapFunc(comparand, context); // Comparand will always be curValue if the interlocked call passes
			var prevValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			if (prevValue == comparand) return (true, prevValue, newValue);
			else return (false, prevValue, prevValue);
		}

		public (bool ValueWasSet, long PreviousValue, long CurrentValue) TryExchange<TMapContext, TPredicateContext>(Func<long, TMapContext, long> mapFunc, TMapContext mapContext, Func<long, long, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
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

		public long SpinWaitForMinimumValue(long minValue) {
			var spinner = new SpinWait();
			while (true) {
				var curVal = Get();
				if (curVal >= minValue) return curVal;
				spinner.SpinOnce();
			}
		}

		public long SpinWaitForMaximumValue(long maxValue) {
			var spinner = new SpinWait();
			while (true) {
				var curVal = Get();
				if (curVal <= maxValue) return curVal;
				spinner.SpinOnce();
			}
		}

		public long SpinWaitForBoundedValue(long lowerBoundInclusive, long upperBoundExclusive) {
			var spinner = new SpinWait();
			while (true) {
				var curVal = Get();
				if (curVal >= lowerBoundInclusive && curVal < upperBoundExclusive) return curVal;
				spinner.SpinOnce();
			}
		}

		public (long PreviousValue, long CurrentValue) SpinWaitForMinimumExchange(long newValue, long minValue) {
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

		public (long PreviousValue, long CurrentValue) SpinWaitForMinimumExchange(Func<long, long> mapFunc, long minValue) {
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

		public (long PreviousValue, long CurrentValue) SpinWaitForMinimumExchange<TContext>(Func<long, TContext, long> mapFunc, long minValue, TContext context) {
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

		public (long PreviousValue, long CurrentValue) SpinWaitForMaximumExchange(long newValue, long maxValue) {
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

		public (long PreviousValue, long CurrentValue) SpinWaitForMaximumExchange(Func<long, long> mapFunc, long maxValue) {
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

		public (long PreviousValue, long CurrentValue) SpinWaitForMaximumExchange<TContext>(Func<long, TContext, long> mapFunc, long maxValue, TContext context) {
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

		public (long PreviousValue, long CurrentValue) SpinWaitForBoundedExchange(long newValue, long lowerBoundInclusive, long upperBoundExclusive) {
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

		public (long PreviousValue, long CurrentValue) SpinWaitForBoundedExchange(Func<long, long> mapFunc, long lowerBoundInclusive, long upperBoundExclusive) {
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

		public (long PreviousValue, long CurrentValue) SpinWaitForBoundedExchange<TContext>(Func<long, TContext, long> mapFunc, long lowerBoundInclusive, long upperBoundExclusive, TContext context) {
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

		public (bool ValueWasSet, long PreviousValue, long CurrentValue) TryMinimumExchange(long newValue, long minValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) return (false, curValue, curValue);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, long PreviousValue, long CurrentValue) TryMinimumExchange(Func<long, long> mapFunc, long minValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) return (false, curValue, curValue);
				var newValue = mapFunc(curValue);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, long PreviousValue, long CurrentValue) TryMinimumExchange<TContext>(Func<long, TContext, long> mapFunc, long minValue, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) return (false, curValue, curValue);
				var newValue = mapFunc(curValue, context);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, long PreviousValue, long CurrentValue) TryMaximumExchange(long newValue, long maxValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) return (false, curValue, curValue);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, long PreviousValue, long CurrentValue) TryMaximumExchange(Func<long, long> mapFunc, long maxValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) return (false, curValue, curValue);
				var newValue = mapFunc(curValue);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, long PreviousValue, long CurrentValue) TryMaximumExchange<TContext>(Func<long, TContext, long> mapFunc, long maxValue, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) return (false, curValue, curValue);
				var newValue = mapFunc(curValue, context);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, long PreviousValue, long CurrentValue) TryBoundedExchange(long newValue, long lowerBoundInclusive, long upperBoundExclusive) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) return (false, curValue, curValue);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, long PreviousValue, long CurrentValue) TryBoundedExchange(Func<long, long> mapFunc, long lowerBoundInclusive, long upperBoundExclusive) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) return (false, curValue, curValue);
				var newValue = mapFunc(curValue);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, long PreviousValue, long CurrentValue) TryBoundedExchange<TContext>(Func<long, TContext, long> mapFunc, long lowerBoundInclusive, long upperBoundExclusive, TContext context) {
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
		public long FastIncrement() => Interlocked.Increment(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long FastDecrement() => Interlocked.Decrement(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long FastAdd(long operand) => Interlocked.Add(ref _value, operand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long FastSubtract(long operand) => Interlocked.Add(ref _value, -operand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long PreviousValue, long CurrentValue) Increment() {
			var newVal = Interlocked.Increment(ref _value);
			return (newVal - 1L, newVal);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long PreviousValue, long CurrentValue) Decrement() {
			var newVal = Interlocked.Decrement(ref _value);
			return (newVal + 1L, newVal);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long PreviousValue, long CurrentValue) Add(long operand) {
			var newVal = Interlocked.Add(ref _value, operand);
			return (newVal - operand, newVal);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long PreviousValue, long CurrentValue) Subtract(long operand) {
			var newVal = Interlocked.Add(ref _value, -operand);
			return (newVal + operand, newVal);
		}

		public (long PreviousValue, long CurrentValue) MultiplyBy(long operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = curValue * operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (long PreviousValue, long CurrentValue) DivideBy(long operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = curValue / operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator long(AtomicInt64 operand) => operand.Get();

		public override string ToString() => Get().ToString();
		public string ToString(IFormatProvider provider) => Get().ToString(provider);
		public string ToString(string format) => Get().ToString(format);
		public string ToString(string format, IFormatProvider provider) => Get().ToString(format, provider);
	}
}