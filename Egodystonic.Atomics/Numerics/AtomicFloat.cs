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
		public void Set(float CurrentValue) => Volatile.Write(ref _value, CurrentValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(float CurrentValue) => _value = CurrentValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (float PreviousValue, float CurrentValue) Exchange(float CurrentValue) => (Interlocked.Exchange(ref _value, CurrentValue), CurrentValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float SpinWaitForValue(float targetValue) {
			var spinner = new SpinWait();
			while (Get() != targetValue) spinner.SpinOnce();
			return targetValue;
		}

		public (float PreviousValue, float CurrentValue) Exchange<TContext>(Func<float, TContext, float> mapFunc, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var CurrentValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (float PreviousValue, float CurrentValue) SpinWaitForExchange(float CurrentValue, float comparand) {
			var spinner = new SpinWait();

			while (true) {
				if (Interlocked.CompareExchange(ref _value, CurrentValue, comparand) == comparand) return (comparand, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (float PreviousValue, float CurrentValue) SpinWaitForExchange<TContext>(Func<float, TContext, float> mapFunc, TContext context, float comparand) {
			var spinner = new SpinWait();
			var CurrentValue = mapFunc(comparand, context); // curValue will always be comparand when this method returns

			while (true) {
				if (Interlocked.CompareExchange(ref _value, CurrentValue, comparand) == comparand) return (comparand, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (float PreviousValue, float CurrentValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<float, TMapContext, float> mapFunc, TMapContext mapContext, Func<float, float, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, float PreviousValue, float CurrentValue) TryExchange(float CurrentValue, float comparand) {
			var oldValue = Interlocked.CompareExchange(ref _value, CurrentValue, comparand);
			var wasSet = oldValue == comparand;
			return (wasSet, oldValue, wasSet ? CurrentValue : oldValue);
		}

		public (bool ValueWasSet, float PreviousValue, float CurrentValue) TryExchange<TContext>(Func<float, TContext, float> mapFunc, TContext context, float comparand) {
			var CurrentValue = mapFunc(comparand, context); // Comparand will always be curValue if the interlocked call passes
			var prevValue = Interlocked.CompareExchange(ref _value, CurrentValue, comparand);
			if (prevValue == comparand) return (true, prevValue, CurrentValue);
			else return (false, prevValue, prevValue);
		}

		public (bool ValueWasSet, float PreviousValue, float CurrentValue) TryExchange<TMapContext, TPredicateContext>(Func<float, TMapContext, float> mapFunc, TMapContext mapContext, Func<float, float, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
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

		public float SpinWaitForMinimumValue(float minValue) {
			var spinner = new SpinWait();
			while (true) {
				var curVal = Get();
				if (curVal >= minValue) return curVal;
				spinner.SpinOnce();
			}
		}

		public float SpinWaitForMaximumValue(float maxValue) {
			var spinner = new SpinWait();
			while (true) {
				var curVal = Get();
				if (curVal <= maxValue) return curVal;
				spinner.SpinOnce();
			}
		}

		public float SpinWaitForBoundedValue(float lowerBoundInclusive, float upperBoundExclusive) {
			var spinner = new SpinWait();
			while (true) {
				var curVal = Get();
				if (curVal >= lowerBoundInclusive && curVal < upperBoundExclusive) return curVal;
				spinner.SpinOnce();
			}
		}

		public (float PreviousValue, float CurrentValue) SpinWaitForMinimumExchange(float CurrentValue, float minValue) {
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

		public (float PreviousValue, float CurrentValue) SpinWaitForMinimumExchange(Func<float, float> mapFunc, float minValue) {
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

		public (float PreviousValue, float CurrentValue) SpinWaitForMinimumExchange<TContext>(Func<float, TContext, float> mapFunc, float minValue, TContext context) {
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

		public (float PreviousValue, float CurrentValue) SpinWaitForMaximumExchange(float CurrentValue, float maxValue) {
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

		public (float PreviousValue, float CurrentValue) SpinWaitForMaximumExchange(Func<float, float> mapFunc, float maxValue) {
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

		public (float PreviousValue, float CurrentValue) SpinWaitForMaximumExchange<TContext>(Func<float, TContext, float> mapFunc, float maxValue, TContext context) {
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

		public (float PreviousValue, float CurrentValue) SpinWaitForBoundedExchange(float CurrentValue, float lowerBoundInclusive, float upperBoundExclusive) {
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

		public (float PreviousValue, float CurrentValue) SpinWaitForBoundedExchange(Func<float, float> mapFunc, float lowerBoundInclusive, float upperBoundExclusive) {
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

		public (float PreviousValue, float CurrentValue) SpinWaitForBoundedExchange<TContext>(Func<float, TContext, float> mapFunc, float lowerBoundInclusive, float upperBoundExclusive, TContext context) {
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

		public (bool ValueWasSet, float PreviousValue, float CurrentValue) TryMinimumExchange(float CurrentValue, float minValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) return (false, curValue, curValue);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, float PreviousValue, float CurrentValue) TryMinimumExchange(Func<float, float> mapFunc, float minValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) return (false, curValue, curValue);
				var CurrentValue = mapFunc(curValue);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, float PreviousValue, float CurrentValue) TryMinimumExchange<TContext>(Func<float, TContext, float> mapFunc, float minValue, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) return (false, curValue, curValue);
				var CurrentValue = mapFunc(curValue, context);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, float PreviousValue, float CurrentValue) TryMaximumExchange(float CurrentValue, float maxValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) return (false, curValue, curValue);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, float PreviousValue, float CurrentValue) TryMaximumExchange(Func<float, float> mapFunc, float maxValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) return (false, curValue, curValue);
				var CurrentValue = mapFunc(curValue);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, float PreviousValue, float CurrentValue) TryMaximumExchange<TContext>(Func<float, TContext, float> mapFunc, float maxValue, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) return (false, curValue, curValue);
				var CurrentValue = mapFunc(curValue, context);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, float PreviousValue, float CurrentValue) TryBoundedExchange(float CurrentValue, float lowerBoundInclusive, float upperBoundExclusive) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) return (false, curValue, curValue);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, float PreviousValue, float CurrentValue) TryBoundedExchange(Func<float, float> mapFunc, float lowerBoundInclusive, float upperBoundExclusive) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) return (false, curValue, curValue);
				var CurrentValue = mapFunc(curValue);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, float PreviousValue, float CurrentValue) TryBoundedExchange<TContext>(Func<float, TContext, float> mapFunc, float lowerBoundInclusive, float upperBoundExclusive, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) return (false, curValue, curValue);
				var CurrentValue = mapFunc(curValue, context);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (float PreviousValue, float CurrentValue) Increment() => Add(1f);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (float PreviousValue, float CurrentValue) Decrement() => Subtract(1f);

		public (float PreviousValue, float CurrentValue) Add(float operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var CurrentValue = curValue + operand;

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (float PreviousValue, float CurrentValue) Subtract(float operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var CurrentValue = curValue - operand;

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (float PreviousValue, float CurrentValue) MultiplyBy(float operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var CurrentValue = curValue * operand;

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (float PreviousValue, float CurrentValue) DivideBy(float operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var CurrentValue = curValue / operand;

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		// ============================ Floating-Point API ============================

		public float SpinWaitForValueWithMaxDelta(float targetValue, float maxDelta) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (Math.Abs(targetValue - curValue) <= maxDelta) return curValue;

				spinner.SpinOnce();
			}
		}

		public (float PreviousValue, float CurrentValue) SpinWaitForExchangeWithMaxDelta(float CurrentValue, float comparand, float maxDelta) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (Math.Abs(comparand - curValue) > maxDelta) {
					spinner.SpinOnce();
					continue;
				}

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (float PreviousValue, float CurrentValue) SpinWaitForExchangeWithMaxDelta(Func<float, float> mapFunc, float comparand, float maxDelta) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (Math.Abs(comparand - curValue) > maxDelta) {
					spinner.SpinOnce();
					continue;
				}

				var CurrentValue = mapFunc(curValue);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (float PreviousValue, float CurrentValue) SpinWaitForExchangeWithMaxDelta<TContext>(Func<float, TContext, float> mapFunc, float comparand, float maxDelta, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (Math.Abs(comparand - curValue) > maxDelta) {
					spinner.SpinOnce();
					continue;
				}

				var CurrentValue = mapFunc(curValue, context);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, float PreviousValue, float CurrentValue) TryExchangeWithMaxDelta(float CurrentValue, float comparand, float maxDelta) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (Math.Abs(curValue - comparand) > maxDelta) return (false, curValue, curValue);

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, float PreviousValue, float CurrentValue) TryExchangeWithMaxDelta(Func<float, float> mapFunc, float comparand, float maxDelta) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (Math.Abs(curValue - comparand) > maxDelta) return (false, curValue, curValue);

				var CurrentValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, float PreviousValue, float CurrentValue) TryExchangeWithMaxDelta<TContext>(Func<float, TContext, float> mapFunc, float comparand, float maxDelta, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (Math.Abs(curValue - comparand) > maxDelta) return (false, curValue, curValue);

				var CurrentValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator float(AtomicFloat operand) => operand.Get();
	}
}