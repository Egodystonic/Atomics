// (c) Egodystonic Studios 2018
// Author: Ben Bowen
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Egodystonic.Atomics {
	public interface IScalableAtomic<T> : IAtomic<T> {
		T GetUnsafe();
		ref T GetUnsafeRef();
		void SetUnsafe(T newValue);
		ref T SetUnsafeRef(ref T newValueRef);

		T Exchange(T newValue);
		T TryExchange(T newValue, T comparand);
	}
}
