// (c) Egodystonic Studios 2018


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Egodystonic.Atomics.Tests.DummyObjects {
	[StructLayout(LayoutKind.Explicit, Size = 16)]
	struct DummySixteenByteVal : IEquatable<DummySixteenByteVal> {
		[FieldOffset(0)]
		public readonly float Alpha;
		[FieldOffset(4)]
		public readonly float NonEquatedValue1;
		[FieldOffset(8)]
		public readonly float Bravo;
		[FieldOffset(12)]
		public readonly float NonEquatedValue2;

		public DummySixteenByteVal(float alpha, float bravo) {
			Alpha = alpha;
			Bravo = bravo;

			// Just to add some nonzero values for testing
			NonEquatedValue1 = Alpha * Bravo;
			NonEquatedValue2 = Bravo - Alpha;
		}

		public DummySixteenByteVal(float alpha, float nonEquatedValue1, float bravo, float nonEquatedValue2) {
			Alpha = alpha;
			NonEquatedValue1 = nonEquatedValue1;
			Bravo = bravo;
			NonEquatedValue2 = nonEquatedValue2;
		}

		public bool Equals(DummySixteenByteVal other) {
			return Alpha.Equals(other.Alpha) && Bravo.Equals(other.Bravo);
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			return obj is DummySixteenByteVal other && Equals(other);
		}

		public override int GetHashCode() {
			unchecked {
				return (Alpha.GetHashCode() * 397) ^ Bravo.GetHashCode();
			}
		}

		public static bool operator ==(DummySixteenByteVal left, DummySixteenByteVal right) { return left.Equals(right); }
		public static bool operator !=(DummySixteenByteVal left, DummySixteenByteVal right) { return !left.Equals(right); }
	}
}