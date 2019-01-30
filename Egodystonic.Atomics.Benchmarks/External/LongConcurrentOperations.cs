// (c) Egodystonic Studios 2018

using System;
using System.Collections.Generic;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Egodystonic.Atomics.Numerics;

namespace Egodystonic.Atomics.Benchmarks.External {
	/// <summary>
	/// Benchmark comparing atomic lib vs standard locking for concurrent long ops
	/// </summary>
	[CoreJob, MemoryDiagnoser]
	public class LongConcurrentOperations {
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

		#region Benchmark: Atomic Long
		ManualResetEvent _atomicLongBarrier;
		List<Thread> _atomicLongThreads;
		AtomicLong _atomicLong;

		[IterationSetup(Target = nameof(WithAtomicLong))]
		public void CreateAtomicLongContext() {
			_atomicLong = new AtomicLong(0L);
			_atomicLongBarrier = new ManualResetEvent(false);
			_atomicLongThreads = new List<Thread>();
			BenchmarkUtils.PrepareThreads(NumThreads, _atomicLongBarrier, WithAtomicLong_Entry, _atomicLongThreads);
		}

		[Benchmark(Baseline = true)]
		public void WithAtomicLong() {
			BenchmarkUtils.ExecutePreparedThreads(_atomicLongBarrier, _atomicLongThreads);
		}

		void WithAtomicLong_Entry() {
			for (var i = 0; i < NumIterations; ++i) {
				_atomicLong.Decrement();
				_atomicLong.Increment();

				BenchmarkUtils.SimulateContention(ContentionLevel);

				_atomicLong.Increment();
				var curVal = _atomicLong.Value;
				curVal = _atomicLong.Exchange(curVal * 2L).CurrentValue;

				BenchmarkUtils.SimulateContention(ContentionLevel);

				curVal = _atomicLong.TryExchange(curVal / 2L, curVal).CurrentValue;
				var prevVal = _atomicLong.TryBoundedExchange(curVal + 10L, curVal - 3L, curVal + 3L).PreviousValue;
				if (prevVal == curVal) _atomicLong.Subtract(10L);

				BenchmarkUtils.SimulateContention(ContentionLevel);
			}
		}
		#endregion

		#region Benchmark: Locked Long
		ManualResetEvent _lockedLongBarrier;
		List<Thread> _lockedLongThreads;
		object _lockedLongLock;
		long _lockedLong;

		[IterationSetup(Target = nameof(WithLockedLong))]
		public void CreateLockedLongContext() {
			_lockedLongLock = new object();
			_lockedLong = new AtomicLong(0L);
			_lockedLongBarrier = new ManualResetEvent(false);
			_lockedLongThreads = new List<Thread>();
			BenchmarkUtils.PrepareThreads(NumThreads, _lockedLongBarrier, WithLockedLong_Entry, _lockedLongThreads);
		}

		[Benchmark]
		public void WithLockedLong() {
			BenchmarkUtils.ExecutePreparedThreads(_lockedLongBarrier, _lockedLongThreads);
		}

		void WithLockedLong_Entry() {
			for (var i = 0; i < NumIterations; ++i) {
				lock (_lockedLongLock) _lockedLong--;
				lock (_lockedLongLock) _lockedLong++;

				BenchmarkUtils.SimulateContention(ContentionLevel);

				lock (_lockedLongLock) _lockedLong++;
				long curVal;
				lock (_lockedLongLock) curVal = _lockedLong;
				lock (_lockedLongLock) {
					curVal = _lockedLong = curVal * 2L;
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);

				lock (_lockedLongLock) {
					if (_lockedLong == curVal) _lockedLong = curVal / 2L;
					curVal = _lockedLong;
				}
				long prevVal;
				lock (_lockedLongLock) {
					prevVal = _lockedLong;
					if (_lockedLong >= curVal - 3L && _lockedLong < curVal + 3L) _lockedLong = curVal + 10L;
				}
				if (prevVal == curVal) {
					lock (_lockedLongLock) _lockedLong -= 10L;
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);
			}
		}
		#endregion

		#region Benchmark: Locked Long Less Granular Locking
		ManualResetEvent _lockedLessGranularLongBarrier;
		List<Thread> _lockedLessGranularLongThreads;
		object _lockedLessGranularLongLock;
		long _lockedLessGranularLong;

		[IterationSetup(Target = nameof(WithLockedLongLessGranular))]
		public void CreateLockedLongLessGranularContext() {
			_lockedLessGranularLongLock = new object();
			_lockedLessGranularLong = new AtomicLong(0L);
			_lockedLessGranularLongBarrier = new ManualResetEvent(false);
			_lockedLessGranularLongThreads = new List<Thread>();
			BenchmarkUtils.PrepareThreads(NumThreads, _lockedLessGranularLongBarrier, WithLockedLongLessGranular_Entry, _lockedLessGranularLongThreads);
		}

		[Benchmark]
		public void WithLockedLongLessGranular() {
			BenchmarkUtils.ExecutePreparedThreads(_lockedLessGranularLongBarrier, _lockedLessGranularLongThreads);
		}

		void WithLockedLongLessGranular_Entry() {
			for (var i = 0; i < NumIterations; ++i) {
				lock (_lockedLessGranularLongLock) {
					_lockedLessGranularLong--;
					_lockedLessGranularLong++;
				}
				
				BenchmarkUtils.SimulateContention(ContentionLevel);

				long curVal;
				lock (_lockedLessGranularLongLock) {
					_lockedLessGranularLong++;
					curVal = _lockedLessGranularLong;
					curVal = _lockedLessGranularLong = curVal * 2L;
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);

				lock (_lockedLessGranularLongLock) {
					if (_lockedLessGranularLong == curVal) _lockedLessGranularLong = curVal / 2L;
					curVal = _lockedLessGranularLong;
					var prevVal = _lockedLessGranularLong;
					if (_lockedLessGranularLong >= curVal - 3L && _lockedLessGranularLong < curVal + 3L) _lockedLessGranularLong = curVal + 10L;
					if (prevVal == curVal) _lockedLessGranularLong -= 10L;
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);
			}
		}
		#endregion
	}
}