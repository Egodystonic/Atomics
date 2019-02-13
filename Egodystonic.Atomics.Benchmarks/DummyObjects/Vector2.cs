// (c) Egodystonic Studios 2019

using System;
using System.Runtime.InteropServices;

namespace Egodystonic.Atomics.Benchmarks.DummyObjects {
	[StructLayout(LayoutKind.Explicit)]
	struct Vector2 : IEquatable<Vector2> {
		[FieldOffset(0)]
		public readonly float X;
		[FieldOffset(sizeof(float))]
		public readonly float Y;

		[FieldOffset(0)]
		public long L;

		public Vector2(float x, float y) {
			L = 0L;
			X = x;
			Y = y;
		}

		public Vector2(long l) {
			X = 0f;
			Y = 0f;
			L = l;
		}

		public bool Equals(Vector2 other) {
			return L == other.L;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			return obj is Vector2 other && Equals(other);
		}

		public override int GetHashCode() {
			return L.GetHashCode();
		}

		public static bool operator ==(Vector2 left, Vector2 right) { return left.Equals(right); }
		public static bool operator !=(Vector2 left, Vector2 right) { return !left.Equals(right); }
	}
}