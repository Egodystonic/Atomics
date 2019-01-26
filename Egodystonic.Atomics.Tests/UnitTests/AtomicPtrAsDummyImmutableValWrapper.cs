// (c) Egodystonic Studios 2018

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;

namespace Egodystonic.Atomics.Tests.UnitTests {
	sealed unsafe class AtomicPtrAsDummyImmutableValWrapper : IAtomic<DummyImmutableVal> {
		readonly AtomicPtr<int> _ptr = new AtomicPtr<int>();
		readonly IAtomic<IntPtr> _castPtr;

		public AtomicPtrAsDummyImmutableValWrapper() {
			_castPtr = _ptr;
		}

		public DummyImmutableVal Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _ptr.Value;
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => _ptr.Value = value;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DummyImmutableVal Get() => _ptr.Get();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DummyImmutableVal GetUnsafe() => _ptr.GetUnsafe();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(DummyImmutableVal CurrentValue) => _ptr.Set((IntPtr) CurrentValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(DummyImmutableVal CurrentValue) => _ptr.SetUnsafe((IntPtr) CurrentValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyImmutableVal PreviousValue, DummyImmutableVal CurrentValue) Exchange(DummyImmutableVal CurrentValue) => Cast(_castPtr.Exchange((IntPtr) CurrentValue));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, DummyImmutableVal PreviousValue, DummyImmutableVal CurrentValue) TryExchange(DummyImmutableVal CurrentValue, DummyImmutableVal comparand) => Cast(_castPtr.TryExchange(CurrentValue, (IntPtr) comparand));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DummyImmutableVal SpinWaitForValue(DummyImmutableVal targetValue) => _castPtr.SpinWaitForValue((IntPtr) targetValue);

		public (DummyImmutableVal PreviousValue, DummyImmutableVal CurrentValue) Exchange<TContext>(Func<DummyImmutableVal, TContext, DummyImmutableVal> mapFunc, TContext context) {
			return Cast(_castPtr.Exchange((cur, ctx) => (IntPtr) mapFunc(cur, ctx), context));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyImmutableVal PreviousValue, DummyImmutableVal CurrentValue) SpinWaitForExchange(DummyImmutableVal CurrentValue, DummyImmutableVal comparand) {
			return Cast(_castPtr.SpinWaitForExchange(CurrentValue, (IntPtr) comparand));
		}

		public (DummyImmutableVal PreviousValue, DummyImmutableVal CurrentValue) SpinWaitForExchange<TContext>(Func<DummyImmutableVal, TContext, DummyImmutableVal> mapFunc, TContext context, DummyImmutableVal comparand) {
			return Cast(_castPtr.SpinWaitForExchange((cur, ctx) => (IntPtr) mapFunc(cur, ctx), context, comparand));
		}

		public (DummyImmutableVal PreviousValue, DummyImmutableVal CurrentValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<DummyImmutableVal, TMapContext, DummyImmutableVal> mapFunc, TMapContext mapContext, Func<DummyImmutableVal, DummyImmutableVal, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
			return Cast(_castPtr.SpinWaitForExchange((cur, ctx) => (IntPtr) mapFunc(cur, ctx), mapContext, (c, n, ctx) => predicate(c, n, ctx), predicateContext));
		}

		public (bool ValueWasSet, DummyImmutableVal PreviousValue, DummyImmutableVal CurrentValue) TryExchange<TContext>(Func<DummyImmutableVal, TContext, DummyImmutableVal> mapFunc, TContext context, DummyImmutableVal comparand) {
			return Cast(_castPtr.TryExchange((cur, ctx) => (IntPtr) mapFunc(cur, ctx), context, comparand));
		}

		public (bool ValueWasSet, DummyImmutableVal PreviousValue, DummyImmutableVal CurrentValue) TryExchange<TMapContext, TPredicateContext>(Func<DummyImmutableVal, TMapContext, DummyImmutableVal> mapFunc, TMapContext mapContext, Func<DummyImmutableVal, DummyImmutableVal, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
			return Cast(_castPtr.TryExchange((cur, ctx) => (IntPtr) mapFunc(cur, ctx), mapContext, (c, n, ctx) => predicate(c, n, ctx), predicateContext));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static (DummyImmutableVal, DummyImmutableVal) Cast((IntPtr, IntPtr) operand) => (operand.Item1, operand.Item2);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static (bool, DummyImmutableVal, DummyImmutableVal) Cast((bool, IntPtr, IntPtr) operand) => (operand.Item1, operand.Item2, operand.Item3);
	}
}