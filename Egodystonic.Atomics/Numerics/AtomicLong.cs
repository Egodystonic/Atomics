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
		public long Exchange(long newValue) => Interlocked.Exchange(ref _value, newValue);

		public (bool ValueWasSet, long PreviousValue) Exchange(long newValue, long comparand) {
			var oldValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			return (oldValue == comparand, oldValue);
		}

		public (bool ValueWasSet, long PreviousValue) Exchange(long newValue, Func<long, bool> predicate) {
			bool trySetValue;
			long curValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				trySetValue = predicate(curValue);

				if (!trySetValue || Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue);
		}

		public (bool ValueWasSet, long PreviousValue) Exchange(long newValue, Func<long, long, bool> predicate) {
			bool trySetValue;
			long curValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				trySetValue = predicate(curValue, newValue);

				if (!trySetValue || Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue);
		}

		public (long PreviousValue, long NewValue) Exchange(Func<long, long> mapFunc) {
			long curValue;
			long newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		public (bool ValueWasSet, long PreviousValue, long NewValue) Exchange(Func<long, long> mapFunc, long comparand) {
			bool trySetValue;
			long curValue;
			long newValue = default;

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

		public (bool ValueWasSet, long PreviousValue, long NewValue) Exchange(Func<long, long> mapFunc, Func<long, bool> predicate) {
			bool trySetValue;
			long curValue;
			long newValue = default;

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

		public (bool ValueWasSet, long PreviousValue, long NewValue) Exchange(Func<long, long> mapFunc, Func<long, long, bool> predicate) {
			bool trySetValue;
			long curValue;
			long newValue;

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

		public (long PreviousValue, long NewValue) Increment() {
			var newValue = Interlocked.Increment(ref _value);
			return (newValue - 1L, newValue);
		}

		public (long PreviousValue, long NewValue) Decrement() {
			var newValue = Interlocked.Decrement(ref _value);
			return (newValue + 1L, newValue);
		}

		public (long PreviousValue, long NewValue) Add(long operand) {
			var newValue = Interlocked.Add(ref _value, operand);
			return (newValue - operand, newValue);
		}

		public (long PreviousValue, long NewValue) Subtract(long operand) {
			var newValue = Interlocked.Add(ref _value, -operand);
			return (newValue + operand, newValue);
		}

		public (long PreviousValue, long NewValue) MultiplyBy(long operand) {
			long curValue;
			long newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				newValue = curValue * operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		public (long PreviousValue, long NewValue) DivideBy(long operand) {
			long curValue;
			long newValue;

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
		public static implicit operator long(AtomicLong operand) => operand.Get();
	}
}