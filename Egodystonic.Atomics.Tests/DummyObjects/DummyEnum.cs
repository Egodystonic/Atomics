// (c) Egodystonic Studios 2018

using System;
using System.Collections.Generic;
using System.Linq;

namespace Egodystonic.Atomics.Tests.DummyObjects {
	public enum DummyEnum : long {
		None = 0x0L,
		Alpha = 0x1L,
		Bravo = Int64.MaxValue,
		Charlie = Int64.MinValue,
		Delta = -0x1L
	}
}