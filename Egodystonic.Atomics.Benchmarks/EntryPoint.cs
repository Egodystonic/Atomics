// (c) Egodystonic Studios 2018


using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using Egodystonic.Atomics.Benchmarks.API;
using Egodystonic.Atomics.Benchmarks.Internal;

namespace Egodystonic.Atomics.Benchmarks {
	static class EntryPoint {
		public static void Main(string[] args) {
			BenchmarkRunner.Run<InlinedVsNonInlinedInt>();
			//BenchmarkRunner.Run<CustomIntVsUnmanaged>();
			//BenchmarkRunner.Run<RefTypeConcurrentReadWrite>();
		}
	}
}