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
		public (long PreviousValue, long NewValue) Exchange(long newValue) => Cast(_ptr.Exchange((IntPtr) newValue));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, long PreviousValue, long NewValue) TryExchange(long newValue, long comparand) => Cast(_ptr.TryExchange((IntPtr) newValue, (IntPtr) comparand));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long SpinWaitForValue(long targetValue) => (long) _ptr.SpinWaitForValue((IntPtr) targetValue);

		public (long PreviousValue, long NewValue) Exchange<TContext>(Func<long, TContext, long> mapFunc, TContext context) {
			return Cast(_ptr.Exchange((cur, ctx) => (IntPtr) mapFunc((long) cur, ctx), context));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long PreviousValue, long NewValue) SpinWaitForExchange(long newValue, long comparand) {
			return Cast(_ptr.SpinWaitForExchange((IntPtr) newValue, (IntPtr) comparand));
		}

		public (long PreviousValue, long NewValue) SpinWaitForExchange<TContext>(Func<long, TContext, long> mapFunc, long comparand, TContext context) {
			return Cast(_ptr.SpinWaitForExchange((cur, ctx) => (IntPtr) mapFunc((long) cur, ctx), (IntPtr) comparand, context));
		}

		public (long PreviousValue, long NewValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<long, TMapContext, long> mapFunc, Func<long, long, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
			return Cast(_ptr.SpinWaitForExchange((cur, ctx) => (IntPtr) mapFunc((long) cur, ctx), (c, n, ctx) => predicate((long) c, (long) n, ctx), mapContext, predicateContext));
		}

		public (bool ValueWasSet, long PreviousValue, long NewValue) TryExchange<TContext>(Func<long, TContext, long> mapFunc, long comparand, TContext context) {
			return Cast(_ptr.TryExchange((cur, ctx) => (IntPtr) mapFunc((long) cur, ctx), (IntPtr) comparand, context));
		}

		public (bool ValueWasSet, long PreviousValue, long NewValue) TryExchange<TMapContext, TPredicateContext>(Func<long, TMapContext, long> mapFunc, Func<long, long, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
			return Cast(_ptr.TryExchange((cur, ctx) => (IntPtr) mapFunc((long) cur, ctx), (c, n, ctx) => predicate((long) c, (long) n, ctx), mapContext, predicateContext));
		}

		public long SpinWaitForBoundedValue(long lowerBound, long upperBound) {
			return (long) _ptr.SpinWaitForBoundedValue((IntPtr) lowerBound, (IntPtr) upperBound);
		}

		public (long PreviousValue, long NewValue) SpinWaitForBoundedExchange(long newValue, long lowerBound, long upperBound) {
			return Cast(_ptr.SpinWaitForBoundedExchange((IntPtr) newValue, (IntPtr) lowerBound, (IntPtr) upperBound));
		}

		public (long PreviousValue, long NewValue) SpinWaitForBoundedExchange(Func<long, long> mapFunc, long lowerBound, long upperBound) {
			return Cast(_ptr.SpinWaitForBoundedExchange(cur => (IntPtr) mapFunc((long) cur), (IntPtr) lowerBound, (IntPtr) upperBound));
		}

		public (long PreviousValue, long NewValue) SpinWaitForBoundedExchange<TContext>(Func<long, TContext, long> mapFunc, long lowerBound, long upperBound, TContext context) {
			return Cast(_ptr.SpinWaitForBoundedExchange((cur, ctx) => (IntPtr) mapFunc((long) cur, ctx), (IntPtr) lowerBound, (IntPtr) upperBound, context));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long PreviousValue, long NewValue) Increment() => Cast(_ptr.Increment());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long PreviousValue, long NewValue) Decrement() => Cast(_ptr.Decrement());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long PreviousValue, long NewValue) Add(long operand) => Cast(_ptr.Add((IntPtr) operand));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long PreviousValue, long NewValue) Subtract(long operand) => Cast(_ptr.Subtract((IntPtr) operand));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long PreviousValue, long NewValue) MultiplyBy(long operand) => Cast(_ptr.MultiplyBy((IntPtr) operand));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (long PreviousValue, long NewValue) DivideBy(long operand) => Cast(_ptr.DivideBy((IntPtr) operand));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static (long, long) Cast((IntPtr, IntPtr) operand) => ((long) operand.Item1, (long) operand.Item2);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static (bool, long, long) Cast((bool, IntPtr, IntPtr) operand) => (operand.Item1, (long) operand.Item2, (long) operand.Item3);
	}
}