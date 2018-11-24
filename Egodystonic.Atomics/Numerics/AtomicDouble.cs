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

		public double Get() {
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
		public double Exchange(double newValue) => Interlocked.Exchange(ref _value, newValue);

		public (bool ValueWasSet, double PreviousValue) TryExchange(double newValue, double comparand) {
			var oldValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			return (oldValue == comparand, oldValue);
		}

		public (bool ValueWasSet, double PreviousValue) TryExchange(double newValue, Func<double, double, bool> predicate) {
			bool trySetValue;
			double curValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				trySetValue = predicate(curValue, newValue);

				if (!trySetValue || Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue);
		}

		public (double PreviousValue, double NewValue) Exchange(Func<double, double> mapFunc) {
			double curValue;
			double newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		public (bool ValueWasSet, double PreviousValue, double NewValue) TryExchange(Func<double, double> mapFunc, double comparand) {
			bool trySetValue;
			double curValue;
			double newValue = default;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				trySetValue = comparand == curValue;

				if (!trySetValue) break;

				newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue, newValue);
		}

		public (bool ValueWasSet, double PreviousValue, double NewValue) TryExchange(Func<double, double> mapFunc, Func<double, double, bool> predicate) {
			bool trySetValue;
			double curValue;
			double newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				newValue = mapFunc(curValue);
				trySetValue = predicate(curValue, newValue);

				if (!trySetValue) break;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue, newValue);
		}

		// ============================ Numeric API ============================

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (double PreviousValue, double NewValue) Increment() => Add(1d);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (double PreviousValue, double NewValue) Decrement() => Subtract(1d);

		public (double PreviousValue, double NewValue) Add(double operand) {
			double curValue;
			double newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				newValue = curValue + operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		public (double PreviousValue, double NewValue) Subtract(double operand) {
			double curValue;
			double newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				newValue = curValue - operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		public (double PreviousValue, double NewValue) MultiplyBy(double operand) {
			double curValue;
			double newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				newValue = curValue * operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		public (double PreviousValue, double NewValue) DivideBy(double operand) {
			double curValue;
			double newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				newValue = curValue / operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		// ============================ Floating-Point API ============================

		public (bool ValueWasSet, double PreviousValue) TryExchange(double newValue, double comparand, double maxDelta) {
			bool trySetValue;
			double curValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				trySetValue = Math.Abs(curValue - comparand) <= maxDelta;

				if (!trySetValue || Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue);
		}

		public (bool ValueWasSet, double PreviousValue, double NewValue) TryExchange(Func<double, double> mapFunc, double comparand, double maxDelta) {
			bool trySetValue;
			double curValue;
			double newValue = default;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				trySetValue = Math.Abs(curValue - comparand) <= maxDelta;

				if (!trySetValue) break;

				newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue, newValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator double(AtomicDouble operand) => operand.Get();
	}
}