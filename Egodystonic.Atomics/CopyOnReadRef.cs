// (c) Egodystonic Studios 2018
// Author: Ben Bowen
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Egodystonic.Atomics {
	public sealed class CopyOnReadRef<T> : IAtomic<T> where T : class {
		static readonly bool TargetTypeIsEquatable = typeof(IEquatable<T>).IsAssignableFrom(typeof(T));
		readonly Func<T, T> _copyFunc;
		T _value;

		public T Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => Set(value);
		}

		public CopyOnReadRef(Func<T, T> copyFunc) : this(copyFunc, default) { }
		public CopyOnReadRef(Func<T, T> copyFunc, T initialValue) {
			_copyFunc = copyFunc;
			Set(initialValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get() => _copyFunc(Volatile.Read(ref _value));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetWithoutCopy() => Volatile.Read(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetUnsafe() => _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(T newValue) => Volatile.Write(ref _value, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(T newValue) => _value = newValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T FastExchange(T newValue) => _copyFunc(Interlocked.Exchange(ref _value, newValue));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (T PreviousValue, T CurrentValue) Exchange(T newValue) => (_copyFunc(Interlocked.Exchange(ref _value, newValue)), _copyFunc(newValue));

		public T SpinWaitForValue(T targetValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = GetWithoutCopy();
				if (ValuesAreEqual(curValue, targetValue)) return _copyFunc(curValue);
				spinner.SpinOnce();
			}
		}

		public (T PreviousValue, T CurrentValue) Exchange<TContext>(Func<T, TContext, T> mapFunc, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = GetWithoutCopy();
				var curValueCopy = _copyFunc(curValue);
				var newValue = mapFunc(curValueCopy, context);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValueCopy, _copyFunc(newValue));
				spinner.SpinOnce();
			}
		}

		public (T PreviousValue, T CurrentValue) SpinWaitForExchange(T newValue, T comparand) {
			var spinner = new SpinWait();

			// Branches suck; but hopefully the fact that TargetTypeIsEquatable is invariant for all calls with the same type T will help the branch predictor
			if (TargetTypeIsEquatable) return SpinWaitForExchange((_, ctx) => ctx, newValue, (curVal, _, ctx) => ValuesAreEqual(curVal, ctx), comparand);

			while (true) {
				if (Interlocked.CompareExchange(ref _value, newValue, comparand) == comparand) return (_copyFunc(comparand), _copyFunc(newValue));
				spinner.SpinOnce();
			}
		}

		public (T PreviousValue, T CurrentValue) SpinWaitForExchange<TContext>(Func<T, TContext, T> mapFunc, TContext context, T comparand) {
			// Branches suck; but hopefully the fact that TargetTypeIsEquatable is invariant for all calls with the same type T will help the branch predictor
			if (TargetTypeIsEquatable) return SpinWaitForExchange(mapFunc, context, (curVal, _, ctx) => ValuesAreEqual(curVal, ctx), comparand);

			var comparandCopy = _copyFunc(comparand);
			var spinner = new SpinWait();
			var newValue = mapFunc(comparandCopy, context); // curValue will always be comparand when this method returns

			while (true) {
				if (Interlocked.CompareExchange(ref _value, newValue, comparand) == comparand) return (comparandCopy, _copyFunc(newValue));
				spinner.SpinOnce();
			}
		}

		public (T PreviousValue, T CurrentValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<T, TMapContext, T> mapFunc, TMapContext mapContext, Func<T, T, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = GetWithoutCopy();
				var curValueCopy = _copyFunc(curValue);
				var newValue = mapFunc(curValueCopy, mapContext);
				var newValueCopy = _copyFunc(newValue);
				if (!predicate(curValueCopy, newValueCopy, predicateContext)) {
					spinner.SpinOnce();
					continue;
				}

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValueCopy, newValueCopy);
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T FastTryExchangeRefOnly(T newValue, T comparand) => _copyFunc(Interlocked.CompareExchange(ref _value, newValue, comparand));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T FastTryExchange(T newValue, T comparand) {
			return TargetTypeIsEquatable ? TryExchange((_, ctx) => ctx, newValue, (c, _, ctx) => ValuesAreEqual(c, ctx), comparand).PreviousValue : _copyFunc(Interlocked.CompareExchange(ref _value, newValue, comparand));
		}

		public (bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange(T newValue, T comparand) {
			// Branches suck; but hopefully the fact that TargetTypeIsEquatable is invariant for all calls with the same type T will help the branch predictor
			if (TargetTypeIsEquatable) return TryExchange((_, ctx) => ctx, newValue, (c, _, ctx) => ValuesAreEqual(c, ctx), comparand);

			var oldValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			var oldValueCopy = _copyFunc(oldValue);
			var wasSet = oldValue == comparand;
			return (wasSet, oldValueCopy, wasSet ? _copyFunc(newValue) : oldValueCopy);
		}

		public (bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange<TContext>(Func<T, TContext, T> mapFunc, TContext context, T comparand) {
			// Branches suck; but hopefully the fact that TargetTypeIsEquatable is invariant for all calls with the same type T will help the branch predictor
			if (TargetTypeIsEquatable) return TryExchange(mapFunc, context, (curVal, _, ctx) => ValuesAreEqual(curVal, ctx), comparand);

			var comparandCopy = _copyFunc(comparand);
			var newValue = mapFunc(comparandCopy, context); // Comparand will always be curValue if the interlocked call passes
			var prevValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			var prevValueCopy = _copyFunc(prevValue);
			if (prevValue == comparand) return (true, prevValueCopy, _copyFunc(newValue));
			else return (false, prevValueCopy, prevValueCopy);
		}

		public (bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange<TMapContext, TPredicateContext>(Func<T, TMapContext, T> mapFunc, TMapContext mapContext, Func<T, T, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = GetWithoutCopy();
				var curValueCopy = _copyFunc(curValue);
				var newValue = mapFunc(curValueCopy, mapContext);
				var newValueCopy = _copyFunc(newValue);
				if (!predicate(curValueCopy, newValue, predicateContext)) return (false, curValueCopy, curValueCopy);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValueCopy, newValueCopy);

				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool ValuesAreEqual(T lhs, T rhs) => TargetTypeIsEquatable ? ((IEquatable<T>) lhs).Equals(rhs) : lhs == rhs;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(CopyOnReadRef<T> operand) => operand.Get();

		public override string ToString() => Get()?.ToString() ?? "null";
	}
}
