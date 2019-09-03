﻿// (c) Egodystonic Studios 2018
// Author: Ben Bowen
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Egodystonic.Atomics {
	public interface IAtomic<T> {
		T Value { get; set; }

		T Get();
		void Set(T newValue);
	}
}
