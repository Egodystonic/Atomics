// (c) Egodystonic Studios 2019
// Author: Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Egodystonic.Atomics {
	public sealed class Atomic<T> : INonScalableAtomic<T>, IEquatable<Atomic<T>> {
		readonly object _instanceMutationLock = new object();
		T _value;

		public T Value {
			get {
				lock (_instanceMutationLock) return _value;
			}
			set {
				lock (_instanceMutationLock) _value = value;
			}
		}

		public Atomic() : this(default) { }
		public Atomic(T value) => _value = value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get() => Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(T newValue) => Value = newValue;

		public void Set(T newValue, out T previousValue) {
			lock (_instanceMutationLock) {
				previousValue = _value;
				_value = newValue;
			}
		}

		public T Set(Func<T, T> valueMapFunc) {
			lock (_instanceMutationLock) {
				return _value = valueMapFunc(_value);
			}
		}

		public T Set(Func<T, T> valueMapFunc, out T previousValue) {
			lock (_instanceMutationLock) {
				previousValue = _value;
				return _value = valueMapFunc(_value);
			}
		}

		public bool TryGet(Func<T, bool> valueComparisonPredicate, out T currentValue) {
			lock (_instanceMutationLock) {
				currentValue = _value;
			}
			return valueComparisonPredicate(currentValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TrySet(T newValue, Func<T, bool> setPredicate) => TrySet(newValue, setPredicate, out _);
		public bool TrySet(T newValue, Func<T, bool> setPredicate, out T previousValue) {
			lock (_instanceMutationLock) {
				previousValue = _value;
				if (setPredicate(_value)) {
					_value = newValue;
					return true;
				}
				else return false;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TrySet(Func<T, T> valueMapFunc, Func<T, bool> setPredicate) => TrySet(valueMapFunc, setPredicate, out _);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TrySet(Func<T, T> valueMapFunc, Func<T, bool> setPredicate, out T previousValue) => TrySet(valueMapFunc, setPredicate, out previousValue, out _);
		public bool TrySet(Func<T, T> valueMapFunc, Func<T, bool> setPredicate, out T previousValue, out T newValue) {
			lock (_instanceMutationLock) {
				previousValue = _value;
				if (setPredicate(_value)) {
					newValue = _value = valueMapFunc(_value);
					return true;
				}
				else {
					newValue = previousValue;
					return false;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TrySet(Func<T, T> valueMapFunc, Func<T, T, bool> setPredicate) => TrySet(valueMapFunc, setPredicate, out _);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TrySet(Func<T, T> valueMapFunc, Func<T, T, bool> setPredicate, out T previousValue) => TrySet(valueMapFunc, setPredicate, out previousValue, out _);
		public bool TrySet(Func<T, T> valueMapFunc, Func<T, T, bool> setPredicate, out T previousValue, out T newValue) {
			lock (_instanceMutationLock) {
				previousValue = _value;
				var potentialNewValue = valueMapFunc(previousValue);
				if (setPredicate(_value, potentialNewValue)) {
					newValue = _value = potentialNewValue;
					return true;
				}
				else {
					newValue = previousValue;
					return false;
				}
			}
		}

		public override string ToString() => Value?.ToString() ?? AtomicUtils.NullValueString;

		#region Equality
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(T other) => EqualityComparer<T>.Default.Equals(Value, other);

		public override bool Equals(object obj) {
			if (obj is T value) return Equals(value);
			return ReferenceEquals(this, obj);
		}

		// ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode Base GetHashCode() is appropriate here.
		public override int GetHashCode() => base.GetHashCode();

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(Atomic<T> left, T right) => left?.Equals(right) ?? false;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(Atomic<T> left, T right) => !(left == right);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(T left, Atomic<T> right) => right?.Equals(left) ?? false;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(T left, Atomic<T> right) => !(right == left);
		#endregion
	}
}