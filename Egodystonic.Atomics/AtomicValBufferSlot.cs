// (c) Egodystonic Studios 2019

using System;
using System.Collections.Generic;
using System.Linq;

namespace Egodystonic.Atomics {
	struct AtomicValBufferSlot<T> where T : struct {
		public long WriteCount;
		public T Value;
	}
}