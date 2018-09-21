// ReSharper disable CompareOfFloatsByEqualityOperator Direct comparison is correct behaviour here; we're using as a bitwise equality check, not interpreting sameness/value
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Egodystonic.Atomics.Numerics {
	public sealed class AtomicFloat : INumericAtomic<float> {
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
		public float Exchange(float newValue) => Interlocked.Exchange(ref _value, newValue);

		public (bool ValueWasSet, float PreviousValue) Exchange(float newValue, float comparand) {
			var oldValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			return (oldValue == comparand, oldValue);
		}

		public (bool ValueWasSet, float PreviousValue) Exchange(float newValue, Func<float, bool> predicate) {
			bool trySetValue;
			float curValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				trySetValue = predicate(curValue);

				if (!trySetValue || Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue);
		}

		public (bool ValueWasSet, float PreviousValue) Exchange(float newValue, Func<float, float, bool> predicate) {
			bool trySetValue;
			float curValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				trySetValue = predicate(curValue, newValue);

				if (!trySetValue || Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue);
		}

		public (float PreviousValue, float NewValue) Exchange(Func<float, float> mapFunc) {
			float curValue;
			float newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		public (bool ValueWasSet, float PreviousValue, float NewValue) Exchange(Func<float, float> mapFunc, float comparand) {
			bool trySetValue;
			float curValue;
			float newValue = default;

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

		public (bool ValueWasSet, float PreviousValue, float NewValue) Exchange(Func<float, float> mapFunc, Func<float, bool> predicate) {
			bool trySetValue;
			float curValue;
			float newValue = default;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				trySetValue = predicate(curValue);

				if (!trySetValue) break;

				newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue, newValue);
		}

		public (bool ValueWasSet, float PreviousValue, float NewValue) Exchange(Func<float, float> mapFunc, Func<float, float, bool> predicate) {
			bool trySetValue;
			float curValue;
			float newValue;

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
		public (float PreviousValue, float NewValue) Increment() => Add(1f);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (float PreviousValue, float NewValue) Decrement() => Subtract(1f);

		public (float PreviousValue, float NewValue) Add(float operand) {
			float curValue;
			float newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				newValue = curValue + operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		public (float PreviousValue, float NewValue) Subtract(float operand) {
			float curValue;
			float newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				newValue = curValue - operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		public (float PreviousValue, float NewValue) MultiplyBy(float operand) {
			float curValue;
			float newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				newValue = curValue * operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		public (float PreviousValue, float NewValue) DivideBy(float operand) {
			float curValue;
			float newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				newValue = curValue / operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator float(AtomicFloat operand) => operand.Get();
	}
}