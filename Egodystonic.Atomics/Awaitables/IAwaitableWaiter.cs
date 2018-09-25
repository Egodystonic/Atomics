using System;

namespace Egodystonic.Atomics.Awaitables {
	interface IAwaitableWaiter<T> {
		void HandleNewValue(AtomicValueBackstop backstop, T newValue);
	}
}