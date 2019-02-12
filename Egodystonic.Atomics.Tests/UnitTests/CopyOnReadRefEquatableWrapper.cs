// (c) Egodystonic Studios 2018

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;

namespace Egodystonic.Atomics.Tests.UnitTests {
	sealed class CopyOnReadRefEquatableWrapper : IAtomic<DummyEquatableRef> {
		readonly CopyOnReadRef<DummyEquatableRef> _ref = new CopyOnReadRef<DummyEquatableRef>(Copy);

		public DummyEquatableRef Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _ref.Value;
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => _ref.Value = value;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DummyEquatableRef Get() => _ref.Get();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DummyEquatableRef GetUnsafe() => _ref.GetUnsafe();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(DummyEquatableRef CurrentValue) => _ref.Set(CurrentValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(DummyEquatableRef CurrentValue) => _ref.SetUnsafe(CurrentValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyEquatableRef PreviousValue, DummyEquatableRef CurrentValue) Exchange(DummyEquatableRef CurrentValue) => _ref.Exchange(CurrentValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, DummyEquatableRef PreviousValue, DummyEquatableRef CurrentValue) TryExchange(DummyEquatableRef CurrentValue, DummyEquatableRef comparand) => _ref.TryExchange(CurrentValue, comparand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DummyEquatableRef SpinWaitForValue(DummyEquatableRef targetValue) => _ref.SpinWaitForValue(targetValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyEquatableRef PreviousValue, DummyEquatableRef CurrentValue) Exchange<TContext>(Func<DummyEquatableRef, TContext, DummyEquatableRef> mapFunc, TContext context) {
			return _ref.Exchange(mapFunc, context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyEquatableRef PreviousValue, DummyEquatableRef CurrentValue) SpinWaitForExchange(DummyEquatableRef CurrentValue, DummyEquatableRef comparand) {
			return _ref.SpinWaitForExchange(CurrentValue, comparand);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyEquatableRef PreviousValue, DummyEquatableRef CurrentValue) SpinWaitForExchange<TContext>(Func<DummyEquatableRef, TContext, DummyEquatableRef> mapFunc, TContext context, DummyEquatableRef comparand) {
			return _ref.SpinWaitForExchange(mapFunc, context, comparand);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyEquatableRef PreviousValue, DummyEquatableRef CurrentValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<DummyEquatableRef, TMapContext, DummyEquatableRef> mapFunc, TMapContext mapContext, Func<DummyEquatableRef, DummyEquatableRef, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
			return _ref.SpinWaitForExchange(mapFunc, mapContext, predicate, predicateContext);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, DummyEquatableRef PreviousValue, DummyEquatableRef CurrentValue) TryExchange<TContext>(Func<DummyEquatableRef, TContext, DummyEquatableRef> mapFunc, TContext context, DummyEquatableRef comparand) {
			return _ref.TryExchange(mapFunc, context, comparand);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, DummyEquatableRef PreviousValue, DummyEquatableRef CurrentValue) TryExchange<TMapContext, TPredicateContext>(Func<DummyEquatableRef, TMapContext, DummyEquatableRef> mapFunc, TMapContext mapContext, Func<DummyEquatableRef, DummyEquatableRef, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
			return _ref.TryExchange(mapFunc, mapContext, predicate, predicateContext);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DummyEquatableRef FastExchange(DummyEquatableRef newValue) { return _ref.FastExchange(newValue); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DummyEquatableRef FastTryExchangeRefOnly(DummyEquatableRef newValue, DummyEquatableRef comparand) { return _ref.FastTryExchangeRefOnly(newValue, comparand); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DummyEquatableRef FastTryExchange(DummyEquatableRef newValue, DummyEquatableRef comparand) { return _ref.FastTryExchange(newValue, comparand); }

		static DummyEquatableRef Copy(DummyEquatableRef r) => r == null ? null : new DummyEquatableRef(r.StringProp, r.LongProp, Copy(r.RefProp));
	}
}