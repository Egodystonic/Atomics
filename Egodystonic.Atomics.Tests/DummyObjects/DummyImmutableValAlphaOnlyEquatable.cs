using System;

namespace Egodystonic.Atomics.Tests.DummyObjects {
	struct DummyImmutableValAlphaOnlyEquatable : IEquatable<DummyImmutableValAlphaOnlyEquatable> {
		public readonly int Alpha;
		public readonly int Bravo;

		public DummyImmutableValAlphaOnlyEquatable(int alpha, int bravo) {
			Alpha = alpha;
			Bravo = bravo;
		}

		public bool Equals(DummyImmutableValAlphaOnlyEquatable other) {
			return Alpha == other.Alpha;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			return obj is DummyImmutableValAlphaOnlyEquatable other && Equals(other);
		}

		public override int GetHashCode() => Alpha;

		public static bool operator ==(DummyImmutableValAlphaOnlyEquatable left, DummyImmutableValAlphaOnlyEquatable right) { return left.Equals(right); }
		public static bool operator !=(DummyImmutableValAlphaOnlyEquatable left, DummyImmutableValAlphaOnlyEquatable right) { return !left.Equals(right); }

		public override string ToString() => $"{Alpha}, {Bravo}";
	}
}