// (c) Egodystonic Studios 2018


using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using Egodystonic.Atomics.Benchmarks.External;
using Egodystonic.Atomics.Benchmarks.Internal;

namespace Egodystonic.Atomics.Benchmarks {
	static class EntryPoint {
		public static void Main(string[] args) {
			BenchmarkRunner.Run<AtomicValFastWrites>();
			//BenchmarkRunner.Run<ReadWriteLongPtrMethods>();
			//BenchmarkRunner.Run<FuncOverheads>();
			//BenchmarkRunner.Run<TupleReturnVsSingleValueReturn>();
			//BenchmarkRunner.Run<UnmanagedValVsManualLoop>();
			//BenchmarkRunner.Run<LongConcurrentOperations>();
			//BenchmarkRunner.Run<ExtensionVsInstanceVsInherit>();
			//BenchmarkRunner.Run<InlinedVsNonInlinedInt>();
			//BenchmarkRunner.Run<CustomIntVsUnmanaged>();
			//BenchmarkRunner.Run<RefTypeConcurrentReadWrite>();
		}
	}
}