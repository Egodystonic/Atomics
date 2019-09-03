// (c) Egodystonic Studios 2019
// Author: Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Egodystonic.Atomics {
	public sealed class Atomic<T> : INonScalableAtomic<T> {
		readonly object _instanceMutationLock = new object();
		T _value;

		public T Value {
			get {
				lock (_instanceMutationLock) return _value;
			}
			set {
				lock (_instanceMutationLock) _value = value;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get() => Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(T newValue) => Value = newValue;

		
	}
}