// (c) Egodystonic Studios 2018

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Egodystonic.Atomics.Numerics;
using static Egodystonic.Atomics.Benchmarks.BenchmarkUtils;

namespace Egodystonic.Atomics.Benchmarks.Internal {
	[CoreJob, MemoryDiagnoser]
	public class CustomIntVsUnmanaged {
		#region Parameters
		const int NumIterations = 100_000;

		public static object[] ThreadCounts { get; } = BenchmarkUtils.ThreadCounts;

		[ParamsSource(nameof(ThreadCounts))]
		public int NumThreads { get; set; }
		#endregion

		#region Test Setup
		
		#endregion

		#region Benchmark: Custom Int
		ManualResetEvent _customIntBarrier;
		List<Thread> _customIntThreads;
		AtomicInt _customInt;

		[IterationSetup(Target = nameof(WithCustomInt))]
		public void CreateCustomIntContext() {
			_customInt = new AtomicInt(0);
			_customIntBarrier = new ManualResetEvent(false);
			_customIntThreads = new List<Thread>();
			PrepareThreads(NumThreads, _customIntBarrier, WithCustomInt_Entry, _customIntThreads);
		}

		[Benchmark(Baseline = true)]
		public void WithCustomInt() {
			ExecutePreparedThreads(_customIntBarrier, _customIntThreads);
		}

		void WithCustomInt_Entry() {
			const int Bitmask = 0b101010101;
			for (var i = 0; i < NumIterations; ++i) {
				var incResult = _customInt.Increment();
				Assert(incResult.NewValue == incResult.PreviousValue + 1);
				var addResult = _customInt.Add(10);
				Assert(addResult.NewValue == addResult.PreviousValue + 10);
				var subResult = _customInt.Subtract(9);
				Assert(subResult.NewValue == subResult.PreviousValue - 9);
				var multResult = _customInt.MultiplyBy(3);
				Assert(multResult.NewValue == multResult.PreviousValue * 3);
				var divResult = _customInt.DivideBy(4);
				Assert(divResult.NewValue == divResult.PreviousValue / 4);
				var decResult = _customInt.Decrement();
				Assert(decResult.NewValue == decResult.PreviousValue - 1);
				var exchangeResult = _customInt.Exchange(curVal => curVal & Bitmask);
				Assert(exchangeResult.NewValue == (exchangeResult.PreviousValue & Bitmask));
			}
		}
		#endregion

		#region Benchmark: Unmanaged Int
		ManualResetEvent _unmanagedIntBarrier;
		List<Thread> _unmanagedIntThreads;
		AtomicValUnmanaged<int> _unmanagedInt;

		[IterationSetup(Target = nameof(WithUnmanagedInt))]
		public void CreateUnmanagedIntContext() {
			_unmanagedInt = new AtomicValUnmanaged<int>(0);
			_unmanagedIntBarrier = new ManualResetEvent(false);
			_unmanagedIntThreads = new List<Thread>();
			PrepareThreads(NumThreads, _unmanagedIntBarrier, WithUnmanagedInt_Entry, _unmanagedIntThreads);
		}

		[Benchmark]
		public void WithUnmanagedInt() {
			ExecutePreparedThreads(_unmanagedIntBarrier, _unmanagedIntThreads);
		}

		void WithUnmanagedInt_Entry() {
			const int Bitmask = 0b101010101;
			for (var i = 0; i < NumIterations; ++i) {
				var incResult = _unmanagedInt.Exchange(curVal => curVal + 1);
				Assert(incResult.NewValue == incResult.PreviousValue + 1);
				var addResult = _unmanagedInt.Exchange(curVal => curVal + 10);
				Assert(addResult.NewValue == addResult.PreviousValue + 10);
				var subResult = _unmanagedInt.Exchange(curVal => curVal - 9);
				Assert(subResult.NewValue == subResult.PreviousValue - 9);
				var multResult = _unmanagedInt.Exchange(curVal => curVal * 3);
				Assert(multResult.NewValue == multResult.PreviousValue * 3);
				var divResult = _unmanagedInt.Exchange(curVal => curVal / 4);
				Assert(divResult.NewValue == divResult.PreviousValue / 4);
				var decResult = _unmanagedInt.Exchange(curVal => curVal - 1);
				Assert(decResult.NewValue == decResult.PreviousValue - 1);
				var exchangeResult = _unmanagedInt.Exchange(curVal => curVal & Bitmask);
				Assert(exchangeResult.NewValue == (exchangeResult.PreviousValue & Bitmask));
			}
		}
		#endregion
	}
}