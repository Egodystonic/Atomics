// (c) Egodystonic Studios 2018


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Egodystonic.Atomics.Tests.DummyObjects {
	[StructLayout(LayoutKind.Auto, Size = 16)] // Unnecessary in reality but it's good to be ultra-specific
	struct DummySixteenByteVal : IEquatable<DummySixteenByteVal> {
		public readonly float Alligator;
		public readonly float Bear;
		public readonly float Crocodile;
		public readonly float Dragon; // Only dragons are worth comparing for equality; the others are inferior species

		public DummySixteenByteVal(float alligator, float bear, float crocodile, float dragon) {
			Alligator = alligator;
			Bear = bear;
			Crocodile = crocodile;
			Dragon = dragon;
		}

		public bool Equals(DummySixteenByteVal other) {
			return Dragon.Equals(other.Dragon);
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			return obj is DummySixteenByteVal other && Equals(other);
		}

		public override int GetHashCode() {
			return Dragon.GetHashCode();
		}

		public static bool operator ==(DummySixteenByteVal left, DummySixteenByteVal right) { return left.Equals(right); }
		public static bool operator !=(DummySixteenByteVal left, DummySixteenByteVal right) { return !left.Equals(right); }
	}
}