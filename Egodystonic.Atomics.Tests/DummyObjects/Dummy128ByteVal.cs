// (c) Egodystonic Studios 2018


using System;
using System.Runtime.InteropServices;

namespace Egodystonic.Atomics.Tests.DummyObjects {
	[StructLayout(LayoutKind.Explicit, Size = 16)]
	unsafe struct Dummy128ByteVal : IEquatable<Dummy128ByteVal> {
		[FieldOffset(0)]
		public readonly int Alpha;
		[FieldOffset(4)]
		public fixed byte BufferA[60];
		[FieldOffset(64)]
		public readonly int Bravo;
		[FieldOffset(68)]
		public fixed byte BufferB[60];

		public Dummy128ByteVal(int alpha, int bravo) {
			Alpha = alpha;
			Bravo = bravo;
		}

		public bool Equals(Dummy128ByteVal other) {
			return Alpha.Equals(other.Alpha) && Bravo.Equals(other.Bravo);
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			return obj is Dummy128ByteVal other && Equals(other);
		}

		public override int GetHashCode() {
			unchecked {
				return (Alpha.GetHashCode() * 397) ^ Bravo.GetHashCode();
			}
		}

		public override string ToString() => $"{Alpha}, {Bravo}";

		public static bool operator ==(Dummy128ByteVal left, Dummy128ByteVal right) { return left.Equals(right); }
		public static bool operator !=(Dummy128ByteVal left, Dummy128ByteVal right) { return !left.Equals(right); }
	}
}