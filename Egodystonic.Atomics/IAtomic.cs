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

		T Exchange(T newValue);
		(bool ValueWasSet, T PreviousValue) TryExchange(T newValue, T comparand);
		(bool ValueWasSet, T PreviousValue) TryExchange(T newValue, Func<T, bool> predicate);
		(bool ValueWasSet, T PreviousValue) TryExchange(T newValue, Func<T, T, bool> predicate);
		(T PreviousValue, T NewValue) Exchange(Func<T, T> mapFunc);
		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(Func<T, T> mapFunc, T comparand);
		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(Func<T, T> mapFunc, Func<T, bool> predicate);
		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(Func<T, T> mapFunc, Func<T, T, bool> predicate);
	}
}
