// (c) Egodystonic Studios 2018
// Author: Ben Bowen
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Egodystonic.Atomics.Numerics {
	public sealed class LockFreeInt32 : INonLockingIntegerAtomic<int>, IFormattable {
		int _value;

		public int Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Set(value);
		}

		public LockFreeInt32() : this(default) { }
		public LockFreeInt32(int initialValue) => Set(initialValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Get() => Volatile.Read(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetUnsafe() => _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref int GetUnsafeRef() => ref _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(int newValue) => Volatile.Write(ref _value, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(int newValue) => _value = newValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Swap(int newValue) => Interlocked.Exchange(ref _value, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int TrySwap(int newValue, int comparand) => Interlocked.CompareExchange(ref _value, newValue, comparand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int IncrementAndGet() => Interlocked.Increment(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int DecrementAndGet() => Interlocked.Decrement(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int AddAndGet(int operand) => Interlocked.Add(ref _value, operand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int SubtractAndGet(int operand) => Interlocked.Add(ref _value, -operand);

		public ExchangeResult<int> Multiply(int operand) {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var newValue = previousValue * operand;
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, newValue, previousValue);
				if (updatedPreviousValue == previousValue) return new ExchangeResult<int>(previousValue, newValue);
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public ExchangeResult<int> Divide(int operand) {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var newValue = previousValue / operand;
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, newValue, previousValue);
				if (updatedPreviousValue == previousValue) return new ExchangeResult<int>(previousValue, newValue);
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public ExchangeResult<int> BitwiseAnd(int operand) {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var newValue = previousValue & operand;
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, newValue, previousValue);
				if (updatedPreviousValue == previousValue) return new ExchangeResult<int>(previousValue, newValue);
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public ExchangeResult<int> BitwiseOr(int operand) {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var newValue = previousValue | operand;
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, newValue, previousValue);
				if (updatedPreviousValue == previousValue) return new ExchangeResult<int>(previousValue, newValue);
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public ExchangeResult<int> BitwiseExclusiveOr(int operand) {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var newValue = previousValue ^ operand;
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, newValue, previousValue);
				if (updatedPreviousValue == previousValue) return new ExchangeResult<int>(previousValue, newValue);
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public ExchangeResult<int> BitwiseNegate() {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var newValue = ~previousValue;
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, newValue, previousValue);
				if (updatedPreviousValue == previousValue) return new ExchangeResult<int>(previousValue, newValue);
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public ExchangeResult<int> BitwiseLeftShift(int operand) {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var newValue = previousValue << operand;
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, newValue, previousValue);
				if (updatedPreviousValue == previousValue) return new ExchangeResult<int>(previousValue, newValue);
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public ExchangeResult<int> BitwiseRightShift(int operand) {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var newValue = previousValue >> operand;
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, newValue, previousValue);
				if (updatedPreviousValue == previousValue) return new ExchangeResult<int>(previousValue, newValue);
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public override string ToString() => Get().ToString();
		public string ToString(string format, IFormatProvider formatProvider) => Get().ToString(format, formatProvider);

		#region Equality
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(int other) => Value == other;

		public override bool Equals(object obj) {
			if (obj is int value) return Equals(value);
			return ReferenceEquals(this, obj);
		}

		// ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode Base GetHashCode() is appropriate here.
		public override int GetHashCode() => base.GetHashCode();

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(LockFreeInt32 left, int right) => left?.Equals(right) ?? false;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(LockFreeInt32 left, int right) => !(left == right);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(int left, LockFreeInt32 right) => right?.Equals(left) ?? false;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(int left, LockFreeInt32 right) => !(right == left);
		#endregion
	}
}