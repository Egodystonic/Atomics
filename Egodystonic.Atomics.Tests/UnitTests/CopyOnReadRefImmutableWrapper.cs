// (c) Egodystonic Studios 2018

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;

namespace Egodystonic.Atomics.Tests.UnitTests {
	sealed class CopyOnReadRefImmutableWrapper : IAtomic<DummyImmutableRef> {
		readonly CopyOnReadRef<DummyImmutableRef> _ref = new CopyOnReadRef<DummyImmutableRef>(Copy);

		public DummyImmutableRef Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _ref.Value;
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => _ref.Value = value;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DummyImmutableRef Get() => _ref.Get();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DummyImmutableRef GetUnsafe() => _ref.GetUnsafe();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(DummyImmutableRef CurrentValue) => _ref.Set(CurrentValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(DummyImmutableRef CurrentValue) => _ref.SetUnsafe(CurrentValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyImmutableRef PreviousValue, DummyImmutableRef CurrentValue) Exchange(DummyImmutableRef CurrentValue) => _ref.Exchange(CurrentValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, DummyImmutableRef PreviousValue, DummyImmutableRef CurrentValue) TryExchange(DummyImmutableRef CurrentValue, DummyImmutableRef comparand) => _ref.TryExchange(CurrentValue, comparand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DummyImmutableRef SpinWaitForValue(DummyImmutableRef targetValue) => _ref.SpinWaitForValue(targetValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyImmutableRef PreviousValue, DummyImmutableRef CurrentValue) Exchange<TContext>(Func<DummyImmutableRef, TContext, DummyImmutableRef> mapFunc, TContext context) {
			return _ref.Exchange(mapFunc, context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyImmutableRef PreviousValue, DummyImmutableRef CurrentValue) SpinWaitForExchange(DummyImmutableRef CurrentValue, DummyImmutableRef comparand) {
			return _ref.SpinWaitForExchange(CurrentValue, comparand);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyImmutableRef PreviousValue, DummyImmutableRef CurrentValue) SpinWaitForExchange<TContext>(Func<DummyImmutableRef, TContext, DummyImmutableRef> mapFunc, TContext context, DummyImmutableRef comparand) {
			return _ref.SpinWaitForExchange(mapFunc, context, comparand);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyImmutableRef PreviousValue, DummyImmutableRef CurrentValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<DummyImmutableRef, TMapContext, DummyImmutableRef> mapFunc, TMapContext mapContext, Func<DummyImmutableRef, DummyImmutableRef, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
			return _ref.SpinWaitForExchange(mapFunc, mapContext, predicate, predicateContext);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, DummyImmutableRef PreviousValue, DummyImmutableRef CurrentValue) TryExchange<TContext>(Func<DummyImmutableRef, TContext, DummyImmutableRef> mapFunc, TContext context, DummyImmutableRef comparand) {
			return _ref.TryExchange(mapFunc, context, comparand);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, DummyImmutableRef PreviousValue, DummyImmutableRef CurrentValue) TryExchange<TMapContext, TPredicateContext>(Func<DummyImmutableRef, TMapContext, DummyImmutableRef> mapFunc, TMapContext mapContext, Func<DummyImmutableRef, DummyImmutableRef, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
			return _ref.TryExchange(mapFunc, mapContext, predicate, predicateContext);
		}

		static DummyImmutableRef Copy(DummyImmutableRef r) => r == null ? null : new DummyImmutableRef(r.StringProp, r.LongProp, Copy(r.RefProp));
	}
}