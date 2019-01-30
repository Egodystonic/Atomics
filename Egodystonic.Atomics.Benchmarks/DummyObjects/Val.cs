// (c) Egodystonic Studios 2019

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Egodystonic.Atomics.Benchmarks.DummyObjects {
	[StructLayout(LayoutKind.Sequential, Size = 8)]
	readonly struct Val8 : IEquatable<Val8> {
		public readonly long A;

		public Val8(long a) { A = a; }

		public bool Equals(Val8 other) {
			return A == other.A;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			return obj is Val8 other && Equals(other);
		}

		public override int GetHashCode() {
			return A.GetHashCode();
		}

		public static bool operator ==(Val8 left, Val8 right) { return left.Equals(right); }
		public static bool operator !=(Val8 left, Val8 right) { return !left.Equals(right); }
	}

	[StructLayout(LayoutKind.Sequential, Size = 16)]
	readonly struct Val16 : IEquatable<Val16> {
		public readonly long A, B;

		public Val16(long a) : this() { A = a; }

		public Val16(long a, long b) {
			A = a;
			B = b;
		}

		public bool Equals(Val16 other) {
			return A == other.A && B == other.B;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			return obj is Val16 other && Equals(other);
		}

		public override int GetHashCode() {
			unchecked {
				return (A.GetHashCode() * 397) ^ B.GetHashCode();
			}
		}

		public static bool operator ==(Val16 left, Val16 right) { return left.Equals(right); }
		public static bool operator !=(Val16 left, Val16 right) { return !left.Equals(right); }
	}

	[StructLayout(LayoutKind.Sequential, Size = 32)]
	readonly struct Val32 : IEquatable<Val32> {
		public readonly long A, B, C, D;

		public Val32(long a) : this() { A = a; }

		public Val32(long a, long b, long c, long d) {
			A = a;
			B = b;
			C = c;
			D = d;
		}

		public bool Equals(Val32 other) {
			return A == other.A && B == other.B && C == other.C && D == other.D;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			return obj is Val32 other && Equals(other);
		}

		public override int GetHashCode() {
			unchecked {
				var hashCode = A.GetHashCode();
				hashCode = (hashCode * 397) ^ B.GetHashCode();
				hashCode = (hashCode * 397) ^ C.GetHashCode();
				hashCode = (hashCode * 397) ^ D.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(Val32 left, Val32 right) { return left.Equals(right); }
		public static bool operator !=(Val32 left, Val32 right) { return !left.Equals(right); }
	}

	[StructLayout(LayoutKind.Sequential, Size = 64)]
	readonly struct Val64 : IEquatable<Val64> {
		public readonly long A, B, C, D, E, F, G, H;

		public Val64(long a) : this() { A = a; }

		public Val64(long a, long b, long c, long d, long e, long f, long g, long h) {
			A = a;
			B = b;
			C = c;
			D = d;
			E = e;
			F = f;
			G = g;
			H = h;
		}

		public bool Equals(Val64 other) {
			return A == other.A && B == other.B && C == other.C && D == other.D && E == other.E && F == other.F && G == other.G && H == other.H;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			return obj is Val64 other && Equals(other);
		}

		public override int GetHashCode() {
			unchecked {
				var hashCode = A.GetHashCode();
				hashCode = (hashCode * 397) ^ B.GetHashCode();
				hashCode = (hashCode * 397) ^ C.GetHashCode();
				hashCode = (hashCode * 397) ^ D.GetHashCode();
				hashCode = (hashCode * 397) ^ E.GetHashCode();
				hashCode = (hashCode * 397) ^ F.GetHashCode();
				hashCode = (hashCode * 397) ^ G.GetHashCode();
				hashCode = (hashCode * 397) ^ H.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(Val64 left, Val64 right) { return left.Equals(right); }
		public static bool operator !=(Val64 left, Val64 right) { return !left.Equals(right); }
	}
}