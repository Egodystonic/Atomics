// ReSharper disable CompareOfFloatsByEqualityOperator Direct comparison is correct behaviour here; we're using as a bitwise equality check, not interpreting sameness/value

using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Egodystonic.Atomics.Benchmarks.DummyObjects {
	public sealed class NonTupledAtomicInt {
		int _value;

		public NonTupledAtomicInt(int initialValue) => Set(initialValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Get() => Volatile.Read(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetUnsafe() => _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(int newValue) => Volatile.Write(ref _value, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(int newValue) => _value = newValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Exchange(int newValue) => Interlocked.Exchange(ref _value, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryExchange(int newValue, int comparand) {
			var oldValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			var wasSet = oldValue == comparand;
			return wasSet;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Increment() {
			return Interlocked.Increment(ref _value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Decrement() {
			return Interlocked.Decrement(ref _value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator int(NonTupledAtomicInt operand) => operand.Get();
	}
}