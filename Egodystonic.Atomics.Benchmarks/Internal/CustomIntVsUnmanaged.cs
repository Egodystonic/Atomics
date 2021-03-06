﻿// (c) Egodystonic Studios 2018

using System;
using System.Collections.Generic;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Egodystonic.Atomics.Numerics;
using static Egodystonic.Atomics.Benchmarks.BenchmarkUtils;

namespace Egodystonic.Atomics.Benchmarks.Internal {
	/// <summary>
	/// Benchmark used to justify custom implementation for AtomicInt32 and other numeric types; as opposed to delegation/deferral to
	/// an LockFreeUnmanagedValue.
	/// </summary>
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
		LockFreeInt32 _customInt32;

		[IterationSetup(Target = nameof(WithCustomInt))]
		public void CreateCustomIntContext() {
			_customInt32 = new LockFreeInt32(0);
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
				var incResult = _customInt32.Increment();
				Assert(incResult.CurrentValue == incResult.PreviousValue + 1);
				var addResult = _customInt32.Add(10);
				Assert(addResult.CurrentValue == addResult.PreviousValue + 10);
				var subResult = _customInt32.Subtract(9);
				Assert(subResult.CurrentValue == subResult.PreviousValue - 9);
				var multResult = _customInt32.MultiplyBy(3);
				Assert(multResult.CurrentValue == multResult.PreviousValue * 3);
				var divResult = _customInt32.DivideBy(4);
				Assert(divResult.CurrentValue == divResult.PreviousValue / 4);
				var decResult = _customInt32.Decrement();
				Assert(decResult.CurrentValue == decResult.PreviousValue - 1);
				var exchangeResult = _customInt32.Exchange(curVal => curVal & Bitmask);
				Assert(exchangeResult.CurrentValue == (exchangeResult.PreviousValue & Bitmask));
			}
		}
		#endregion

		#region Benchmark: Unmanaged Int
		ManualResetEvent _unmanagedIntBarrier;
		List<Thread> _unmanagedIntThreads;
		LockFreeValue<int> _unmanagedInt;

		[IterationSetup(Target = nameof(WithUnmanagedInt))]
		public void CreateUnmanagedIntContext() {
			_unmanagedInt = new LockFreeValue<int>(0);
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
				Assert(incResult.CurrentValue == incResult.PreviousValue + 1);
				var addResult = _unmanagedInt.Exchange(curVal => curVal + 10);
				Assert(addResult.CurrentValue == addResult.PreviousValue + 10);
				var subResult = _unmanagedInt.Exchange(curVal => curVal - 9);
				Assert(subResult.CurrentValue == subResult.PreviousValue - 9);
				var multResult = _unmanagedInt.Exchange(curVal => curVal * 3);
				Assert(multResult.CurrentValue == multResult.PreviousValue * 3);
				var divResult = _unmanagedInt.Exchange(curVal => curVal / 4);
				Assert(divResult.CurrentValue == divResult.PreviousValue / 4);
				var decResult = _unmanagedInt.Exchange(curVal => curVal - 1);
				Assert(decResult.CurrentValue == decResult.PreviousValue - 1);
				var exchangeResult = _unmanagedInt.Exchange(curVal => curVal & Bitmask);
				Assert(exchangeResult.CurrentValue == (exchangeResult.PreviousValue & Bitmask));
			}
		}
		#endregion
	}
}