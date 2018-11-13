// (c) Egodystonic Studios 2018


using System;
using System.Collections.Generic;
using System.Linq;

namespace Egodystonic.Atomics.Tests.DummyObjects {
	struct DummyImmutableVal : IEquatable<DummyImmutableVal> {
		public readonly int Alpha;
		public readonly int Bravo;

		public DummyImmutableVal(int alpha, int bravo) {
			Alpha = alpha;
			Bravo = bravo;
		}

		public bool Equals(DummyImmutableVal other) {
			return Alpha == other.Alpha && Bravo == other.Bravo;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			return obj is DummyImmutableVal other && Equals(other);
		}

		public override int GetHashCode() {
			unchecked {
				return (Alpha * 397) ^ Bravo;
			}
		}

		public static bool operator ==(DummyImmutableVal left, DummyImmutableVal right) { return left.Equals(right); }
		public static bool operator !=(DummyImmutableVal left, DummyImmutableVal right) { return !left.Equals(right); }

		public override string ToString() => $"{Alpha}, {Bravo}";
	}
}