// (c) Egodystonic Studios 2018
// Author: Ben Bowen
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Egodystonic.Atomics {
	public interface INonLockingAtomic<T> : IAtomic<T> {
		T GetUnsafe();
		void SetUnsafe(T newValue);
		ref T GetUnsafeRef();

		T Swap(T newValue);
		T TrySwap(T newValue, T comparand);
	}
}
