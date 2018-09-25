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

		public void SpinWaitForValue(int targetValue) {
			var spinner = new SpinWait();
			while (true) {
				if (Get() == targetValue) return;
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Exchange(int newValue) => Interlocked.Exchange(ref _value, newValue);

		public (bool ValueWasSet, int PreviousValue) TryExchange(int newValue, int comparand) {
			var oldValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			return (oldValue == comparand, oldValue);
		}

		public (bool ValueWasSet, int PreviousValue) TryExchange(int newValue, Func<int, bool> predicate) {
			bool trySetValue;
			int curValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				trySetValue = predicate(curValue);

				if (!trySetValue || Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue);
		}

		public (bool ValueWasSet, int PreviousValue) TryExchange(int newValue, Func<int, int, bool> predicate) {
			bool trySetValue;
			int curValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				trySetValue = predicate(curValue, newValue);

				if (!trySetValue || Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue);
		}

		public (int PreviousValue, int NewValue) Exchange(Func<int, int> mapFunc) {
			int curValue;
			int newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		public (bool ValueWasSet, int PreviousValue, int NewValue) TryExchange(Func<int, int> mapFunc, int comparand) {
			bool trySetValue;
			int curValue;
			int newValue = default;

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

		public (bool ValueWasSet, int PreviousValue, int NewValue) TryExchange(Func<int, int> mapFunc, Func<int, bool> predicate) {
			bool trySetValue;
			int curValue;
			int newValue = default;

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

		public (bool ValueWasSet, int PreviousValue, int NewValue) TryExchange(Func<int, int> mapFunc, Func<int, int, bool> predicate) {
			bool trySetValue;
			int curValue;
			int newValue;

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

		public (int PreviousValue, int NewValue) Increment() {
			var newValue = Interlocked.Increment(ref _value);
			return (newValue - 1, newValue);
		}

		public (int PreviousValue, int NewValue) Decrement() {
			var newValue = Interlocked.Decrement(ref _value);
			return (newValue + 1, newValue);
		}

		public (int PreviousValue, int NewValue) Add(int operand) {
			var newValue = Interlocked.Add(ref _value, operand);
			return (newValue - operand, newValue);
		}

		public (int PreviousValue, int NewValue) Subtract(int operand) {
			var newValue = Interlocked.Add(ref _value, -operand);
			return (newValue + operand, newValue);
		}

		public (int PreviousValue, int NewValue) MultiplyBy(int operand) {
			int curValue;
			int newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				newValue = curValue * operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		public (int PreviousValue, int NewValue) DivideBy(int operand) {
			int curValue;
			int newValue;

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
		public static implicit operator int(AtomicInt operand) => operand.Get();
	}
}