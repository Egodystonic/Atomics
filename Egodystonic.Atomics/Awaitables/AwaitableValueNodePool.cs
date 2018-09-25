using System;
using System.Collections.Generic;
using System.Text;

namespace Egodystonic.Atomics.Awaitables {
	class AwaitableValueNodePool<T> {
		public AwaitableValueNode<T> Borrow() { }
		public void Return(AwaitableValueNode<T> node) { }
	}
}
