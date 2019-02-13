// (c) Egodystonic Studios 2018

using System;
using System.Collections.Generic;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Egodystonic.Atomics.Benchmarks.DummyObjects;

namespace Egodystonic.Atomics.Benchmarks.External {
	/// <summary>
	/// Benchmark comparing various atomic value methods.
	/// Should be noted that the scenario this is testing (many concurrent, consecutive accesses with a lot of writes and little time between them)
	/// is specifically one where we're hoping this library will excel (at least the unmanaged val); it shouldn't be taken to mean 'Atomic types are always faster than X'.
	/// </summary>
	[CoreJob, MemoryDiagnoser]
	public class AtomicValFastWrites {
		#region Parameters
		const int NumIterations = 100_000;

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
		AtomicVal<Vector2> _atomicVal;

		[IterationSetup(Target = nameof(WithAtomicVal))]
		public void CreateAtomicValContext() {
			_atomicVal = new AtomicVal<Vector2>(new Vector2(-1f, -1f));
			_atomicValBarrier = new ManualResetEvent(false);
			_atomicValThreads = new List<Thread>();
			BenchmarkUtils.PrepareThreads(NumThreads, _atomicValBarrier, WithAtomicVal_Entry, _atomicValThreads);
		}

		[Benchmark]
		public void WithAtomicVal() {
			BenchmarkUtils.ExecutePreparedThreads(_atomicValBarrier, _atomicValThreads);
		}

		void WithAtomicVal_Entry() {
			for (var i = 0; i < NumIterations; i++) {
				var prevVal = _atomicVal.Exchange(new Vector2(i, i)).PreviousValue;
				var curVal = _atomicVal.TryExchange(new Vector2(prevVal.X + 2, prevVal.Y + 2), new Vector2(i, i)).CurrentValue;

				BenchmarkUtils.SimulateContention(ContentionLevel);

				prevVal = _atomicVal.Exchange(new Vector2(curVal.X + 1, curVal.Y + 1)).PreviousValue;
				curVal = _atomicVal.TryExchange(new Vector2(prevVal.X + 2, prevVal.Y + 2), new Vector2(i, i)).CurrentValue;

				BenchmarkUtils.SimulateContention(ContentionLevel);

				prevVal = _atomicVal.Exchange(new Vector2(curVal.X + 1, curVal.Y + 1)).PreviousValue;
				_atomicVal.TryExchange(new Vector2(prevVal.X + 2, prevVal.Y + 2), new Vector2(i, i));

				BenchmarkUtils.SimulateContention(ContentionLevel);
			}
		}
		#endregion

		#region Benchmark: Atomic Val Unmanaged
		ManualResetEvent _atomicValUnmanagedBarrier;
		List<Thread> _atomicValUnmanagedThreads;
		AtomicValUnmanaged<Vector2> _atomicValUnmanaged;

		[IterationSetup(Target = nameof(WithAtomicValUnmanaged))]
		public void CreateAtomicValUnmanagedContext() {
			_atomicValUnmanaged = new AtomicValUnmanaged<Vector2>(new Vector2(-1f, -1f));
			_atomicValUnmanagedBarrier = new ManualResetEvent(false);
			_atomicValUnmanagedThreads = new List<Thread>();
			BenchmarkUtils.PrepareThreads(NumThreads, _atomicValUnmanagedBarrier, WithAtomicValUnmanaged_Entry, _atomicValUnmanagedThreads);
		}

		[Benchmark]
		public void WithAtomicValUnmanaged() {
			BenchmarkUtils.ExecutePreparedThreads(_atomicValUnmanagedBarrier, _atomicValUnmanagedThreads);
		}

		void WithAtomicValUnmanaged_Entry() {
			for (var i = 0; i < NumIterations; i++) {
				var prevVal = _atomicValUnmanaged.Exchange(new Vector2(i, i)).PreviousValue;
				var curVal = _atomicValUnmanaged.TryExchange(new Vector2(prevVal.X + 2, prevVal.Y + 2), new Vector2(i, i)).CurrentValue;

				BenchmarkUtils.SimulateContention(ContentionLevel);

				prevVal = _atomicValUnmanaged.Exchange(new Vector2(curVal.X + 1, curVal.Y + 1)).PreviousValue;
				curVal = _atomicValUnmanaged.TryExchange(new Vector2(prevVal.X + 2, prevVal.Y + 2), new Vector2(i, i)).CurrentValue;

				BenchmarkUtils.SimulateContention(ContentionLevel);

				prevVal = _atomicValUnmanaged.Exchange(new Vector2(curVal.X + 1, curVal.Y + 1)).PreviousValue;
				_atomicValUnmanaged.TryExchange(new Vector2(prevVal.X + 2, prevVal.Y + 2), new Vector2(i, i));

				BenchmarkUtils.SimulateContention(ContentionLevel);
			}
		}
		#endregion

		#region Benchmark: Lock
		ManualResetEvent _lockBarrier;
		List<Thread> _lockThreads;
		Vector2 _lockVal;
		object _lock;

		[IterationSetup(Target = nameof(WithLock))]
		public void CreateLockContext() {
			_lock = new object();
			_lockVal = new Vector2(-1f, -1f);
			_lockBarrier = new ManualResetEvent(false);
			_lockThreads = new List<Thread>();
			BenchmarkUtils.PrepareThreads(NumThreads, _lockBarrier, WithLock_Entry, _lockThreads);
		}

		[Benchmark]
		public void WithLock() {
			BenchmarkUtils.ExecutePreparedThreads(_lockBarrier, _lockThreads);
		}

		void WithLock_Entry() {
			for (var i = 0; i < NumIterations; i++) {
				Vector2 prevVal, curVal;

				lock (_lock) {
					prevVal = _lockVal;
					_lockVal = new Vector2(i, i);

					if (_lockVal == new Vector2(i, i)) _lockVal = new Vector2(prevVal.X + 2, prevVal.Y + 2);
					curVal = _lockVal;
				}
				BenchmarkUtils.SimulateContention(ContentionLevel);
				lock (_lock) {
					prevVal = _lockVal;
					_lockVal = new Vector2(curVal.X + 1, curVal.Y + 1);

					if (_lockVal == new Vector2(i, i)) _lockVal = new Vector2(prevVal.X + 2, prevVal.Y + 2);
					curVal = _lockVal;
				}
				BenchmarkUtils.SimulateContention(ContentionLevel);
				lock (_lock) {
					prevVal = _lockVal;
					_lockVal = new Vector2(curVal.X + 1, curVal.Y + 1);

					if (_lockVal == new Vector2(i, i)) _lockVal = new Vector2(prevVal.X + 2, prevVal.Y + 2);
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);
			}
		}
		#endregion

		#region Benchmark: Less Granular Lock
		ManualResetEvent _lessGranularLockBarrier;
		List<Thread> _lessGranularLockThreads;
		Vector2 _lessGranularLockVal;
		object _lessGranularLock;

		[IterationSetup(Target = nameof(WithLessGranularLock))]
		public void CreateLessGranularLockContext() {
			_lessGranularLock = new object();
			_lessGranularLockVal = new Vector2(-1f, -1f);
			_lessGranularLockBarrier = new ManualResetEvent(false);
			_lessGranularLockThreads = new List<Thread>();
			BenchmarkUtils.PrepareThreads(NumThreads, _lessGranularLockBarrier, WithLessGranularLock_Entry, _lessGranularLockThreads);
		}

		[Benchmark]
		public void WithLessGranularLock() {
			BenchmarkUtils.ExecutePreparedThreads(_lessGranularLockBarrier, _lessGranularLockThreads);
		}

		void WithLessGranularLock_Entry() {
			for (var i = 0; i < NumIterations; i++) {
				Vector2 prevVal, curVal;

				lock (_lessGranularLock) {
					prevVal = _lessGranularLockVal;
					_lessGranularLockVal = new Vector2(i, i);

					if (_lessGranularLockVal == new Vector2(i, i)) _lessGranularLockVal = new Vector2(prevVal.X + 2, prevVal.Y + 2);
					curVal = _lessGranularLockVal;

					BenchmarkUtils.SimulateContention(ContentionLevel);

					prevVal = _lessGranularLockVal;
					_lessGranularLockVal = new Vector2(curVal.X + 1, curVal.Y + 1);

					if (_lessGranularLockVal == new Vector2(i, i)) _lessGranularLockVal = new Vector2(prevVal.X + 2, prevVal.Y + 2);
					curVal = _lessGranularLockVal;

					BenchmarkUtils.SimulateContention(ContentionLevel);

					prevVal = _lessGranularLockVal;
					_lessGranularLockVal = new Vector2(curVal.X + 1, curVal.Y + 1);

					if (_lessGranularLockVal == new Vector2(i, i)) _lessGranularLockVal = new Vector2(prevVal.X + 2, prevVal.Y + 2);
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);
			}
		}
		#endregion

		#region Benchmark: Reader Writer Lock Slim
		ManualResetEvent _rwlsBarrier;
		List<Thread> _rwlsThreads;
		Vector2 _rwlsVal;
		ReaderWriterLockSlim _rwls;

		[IterationSetup(Target = nameof(WithRWLS))]
		public void CreateRWLSContext() {
			_rwls = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
			_rwlsVal = new Vector2(-1f, -1f);
			_rwlsBarrier = new ManualResetEvent(false);
			_rwlsThreads = new List<Thread>();
			BenchmarkUtils.PrepareThreads(NumThreads, _rwlsBarrier, WithRWLS_Entry, _rwlsThreads);
		}

		[Benchmark]
		public void WithRWLS() {
			BenchmarkUtils.ExecutePreparedThreads(_rwlsBarrier, _rwlsThreads);
		}

		void WithRWLS_Entry() {
			for (var i = 0; i < NumIterations; i++) {
				Vector2 prevVal, curVal;

				_rwls.EnterUpgradeableReadLock();
				prevVal = _rwlsVal;
				_rwls.EnterWriteLock();
				_rwlsVal = new Vector2(i, i);
				_rwls.ExitWriteLock();
				_rwls.ExitUpgradeableReadLock();

				_rwls.EnterUpgradeableReadLock();
				if (_rwlsVal == new Vector2(i, i)) {
					_rwls.EnterWriteLock();
					_rwlsVal = new Vector2(prevVal.X + 2, prevVal.Y + 2);
					_rwls.ExitWriteLock();
				}
				curVal = _rwlsVal;
				_rwls.ExitUpgradeableReadLock();

				BenchmarkUtils.SimulateContention(ContentionLevel);

				_rwls.EnterUpgradeableReadLock();
				prevVal = _rwlsVal;
				_rwls.EnterWriteLock();
				_rwlsVal = new Vector2(curVal.X + 1, curVal.Y + 1);
				_rwls.ExitWriteLock();
				_rwls.ExitUpgradeableReadLock();

				_rwls.EnterUpgradeableReadLock();
				if (_rwlsVal == new Vector2(i, i)) {
					_rwls.EnterWriteLock();
					_rwlsVal = new Vector2(prevVal.X + 2, prevVal.Y + 2);
					_rwls.ExitWriteLock();
				}
				curVal = _rwlsVal;
				_rwls.ExitUpgradeableReadLock();

				BenchmarkUtils.SimulateContention(ContentionLevel);

				_rwls.EnterUpgradeableReadLock();
				prevVal = _rwlsVal;
				_rwls.EnterWriteLock();
				_rwlsVal = new Vector2(curVal.X + 1, curVal.Y + 1);
				_rwls.ExitWriteLock();
				_rwls.ExitUpgradeableReadLock();

				_rwls.EnterUpgradeableReadLock();
				if (_rwlsVal == new Vector2(i, i)) {
					_rwls.EnterWriteLock();
					_rwlsVal = new Vector2(prevVal.X + 2, prevVal.Y + 2);
					_rwls.ExitWriteLock();
				}
				_rwls.ExitUpgradeableReadLock();

				BenchmarkUtils.SimulateContention(ContentionLevel);
			}
		}
		#endregion
	}
}