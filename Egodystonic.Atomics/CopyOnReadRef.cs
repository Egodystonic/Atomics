using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
		public void Set(T CurrentValue) => Volatile.Write(ref _value, CurrentValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(T CurrentValue) => _value = CurrentValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (T PreviousValue, T CurrentValue) Exchange(T CurrentValue) => (_copyFunc(Interlocked.Exchange(ref _value, CurrentValue)), _copyFunc(CurrentValue));

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
				var CurrentValue = mapFunc(curValueCopy, context);

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValueCopy, _copyFunc(CurrentValue));
				spinner.SpinOnce();
			}
		}

		public (T PreviousValue, T CurrentValue) SpinWaitForExchange(T CurrentValue, T comparand) {
			var spinner = new SpinWait();

			// Branches suck; but hopefully the fact that TargetTypeIsEquatable is invariant for all calls with the same type T will help the branch predictor
			if (TargetTypeIsEquatable) return SpinWaitForExchange((_, ctx) => ctx, CurrentValue, (curVal, _, ctx) => ValuesAreEqual(curVal, ctx), comparand);

			while (true) {
				if (Interlocked.CompareExchange(ref _value, CurrentValue, comparand) == comparand) return (_copyFunc(comparand), _copyFunc(CurrentValue));
				spinner.SpinOnce();
			}
		}

		public (T PreviousValue, T CurrentValue) SpinWaitForExchange<TContext>(Func<T, TContext, T> mapFunc, TContext context, T comparand) {
			// Branches suck; but hopefully the fact that TargetTypeIsEquatable is invariant for all calls with the same type T will help the branch predictor
			if (TargetTypeIsEquatable) return SpinWaitForExchange(mapFunc, context, (curVal, _, ctx) => ValuesAreEqual(curVal, ctx), comparand);

			var comparandCopy = _copyFunc(comparand);
			var spinner = new SpinWait();
			var CurrentValue = mapFunc(comparandCopy, context); // curValue will always be comparand when this method returns

			while (true) {
				if (Interlocked.CompareExchange(ref _value, CurrentValue, comparand) == comparand) return (comparandCopy, _copyFunc(CurrentValue));
				spinner.SpinOnce();
			}
		}

		public (T PreviousValue, T CurrentValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<T, TMapContext, T> mapFunc, TMapContext mapContext, Func<T, T, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = GetWithoutCopy();
				var curValueCopy = _copyFunc(curValue);
				var CurrentValue = mapFunc(curValueCopy, mapContext);
				var CurrentValueCopy = _copyFunc(CurrentValue);
				if (!predicate(curValueCopy, CurrentValueCopy, predicateContext)) {
					spinner.SpinOnce();
					continue;
				}

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (curValueCopy, CurrentValueCopy);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange(T CurrentValue, T comparand) {
			// Branches suck; but hopefully the fact that TargetTypeIsEquatable is invariant for all calls with the same type T will help the branch predictor
			if (TargetTypeIsEquatable) return TryExchange((_, ctx) => ctx, CurrentValue, (c, _, ctx) => ValuesAreEqual(c, ctx), comparand);

			var oldValue = Interlocked.CompareExchange(ref _value, CurrentValue, comparand);
			var oldValueCopy = _copyFunc(oldValue);
			var wasSet = oldValue == comparand;
			return (wasSet, oldValueCopy, wasSet ? _copyFunc(CurrentValue) : oldValueCopy);
		}

		public (bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange<TContext>(Func<T, TContext, T> mapFunc, TContext context, T comparand) {
			// Branches suck; but hopefully the fact that TargetTypeIsEquatable is invariant for all calls with the same type T will help the branch predictor
			if (TargetTypeIsEquatable) return TryExchange(mapFunc, context, (curVal, _, ctx) => ValuesAreEqual(curVal, ctx), comparand);

			var comparandCopy = _copyFunc(comparand);
			var CurrentValue = mapFunc(comparandCopy, context); // Comparand will always be curValue if the interlocked call passes
			var prevValue = Interlocked.CompareExchange(ref _value, CurrentValue, comparand);
			var prevValueCopy = _copyFunc(prevValue);
			if (prevValue == comparand) return (true, prevValueCopy, _copyFunc(CurrentValue));
			else return (false, prevValueCopy, prevValueCopy);
		}

		public (bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange<TMapContext, TPredicateContext>(Func<T, TMapContext, T> mapFunc, TMapContext mapContext, Func<T, T, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = GetWithoutCopy();
				var curValueCopy = _copyFunc(curValue);
				var CurrentValue = mapFunc(curValueCopy, mapContext);
				var CurrentValueCopy = _copyFunc(CurrentValue);
				if (!predicate(curValueCopy, CurrentValue, predicateContext)) return (false, curValueCopy, curValueCopy);

				if (Interlocked.CompareExchange(ref _value, CurrentValue, curValue) == curValue) return (true, curValueCopy, CurrentValueCopy);

				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool ValuesAreEqual(T lhs, T rhs) => TargetTypeIsEquatable ? ((IEquatable<T>) lhs).Equals(rhs) : lhs == rhs;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(CopyOnReadRef<T> operand) => operand.Get();
	}
}
