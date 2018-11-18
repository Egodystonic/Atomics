using System;
using System.Collections.Generic;
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

		(bool ValueWasSet, T PreviousValue) TryExchange(T newValue, T comparand);
		(bool ValueWasSet, T PreviousValue) TryExchange(T newValue, Func<T, bool> predicate);
		(bool ValueWasSet, T PreviousValue) TryExchange(T newValue, Func<T, T, bool> predicate);
		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(Func<T, T> mapFunc, T comparand);
		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(Func<T, T> mapFunc, Func<T, bool> predicate);
		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(Func<T, T> mapFunc, Func<T, T, bool> predicate);

		// For the context stuff I wouldn't mind going with extension methods that delegate to the main thing.

		//		(bool ValueWasSet, T PreviousValue) TryExchange<TContext>(T newValue, Func<T, TContext, bool> predicate, TContext context);
		//		(bool ValueWasSet, T PreviousValue) TryExchange<TContext>(T newValue, Func<T, T, TContext, bool> predicate, TContext context);
		//		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<TContext>(Func<T, TContext, T> mapFunc, T comparand, TContext context);
		//		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<TContext>(Func<T, TContext, T> mapFunc, Func<T, bool> predicate, TContext context);
		//		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<TContext>(Func<T, TContext, T> mapFunc, Func<T, T, bool> predicate, TContext context);
		//		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<TContext>(Func<T, T> mapFunc, Func<T, TContext, bool> predicate, TContext context);
		//		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<TContext>(Func<T, T> mapFunc, Func<T, T, TContext, bool> predicate, TContext context);
		//		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<TContext>(Func<T, TContext, T> mapFunc, Func<T, TContext, bool> predicate, TContext context);
		//		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<TContext>(Func<T, TContext, T> mapFunc, Func<T, T, TContext, bool> predicate, TContext context);
		//		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<TMapContext, TPredicateContext>(Func<T, TMapContext, T> mapFunc, Func<T, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext);
		//		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<TMapContext, TPredicateContext>(Func<T, TMapContext, T> mapFunc, Func<T, T, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext);

		//		void SpinWaitForExchange(T newValue, T targetValue);
		//		T SpinWaitForExchange(T newValue, Func<T, bool> predicate);
		//		(T PreviousValue, T NewValue) SpinWaitForExchange(Func<T, T> mapFunc, Func<T, bool> predicate);
		//		(T PreviousValue, T NewValue) SpinWaitForExchange(Func<T, T> mapFunc, Func<T, T, bool> predicate);
		// TODO add context versions of these

	}
}
