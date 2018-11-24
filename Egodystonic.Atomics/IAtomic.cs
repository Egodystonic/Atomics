using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Egodystonic.Atomics {
	public interface IAtomic<T> {
		T Value { get; set; }

		T Get();
		T GetUnsafe();

		void Set(T newValue);
		void SetUnsafe(T newValue);

//		void SpinWaitForValue(T targetValue);
//		T SpinWaitForValue(Func<T, bool> predicate);
//		T SpinWaitForValue<TContext>(Func<T, TContext, bool> predicate, TContext context);

		T Exchange(T newValue);
		(T PreviousValue, T NewValue) Exchange(Func<T, T> mapFunc);
//		(T PreviousValue, T NewValue) Exchange<TContext>(Func<T, TContext, T> mapFunc, TContext context);

//		void SpinWaitForExchange(T newValue, T comparand);
//		T SpinWaitForExchange(T newValue, Func<T, T, bool> predicate);
//		T SpinWaitForExchange<TContext>(T newValue, Func<T, T, TContext, bool> predicate, TContext context);
//		(T PreviousValue, T NewValue) SpinWaitForExchange(Func<T, T> mapFunc, T comparand);
//		(T PreviousValue, T NewValue) SpinWaitForExchange<TContext>(Func<T, TContext, T> mapFunc, T comparand, TContext context);
//		(T PreviousValue, T NewValue) SpinWaitForExchange(Func<T, T> mapFunc, Func<T, T, bool> predicate);
//		(T PreviousValue, T NewValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<T, TMapContext, T> mapFunc, Func<T, T, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext);

		(bool ValueWasSet, T PreviousValue) TryExchange(T newValue, T comparand);
		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<TContext>(Func<T, TContext, T> mapFunc, T comparand, TContext context); // TODO replace this with an extension method once we can farm out the comparand equality comparison to a static internal interface method (C# future)
		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<TMapContext, TPredicateContext>(Func<T, TMapContext, T> mapFunc, Func<T, T, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext);
	}

	public static class AtomicExtensions {
		public static (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<T>(this IAtomic<T> @this, T newValue, Func<T, T, bool> predicate) {
			return @this.TryExchange<object, object>((_, __) => newValue, (curVal, newVal, _) => predicate(curVal, newVal), null, null);
		}

		public static (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<T, TContext>(this IAtomic<T> @this, T newValue, Func<T, T, TContext, bool> predicate, TContext context) {
			return @this.TryExchange<object, TContext>((_, __) => newValue, predicate, null, context);
		}


		public static (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<T>(this IAtomic<T> @this, Func<T, T> mapFunc, T comparand) {
			return @this.TryExchange<object>((curVal, _) => mapFunc(curVal), comparand, null);
		}


		public static (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<T>(this IAtomic<T> @this, Func<T, T> mapFunc, Func<T, T, bool> predicate) {

		}

		public static (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<T, TContext>(this IAtomic<T> @this, Func<T, TContext, T> mapFunc, Func<T, T, bool> predicate, TContext context) {

		}

		public static (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<T, TContext>(this IAtomic<T> @this, Func<T, T> mapFunc, Func<T, T, TContext, bool> predicate, TContext context) {

		}

		public static (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<T, TContext>(this IAtomic<T> @this, Func<T, TContext, T> mapFunc, Func<T, T, TContext, bool> predicate, TContext context) {

		}
	}
}
