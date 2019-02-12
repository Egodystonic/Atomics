using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Egodystonic.Atomics {
	public sealed class AtomicRef<T> : IAtomic<T> where T : class {
		static readonly bool TargetTypeIsEquatable = typeof(IEquatable<T>).IsAssignableFrom(typeof(T));
		T _value;

		public T Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => Set(value);
		}

		public AtomicRef() : this(default) { }
		public AtomicRef(T initialValue) => Set(initialValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get() => Volatile.Read(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetUnsafe() => _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(T newValue) => Volatile.Write(ref _value, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(T newValue) => _value = newValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T FastExchange(T newValue) => Interlocked.Exchange(ref _value, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (T PreviousValue, T CurrentValue) Exchange(T newValue) => (Interlocked.Exchange(ref _value, newValue), newValue);

		public T SpinWaitForValue(T targetValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (ValuesAreEqual(curValue, targetValue)) return curValue;
				spinner.SpinOnce();
			}
		}

		public (T PreviousValue, T CurrentValue) Exchange<TContext>(Func<T, TContext, T> mapFunc, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (T PreviousValue, T CurrentValue) SpinWaitForExchange(T newValue, T comparand) {
			var spinner = new SpinWait();

			// Branches suck; but hopefully the fact that TargetTypeIsEquatable is invariant for all calls with the same type T will help the branch predictor
			if (TargetTypeIsEquatable) return SpinWaitForExchange((_, ctx) => ctx, comparand, (curVal, _, ctx) => ValuesAreEqual(curVal, ctx), comparand);

			while (true) {
				if (Interlocked.CompareExchange(ref _value, newValue, comparand) == comparand) return (comparand, newValue);
				spinner.SpinOnce();
			}
		}

		public (T PreviousValue, T CurrentValue) SpinWaitForExchange<TContext>(Func<T, TContext, T> mapFunc, TContext context, T comparand) {
			// Branches suck; but hopefully the fact that TargetTypeIsEquatable is invariant for all calls with the same type T will help the branch predictor
			if (TargetTypeIsEquatable) return SpinWaitForExchange(mapFunc, context, (curVal, _, ctx) => ValuesAreEqual(curVal, ctx), comparand);

			var spinner = new SpinWait();
			var newValue = mapFunc(comparand, context); // curValue will always be comparand when this method returns

			while (true) {
				if (Interlocked.CompareExchange(ref _value, newValue, comparand) == comparand) return (comparand, newValue);
				spinner.SpinOnce();
			}
		}

		public (T PreviousValue, T CurrentValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<T, TMapContext, T> mapFunc, TMapContext mapContext, Func<T, T, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = mapFunc(curValue, mapContext);
				if (!predicate(curValue, newValue, predicateContext)) {
					spinner.SpinOnce();
					continue;
				}

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T FastTryExchangeRefOnly(T newValue, T comparand) => Interlocked.CompareExchange(ref _value, newValue, comparand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T FastTryExchange(T newValue, T comparand) {
			return TargetTypeIsEquatable ? TryExchange((_, ctx) => ctx, newValue, (c, _, ctx) => ValuesAreEqual(c, ctx), comparand).PreviousValue : Interlocked.CompareExchange(ref _value, newValue, comparand);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange(T newValue, T comparand) {
			// Branches suck; but hopefully the fact that TargetTypeIsEquatable is invariant for all calls with the same type T will help the branch predictor
			if (TargetTypeIsEquatable) return TryExchange((_, ctx) => ctx, newValue, (c, _, ctx) => ValuesAreEqual(c, ctx), comparand);

			var oldValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			var wasSet = oldValue == comparand;
			return (wasSet, oldValue, wasSet ? newValue : oldValue);
		}

		public (bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange<TContext>(Func<T, TContext, T> mapFunc, TContext context, T comparand) {
			// Branches suck; but hopefully the fact that TargetTypeIsEquatable is invariant for all calls with the same type T will help the branch predictor
			if (TargetTypeIsEquatable) return TryExchange(mapFunc, context, (curVal, _, ctx) => ValuesAreEqual(curVal, ctx), comparand);

			var newValue = mapFunc(comparand, context); // Comparand will always be curValue if the interlocked call passes
			var prevValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			if (prevValue == comparand) return (true, prevValue, newValue);
			else return (false, prevValue, prevValue);
		}

		public (bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange<TMapContext, TPredicateContext>(Func<T, TMapContext, T> mapFunc, TMapContext mapContext, Func<T, T, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = mapFunc(curValue, mapContext);
				if (!predicate(curValue, newValue, predicateContext)) return (false, curValue, curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);

				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool ValuesAreEqual(T lhs, T rhs) => TargetTypeIsEquatable ? ((IEquatable<T>) lhs).Equals(rhs) : lhs == rhs;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(AtomicRef<T> operand) => operand.Get();
	}
}
