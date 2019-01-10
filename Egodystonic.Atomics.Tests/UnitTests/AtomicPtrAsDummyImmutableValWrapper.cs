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
		public void Set(DummyImmutableVal newValue) => _ptr.Set((IntPtr) newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(DummyImmutableVal newValue) => _ptr.SetUnsafe((IntPtr) newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyImmutableVal PreviousValue, DummyImmutableVal NewValue) Exchange(DummyImmutableVal newValue) => Cast(_castPtr.Exchange((IntPtr) newValue));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, DummyImmutableVal PreviousValue, DummyImmutableVal NewValue) TryExchange(DummyImmutableVal newValue, DummyImmutableVal comparand) => Cast(_castPtr.TryExchange(newValue, (IntPtr) comparand));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DummyImmutableVal SpinWaitForValue(DummyImmutableVal targetValue) => _castPtr.SpinWaitForValue((IntPtr) targetValue);

		public (DummyImmutableVal PreviousValue, DummyImmutableVal NewValue) Exchange<TContext>(Func<DummyImmutableVal, TContext, DummyImmutableVal> mapFunc, TContext context) {
			return Cast(_castPtr.Exchange((cur, ctx) => (IntPtr) mapFunc(cur, ctx), context));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyImmutableVal PreviousValue, DummyImmutableVal NewValue) SpinWaitForExchange(DummyImmutableVal newValue, DummyImmutableVal comparand) {
			return Cast(_castPtr.SpinWaitForExchange(newValue, (IntPtr) comparand));
		}

		public (DummyImmutableVal PreviousValue, DummyImmutableVal NewValue) SpinWaitForExchange<TContext>(Func<DummyImmutableVal, TContext, DummyImmutableVal> mapFunc, DummyImmutableVal comparand, TContext context) {
			return Cast(_castPtr.SpinWaitForExchange((cur, ctx) => (IntPtr) mapFunc(cur, ctx), comparand, context));
		}

		public (DummyImmutableVal PreviousValue, DummyImmutableVal NewValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<DummyImmutableVal, TMapContext, DummyImmutableVal> mapFunc, Func<DummyImmutableVal, DummyImmutableVal, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
			return Cast(_castPtr.SpinWaitForExchange((cur, ctx) => (IntPtr) mapFunc(cur, ctx), (c, n, ctx) => predicate(c, n, ctx), mapContext, predicateContext));
		}

		public (bool ValueWasSet, DummyImmutableVal PreviousValue, DummyImmutableVal NewValue) TryExchange<TContext>(Func<DummyImmutableVal, TContext, DummyImmutableVal> mapFunc, DummyImmutableVal comparand, TContext context) {
			return Cast(_castPtr.TryExchange((cur, ctx) => (IntPtr) mapFunc(cur, ctx), comparand, context));
		}

		public (bool ValueWasSet, DummyImmutableVal PreviousValue, DummyImmutableVal NewValue) TryExchange<TMapContext, TPredicateContext>(Func<DummyImmutableVal, TMapContext, DummyImmutableVal> mapFunc, Func<DummyImmutableVal, DummyImmutableVal, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
			return Cast(_castPtr.TryExchange((cur, ctx) => (IntPtr) mapFunc(cur, ctx), (c, n, ctx) => predicate(c, n, ctx), mapContext, predicateContext));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static (DummyImmutableVal, DummyImmutableVal) Cast((IntPtr, IntPtr) operand) => (operand.Item1, operand.Item2);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static (bool, DummyImmutableVal, DummyImmutableVal) Cast((bool, IntPtr, IntPtr) operand) => (operand.Item1, operand.Item2, operand.Item3);
	}
}