// (c) Egodystonic Studios 2018
// Author: Ben Bowen
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

#pragma warning disable CS8613  // "Nullability of reference types in return type doesn't match implicitly implemented member"
								// ... This is hard to solve currently. IAtomic<T> and its children can't mark T as nullable because they don't constrain to a class;
								// but we want to use nullability in this class. The other option is to explicitly implement the interfaces (i.e. IAtomic<T>.Whatever())
								// but that's just violating DRY for no real reason really and the better alternative IMO is just to disable the warning for now.

namespace Egodystonic.Atomics {
	public sealed class LockFreeReference<T> : INonLockingAtomic<T> where T : class {
		static readonly bool TargetTypeIsEquatable = typeof(IEquatable<T>).IsAssignableFrom(typeof(T));
		T? _value;

		public T? Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => Set(value);
		}

		public LockFreeReference() : this(default) { }
		public LockFreeReference(T? initialValue) => Set(initialValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T? Get() => Volatile.Read(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T? GetUnsafe() => _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(T? newValue) => Volatile.Write(ref _value, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(T? newValue) => _value = newValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T? GetUnsafeRef() => ref _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T? Swap(T? newValue) => Interlocked.Exchange(ref _value, newValue);

		public T? TrySwap(T? newValue, T? comparand) {
			if (!TargetTypeIsEquatable) return TrySwapByRefOnly(newValue, comparand);

			var spinner = new SpinWait();
			while (true) {
				var currentValue = Value;
				if (!EquatableEquals(currentValue, comparand) || Interlocked.CompareExchange(ref _value, newValue, currentValue) == currentValue) return currentValue;
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T? TrySwapByRefOnly(T? newValue, T? comparand) => Interlocked.CompareExchange(ref _value, newValue, comparand);

		static bool EquatableEquals(T? lhs, T? rhs) {
			if (lhs is null || rhs is null) return lhs is null && rhs is null;
			return ((IEquatable<T>) lhs).Equals(rhs);
		}

		public override string ToString() => Get()?.ToString() ?? AtomicUtils.NullValueString;

		#region Equality
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(T? other) => TargetTypeIsEquatable ? EquatableEquals(Value, other) : ReferenceEquals(Value, other);

		public override bool Equals(object obj) {
			if (obj is T value) return Equals(value);
			return ReferenceEquals(this, obj);
		}

		// ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode Base GetHashCode() is appropriate here.
		public override int GetHashCode() => base.GetHashCode();

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(LockFreeReference<T> left, T? right) => left?.Equals(right) ?? false;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(LockFreeReference<T> left, T? right) => !(left == right);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(T? left, LockFreeReference<T> right) => right?.Equals(left) ?? false;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(T? left, LockFreeReference<T> right) => !(right == left);
		#endregion
	}
}
