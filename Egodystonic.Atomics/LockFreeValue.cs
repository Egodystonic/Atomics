// (c) Egodystonic Studios 2018
// Author: Ben Bowen
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Egodystonic.Atomics {
	public sealed unsafe class LockFreeValue<T> : IScalableAtomic<T> where T : unmanaged {
		static readonly bool TargetTypeIsEquatable = typeof(IEquatable<T>).IsAssignableFrom(typeof(T));
		long _valueAsLong;

		public T Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => Set(value);
		}

		public LockFreeValue() : this(default) { }
		public LockFreeValue(T initialValue) {
			if (sizeof(T) > sizeof(long)) {
				throw new ArgumentException($"Generic type parameter in {typeof(LockFreeValue<>).Name} must not exceed {sizeof(long)} bytes. " +
											$"Given type '{typeof(T)}' has a size of {sizeof(T)} bytes. " +
											$"Use {typeof(AtomicVal<>).Name} instead for large unmanaged types.");
			}
			Set(initialValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get() {
			var valueCopy = SafeGetAsLong();
			return ReadFromLong(&valueCopy);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		long SafeGetAsLong() {
			if (IntPtr.Size == sizeof(long)) return Volatile.Read(ref _valueAsLong);
			else return Interlocked.Read(ref _valueAsLong);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetUnsafe() {
			var valueCopy = _valueAsLong;
			return ReadFromLong(&valueCopy);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(T newValue) {
			long newValueAsLong;
			WriteToLong(&newValueAsLong, newValue);
			SafeSetAsLong(newValueAsLong);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void SafeSetAsLong(long newValueAsLong) {
			if (IntPtr.Size == sizeof(long)) Volatile.Write(ref _valueAsLong, newValueAsLong);
			else Interlocked.Exchange(ref _valueAsLong, newValueAsLong);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(T newValue) {
			long newValueAsLong;
			WriteToLong(&newValueAsLong, newValue);
			_valueAsLong = newValueAsLong;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetUnsafeRef() => ref Unsafe.As<long, T>(ref _valueAsLong);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Swap(T newValue) {
			long newValueAsLong;
			WriteToLong(&newValueAsLong, newValue);
			var previousValueAsLong = Interlocked.Exchange(ref _valueAsLong, newValueAsLong);
			return ReadFromLong(&previousValueAsLong);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T TrySwap(T newValue, T comparand) {
			if (!TargetTypeIsEquatable) return TrySwapByValueOnly(newValue, comparand);

			long newValueAsLong;
			WriteToLong(&newValueAsLong, newValue);

			var spinner = new SpinWait();
			while (true) {
				var currentValue = Value;
				long currentValueAsLong;
				WriteToLong(&currentValueAsLong, currentValue);
				if (!EquatableEquals(currentValue, comparand) || Interlocked.CompareExchange(ref _valueAsLong, newValueAsLong, currentValueAsLong) == currentValueAsLong) return currentValue;
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T TrySwapByValueOnly(T newValue, T comparand) {
			long newValueAsLong, comparandAsLong;
			WriteToLong(&newValueAsLong, newValue);
			WriteToLong(&comparandAsLong, comparand);
			var previousValueAsLong = Interlocked.CompareExchange(ref _valueAsLong, newValueAsLong, comparandAsLong);
			return ReadFromLong(&previousValueAsLong);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void WriteToLong(long* target, T val) {
			*((T*) target) = val;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static T ReadFromLong(long* src) {
			return *((T*) src);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool EquatableEquals(T lhs, T rhs) => ((IEquatable<T>) lhs).Equals(rhs);

		public override string ToString() => Get().ToString();

		#region Equality
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(T other) {
			if (TargetTypeIsEquatable) return EquatableEquals(Value, other);

			long otherAsLong;
			WriteToLong(&otherAsLong, other);
			return otherAsLong == SafeGetAsLong();
		}

		public override bool Equals(object obj) {
			if (obj is T value) return Equals(value);
			return ReferenceEquals(this, obj);
		}

		// ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode Base GetHashCode() is appropriate here.
		public override int GetHashCode() => base.GetHashCode();

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(LockFreeValue<T> left, T right) => left?.Equals(right) ?? false;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(LockFreeValue<T> left, T right) => !(left == right);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(T left, LockFreeValue<T> right) => right?.Equals(left) ?? false;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(T left, LockFreeValue<T> right) => !(right == left);
		#endregion
	}
}
