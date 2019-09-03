// (c) Egodystonic Studios 2019
// Author: Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;

namespace Egodystonic.Atomics.Numerics {
	public static class NonScalableAtomicNumberExtensions {
		public static Int32 Increment(this INonScalableAtomic<Int32> @this) => @this.Set(i => ++i);
		public static Int32 Increment(this INonScalableAtomic<Int32> @this, out Int32 previousValue) => @this.Set(i => ++i, out previousValue);

		public static Int32 Increment(this INonScalableAtomic<Int32> @this) => @this.Set(i => ++i);
		public static Int32 Increment(this INonScalableAtomic<Int32> @this, out Int32 previousValue) => @this.Set(i => ++i, out previousValue);
	}
}