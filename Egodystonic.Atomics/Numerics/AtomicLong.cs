// ReSharper disable CompareOfFloatsByEqualityOperator Direct comparison is correct behaviour here; we're using as a bitwise equality check, not interpreting sameness/value
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Egodystonic.Atomics.Numerics {
	public sealed class AtomicLong : INumericAtomic<long> {
		long _value;

		public long Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Set(value);
		}

		public AtomicLong() : this(default) { }
		public AtomicLong(long initialValue) => Set(initialValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Get() => Interlocked.Read(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long GetUnsafe() => _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(long newValue) => Interlocked.Exchange(ref _value, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(long newValue) => _value = newValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long PreviousValue, long NewValue) Exchange(long newValue) => (Interlocked.Exchange(ref _value, newValue), newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long SpinWaitForValue(long targetValue) {
			var spinner = new SpinWait();
			while (Get() != targetValue) spinner.SpinOnce();
			return targetValue;
		}

		public (long PreviousValue, long NewValue) Exchange<TContext>(Func<long, TContext, long> mapFunc, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (long PreviousValue, long NewValue) SpinWaitForExchange(long newValue, long comparand) {
			var spinner = new SpinWait();

			while (true) {
				if (Interlocked.CompareExchange(ref _value, newValue, comparand) == comparand) return (comparand, newValue);
				spinner.SpinOnce();
			}
		}

		public (long PreviousValue, long NewValue) SpinWaitForExchange<TContext>(Func<long, TContext, long> mapFunc, long comparand, TContext context) {
			var spinner = new SpinWait();
			var newValue = mapFunc(comparand, context); // curValue will always be comparand when this method returns

			while (true) {
				if (Interlocked.CompareExchange(ref _value, newValue, comparand) == comparand) return (comparand, newValue);
				spinner.SpinOnce();
			}
		}

		public (long PreviousValue, long NewValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<long, TMapContext, long> mapFunc, Func<long, long, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
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
		public (bool ValueWasSet, long PreviousValue, long NewValue) TryExchange(long newValue, long comparand) {
			var oldValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			var wasSet = oldValue == comparand;
			return (wasSet, oldValue, wasSet ? newValue : oldValue);
		}

		public (bool ValueWasSet, long PreviousValue, long NewValue) TryExchange<TContext>(Func<long, TContext, long> mapFunc, long comparand, TContext context) {
			var newValue = mapFunc(comparand, context); // Comparand will always be curValue if the interlocked call passes
			var prevValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			if (prevValue == comparand) return (true, prevValue, newValue);
			else return (false, prevValue, prevValue);
		}

		public (bool ValueWasSet, long PreviousValue, long NewValue) TryExchange<TMapContext, TPredicateContext>(Func<long, TMapContext, long> mapFunc, Func<long, long, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
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

		public long SpinWaitForBoundedValue(long lowerBound, long upperBound) {
			var spinner = new SpinWait();
			while (true) {
				var curVal = Get();
				if (curVal >= lowerBound && curVal <= upperBound) return curVal;
				spinner.SpinOnce();
			}
		}

		public (long PreviousValue, long NewValue) SpinWaitForBoundedExchange(long newValue, long lowerBound, long upperBound) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBound || curValue > upperBound) {
					spinner.SpinOnce();
					continue;
				}

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (long PreviousValue, long NewValue) SpinWaitForBoundedExchange(Func<long, long> mapFunc, long lowerBound, long upperBound) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBound || curValue > upperBound) {
					spinner.SpinOnce();
					continue;
				}

				var newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (long PreviousValue, long NewValue) SpinWaitForBoundedExchange<TContext>(Func<long, TContext, long> mapFunc, long lowerBound, long upperBound, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBound || curValue > upperBound) {
					spinner.SpinOnce();
					continue;
				}

				var newValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long PreviousValue, long NewValue) Increment() {
			var newVal = Interlocked.Increment(ref _value);
			return (newVal - 1L, newVal);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long PreviousValue, long NewValue) Decrement() {
			var newVal = Interlocked.Decrement(ref _value);
			return (newVal + 1L, newVal);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long PreviousValue, long NewValue) Add(long operand) {
			var newVal = Interlocked.Add(ref _value, operand);
			return (newVal - operand, newVal);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long PreviousValue, long NewValue) Subtract(long operand) {
			var newVal = Interlocked.Add(ref _value, -operand);
			return (newVal + operand, newVal);
		}

		public (long PreviousValue, long NewValue) MultiplyBy(long operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = curValue * operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (long PreviousValue, long NewValue) DivideBy(long operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = curValue / operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator long(AtomicLong operand) => operand.Get();
	}
}