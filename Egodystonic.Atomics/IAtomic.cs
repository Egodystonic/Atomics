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

		(T PreviousValue, T CurrentValue) Exchange(T newValue);
		(T PreviousValue, T CurrentValue) Exchange<TContext>(Func<T, TContext, T> mapFunc, TContext context);

		(T PreviousValue, T CurrentValue) SpinWaitForExchange(T newValue, T comparand);
		(T PreviousValue, T CurrentValue) SpinWaitForExchange<TContext>(Func<T, TContext, T> mapFunc, TContext context, T comparand); // TODO replace this with an extension method once we can farm out the comparand equality comparison to a static internal interface method (C# future)
		(T PreviousValue, T CurrentValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<T, TMapContext, T> mapFunc, TMapContext mapContext, Func<T, T, TPredicateContext, bool> predicate, TPredicateContext predicateContext);

		(bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange(T newValue, T comparand);
		(bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange<TContext>(Func<T, TContext, T> mapFunc, TContext context, T comparand); // TODO replace this with an extension method once we can farm out the comparand equality comparison to a static internal interface method (C# future)
		(bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange<TMapContext, TPredicateContext>(Func<T, TMapContext, T> mapFunc, TMapContext mapContext, Func<T, T, TPredicateContext, bool> predicate, TPredicateContext predicateContext);
	}

	// TODO C# 8- replace these with default interface implementations that we redeclare in each class.
	// TODO We can provide a default impl for pretty much all functions, using internal interface methods to provide
	// TODO impls for things like CompareExchange, Equality, etc., which will remove the need for delegation.
	// TODO It will also allow polymorphic specialization for cases where we can provide better implementations.
	// TODO Assuming performance is superior (which it should be).
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (T PreviousValue, T CurrentValue) Exchange<T>(this IAtomic<T> @this, Func<T, T> mapFunc) {
			return @this.Exchange((curVal, ctx) => ctx(curVal), mapFunc);
		}
		#endregion

		#region SpinWaitForExchange
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (T PreviousValue, T CurrentValue) SpinWaitForExchange<T>(this IAtomic<T> @this, T newValue, Func<T, T, bool> predicate) {
			return @this.SpinWaitForExchange((_, ctx) => ctx, newValue, (curVal, newVal, ctx) => ctx(curVal, newVal), predicate);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (T PreviousValue, T CurrentValue) SpinWaitForExchange<T, TContext>(this IAtomic<T> @this, T newValue, Func<T, T, TContext, bool> predicate, TContext context) {
			return @this.SpinWaitForExchange((_, ctx) => ctx, newValue, predicate, context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (T PreviousValue, T CurrentValue) SpinWaitForExchange<T>(this IAtomic<T> @this, Func<T, T> mapFunc, T comparand) {
			return @this.SpinWaitForExchange((curVal, ctx) => ctx(curVal), mapFunc, comparand);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (T PreviousValue, T CurrentValue) SpinWaitForExchange<T>(this IAtomic<T> @this, Func<T, T> mapFunc, Func<T, T, bool> predicate) {
			return @this.SpinWaitForExchange((curVal, ctx) => ctx(curVal), mapFunc, (curVal, newVal, ctx) => ctx(curVal, newVal), predicate);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (T PreviousValue, T CurrentValue) SpinWaitForExchange<T, TContext>(this IAtomic<T> @this, Func<T, TContext, T> mapFunc, TContext context, Func<T, T, bool> predicate) {
			return @this.SpinWaitForExchange(mapFunc, context, (curVal, newVal, ctx) => ctx(curVal, newVal), predicate);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (T PreviousValue, T CurrentValue) SpinWaitForExchange<T, TContext>(this IAtomic<T> @this, Func<T, T> mapFunc, Func<T, T, TContext, bool> predicate, TContext context) {
			return @this.SpinWaitForExchange((curVal, ctx) => ctx(curVal), mapFunc, predicate, context);
		}
		#endregion

		#region TryExchange
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange<T>(this IAtomic<T> @this, T newValue, Func<T, T, bool> predicate) {
			return @this.TryExchange((_, ctx) => ctx, newValue, (curVal, newVal, ctx) => ctx(curVal, newVal), predicate);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange<T, TContext>(this IAtomic<T> @this, T newValue, Func<T, T, TContext, bool> predicate, TContext context) {
			return @this.TryExchange((_, ctx) => ctx, newValue, predicate, context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange<T>(this IAtomic<T> @this, Func<T, T> mapFunc, T comparand) {
			return @this.TryExchange((curVal, ctx) => ctx(curVal), mapFunc, comparand);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange<T>(this IAtomic<T> @this, Func<T, T> mapFunc, Func<T, T, bool> predicate) {
			return @this.TryExchange((curVal, ctx) => ctx(curVal), mapFunc, (curVal, newVal, ctx) => ctx(curVal, newVal), predicate);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange<T, TContext>(this IAtomic<T> @this, Func<T, TContext, T> mapFunc, TContext context, Func<T, T, bool> predicate) {
			return @this.TryExchange(mapFunc, context, (curVal, newVal, ctx) => ctx(curVal, newVal), predicate);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static (bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange<T, TContext>(this IAtomic<T> @this, Func<T, T> mapFunc, Func<T, T, TContext, bool> predicate, TContext context) {
			return @this.TryExchange((curVal, ctx) => ctx(curVal), mapFunc, predicate, context);
		}
		#endregion
	}
}
