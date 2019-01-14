// (c) Egodystonic Studios 2018


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Egodystonic.Atomics.Tests.DummyObjects {
	[StructLayout(LayoutKind.Explicit, Size = 16)]
	struct DummySixteenByteVal : IEquatable<DummySixteenByteVal> {
		[FieldOffset(0)]
		public readonly int Alpha;
		[FieldOffset(4)]
		public readonly int NonEquatedValue1;
		[FieldOffset(8)]
		public readonly int NonEquatedValue2;
		[FieldOffset(12)]
		public readonly int Bravo;

		public DummySixteenByteVal(int alpha, int bravo) {
			Alpha = alpha;
			Bravo = bravo;

			// Just to add some nonzero values for testing
			NonEquatedValue1 = Alpha * Bravo;
			NonEquatedValue2 = Bravo - Alpha;
		}

		public DummySixteenByteVal(int alpha, int nonEquatedValue1, int bravo, int nonEquatedValue2) {
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

		public override string ToString() => $"{Alpha}, {Bravo}, ({NonEquatedValue1}, {NonEquatedValue2})";

		public static bool operator ==(DummySixteenByteVal left, DummySixteenByteVal right) { return left.Equals(right); }
		public static bool operator !=(DummySixteenByteVal left, DummySixteenByteVal right) { return !left.Equals(right); }
	}
}