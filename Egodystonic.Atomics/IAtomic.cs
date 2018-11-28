using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Egodystonic.Atomics {
	public interface IAtomic<T> {
		T Value { get; set; }

		T Get();
		T GetUnsafe();

		void Set(T newValue);
		void SetUnsafe(T newValue);

		T SpinWaitForValue(T targetValue);

		(T PreviousValue, T NewValue) Exchange(T newValue);
		(T PreviousValue, T NewValue) Exchange<TContext>(Func<T, TContext, T> mapFunc, TContext context);

		(T PreviousValue, T NewValue) SpinWaitForExchange(T newValue, T comparand);
		(T PreviousValue, T NewValue) SpinWaitForExchange<TContext>(Func<T, TContext, T> mapFunc, T comparand, TContext context); // TODO replace this with an extension method once we can farm out the comparand equality comparison to a static internal interface method (C# future)
		(T PreviousValue, T NewValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<T, TMapContext, T> mapFunc, Func<T, T, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext);

		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(T newValue, T comparand);
		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<TContext>(Func<T, TContext, T> mapFunc, T comparand, TContext context); // TODO replace this with an extension method once we can farm out the comparand equality comparison to a static internal interface method (C# future)
		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<TMapContext, TPredicateContext>(Func<T, TMapContext, T> mapFunc, Func<T, T, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext);
	}

	// TODO C# 8- replace these with default interface implementations that we redeclare in each class.
	// TODO We can provide a default impl for pretty much all functions (via static interface methods),
	// TODO which will remove the need for delegation; and it will also allow polymorphic specialization
	// TODO for cases where we can provide better implementations. Assuming performance is superior (which it should be).
	public static class AtomicExtensions {
		#region SpinWaitForValue
		public static T SpinWaitForValue<T>(this IAtomic<T> @this, Func<T, bool> predicate) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = @this.Get();
				if (predicate(curValue)) return curValue;
				spinner.SpinOnce();
			}
		}

		public static T SpinWaitForValue<T, TContext>(this IAtomic<T> @this, Func<T, TContext, bool> predicate, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = @this.Get();
				if (predicate(curValue, context)) return curValue;
				spinner.SpinOnce();
			}
		}
		#endregion

		#region Exchange
		public static (T PreviousValue, T NewValue) Exchange<T>(this IAtomic<T> @this, Func<T, T> mapFunc) {
			return @this.Exchange<object>((curVal, _) => mapFunc(curVal), null);
		}
		#endregion

		#region SpinWaitForExchange
		public static (T PreviousValue, T NewValue) SpinWaitForExchange<T>(this IAtomic<T> @this, T newValue, Func<T, T, bool> predicate) {
			return @this.SpinWaitForExchange((_, ctx) => ctx, (curVal, newVal, ctx) => ctx(curVal, newVal), newValue, predicate);
		}

		public static (T PreviousValue, T NewValue) SpinWaitForExchange<T, TContext>(this IAtomic<T> @this, T newValue, Func<T, T, TContext, bool> predicate, TContext context) {
			return @this.SpinWaitForExchange((_, ctx) => ctx, predicate, newValue, context);
		}

		public static (T PreviousValue, T NewValue) SpinWaitForExchange<T>(this IAtomic<T> @this, Func<T, T> mapFunc, T comparand) {
			return @this.SpinWaitForExchange((curVal, ctx) => ctx(curVal), comparand, mapFunc);
		}

		public static (T PreviousValue, T NewValue) SpinWaitForExchange<T>(this IAtomic<T> @this, Func<T, T> mapFunc, Func<T, T, bool> predicate) {
			return @this.SpinWaitForExchange((curVal, ctx) => ctx(curVal), (curVal, newVal, ctx) => ctx(curVal, newVal), mapFunc, predicate);
		}

		public static (T PreviousValue, T NewValue) SpinWaitForExchange<T, TContext>(this IAtomic<T> @this, Func<T, TContext, T> mapFunc, Func<T, T, bool> predicate, TContext context) {
			return @this.SpinWaitForExchange(mapFunc, (curVal, newVal, ctx) => ctx(curVal, newVal), context, predicate);
		}

		public static (T PreviousValue, T NewValue) SpinWaitForExchange<T, TContext>(this IAtomic<T> @this, Func<T, T> mapFunc, Func<T, T, TContext, bool> predicate, TContext context) {
			return @this.SpinWaitForExchange((curVal, ctx) => ctx(curVal), predicate, mapFunc, context);
		}

		public static (T PreviousValue, T NewValue) SpinWaitForExchange<T, TContext>(this IAtomic<T> @this, Func<T, TContext, T> mapFunc, Func<T, T, TContext, bool> predicate, TContext context) {
			return @this.SpinWaitForExchange(mapFunc, predicate, context, context);
		}
		#endregion

		#region TryExchange
		public static (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<T>(this IAtomic<T> @this, T newValue, Func<T, T, bool> predicate) {
			return @this.TryExchange((_, ctx) => ctx, (curVal, newVal, ctx) => ctx(curVal, newVal), newValue, predicate);
		}

		public static (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<T, TContext>(this IAtomic<T> @this, T newValue, Func<T, T, TContext, bool> predicate, TContext context) {
			return @this.TryExchange((_, ctx) => ctx, predicate, newValue, context);
		}

		public static (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<T>(this IAtomic<T> @this, Func<T, T> mapFunc, T comparand) {
			return @this.TryExchange((curVal, ctx) => ctx(curVal), comparand, mapFunc);
		}

		public static (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<T>(this IAtomic<T> @this, Func<T, T> mapFunc, Func<T, T, bool> predicate) {
			return @this.TryExchange((curVal, ctx) => ctx(curVal), (curVal, newVal, ctx) => ctx(curVal, newVal), mapFunc, predicate);
		}

		public static (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<T, TContext>(this IAtomic<T> @this, Func<T, TContext, T> mapFunc, Func<T, T, bool> predicate, TContext context) {
			return @this.TryExchange(mapFunc, (curVal, newVal, ctx) => ctx(curVal, newVal), context, predicate);
		}

		public static (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<T, TContext>(this IAtomic<T> @this, Func<T, T> mapFunc, Func<T, T, TContext, bool> predicate, TContext context) {
			return @this.TryExchange((curVal, ctx) => ctx(curVal), predicate, mapFunc, context);
		}

		public static (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<T, TContext>(this IAtomic<T> @this, Func<T, TContext, T> mapFunc, Func<T, T, TContext, bool> predicate, TContext context) {
			return @this.TryExchange(mapFunc, predicate, context, context);
		}
		#endregion
	}
}
