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
		public void Set(double newValue) => Interlocked.Exchange(ref _value, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(double newValue) => _value = newValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (double PreviousValue, double NewValue) Exchange(double newValue) => (Interlocked.Exchange(ref _value, newValue), newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double SpinWaitForValue(double targetValue) {
			var spinner = new SpinWait();
			while (Get() != targetValue) spinner.SpinOnce();
			return targetValue;
		}

		public (double PreviousValue, double NewValue) Exchange<TContext>(Func<double, TContext, double> mapFunc, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (double PreviousValue, double NewValue) SpinWaitForExchange(double newValue, double comparand) {
			var spinner = new SpinWait();

			while (true) {
				if (Interlocked.CompareExchange(ref _value, newValue, comparand) == comparand) return (comparand, newValue);
				spinner.SpinOnce();
			}
		}

		public (double PreviousValue, double NewValue) SpinWaitForExchange<TContext>(Func<double, TContext, double> mapFunc, double comparand, TContext context) {
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

		public (double PreviousValue, double NewValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<double, TMapContext, double> mapFunc, Func<double, double, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
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
		public (bool ValueWasSet, double PreviousValue, double NewValue) TryExchange(double newValue, double comparand) {
			var oldValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			var wasSet = oldValue == comparand;
			return (wasSet, oldValue, wasSet ? newValue : oldValue);
		}

		public (bool ValueWasSet, double PreviousValue, double NewValue) TryExchange<TContext>(Func<double, TContext, double> mapFunc, double comparand, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue != comparand) return (false, curValue, curValue);

				var newValue = mapFunc(curValue, context);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);

				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, double PreviousValue, double NewValue) TryExchange<TMapContext, TPredicateContext>(Func<double, TMapContext, double> mapFunc, Func<double, double, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
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

		public double SpinWaitForBoundedValue(double lowerBound, double upperBound) {
			var spinner = new SpinWait();
			while (true) {
				var curVal = Get();
				if (curVal >= lowerBound && curVal <= upperBound) return curVal;
				spinner.SpinOnce();
			}
		}

		public (double PreviousValue, double NewValue) SpinWaitForBoundedExchange(double newValue, double lowerBound, double upperBound) {
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

		public (double PreviousValue, double NewValue) SpinWaitForBoundedExchange(Func<double, double> mapFunc, double lowerBound, double upperBound) {
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

		public (double PreviousValue, double NewValue) SpinWaitForBoundedExchange<TContext>(Func<double, TContext, double> mapFunc, double lowerBound, double upperBound, TContext context) {
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
		public (double PreviousValue, double NewValue) Increment() => Add(1d);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (double PreviousValue, double NewValue) Decrement() => Subtract(1d);

		public (double PreviousValue, double NewValue) Add(double operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = curValue + operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (double PreviousValue, double NewValue) Subtract(double operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = curValue - operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (double PreviousValue, double NewValue) MultiplyBy(double operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = curValue * operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (double PreviousValue, double NewValue) DivideBy(double operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = curValue / operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		// ============================ Floating-Point API ============================

		public (bool ValueWasSet, double PreviousValue, double NewValue) TryExchangeWithMaxDelta(double newValue, double comparand, double maxDelta) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (Math.Abs(curValue - comparand) > maxDelta) return (false, curValue, curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, double PreviousValue, double NewValue) TryExchangeWithMaxDelta(Func<double, double> mapFunc, double comparand, double maxDelta) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (Math.Abs(curValue - comparand) > maxDelta) return (false, curValue, curValue);

				var newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, double PreviousValue, double NewValue) TryExchangeWithMaxDelta<TContext>(Func<double, TContext, double> mapFunc, double comparand, double maxDelta, TContext context) {
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
		public static implicit operator double(AtomicDouble operand) => operand.Get();
	}
}