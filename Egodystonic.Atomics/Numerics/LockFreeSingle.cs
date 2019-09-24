// (c) Egodystonic Studios 2018
// Author: Ben Bowen
// ReSharper disable CompareOfFloatsByEqualityOperator Direct comparisons are correct throughout this file.
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Egodystonic.Atomics.Numerics {
	public sealed class LockFreeSingle : INonLockingFloatingPointAtomic<float>, IFormattable {
		float _value;

		public float Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Set(value);
		}

		public LockFreeSingle() : this(default) { }
		public LockFreeSingle(float initialValue) => Set(initialValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float Get() => Volatile.Read(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float GetUnsafe() => _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref float GetUnsafeRef() => ref _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(float newValue) => Volatile.Write(ref _value, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(float newValue) => _value = newValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float Swap(float newValue) => Interlocked.Exchange(ref _value, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float TrySwap(float newValue, float comparand) => Interlocked.CompareExchange(ref _value, newValue, comparand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TryExchangeResult<float> TrySwap(float newValue, float comparand, float maxDelta) {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				if (Math.Abs(previousValue - comparand) > maxDelta) return new TryExchangeResult<float>(false, previousValue, previousValue);
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, newValue, previousValue);
				if (updatedPreviousValue == previousValue) return new TryExchangeResult<float>(true, previousValue, newValue);
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public float IncrementAndGet() {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, previousValue + 1f, previousValue);
				if (updatedPreviousValue == previousValue) return previousValue;
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public float DecrementAndGet() {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, previousValue - 1f, previousValue);
				if (updatedPreviousValue == previousValue) return previousValue;
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public float AddAndGet(float operand) {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, previousValue + operand, previousValue);
				if (updatedPreviousValue == previousValue) return previousValue;
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public float SubtractAndGet(float operand) {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, previousValue - operand, previousValue);
				if (updatedPreviousValue == previousValue) return previousValue;
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public ExchangeResult<float> Multiply(float operand) {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var newValue = previousValue * operand;
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, newValue, previousValue);
				if (updatedPreviousValue == previousValue) return new ExchangeResult<float>(previousValue, newValue);
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		public ExchangeResult<float> Divide(float operand) {
			var spinner = new SpinWait();
			var previousValue = Get();

			while (true) {
				var newValue = previousValue / operand;
				var updatedPreviousValue = Interlocked.CompareExchange(ref _value, newValue, previousValue);
				if (updatedPreviousValue == previousValue) return new ExchangeResult<float>(previousValue, newValue);
				previousValue = updatedPreviousValue;
				spinner.SpinOnce();
			}
		}

		// ReSharper disable once SpecifyACultureInStringConversionExplicitly It's up to the user of our API to pick the overload they prefer (but they should probably specify a culture indeed).
		public override string ToString() => Get().ToString();
		public string ToString(string format, IFormatProvider formatProvider) => Get().ToString(format, formatProvider);

		#region Equality
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(float other) => other == Value;

		public override bool Equals(object obj) {
			if (obj is float value) return Equals(value);
			return ReferenceEquals(this, obj);
		}

		// ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode Base GetHashCode() is appropriate here.
		public override int GetHashCode() => base.GetHashCode();

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(LockFreeSingle left, float right) => left?.Equals(right) ?? false;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(LockFreeSingle left, float right) => !(left == right);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(float left, LockFreeSingle right) => right?.Equals(left) ?? false;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(float left, LockFreeSingle right) => !(right == left);
		#endregion
	}
}