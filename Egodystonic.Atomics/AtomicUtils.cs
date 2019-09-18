// (c) Egodystonic Studios 2019
// Author: Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Egodystonic.Atomics {
	static class AtomicUtils {
		public const string NullValueString = "<null>";

		[StructLayout(LayoutKind.Explicit)]
		public struct Union<T1, T2> where T1 : unmanaged where T2 : unmanaged {
			[FieldOffset(0)]
			public T1 AsTypeOne;

			[FieldOffset(0)]
			public T2 AsTypeTwo;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Union(T1 asTypeOne) : this() => AsTypeOne = asTypeOne;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Union(T2 asTypeTwo) : this() => AsTypeTwo = asTypeTwo;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static implicit operator T1(Union<T1, T2> operand) => operand.AsTypeOne;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static implicit operator T2(Union<T1, T2> operand) => operand.AsTypeTwo;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static implicit operator Union<T1, T2>(T1 operand) => new Union<T1, T2>(operand);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static implicit operator Union<T1, T2>(T2 operand) => new Union<T1, T2>(operand);
		}
	}
}