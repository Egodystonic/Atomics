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
		public void Set(DummyImmutableVal newValue) => _enum.Set((DummyEnum) newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(DummyImmutableVal newValue) => _enum.SetUnsafe((DummyEnum) newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyImmutableVal PreviousValue, DummyImmutableVal NewValue) Exchange(DummyImmutableVal newValue) => Cast(_enum.Exchange((DummyEnum) newValue));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, DummyImmutableVal PreviousValue, DummyImmutableVal NewValue) TryExchange(DummyImmutableVal newValue, DummyImmutableVal comparand) => Cast(_enum.TryExchange(newValue, (DummyEnum) comparand));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DummyImmutableVal SpinWaitForValue(DummyImmutableVal targetValue) => _enum.SpinWaitForValue((DummyEnum) targetValue);

		public (DummyImmutableVal PreviousValue, DummyImmutableVal NewValue) Exchange<TContext>(Func<DummyImmutableVal, TContext, DummyImmutableVal> mapFunc, TContext context) {
			return Cast(_enum.Exchange((cur, ctx) => (DummyEnum) mapFunc(cur, ctx), context));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyImmutableVal PreviousValue, DummyImmutableVal NewValue) SpinWaitForExchange(DummyImmutableVal newValue, DummyImmutableVal comparand) {
			return Cast(_enum.SpinWaitForExchange(newValue, (DummyEnum) comparand));
		}

		public (DummyImmutableVal PreviousValue, DummyImmutableVal NewValue) SpinWaitForExchange<TContext>(Func<DummyImmutableVal, TContext, DummyImmutableVal> mapFunc, DummyImmutableVal comparand, TContext context) {
			return Cast(_enum.SpinWaitForExchange((cur, ctx) => (DummyEnum) mapFunc(cur, ctx), comparand, context));
		}

		public (DummyImmutableVal PreviousValue, DummyImmutableVal NewValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<DummyImmutableVal, TMapContext, DummyImmutableVal> mapFunc, Func<DummyImmutableVal, DummyImmutableVal, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
			return Cast(_enum.SpinWaitForExchange((cur, ctx) => (DummyEnum) mapFunc(cur, ctx), (c, n, ctx) => predicate(c, n, ctx), mapContext, predicateContext));
		}

		public (bool ValueWasSet, DummyImmutableVal PreviousValue, DummyImmutableVal NewValue) TryExchange<TContext>(Func<DummyImmutableVal, TContext, DummyImmutableVal> mapFunc, DummyImmutableVal comparand, TContext context) {
			return Cast(_enum.TryExchange((cur, ctx) => (DummyEnum) mapFunc(cur, ctx), comparand, context));
		}

		public (bool ValueWasSet, DummyImmutableVal PreviousValue, DummyImmutableVal NewValue) TryExchange<TMapContext, TPredicateContext>(Func<DummyImmutableVal, TMapContext, DummyImmutableVal> mapFunc, Func<DummyImmutableVal, DummyImmutableVal, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
			return Cast(_enum.TryExchange((cur, ctx) => (DummyEnum) mapFunc(cur, ctx), (c, n, ctx) => predicate(c, n, ctx), mapContext, predicateContext));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static (DummyImmutableVal, DummyImmutableVal) Cast((DummyEnum, DummyEnum) operand) => (operand.Item1, operand.Item2);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static (bool, DummyImmutableVal, DummyImmutableVal) Cast((bool, DummyEnum, DummyEnum) operand) => (operand.Item1, operand.Item2, operand.Item3);
	}
}