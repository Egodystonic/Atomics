// (c) Egodystonic Studios 2018

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;

namespace Egodystonic.Atomics.Tests.UnitTests {
	sealed class AtomicPtrAsLongWrapper : INumericAtomic<long> {
		readonly INumericAtomic<IntPtr> _ptr = new AtomicPtr<DummyImmutableVal>();

		public long Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => (long) _ptr.Value;
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => _ptr.Value = (IntPtr) value;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Get() => (long) _ptr.Get();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long GetUnsafe() => (long) _ptr.GetUnsafe();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(long newValue) => _ptr.Set((IntPtr) newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(long newValue) => _ptr.SetUnsafe((IntPtr) newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long Exchange(long newValue) => (long) _ptr.Exchange((IntPtr) newValue);

		public (long PreviousValue, long NewValue) Exchange(Func<long, long> mapFunc) {
			var res = _ptr.Exchange(ptr => (IntPtr) mapFunc((long) ptr));
			return ((long) res.PreviousValue, (long) res.NewValue);
		}

		public (bool ValueWasSet, long PreviousValue) TryExchange(long newValue, long comparand) {
			var res = _ptr.TryExchange((IntPtr) newValue, (IntPtr) comparand);
			return (res.ValueWasSet, (long) res.PreviousValue);
		}

		public (bool ValueWasSet, long PreviousValue) TryExchange(long newValue, Func<long, long, bool> predicate) {
			var res = _ptr.TryExchange((IntPtr) newValue, (c, n) => predicate((long) c, (long) n));
			return (res.ValueWasSet, (long) res.PreviousValue);
		}

		public (bool ValueWasSet, long PreviousValue, long NewValue) TryExchange(Func<long, long> mapFunc, long comparand) {
			var res = _ptr.TryExchange(ptr => (IntPtr) mapFunc((long) ptr), (IntPtr) comparand);
			return (res.ValueWasSet, (long) res.PreviousValue, (long) res.NewValue);
		}

		public (bool ValueWasSet, long PreviousValue, long NewValue) TryExchange(Func<long, long> mapFunc, Func<long, long, bool> predicate) {
			var res = _ptr.TryExchange(ptr => (IntPtr) mapFunc((long) ptr), (c, n) => predicate((long) c, (long) n));
			return (res.ValueWasSet, (long) res.PreviousValue, (long) res.NewValue);
		}

		public (long PreviousValue, long NewValue) Increment() {
			var res = _ptr.Increment();
			return ((long) res.PreviousValue, (long) res.NewValue);
		}

		public (long PreviousValue, long NewValue) Decrement() {
			var res = _ptr.Decrement();
			return ((long) res.PreviousValue, (long) res.NewValue);
		}

		public (long PreviousValue, long NewValue) Add(long operand) {
			var res = _ptr.Add((IntPtr) operand);
			return ((long) res.PreviousValue, (long) res.NewValue);
		}

		public (long PreviousValue, long NewValue) Subtract(long operand) {
			var res = _ptr.Subtract((IntPtr) operand);
			return ((long) res.PreviousValue, (long) res.NewValue);
		}

		public (long PreviousValue, long NewValue) MultiplyBy(long operand) {
			var res = _ptr.MultiplyBy((IntPtr) operand);
			return ((long) res.PreviousValue, (long) res.NewValue);
		}

		public (long PreviousValue, long NewValue) DivideBy(long operand) {
			var res = _ptr.DivideBy((IntPtr) operand);
			return ((long) res.PreviousValue, (long) res.NewValue);
		}
	}
}