using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Egodystonic.Atomics.Awaitables {
	struct SequencedValue<T> : IEquatable<SequencedValue<T>> {
		public readonly AtomicValueBackstop Backstop;
		public readonly T Value;

		public SequencedValue(AtomicValueBackstop backstop, T value) {
			Backstop = backstop;
			Value = value;
		}

		public SequencedValue<T> Increment(T newValue) => new SequencedValue<T>(Backstop.Increment(), newValue);

		public static SequencedValue<T> Initial(T initialValue) => new SequencedValue<T>(AtomicValueBackstop.SequenceInitial, initialValue);

		public bool Equals(SequencedValue<T> other) {
			return Backstop.Equals(other.Backstop);
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			return obj is SequencedValue<T> other && Equals(other);
		}

		public override int GetHashCode() {
			return Backstop.GetHashCode();
		}

		public static bool operator ==(SequencedValue<T> left, SequencedValue<T> right) { return left.Equals(right); }
		public static bool operator !=(SequencedValue<T> left, SequencedValue<T> right) { return !left.Equals(right); }
	}
}
