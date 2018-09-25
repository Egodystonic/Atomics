using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Egodystonic.Atomics.Awaitables {
	class AwaitableNumericAtomicWrapper<T> : AwaitableAtomicWrapper<T>, IAwaitableNumericAtomic<T> where T : IEquatable<T> {
		public (T PreviousValue, T NewValue) Increment() {
			// TODO don't forget to notify of the new value
		}

		public (T PreviousValue, T NewValue) Decrement() {
			// TODO don't forget to notify of the new value
		}

		public (T PreviousValue, T NewValue) Add(T operand) {
			// TODO don't forget to notify of the new value
		}

		public (T PreviousValue, T NewValue) Subtract(T operand) {
			// TODO don't forget to notify of the new value
		}

		public (T PreviousValue, T NewValue) MultiplyBy(T operand) {
			// TODO don't forget to notify of the new value
		}

		public (T PreviousValue, T NewValue) DivideBy(T operand) {
			// TODO don't forget to notify of the new value
		}
	}
}