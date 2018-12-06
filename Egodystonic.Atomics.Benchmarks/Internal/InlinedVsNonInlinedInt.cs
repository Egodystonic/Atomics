// (c) Egodystonic Studios 2018

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Egodystonic.Atomics.Benchmarks.DummyObjects;
using Egodystonic.Atomics.Numerics;
using static Egodystonic.Atomics.Benchmarks.BenchmarkUtils;

namespace Egodystonic.Atomics.Benchmarks.Internal {
	/// <summary>
	/// Benchmark used to justify inlining on various methods.
	/// </summary>
	[CoreJob, MemoryDiagnoser]
	public class InlinedVsNonInlinedInt {
		#region Parameters
		const int NumIterations = 100_000;

		public static object[] ThreadCounts { get; } = BenchmarkUtils.ThreadCounts;

		[ParamsSource(nameof(ThreadCounts))]
		public int NumThreads { get; set; }
		#endregion

		#region Test Setup
		
		#endregion

		#region Benchmark: Inlined
		ManualResetEvent _inlinedBarrier;
		List<Thread> _inlinedThreads;
		AtomicInt _inlined;

		[IterationSetup(Target = nameof(WithInlined))]
		public void CreateInlinedContext() {
			_inlined = new AtomicInt(0);
			_inlinedBarrier = new ManualResetEvent(false);
			_inlinedThreads = new List<Thread>();
			PrepareThreads(NumThreads, _inlinedBarrier, WithInlined_Entry, _inlinedThreads);
		}

		[Benchmark(Baseline = true)]
		public void WithInlined() {
			ExecutePreparedThreads(_inlinedBarrier, _inlinedThreads);
		}

		void WithInlined_Entry() {
			for (var i = 0; i < NumIterations; ++i) {
				var incResult = _inlined.Increment();
				Assert(incResult.NewValue == incResult.PreviousValue + 1);

				SimulateContention(ContentionLevel.B_Moderate);

				var addResult = _inlined.Add(10);
				Assert(addResult.NewValue == addResult.PreviousValue + 10);

				SimulateContention(ContentionLevel.B_Moderate);

				var subResult = _inlined.Subtract(9);
				Assert(subResult.NewValue == subResult.PreviousValue - 9);

				SimulateContention(ContentionLevel.B_Moderate);

				var multResult = _inlined.MultiplyBy(3);
				Assert(multResult.NewValue == multResult.PreviousValue * 3);

				SimulateContention(ContentionLevel.B_Moderate);

				var divResult = _inlined.DivideBy(4);
				Assert(divResult.NewValue == divResult.PreviousValue / 4);

				SimulateContention(ContentionLevel.B_Moderate);

				var decResult = _inlined.Decrement();
				Assert(decResult.NewValue == decResult.PreviousValue - 1);

				SimulateContention(ContentionLevel.B_Moderate);

				var value = _inlined.Value;
				var exchVal = _inlined.Exchange(value + 3);
				_inlined.Value = exchVal.PreviousValue - 3;
			}
		}
		#endregion

		#region Benchmark: Non-Inlined
		ManualResetEvent _nonInlinedBarrier;
		List<Thread> _nonInlinedThreads;
		NonInlinedAtomicInt _nonInlined;

		[IterationSetup(Target = nameof(WithNonInlined))]
		public void CreateNonInlinedContext() {
			_nonInlined = new NonInlinedAtomicInt(0);
			_nonInlinedBarrier = new ManualResetEvent(false);
			_nonInlinedThreads = new List<Thread>();
			PrepareThreads(NumThreads, _nonInlinedBarrier, WithNonInlined_Entry, _nonInlinedThreads);
		}

		[Benchmark]
		public void WithNonInlined() {
			ExecutePreparedThreads(_nonInlinedBarrier, _nonInlinedThreads);
		}

		void WithNonInlined_Entry() {
			for (var i = 0; i < NumIterations; ++i) {
				var incResult = _nonInlined.Increment();
				Assert(incResult.NewValue == incResult.PreviousValue + 1);

				SimulateContention(ContentionLevel.B_Moderate);

				var addResult = _nonInlined.Add(10);
				Assert(addResult.NewValue == addResult.PreviousValue + 10);

				SimulateContention(ContentionLevel.B_Moderate);

				var subResult = _nonInlined.Subtract(9);
				Assert(subResult.NewValue == subResult.PreviousValue - 9);

				SimulateContention(ContentionLevel.B_Moderate);

				var multResult = _nonInlined.MultiplyBy(3);
				Assert(multResult.NewValue == multResult.PreviousValue * 3);

				SimulateContention(ContentionLevel.B_Moderate);

				var divResult = _nonInlined.DivideBy(4);
				Assert(divResult.NewValue == divResult.PreviousValue / 4);

				SimulateContention(ContentionLevel.B_Moderate);

				var decResult = _nonInlined.Decrement();
				Assert(decResult.NewValue == decResult.PreviousValue - 1);

				SimulateContention(ContentionLevel.B_Moderate);

				var value = _nonInlined.Value;
				var exchVal = _nonInlined.Exchange(value + 3);
				_nonInlined.Value = exchVal.PreviousValue - 3;
			}
		}
		#endregion
	}
}