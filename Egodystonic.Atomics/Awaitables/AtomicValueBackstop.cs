using System;

namespace Egodystonic.Atomics.Awaitables {
	public struct AtomicValueBackstop : IEquatable<AtomicValueBackstop> {
		public static readonly AtomicValueBackstop None = default;
		readonly long _value;

		AtomicValueBackstop(long value) => _value = value;

		public bool Equals(AtomicValueBackstop other) => _value == other._value;
		public override bool Equals(object obj) => obj is AtomicValueBackstop avb && avb.Equals(this);
		public override int GetHashCode() => _value.GetHashCode();

		public bool IsEarlierThan(AtomicValueBackstop other) => _value < other._value;
		public bool IsLaterThan(AtomicValueBackstop other) => _value > other._value;
		public bool IsEarlierThanOrEqualTo(AtomicValueBackstop other) => _value <= other._value;
		public bool IsLaterThanOrEqualTo(AtomicValueBackstop other) => _value >= other._value;

		public static bool operator ==(AtomicValueBackstop lhs, AtomicValueBackstop rhs) => lhs.Equals(rhs);
		public static bool operator !=(AtomicValueBackstop lhs, AtomicValueBackstop rhs) => !lhs.Equals(rhs);
		public static bool operator <(AtomicValueBackstop lhs, AtomicValueBackstop rhs) => lhs.IsEarlierThan(rhs);
		public static bool operator >(AtomicValueBackstop lhs, AtomicValueBackstop rhs) => lhs.IsLaterThan(rhs);
		public static bool operator <=(AtomicValueBackstop lhs, AtomicValueBackstop rhs) => lhs.IsEarlierThanOrEqualTo(rhs);
		public static bool operator >=(AtomicValueBackstop lhs, AtomicValueBackstop rhs) => lhs.IsLaterThanOrEqualTo(rhs);
	}
}