// (c) Egodystonic Studios 2019
// Author: Ben Bowen

#pragma warning disable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Egodystonic.Atomics.Tests.Examples {
	struct Matrix4x4 : IEquatable<Matrix4x4> {
		float _a0, _a1, _a2, _a3;
		float _b0, _b1, _b2, _b3;
		float _c0, _c1, _c2, _c3;
		float _d0, _d1, _d2, _d3;

		public Matrix4x4 Transpose() => new Matrix4x4();

		public bool IsIdentity { get; }

		public bool Equals(Matrix4x4 other) {
			return _a0.Equals(other._a0);
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			return obj is Matrix4x4 other && Equals(other);
		}

		public override int GetHashCode() {
			return _a0.GetHashCode();
		}

		public static bool operator ==(Matrix4x4 left, Matrix4x4 right) { return left.Equals(right); }
		public static bool operator !=(Matrix4x4 left, Matrix4x4 right) { return !left.Equals(right); }

		public static Matrix4x4 operator *(Matrix4x4 l, Matrix4x4 r) => new Matrix4x4();
	}
}

#pragma warning enable