// ReSharper disable CompareOfFloatsByEqualityOperator Direct comparison is correct behaviour here; we're using as a bitwise equality check, not interpreting sameness/value
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Egodystonic.Atomics.Numerics {
	public sealed class AtomicFloat : IFloatingPointAtomic<float> {
		float _value;

		public float Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Set(value);
		}

		public AtomicFloat() : this(default) { }
		public AtomicFloat(float initialValue) => Set(initialValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float Get() => Volatile.Read(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float GetUnsafe() => _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(float newValue) => Volatile.Write(ref _value, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(float newValue) => _value = newValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (float PreviousValue, float NewValue) Exchange(float newValue) => (Interlocked.Exchange(ref _value, newValue), newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float SpinWaitForValue(float targetValue) {
			var spinner = new SpinWait();
			while (Get() != targetValue) spinner.SpinOnce();
			return targetValue;
		}

		public (float PreviousValue, float NewValue) Exchange<TContext>(Func<float, TContext, float> mapFunc, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (float PreviousValue, float NewValue) SpinWaitForExchange(float newValue, float comparand) {
			var spinner = new SpinWait();

			while (true) {
				if (Interlocked.CompareExchange(ref _value, newValue, comparand) == comparand) return (comparand, newValue);
				spinner.SpinOnce();
			}
		}

		public (float PreviousValue, float NewValue) SpinWaitForExchange<TContext>(Func<float, TContext, float> mapFunc, float comparand, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue != comparand) {
					spinner.SpinOnce();
					continue;
				}

				var newValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (float PreviousValue, float NewValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<float, TMapContext, float> mapFunc, Func<float, float, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
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
		public (bool ValueWasSet, float PreviousValue, float NewValue) TryExchange(float newValue, float comparand) {
			var oldValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			var wasSet = oldValue == comparand;
			return (wasSet, oldValue, wasSet ? newValue : oldValue);
		}

		public (bool ValueWasSet, float PreviousValue, float NewValue) TryExchange<TContext>(Func<float, TContext, float> mapFunc, float comparand, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue != comparand) return (false, curValue, curValue);

				var newValue = mapFunc(curValue, context);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);

				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, float PreviousValue, float NewValue) TryExchange<TMapContext, TPredicateContext>(Func<float, TMapContext, float> mapFunc, Func<float, float, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
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

		public float SpinWaitForBoundedValue(float lowerBound, float upperBound) {
			var spinner = new SpinWait();
			while (true) {
				var curVal = Get();
				if (curVal >= lowerBound && curVal <= upperBound) return curVal;
				spinner.SpinOnce();
			}
		}

		public (float PreviousValue, float NewValue) SpinWaitForBoundedExchange(float newValue, float lowerBound, float upperBound) {
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

		public (float PreviousValue, float NewValue) SpinWaitForBoundedExchange(Func<float, float> mapFunc, float lowerBound, float upperBound) {
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

		public (float PreviousValue, float NewValue) SpinWaitForBoundedExchange<TContext>(Func<float, TContext, float> mapFunc, float lowerBound, float upperBound, TContext context) {
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
		public (float PreviousValue, float NewValue) Increment() => Add(1f);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (float PreviousValue, float NewValue) Decrement() => Subtract(1f);

		public (float PreviousValue, float NewValue) Add(float operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = curValue + operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (float PreviousValue, float NewValue) Subtract(float operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = curValue - operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (float PreviousValue, float NewValue) MultiplyBy(float operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = curValue * operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (float PreviousValue, float NewValue) DivideBy(float operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = curValue / operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		// ============================ Floating-Point API ============================

		public (bool ValueWasSet, float PreviousValue, float NewValue) TryExchangeWithMaxDelta(float newValue, float comparand, float maxDelta) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (Math.Abs(curValue - comparand) > maxDelta) return (false, curValue, curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, float PreviousValue, float NewValue) TryExchangeWithMaxDelta(Func<float, float> mapFunc, float comparand, float maxDelta) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (Math.Abs(curValue - comparand) > maxDelta) return (false, curValue, curValue);

				var newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, float PreviousValue, float NewValue) TryExchangeWithMaxDelta<TContext>(Func<float, TContext, float> mapFunc, float comparand, float maxDelta, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (Math.Abs(curValue - comparand) > maxDelta) return (false, curValue, curValue);

				var newValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator float(AtomicFloat operand) => operand.Get();
	}
}