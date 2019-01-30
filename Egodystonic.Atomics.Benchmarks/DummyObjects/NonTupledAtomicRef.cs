using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Egodystonic.Atomics {
	public sealed class NonTupledAtomicRef<T> where T : class {
		static readonly bool TargetTypeIsEquatable = typeof(IEquatable<T>).IsAssignableFrom(typeof(T));
		T _value;

		public NonTupledAtomicRef(T initialValue) => Set(initialValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get() => Volatile.Read(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetUnsafe() => _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(T newValue) => Volatile.Write(ref _value, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(T newValue) => _value = newValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Exchange(T newValue) => Interlocked.Exchange(ref _value, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryExchange(T newValue, T comparand) {
			// Branches suck; but hopefully the fact that TargetTypeIsEquatable is invariant for all calls with the same type T will help the branch predictor
			if (TargetTypeIsEquatable) return TryExchange((_, ctx) => ctx, newValue, (c, _, ctx) => ValuesAreEqual(c, ctx), comparand);

			var oldValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			var wasSet = oldValue == comparand;
			return wasSet;
		}

		public bool TryExchange<TMapContext, TPredicateContext>(Func<T, TMapContext, T> mapFunc, TMapContext mapContext, Func<T, T, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = mapFunc(curValue, mapContext);
				if (!predicate(curValue, newValue, predicateContext)) return false;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return true;

				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool ValuesAreEqual(T lhs, T rhs) => TargetTypeIsEquatable ? ((IEquatable<T>) lhs).Equals(rhs) : lhs == rhs;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(NonTupledAtomicRef<T> operand) => operand.Get();
	}
}
