// (c) Egodystonic Studios 2018

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;

namespace Egodystonic.Atomics.Tests.UnitTests {
	sealed class CopyOnReadRefDefaultCopyWrapper : IAtomic<DummyEquatableRef> {
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
		public void Set(DummyEquatableRef newValue) => _ref.Set(newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(DummyEquatableRef newValue) => _ref.SetUnsafe(newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyEquatableRef PreviousValue, DummyEquatableRef NewValue) Exchange(DummyEquatableRef newValue) => _ref.Exchange(newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, DummyEquatableRef PreviousValue, DummyEquatableRef NewValue) TryExchange(DummyEquatableRef newValue, DummyEquatableRef comparand) => _ref.TryExchange(newValue, comparand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DummyEquatableRef SpinWaitForValue(DummyEquatableRef targetValue) => _ref.SpinWaitForValue(targetValue);

		public (DummyEquatableRef PreviousValue, DummyEquatableRef NewValue) Exchange<TContext>(Func<DummyEquatableRef, TContext, DummyEquatableRef> mapFunc, TContext context) {
			return _ref.Exchange(mapFunc, context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (DummyEquatableRef PreviousValue, DummyEquatableRef NewValue) SpinWaitForExchange(DummyEquatableRef newValue, DummyEquatableRef comparand) {
			return _ref.SpinWaitForExchange(newValue, comparand);
		}

		public (DummyEquatableRef PreviousValue, DummyEquatableRef NewValue) SpinWaitForExchange<TContext>(Func<DummyEquatableRef, TContext, DummyEquatableRef> mapFunc, DummyEquatableRef comparand, TContext context) {
			return _ref.SpinWaitForExchange(mapFunc, comparand, context);
		}

		public (DummyEquatableRef PreviousValue, DummyEquatableRef NewValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<DummyEquatableRef, TMapContext, DummyEquatableRef> mapFunc, Func<DummyEquatableRef, DummyEquatableRef, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
			return _ref.SpinWaitForExchange(mapFunc, predicate, mapContext, predicateContext);
		}

		public (bool ValueWasSet, DummyEquatableRef PreviousValue, DummyEquatableRef NewValue) TryExchange<TContext>(Func<DummyEquatableRef, TContext, DummyEquatableRef> mapFunc, DummyEquatableRef comparand, TContext context) {
			return _ref.TryExchange(mapFunc, comparand, context);
		}

		public (bool ValueWasSet, DummyEquatableRef PreviousValue, DummyEquatableRef NewValue) TryExchange<TMapContext, TPredicateContext>(Func<DummyEquatableRef, TMapContext, DummyEquatableRef> mapFunc, Func<DummyEquatableRef, DummyEquatableRef, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
			return _ref.TryExchange(mapFunc, predicate, mapContext, predicateContext);
		}

		static DummyEquatableRef Copy(DummyEquatableRef r) => r == null ? null : new DummyEquatableRef(r.StringProp, r.LongProp, Copy(r.RefProp));
	}
}