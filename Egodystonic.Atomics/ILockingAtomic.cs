// (c) Egodystonic Studios 2019
// Author: Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;

namespace Egodystonic.Atomics {
	public interface ILockingAtomic<T> : IAtomic<T> {
		void Set(T newValue, out T previousValue);
		T Set(Func<T, T> valueMapFunc);
		T Set(Func<T, T> valueMapFunc, out T previousValue);

		// May be overkill in some situations, but I think its presence
		// may help protect against accidental check-then-get mistakes a bit
		// e.g. if (myAtomic.Value.X) DoStuff(myAtomic.Value)
		bool TryGet(Func<T, bool> valueComparisonPredicate, out T currentValue);

		bool TrySet(T newValue, Func<T, bool> setPredicate);
		bool TrySet(T newValue, Func<T, bool> setPredicate, out T previousValue);
		bool TrySet(Func<T, T> valueMapFunc, Func<T, bool> setPredicate);
		bool TrySet(Func<T, T> valueMapFunc, Func<T, bool> setPredicate, out T previousValue);
		bool TrySet(Func<T, T> valueMapFunc, Func<T, bool> setPredicate, out T previousValue, out T newValue);
		bool TrySet(Func<T, T> valueMapFunc, Func<T, T, bool> setPredicate);
		bool TrySet(Func<T, T> valueMapFunc, Func<T, T, bool> setPredicate, out T previousValue);
		bool TrySet(Func<T, T> valueMapFunc, Func<T, T, bool> setPredicate, out T previousValue, out T newValue);
	}
}