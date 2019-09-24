// (c) Egodystonic Studios 2018
// Author: Ben Bowen
// ReSharper disable CompareOfFloatsByEqualityOperator Direct comparisons are correct throughout this file.
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Egodystonic.Atomics.Numerics {
	public sealed class LockFreeDouble : INonLockingFloatingPointAtomic<double>, IFormattable {
		double _value;

		public double Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Set(value);
		}

		public LockFreeDouble() : this(default) { }
		public LockFreeDouble(double initialValue) => Set(initialValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double Get() {
			if (IntPtr.Size == sizeof(long)) return Volatile.Read(ref _value);
			else return Interlocked.CompareExchange(ref _value, default, default);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double GetUnsafe() => _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref double GetUnsafeRef() => ref _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(double newValue) {
			if (IntPtr.Size == sizeof(long)) Volatile.Write(ref _value, newValue);
			else Interlocked.Exchange(ref _value, newValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(double newValue) => _value = newValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double Swap(double newValue) => Interlocked.Exchange(ref _value, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double TrySwap(double newValue, double comparand) => Interlocked.CompareExchange(ref _value, newValue, comparand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TryExchangeResult<double> TrySwap(double newValue, double comparand, double maxDelta) {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				if (Math.Abs(previousValue - comparand) > maxDelta) return new TryExchangeResult<double>(false, previousValue, previousValue);
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, newValue, previousValue);
				if (updatedPreviousValue == previousValue) return new TryExchangeResult<double>(true, previousValue, newValue);
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public double IncrementAndGet() {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, previousValue + 1f, previousValue);
				if (updatedPreviousValue == previousValue) return previousValue;
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public double DecrementAndGet() {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, previousValue - 1f, previousValue);
				if (updatedPreviousValue == previousValue) return previousValue;
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public double AddAndGet(double operand) {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, previousValue + operand, previousValue);
				if (updatedPreviousValue == previousValue) return previousValue;
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public double SubtractAndGet(double operand) {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, previousValue - operand, previousValue);
				if (updatedPreviousValue == previousValue) return previousValue;
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public ExchangeResult<double> Multiply(double operand) {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var newValue = previousValue * operand;
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, newValue, previousValue);
				if (updatedPreviousValue == previousValue) return new ExchangeResult<double>(previousValue, newValue);
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public ExchangeResult<double> Divide(double operand) {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var newValue = previousValue / operand;
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, newValue, previousValue);
				if (updatedPreviousValue == previousValue) return new ExchangeResult<double>(previousValue, newValue);
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		// ReSharper disable once SpecifyACultureInStringConversionExplicitly It's up to the user of our API to pick the overload they prefer (but they should probably specify a culture indeed).
		public override string ToString() => Get().ToString();
		public string ToString(string format, IFormatProvider formatProvider) => Get().ToString(format, formatProvider);

		#region Equality
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(double other) => other == Value;

		public override bool Equals(object obj) {
			if (obj is double value) return Equals(value);
			return ReferenceEquals(this, obj);
		}

		// ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode Base GetHashCode() is appropriate here.
		public override int GetHashCode() => base.GetHashCode();

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(LockFreeDouble left, double right) => left?.Equals(right) ?? false;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(LockFreeDouble left, double right) => !(left == right);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(double left, LockFreeDouble right) => right?.Equals(left) ?? false;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(double left, LockFreeDouble right) => !(right == left);
		#endregion
	}
}