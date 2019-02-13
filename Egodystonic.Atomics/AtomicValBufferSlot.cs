// (c) Egodystonic Studios 2019
// Author: Ben Bowen
using System;

namespace Egodystonic.Atomics {
	struct AtomicValBufferSlot<T> where T : struct {
		public long WriteCount;
		public T Value;
	}
}