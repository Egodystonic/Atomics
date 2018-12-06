using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
		public (T PreviousValue, T NewValue) Exchange(T newValue) => _asUnmanaged.Exchange(newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(T newValue, T comparand) => _asUnmanaged.TryExchange(newValue, comparand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T SpinWaitForValue(T targetValue) => _asUnmanaged.SpinWaitForValue(targetValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (T PreviousValue, T NewValue) Exchange<TContext>(Func<T, TContext, T> mapFunc, TContext context) => _asUnmanaged.Exchange(mapFunc, context);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (T PreviousValue, T NewValue) SpinWaitForExchange(T newValue, T comparand) => _asUnmanaged.SpinWaitForExchange(newValue, comparand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (T PreviousValue, T NewValue) SpinWaitForExchange<TContext>(Func<T, TContext, T> mapFunc, T comparand, TContext context) => _asUnmanaged.SpinWaitForExchange(mapFunc, comparand, context);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (T PreviousValue, T NewValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<T, TMapContext, T> mapFunc, Func<T, T, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) => _asUnmanaged.SpinWaitForExchange(mapFunc, predicate, mapContext, predicateContext);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<TContext>(Func<T, TContext, T> mapFunc, T comparand, TContext context) => _asUnmanaged.TryExchange(mapFunc, comparand, context);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<TMapContext, TPredicateContext>(Func<T, TMapContext, T> mapFunc, Func<T, T, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) => _asUnmanaged.TryExchange(mapFunc, predicate, mapContext, predicateContext);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(AtomicEnumVal<T> operand) => operand.Get();
	}
}
