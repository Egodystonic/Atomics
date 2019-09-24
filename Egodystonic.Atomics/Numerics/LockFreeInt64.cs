// (c) Egodystonic Studios 2018
// Author: Ben Bowen
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Egodystonic.Atomics.Numerics {
	public sealed class LockFreeInt64 : INonLockingIntegerAtomic<long>, IFormattable {
		long _value;

		public long Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Set(value);
		}

		public LockFreeInt64() : this(default) { }
		public LockFreeInt64(long initialValue) => Set(initialValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Get() {
			if (IntPtr.Size == sizeof(long)) return Volatile.Read(ref _value);
			else return Interlocked.Read(ref _value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long GetUnsafe() => _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref long GetUnsafeRef() => ref _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(long newValue) {
			if (IntPtr.Size == sizeof(long)) Volatile.Write(ref _value, newValue);
			else Interlocked.Exchange(ref _value, newValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(long newValue) => _value = newValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Swap(long newValue) => Interlocked.Exchange(ref _value, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long TrySwap(long newValue, long comparand) => Interlocked.CompareExchange(ref _value, newValue, comparand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long IncrementAndGet() => Interlocked.Increment(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long DecrementAndGet() => Interlocked.Decrement(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long AddAndGet(long operand) => Interlocked.Add(ref _value, operand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long SubtractAndGet(long operand) => Interlocked.Add(ref _value, -operand);

		public ExchangeResult<long> Multiply(long operand) {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var newValue = previousValue * operand;
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, newValue, previousValue);
				if (updatedPreviousValue == previousValue) return new ExchangeResult<long>(previousValue, newValue);
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public ExchangeResult<long> Divide(long operand) {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var newValue = previousValue / operand;
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, newValue, previousValue);
				if (updatedPreviousValue == previousValue) return new ExchangeResult<long>(previousValue, newValue);
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public ExchangeResult<long> BitwiseAnd(long operand) {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var newValue = previousValue & operand;
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, newValue, previousValue);
				if (updatedPreviousValue == previousValue) return new ExchangeResult<long>(previousValue, newValue);
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public ExchangeResult<long> BitwiseOr(long operand) {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var newValue = previousValue | operand;
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, newValue, previousValue);
				if (updatedPreviousValue == previousValue) return new ExchangeResult<long>(previousValue, newValue);
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public ExchangeResult<long> BitwiseExclusiveOr(long operand) {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var newValue = previousValue ^ operand;
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, newValue, previousValue);
				if (updatedPreviousValue == previousValue) return new ExchangeResult<long>(previousValue, newValue);
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public ExchangeResult<long> BitwiseNegate() {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var newValue = ~previousValue;
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, newValue, previousValue);
				if (updatedPreviousValue == previousValue) return new ExchangeResult<long>(previousValue, newValue);
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public ExchangeResult<long> BitwiseLeftShift(int operand) {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var newValue = previousValue << operand;
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, newValue, previousValue);
				if (updatedPreviousValue == previousValue) return new ExchangeResult<long>(previousValue, newValue);
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public ExchangeResult<long> BitwiseRightShift(int operand) {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var newValue = previousValue >> operand;
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, newValue, previousValue);
				if (updatedPreviousValue == previousValue) return new ExchangeResult<long>(previousValue, newValue);
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public override string ToString() => Get().ToString();
		public string ToString(string format, IFormatProvider formatProvider) => Get().ToString(format, formatProvider);

		#region Equality
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(long other) => Value == other;

		public override bool Equals(object obj) {
			if (obj is long value) return Equals(value);
			return ReferenceEquals(this, obj);
		}

		// ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode Base GetHashCode() is appropriate here.
		public override int GetHashCode() => base.GetHashCode();

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(LockFreeInt64 left, long right) => left?.Equals(right) ?? false;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(LockFreeInt64 left, long right) => !(left == right);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(long left, LockFreeInt64 right) => right?.Equals(left) ?? false;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(long left, LockFreeInt64 right) => !(right == left);
		#endregion
	}
}