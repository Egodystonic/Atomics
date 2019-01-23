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
		public void Set(DummyImmutableRef newValue) => _ref.Set(newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(DummyImmutableRef newValue) => _ref.SetUnsafe(newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyImmutableRef PreviousValue, DummyImmutableRef NewValue) Exchange(DummyImmutableRef newValue) => _ref.Exchange(newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, DummyImmutableRef PreviousValue, DummyImmutableRef NewValue) TryExchange(DummyImmutableRef newValue, DummyImmutableRef comparand) => _ref.TryExchange(newValue, comparand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DummyImmutableRef SpinWaitForValue(DummyImmutableRef targetValue) => _ref.SpinWaitForValue(targetValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyImmutableRef PreviousValue, DummyImmutableRef NewValue) Exchange<TContext>(Func<DummyImmutableRef, TContext, DummyImmutableRef> mapFunc, TContext context) {
			return _ref.Exchange(mapFunc, context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyImmutableRef PreviousValue, DummyImmutableRef NewValue) SpinWaitForExchange(DummyImmutableRef newValue, DummyImmutableRef comparand) {
			return _ref.SpinWaitForExchange(newValue, comparand);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyImmutableRef PreviousValue, DummyImmutableRef NewValue) SpinWaitForExchange<TContext>(Func<DummyImmutableRef, TContext, DummyImmutableRef> mapFunc, DummyImmutableRef comparand, TContext context) {
			return _ref.SpinWaitForExchange(mapFunc, comparand, context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyImmutableRef PreviousValue, DummyImmutableRef NewValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<DummyImmutableRef, TMapContext, DummyImmutableRef> mapFunc, Func<DummyImmutableRef, DummyImmutableRef, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
			return _ref.SpinWaitForExchange(mapFunc, predicate, mapContext, predicateContext);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, DummyImmutableRef PreviousValue, DummyImmutableRef NewValue) TryExchange<TContext>(Func<DummyImmutableRef, TContext, DummyImmutableRef> mapFunc, DummyImmutableRef comparand, TContext context) {
			return _ref.TryExchange(mapFunc, comparand, context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, DummyImmutableRef PreviousValue, DummyImmutableRef NewValue) TryExchange<TMapContext, TPredicateContext>(Func<DummyImmutableRef, TMapContext, DummyImmutableRef> mapFunc, Func<DummyImmutableRef, DummyImmutableRef, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
			return _ref.TryExchange(mapFunc, predicate, mapContext, predicateContext);
		}

		static DummyImmutableRef Copy(DummyImmutableRef r) => r == null ? null : new DummyImmutableRef(r.StringProp, r.LongProp, Copy(r.RefProp));
	}
}