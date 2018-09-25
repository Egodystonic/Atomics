using System;
using System.Collections.Generic;
using System.Linq;

namespace Egodystonic.Atomics.Awaitables {
	class AwaitableValueNode<T> {
		AwaitableValueNode<T> _next;
		SequencedValue<T> _sequencedValue;
		
		public void Set(SequencedValue<T> sequencedValue) {
			_sequencedValue = sequencedValue;
		}
	}
}