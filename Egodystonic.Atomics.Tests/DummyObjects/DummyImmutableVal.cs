// (c) Egodystonic Studios 2018


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Egodystonic.Atomics.Tests.DummyObjects {
	[StructLayout(LayoutKind.Explicit)]
	struct DummyImmutableVal : IEquatable<DummyImmutableVal> {
		[FieldOffset(sizeof(int) * 0)]
		public readonly int Alpha;

		[FieldOffset(sizeof(int) * 1)]
		public readonly int Bravo;

		[FieldOffset(0)]
		readonly IntPtr _ptr; // Used for AtomicPtrAsDummyImmutableVal type

		public DummyImmutableVal(int alpha, int bravo) : this() {
			Alpha = alpha;
			Bravo = bravo;
		}

		DummyImmutableVal(IntPtr ptr) : this() {
			_ptr = ptr;
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

		public static implicit operator IntPtr(DummyImmutableVal operand) => operand._ptr;
		public static implicit operator DummyImmutableVal(IntPtr operand) => new DummyImmutableVal(operand);
		public static unsafe implicit operator int*(DummyImmutableVal operand) => (int*) operand._ptr;
		public static unsafe implicit operator DummyImmutableVal(int* operand) => new DummyImmutableVal((IntPtr) operand);

		public override string ToString() => $"{Alpha}, {Bravo}";
	}
}