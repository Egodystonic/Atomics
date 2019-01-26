// (c) Egodystonic Studios 2018

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;

namespace Egodystonic.Atomics.Tests.UnitTests {
	sealed class AtomicEnumAsDummyImmutableValWrapper : IAtomic<DummyImmutableVal> {
		readonly AtomicEnumVal<DummyEnum> _enum = new AtomicEnumVal<DummyEnum>();

		public DummyImmutableVal Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _enum.Value;
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => _enum.Value = value;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DummyImmutableVal Get() => _enum.Get();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DummyImmutableVal GetUnsafe() => _enum.GetUnsafe();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(DummyImmutableVal CurrentValue) => _enum.Set((DummyEnum) CurrentValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(DummyImmutableVal CurrentValue) => _enum.SetUnsafe((DummyEnum) CurrentValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyImmutableVal PreviousValue, DummyImmutableVal CurrentValue) Exchange(DummyImmutableVal CurrentValue) => Cast(_enum.Exchange((DummyEnum) CurrentValue));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, DummyImmutableVal PreviousValue, DummyImmutableVal CurrentValue) TryExchange(DummyImmutableVal CurrentValue, DummyImmutableVal comparand) => Cast(_enum.TryExchange(CurrentValue, (DummyEnum) comparand));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DummyImmutableVal SpinWaitForValue(DummyImmutableVal targetValue) => _enum.SpinWaitForValue((DummyEnum) targetValue);

		public (DummyImmutableVal PreviousValue, DummyImmutableVal CurrentValue) Exchange<TContext>(Func<DummyImmutableVal, TContext, DummyImmutableVal> mapFunc, TContext context) {
			return Cast(_enum.Exchange((cur, ctx) => (DummyEnum) mapFunc(cur, ctx), context));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyImmutableVal PreviousValue, DummyImmutableVal CurrentValue) SpinWaitForExchange(DummyImmutableVal CurrentValue, DummyImmutableVal comparand) {
			return Cast(_enum.SpinWaitForExchange(CurrentValue, (DummyEnum) comparand));
		}

		public (DummyImmutableVal PreviousValue, DummyImmutableVal CurrentValue) SpinWaitForExchange<TContext>(Func<DummyImmutableVal, TContext, DummyImmutableVal> mapFunc, TContext context, DummyImmutableVal comparand) {
			return Cast(_enum.SpinWaitForExchange((cur, ctx) => (DummyEnum) mapFunc(cur, ctx), context, comparand));
		}

		public (DummyImmutableVal PreviousValue, DummyImmutableVal CurrentValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<DummyImmutableVal, TMapContext, DummyImmutableVal> mapFunc, TMapContext mapContext, Func<DummyImmutableVal, DummyImmutableVal, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
			return Cast(_enum.SpinWaitForExchange((cur, ctx) => (DummyEnum) mapFunc(cur, ctx), mapContext, (c, n, ctx) => predicate(c, n, ctx), predicateContext));
		}

		public (bool ValueWasSet, DummyImmutableVal PreviousValue, DummyImmutableVal CurrentValue) TryExchange<TContext>(Func<DummyImmutableVal, TContext, DummyImmutableVal> mapFunc, TContext context, DummyImmutableVal comparand) {
			return Cast(_enum.TryExchange((cur, ctx) => (DummyEnum) mapFunc(cur, ctx), context, comparand));
		}

		public (bool ValueWasSet, DummyImmutableVal PreviousValue, DummyImmutableVal CurrentValue) TryExchange<TMapContext, TPredicateContext>(Func<DummyImmutableVal, TMapContext, DummyImmutableVal> mapFunc, TMapContext mapContext, Func<DummyImmutableVal, DummyImmutableVal, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
			return Cast(_enum.TryExchange((cur, ctx) => (DummyEnum) mapFunc(cur, ctx), mapContext, (c, n, ctx) => predicate(c, n, ctx), predicateContext));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static (DummyImmutableVal, DummyImmutableVal) Cast((DummyEnum, DummyEnum) operand) => (operand.Item1, operand.Item2);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static (bool, DummyImmutableVal, DummyImmutableVal) Cast((bool, DummyEnum, DummyEnum) operand) => (operand.Item1, operand.Item2, operand.Item3);
	}
}