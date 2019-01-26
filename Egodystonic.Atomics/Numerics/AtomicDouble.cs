// ReSharper disable CompareOfFloatsByEqualityOperator Direct comparison is correct behaviour here; we're using as a bitwise equality check, not interpreting sameness/value
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Egodystonic.Atomics.Numerics {
	public sealed class AtomicDouble : IFloatingPointAtomic<double> {
		double _value;

		public double Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Set(value);
		}

		public AtomicDouble() : this(default) { }
		public AtomicDouble(double initialValue) => Set(initialValue);

		public double Get() { // TODO benchmark whether this is better than turning _value in to a union struct and using Interlocked.Read(ref long) (I'm guessing probably not)
			var spinner = new SpinWait();
			while (true) {
				var valueLocal = Volatile.Read(ref _value);

				if (Interlocked.CompareExchange(ref _value, valueLocal, valueLocal) == valueLocal) return valueLocal;
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double GetUnsafe() => _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(double CurrentValue) => Interlocked.Exchange(ref _value, CurrentValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(double CurrentValue) => _value = CurrentValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (double PreviousValue, double CurrentValue) Exchange(double CurrentValue) => (Interlocked.Exchange(ref _value, CurrentValue), CurrentValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double SpinWaitForValue(double targetValue) {
			var spinner = new SpinWait();
			while (Get() != targetValue) spinner.SpinOnce();
			return targetValue;
		}

		public (double PreviousValue, double CurrentValue) Exchange<TContext>(Func<double, TContext, double> mapFunc, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var CurrentValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (double PreviousValue, double CurrentValue) SpinWaitForMinimumExchange(double CurrentValue, double minValue) {
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

		public (double PreviousValue, double CurrentValue) SpinWaitForMinimumExchange(Func<double, double> mapFunc, double minValue) {
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

		public (double PreviousValue, double CurrentValue) SpinWaitForMinimumExchange<TContext>(Func<double, TContext, double> mapFunc, double minValue, TContext context) {
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

		public (double PreviousValue, double CurrentValue) SpinWaitForMaximumExchange(double CurrentValue, double maxValue) {
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

		public (double PreviousValue, double CurrentValue) SpinWaitForMaximumExchange(Func<double, double> mapFunc, double maxValue) {
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

		public (double PreviousValue, double CurrentValue) SpinWaitForMaximumExchange<TContext>(Func<double, TContext, double> mapFunc, double maxValue, TContext context) {
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

		public (double PreviousValue, double CurrentValue) SpinWaitForExchange(double CurrentValue, double comparand) {
			var spinner = new SpinWait();

			while (true) {
				if (Interlocked.CompareExchange(ref _value, CurrentValue, comparand) == comparand) return (comparand, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (double PreviousValue, double CurrentValue) SpinWaitForExchange<TContext>(Func<double, TContext, double> mapFunc, TContext context, double comparand) {
			var spinner = new SpinWait();
			var CurrentValue = mapFunc(comparand, context); // curValue will always be comparand when this method returns

			while (true) {
				if (Interlocked.CompareExchange(ref _value, CurrentValue, comparand) == comparand) return (comparand, CurrentValue);
				spinner.SpinOnce();
			}		
		}

		public (double PreviousValue, double CurrentValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<double, TMapContext, double> mapFunc, TMapContext mapContext, Func<double, double, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
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

		public (bool ValueWasSet, double PreviousValue, double CurrentValue) TryMinimumExchange(double CurrentValue, double minValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) return (false, curValue, curValue);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, double PreviousValue, double CurrentValue) TryMinimumExchange(Func<double, double> mapFunc, double minValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) return (false, curValue, curValue);
				var CurrentValue = mapFunc(curValue);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, double PreviousValue, double CurrentValue) TryMinimumExchange<TContext>(Func<double, TContext, double> mapFunc, double minValue, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) return (false, curValue, curValue);
				var CurrentValue = mapFunc(curValue, context);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, double PreviousValue, double CurrentValue) TryMaximumExchange(double CurrentValue, double maxValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) return (false, curValue, curValue);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, double PreviousValue, double CurrentValue) TryMaximumExchange(Func<double, double> mapFunc, double maxValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) return (false, curValue, curValue);
				var CurrentValue = mapFunc(curValue);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, double PreviousValue, double CurrentValue) TryMaximumExchange<TContext>(Func<double, TContext, double> mapFunc, double maxValue, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) return (false, curValue, curValue);
				var CurrentValue = mapFunc(curValue, context);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, double PreviousValue, double CurrentValue) TryBoundedExchange(double CurrentValue, double lowerBoundInclusive, double upperBoundExclusive) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) return (false, curValue, curValue);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, double PreviousValue, double CurrentValue) TryBoundedExchange(Func<double, double> mapFunc, double lowerBoundInclusive, double upperBoundExclusive) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) return (false, curValue, curValue);
				var CurrentValue = mapFunc(curValue);
				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, double PreviousValue, double CurrentValue) TryBoundedExchange<TContext>(Func<double, TContext, double> mapFunc, double lowerBoundInclusive, double upperBoundExclusive, TContext context) {
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
		public (bool ValueWasSet, double PreviousValue, double CurrentValue) TryExchange(double CurrentValue, double comparand) {
			var oldValue = Interlocked.CompareExchange(ref _value, CurrentValue, comparand);
			var wasSet = oldValue == comparand;
			return (wasSet, oldValue, wasSet ? CurrentValue : oldValue);
		}

		public (bool ValueWasSet, double PreviousValue, double CurrentValue) TryExchange<TContext>(Func<double, TContext, double> mapFunc, TContext context, double comparand) {
			var CurrentValue = mapFunc(comparand, context); // Comparand will always be curValue if the interlocked call passes
			var prevValue = Interlocked.CompareExchange(ref _value, CurrentValue, comparand);
			if (prevValue == comparand) return (true, prevValue, CurrentValue);
			else return (false, prevValue, prevValue);
		}

		public (bool ValueWasSet, double PreviousValue, double CurrentValue) TryExchange<TMapContext, TPredicateContext>(Func<double, TMapContext, double> mapFunc, TMapContext mapContext, Func<double, double, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
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

		public double SpinWaitForMinimumValue(double minValue) {
			var spinner = new SpinWait();
			while (true) {
				var curVal = Get();
				if (curVal >= minValue) return curVal;
				spinner.SpinOnce();
			}
		}

		public double SpinWaitForMaximumValue(double maxValue) {
			var spinner = new SpinWait();
			while (true) {
				var curVal = Get();
				if (curVal <= maxValue) return curVal;
				spinner.SpinOnce();
			}
		}

		public double SpinWaitForBoundedValue(double lowerBoundInclusive, double upperBoundExclusive) {
			var spinner = new SpinWait();
			while (true) {
				var curVal = Get();
				if (curVal >= lowerBoundInclusive && curVal < upperBoundExclusive) return curVal;
				spinner.SpinOnce();
			}
		}

		public (double PreviousValue, double CurrentValue) SpinWaitForBoundedExchange(double CurrentValue, double lowerBoundInclusive, double upperBoundExclusive) {
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

		public (double PreviousValue, double CurrentValue) SpinWaitForBoundedExchange(Func<double, double> mapFunc, double lowerBoundInclusive, double upperBoundExclusive) {
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

		public (double PreviousValue, double CurrentValue) SpinWaitForBoundedExchange<TContext>(Func<double, TContext, double> mapFunc, double lowerBoundInclusive, double upperBoundExclusive, TContext context) {
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (double PreviousValue, double CurrentValue) Increment() => Add(1d);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (double PreviousValue, double CurrentValue) Decrement() => Subtract(1d);

		public (double PreviousValue, double CurrentValue) Add(double operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var CurrentValue = curValue + operand;

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (double PreviousValue, double CurrentValue) Subtract(double operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var CurrentValue = curValue - operand;

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (double PreviousValue, double CurrentValue) MultiplyBy(double operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var CurrentValue = curValue * operand;

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (double PreviousValue, double CurrentValue) DivideBy(double operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var CurrentValue = curValue / operand;

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		// ============================ Floating-Point API ============================

		public double SpinWaitForValueWithMaxDelta(double targetValue, double maxDelta) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (Math.Abs(targetValue - curValue) <= maxDelta) return curValue;

				spinner.SpinOnce();
			}
		}

		public (double PreviousValue, double CurrentValue) SpinWaitForExchangeWithMaxDelta(double CurrentValue, double comparand, double maxDelta) {
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

		public (double PreviousValue, double CurrentValue) SpinWaitForExchangeWithMaxDelta(Func<double, double> mapFunc, double comparand, double maxDelta) {
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

		public (double PreviousValue, double CurrentValue) SpinWaitForExchangeWithMaxDelta<TContext>(Func<double, TContext, double> mapFunc, double comparand, double maxDelta, TContext context) {
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

		public (bool ValueWasSet, double PreviousValue, double CurrentValue) TryExchangeWithMaxDelta(double CurrentValue, double comparand, double maxDelta) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (Math.Abs(curValue - comparand) > maxDelta) return (false, curValue, curValue);

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, double PreviousValue, double CurrentValue) TryExchangeWithMaxDelta(Func<double, double> mapFunc, double comparand, double maxDelta) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (Math.Abs(curValue - comparand) > maxDelta) return (false, curValue, curValue);

				var CurrentValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, double PreviousValue, double CurrentValue) TryExchangeWithMaxDelta<TContext>(Func<double, TContext, double> mapFunc, double comparand, double maxDelta, TContext context) {
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
		public static implicit operator double(AtomicDouble operand) => operand.Get();
	}
}