// (c) Egodystonic Studios 2018
// Author: Ben Bowen
using System;
using System.Runtime.CompilerServices;

namespace Egodystonic.Atomics {
	public sealed class AtomicEnumVal<T> : IAtomic<T> where T : unmanaged, Enum {
		readonly AtomicValUnmanaged<T> _asUnmanaged;

		public AtomicEnumVal() => _asUnmanaged = new AtomicValUnmanaged<T>();
		public AtomicEnumVal(T initialValue) => _asUnmanaged = new AtomicValUnmanaged<T>(initialValue);

		public T Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _asUnmanaged.Value;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _asUnmanaged.Value = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get() => _asUnmanaged.Get();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetUnsafe() => _asUnmanaged.GetUnsafe();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(T newValue) => _asUnmanaged.Set(newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(T newValue) => _asUnmanaged.SetUnsafe(newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T FastExchange(T newValue) => _asUnmanaged.FastExchange(newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (T PreviousValue, T CurrentValue) Exchange(T newValue) => _asUnmanaged.Exchange(newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T FastTryExchange(T newValue, T comparand) => _asUnmanaged.FastTryExchange(newValue, comparand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange(T newValue, T comparand) => _asUnmanaged.TryExchange(newValue, comparand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T SpinWaitForValue(T targetValue) => _asUnmanaged.SpinWaitForValue(targetValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (T PreviousValue, T CurrentValue) Exchange<TContext>(Func<T, TContext, T> mapFunc, TContext context) => _asUnmanaged.Exchange(mapFunc, context);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (T PreviousValue, T CurrentValue) SpinWaitForExchange(T newValue, T comparand) => _asUnmanaged.SpinWaitForExchange(newValue, comparand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (T PreviousValue, T CurrentValue) SpinWaitForExchange<TContext>(Func<T, TContext, T> mapFunc, TContext context, T comparand) => _asUnmanaged.SpinWaitForExchange(mapFunc, context, comparand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (T PreviousValue, T CurrentValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<T, TMapContext, T> mapFunc, TMapContext mapContext, Func<T, T, TPredicateContext, bool> predicate, TPredicateContext predicateContext) => _asUnmanaged.SpinWaitForExchange(mapFunc, mapContext, predicate, predicateContext);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange<TContext>(Func<T, TContext, T> mapFunc, TContext context, T comparand) => _asUnmanaged.TryExchange(mapFunc, context, comparand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange<TMapContext, TPredicateContext>(Func<T, TMapContext, T> mapFunc, TMapContext mapContext, Func<T, T, TPredicateContext, bool> predicate, TPredicateContext predicateContext) => _asUnmanaged.TryExchange(mapFunc, mapContext, predicate, predicateContext);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(AtomicEnumVal<T> operand) => operand.Get();

		public override string ToString() => Get().ToString();
	}
}
