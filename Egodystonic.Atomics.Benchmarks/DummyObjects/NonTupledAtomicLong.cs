// ReSharper disable CompareOfFloatsByEqualityOperator Direct comparison is correct behaviour here; we're using as a bitwise equality check, not interpreting sameness/value
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Egodystonic.Atomics.Numerics {
	public sealed class NonTupledAtomicLong {
		long _value;

		public NonTupledAtomicLong(long initialValue) => Set(initialValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Get() => Interlocked.Read(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long GetUnsafe() => _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(long newValue) => Interlocked.Exchange(ref _value, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(long newValue) => _value = newValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Exchange(long newValue) => Interlocked.Exchange(ref _value, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryExchange(long newValue, long comparand) {
			var oldValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			var wasSet = oldValue == comparand;
			return wasSet;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Increment() {
			return Interlocked.Increment(ref _value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Decrement() {
			return Interlocked.Decrement(ref _value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator long(NonTupledAtomicLong operand) => operand.Get();
	}
}