// (c) Egodystonic Studios 2018

using System;
using System.Collections.Generic;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Egodystonic.Atomics.Benchmarks.DummyObjects;
using Egodystonic.Atomics.Numerics;

namespace Egodystonic.Atomics.Benchmarks.External {
	/// <summary>
	/// Benchmark comparing atomic lib's unmanaged val vs a highly bespoke solution that uses built-in interlocked functionality and a union of the type with long to achieve this.
	/// </summary>
	[CoreJob, MemoryDiagnoser]
	public class UnmanagedValVsManualLoop {
		#region Parameters
		const int NumIterations = 10_000;

		public static object[] ThreadCounts { get; } = BenchmarkUtils.ThreadCounts;
		public static object[] AllContentionLevels { get; } = BenchmarkUtils.AllContentionLevels;

		[ParamsSource(nameof(ThreadCounts))]
		public int NumThreads { get; set; }

		[ParamsSource(nameof(AllContentionLevels))]
		public ContentionLevel ContentionLevel { get; set; }
		#endregion

		#region Test Setup
		
		#endregion

		#region Benchmark: Atomic Val
		ManualResetEvent _atomicValBarrier;
		List<Thread> _atomicValThreads;
		AtomicValUnmanaged<Vector2> _atomicVal;
		AtomicInt32 _atomicValRemainingThreadCount;

		[IterationSetup(Target = nameof(WithAtomicVal))]
		public void CreateAtomicValContext() {
			_atomicVal = new AtomicValUnmanaged<Vector2>(new Vector2(5f, 10f));
			_atomicValBarrier = new ManualResetEvent(false);
			_atomicValThreads = new List<Thread>();
			_atomicValRemainingThreadCount = new AtomicInt32(NumThreads);
			BenchmarkUtils.PrepareThreads(NumThreads, _atomicValBarrier, WithAtomicVal_EntryA, _atomicValThreads);
			BenchmarkUtils.PrepareThreads(NumThreads, _atomicValBarrier, WithAtomicVal_EntryB, _atomicValThreads);
		}

		[Benchmark(Baseline = true)]
		public void WithAtomicVal() {
			BenchmarkUtils.ExecutePreparedThreads(_atomicValBarrier, _atomicValThreads);
		}

		void WithAtomicVal_EntryA() {
			while (_atomicValRemainingThreadCount > 0) {
				var curVal = _atomicVal.Value;
				_atomicVal.TryExchange(new Vector2(curVal.Y, curVal.X), curVal);

				BenchmarkUtils.SimulateContention(ContentionLevel);
			}
		}

		void WithAtomicVal_EntryB() {
			for (var i = 0; i < NumIterations; ++i) {
				var curVal = _atomicVal.Value;
				_atomicVal.SpinWaitForValue(new Vector2(curVal.Y, curVal.X));

				BenchmarkUtils.SimulateContention(ContentionLevel);

				_atomicVal.SpinWaitForExchange(curVal, new Vector2(curVal.Y, curVal.X));

				BenchmarkUtils.SimulateContention(ContentionLevel);
			}

			_atomicValRemainingThreadCount.Decrement();
		}
		#endregion

		#region Benchmark: Manual Loop
		ManualResetEvent _manualLoopBarrier;
		List<Thread> _manualLoopThreads;
		Vector2 _manualLoopVector2;
		AtomicInt32 _manualLoopRemainingThreadCount;

		[IterationSetup(Target = nameof(WithManualLoop))]
		public void CreateManualLoopContext() {
			_manualLoopVector2 = new Vector2(5f, 10f);
			_manualLoopBarrier = new ManualResetEvent(false);
			_manualLoopThreads = new List<Thread>();
			_manualLoopRemainingThreadCount = new AtomicInt32(NumThreads);
			BenchmarkUtils.PrepareThreads(NumThreads, _manualLoopBarrier, WithManualLoop_EntryA, _manualLoopThreads);
			BenchmarkUtils.PrepareThreads(NumThreads, _manualLoopBarrier, WithManualLoop_EntryB, _manualLoopThreads);
		}

		[Benchmark]
		public void WithManualLoop() {
			BenchmarkUtils.ExecutePreparedThreads(_manualLoopBarrier, _manualLoopThreads);
		}

		void WithManualLoop_EntryA() {
			while (_manualLoopRemainingThreadCount > 0) {
				var curVal = Interlocked.Read(ref _manualLoopVector2.L);
				var curValAsVec = new Vector2(curVal);
				Interlocked.CompareExchange(ref _manualLoopVector2.L, new Vector2(curValAsVec.Y, curValAsVec.X).L, curVal);

				BenchmarkUtils.SimulateContention(ContentionLevel);
			}
		}

		void WithManualLoop_EntryB() {
			for (var i = 0; i < NumIterations; ++i) {
				var curVal = Interlocked.Read(ref _manualLoopVector2.L);
				var curValAsVec = new Vector2(curVal);

				var spinner = new SpinWait();
				var targetVal = new Vector2(curValAsVec.Y, curValAsVec.X).L;
				while (true) {
					if (Interlocked.Read(ref _manualLoopVector2.L) == targetVal) break;
					spinner.SpinOnce();
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);

				spinner = new SpinWait();

				while (true) {
					if (Interlocked.CompareExchange(ref _manualLoopVector2.L, targetVal, curVal) == curVal) break;
					spinner.SpinOnce();
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);
			}

			_manualLoopRemainingThreadCount.Decrement();
		}
		#endregion
	}
}