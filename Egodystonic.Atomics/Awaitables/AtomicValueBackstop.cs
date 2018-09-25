using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Egodystonic.Atomics.Awaitables {
	public struct AtomicValueBackstop : IEquatable<AtomicValueBackstop> {
		public static readonly AtomicValueBackstop None = default;
		internal static readonly AtomicValueBackstop SequenceInitial = None.Increment();
		readonly long _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)] internal AtomicValueBackstop(long value) => _value = value;

		[MethodImpl(MethodImplOptions.AggressiveInlining), Pure] internal AtomicValueBackstop Increment() => new AtomicValueBackstop(_value + 1L);

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public bool Equals(AtomicValueBackstop other) => _value == other._value;
		public override bool Equals(object obj) => obj is AtomicValueBackstop avb && avb.Equals(this);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public override int GetHashCode() => _value.GetHashCode();

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public bool IsEarlierThan(AtomicValueBackstop other) => _value < other._value;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public bool IsLaterThan(AtomicValueBackstop other) => _value > other._value;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public bool IsEarlierThanOrEqualTo(AtomicValueBackstop other) => _value <= other._value;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public bool IsLaterThanOrEqualTo(AtomicValueBackstop other) => _value >= other._value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(AtomicValueBackstop lhs, AtomicValueBackstop rhs) => lhs.Equals(rhs);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(AtomicValueBackstop lhs, AtomicValueBackstop rhs) => !lhs.Equals(rhs);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator <(AtomicValueBackstop lhs, AtomicValueBackstop rhs) => lhs.IsEarlierThan(rhs);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator >(AtomicValueBackstop lhs, AtomicValueBackstop rhs) => lhs.IsLaterThan(rhs);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator <=(AtomicValueBackstop lhs, AtomicValueBackstop rhs) => lhs.IsEarlierThanOrEqualTo(rhs);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator >=(AtomicValueBackstop lhs, AtomicValueBackstop rhs) => lhs.IsLaterThanOrEqualTo(rhs);
	}
}