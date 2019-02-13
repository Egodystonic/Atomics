// (c) Egodystonic Studios 2018

using System;
using System.Collections.Generic;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Egodystonic.Atomics.Benchmarks.DummyObjects;

namespace Egodystonic.Atomics.Benchmarks.External {
	/// <summary>
	/// Benchmark comparing various atomic value methods for scenarios where reads outnumber writes.
	/// </summary>
	[CoreJob, MemoryDiagnoser]
	public class AtomicValReadWrites {
		#region Parameters
		const int NumIterations = 100_000;
		const int IterationsPerBarrier = 10;

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
		Barrier _atomicValSyncBarrier;

		[IterationSetup(Target = nameof(WithAtomicVal))]
		public void CreateAtomicValContext() {
			_atomicVal = new AtomicVal<Vector2>(new Vector2(-1f, -1f));
			_atomicValBarrier = new ManualResetEvent(false);
			_atomicValThreads = new List<Thread>();
			_atomicValSyncBarrier = new Barrier(NumThreads);
			BenchmarkUtils.PrepareThreads(NumThreads, _atomicValBarrier, WithAtomicVal_Entry, _atomicValThreads);
		}

		[Benchmark]
		public void WithAtomicVal() {
			BenchmarkUtils.ExecutePreparedThreads(_atomicValBarrier, _atomicValThreads);
		}

		void WithAtomicVal_Entry() {
			var result = 0L;

			for (var i = 0; i < NumIterations; i++) {
				if (_atomicVal.Value.L < i) {
					result += _atomicVal.Get().L;
				}
				else {
					result -= _atomicVal.Get().L;
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);

				var curVal = _atomicVal.Get();

				BenchmarkUtils.SimulateContention(ContentionLevel);

				if (curVal.L >= _atomicVal.Get().L) {
					if (_atomicVal.Get().X > _atomicVal.Get().Y) {
						result += (long) _atomicVal.Get().Y;
					}
					else {
						result += (long) _atomicVal.Get().X;
					}
				}
				else {
					if (_atomicVal.Get().X > _atomicVal.Get().Y) {
						result += (long) _atomicVal.Get().X;
					}
					else {
						result += (long) _atomicVal.Get().Y;
					}
				}

				if (_atomicVal.Value.L < i) {
					result += _atomicVal.Get().L;
				}
				else {
					result -= _atomicVal.Get().L;
				}

				curVal = _atomicVal.Get();

				if (curVal.L >= _atomicVal.Get().L) {
					if (_atomicVal.Get().X > _atomicVal.Get().Y) {
						result += (long) _atomicVal.Get().Y;
					}
					else {
						result += (long) _atomicVal.Get().X;
					}
				}
				else {
					if (_atomicVal.Get().X > _atomicVal.Get().Y) {
						result += (long) _atomicVal.Get().X;
					}
					else {
						result += (long) _atomicVal.Get().Y;
					}
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);

				if (i % IterationsPerBarrier == 0) {
					if (_atomicVal.Get().X < curVal.L) {
						result += _atomicVal.TryExchange(new Vector2(curVal.L - 1L), curVal).CurrentValue.L;
					}
					else {
						result += _atomicVal.TryExchange(new Vector2(curVal.L + 1L), curVal).CurrentValue.L;
					}
					_atomicValSyncBarrier.SignalAndWait();
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);
			}

			if (result == 0L) Console.Beep(1000, 100);
		}
		#endregion

		#region Benchmark: Atomic Val Unmanaged
		ManualResetEvent _atomicValUnmanagedBarrier;
		List<Thread> _atomicValUnmanagedThreads;
		AtomicValUnmanaged<Vector2> _atomicValUnmanaged;
		Barrier _atomicValUnmanagedSyncBarrier;

		[IterationSetup(Target = nameof(WithAtomicValUnmanaged))]
		public void CreateAtomicValUnmanagedContext() {
			_atomicValUnmanaged = new AtomicValUnmanaged<Vector2>(new Vector2(-1f, -1f));
			_atomicValUnmanagedBarrier = new ManualResetEvent(false);
			_atomicValUnmanagedThreads = new List<Thread>();
			_atomicValUnmanagedSyncBarrier = new Barrier(NumThreads);
			BenchmarkUtils.PrepareThreads(NumThreads, _atomicValUnmanagedBarrier, WithAtomicValUnmanaged_Entry, _atomicValUnmanagedThreads);
		}

		[Benchmark]
		public void WithAtomicValUnmanaged() {
			BenchmarkUtils.ExecutePreparedThreads(_atomicValUnmanagedBarrier, _atomicValUnmanagedThreads);
		}

		void WithAtomicValUnmanaged_Entry() {
			var result = 0L;

			for (var i = 0; i < NumIterations; i++) {
				if (_atomicValUnmanaged.Value.L < i) {
					result += _atomicValUnmanaged.Get().L;
				}
				else {
					result -= _atomicValUnmanaged.Get().L;
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);

				var curVal = _atomicValUnmanaged.Get();

				BenchmarkUtils.SimulateContention(ContentionLevel);

				if (curVal.L >= _atomicValUnmanaged.Get().L) {
					if (_atomicValUnmanaged.Get().X > _atomicValUnmanaged.Get().Y) {
						result += (long) _atomicValUnmanaged.Get().Y;
					}
					else {
						result += (long) _atomicValUnmanaged.Get().X;
					}
				}
				else {
					if (_atomicValUnmanaged.Get().X > _atomicValUnmanaged.Get().Y) {
						result += (long) _atomicValUnmanaged.Get().X;
					}
					else {
						result += (long) _atomicValUnmanaged.Get().Y;
					}
				}

				if (_atomicValUnmanaged.Value.L < i) {
					result += _atomicValUnmanaged.Get().L;
				}
				else {
					result -= _atomicValUnmanaged.Get().L;
				}

				curVal = _atomicValUnmanaged.Get();

				if (curVal.L >= _atomicValUnmanaged.Get().L) {
					if (_atomicValUnmanaged.Get().X > _atomicValUnmanaged.Get().Y) {
						result += (long) _atomicValUnmanaged.Get().Y;
					}
					else {
						result += (long) _atomicValUnmanaged.Get().X;
					}
				}
				else {
					if (_atomicValUnmanaged.Get().X > _atomicValUnmanaged.Get().Y) {
						result += (long) _atomicValUnmanaged.Get().X;
					}
					else {
						result += (long) _atomicValUnmanaged.Get().Y;
					}
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);

				if (i % IterationsPerBarrier == 0) {
					if (_atomicValUnmanaged.Get().X < curVal.L) {
						result += _atomicValUnmanaged.TryExchange(new Vector2(curVal.L - 1L), curVal).CurrentValue.L;
					}
					else {
						result += _atomicValUnmanaged.TryExchange(new Vector2(curVal.L + 1L), curVal).CurrentValue.L;
					}
					_atomicValUnmanagedSyncBarrier.SignalAndWait();
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);
			}

			if (result == 0L) Console.Beep(1000, 100);
		}
		#endregion

		#region Benchmark: Lock
		ManualResetEvent _lockBarrier;
		List<Thread> _lockThreads;
		Vector2 _lockVal;
		object _lock;
		Barrier _lockSyncBarrier;

		[IterationSetup(Target = nameof(WithLock))]
		public void CreateLockContext() {
			_lock = new object();
			_lockVal = new Vector2(-1f, -1f);
			_lockBarrier = new ManualResetEvent(false);
			_lockThreads = new List<Thread>();
			_lockSyncBarrier = new Barrier(NumThreads);
			BenchmarkUtils.PrepareThreads(NumThreads, _lockBarrier, WithLock_Entry, _lockThreads);
		}

		[Benchmark]
		public void WithLock() {
			BenchmarkUtils.ExecutePreparedThreads(_lockBarrier, _lockThreads);
		}

		void WithLock_Entry() {
			var result = 0L;

			for (var i = 0; i < NumIterations; i++) {
				lock (_lock) {
					if (_lockVal.L < i) {
						result += _lockVal.L;
					}
					else {
						result -= _lockVal.L;
					}
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);

				Vector2 curVal;
				lock (_lock) {
					curVal = _lockVal;
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);

				lock (_lock) {
					if (curVal.L >= _lockVal.L) {
						if (_lockVal.X > _lockVal.Y) {
							result += (long) _lockVal.Y;
						}
						else {
							result += (long) _lockVal.X;
						}
					}
					else {
						if (_lockVal.X > _lockVal.Y) {
							result += (long) _lockVal.X;
						}
						else {
							result += (long) _lockVal.Y;
						}
					}

					if (_lockVal.L < i) {
						result += _lockVal.L;
					}
					else {
						result -= _lockVal.L;
					}

					curVal = _lockVal;

					if (curVal.L >= _lockVal.L) {
						if (_lockVal.X > _lockVal.Y) {
							result += (long) _lockVal.Y;
						}
						else {
							result += (long) _lockVal.X;
						}
					}
					else {
						if (_lockVal.X > _lockVal.Y) {
							result += (long) _lockVal.X;
						}
						else {
							result += (long) _lockVal.Y;
						}
					}
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);

				if (i % IterationsPerBarrier == 0) {
					lock (_lock) {
						if (_lockVal.X < curVal.L) {
							if (_lockVal == curVal) _lockVal = new Vector2(curVal.L - 1L);
							result += _lockVal.L;
						}
						else {
							if (_lockVal == curVal) _lockVal = new Vector2(curVal.L + 1L);
							result += _lockVal.L;
						}
					}

					_lockSyncBarrier.SignalAndWait();
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);
			}

			if (result == 0L) Console.Beep(1000, 100);
		}
		#endregion

		#region Benchmark: Less Granular Lock
		ManualResetEvent _lessGranularLockBarrier;
		List<Thread> _lessGranularLockThreads;
		Vector2 _lessGranularLockVal;
		object _lessGranularLock;
		Barrier _lessGranularLockSyncBarrier;

		[IterationSetup(Target = nameof(WithLessGranularLock))]
		public void CreateLessGranularLockContext() {
			_lessGranularLock = new object();
			_lessGranularLockVal = new Vector2(-1f, -1f);
			_lessGranularLockBarrier = new ManualResetEvent(false);
			_lessGranularLockThreads = new List<Thread>();
			_lessGranularLockSyncBarrier = new Barrier(NumThreads);
			BenchmarkUtils.PrepareThreads(NumThreads, _lessGranularLockBarrier, WithLessGranularLock_Entry, _lessGranularLockThreads);
		}

		[Benchmark]
		public void WithLessGranularLock() {
			BenchmarkUtils.ExecutePreparedThreads(_lessGranularLockBarrier, _lessGranularLockThreads);
		}

		void WithLessGranularLock_Entry() {
			var result = 0L;

			for (var i = 0; i < NumIterations; i++) {
				Vector2 curVal;

				lock (_lessGranularLock) {
					if (_lessGranularLockVal.L < i) {
						result += _lessGranularLockVal.L;
					}
					else {
						result -= _lessGranularLockVal.L;
					}

					BenchmarkUtils.SimulateContention(ContentionLevel);

					curVal = _lessGranularLockVal;

					BenchmarkUtils.SimulateContention(ContentionLevel);

					if (curVal.L >= _lessGranularLockVal.L) {
						if (_lessGranularLockVal.X > _lessGranularLockVal.Y) {
							result += (long) _lessGranularLockVal.Y;
						}
						else {
							result += (long) _lessGranularLockVal.X;
						}
					}
					else {
						if (_lessGranularLockVal.X > _lessGranularLockVal.Y) {
							result += (long) _lessGranularLockVal.X;
						}
						else {
							result += (long) _lessGranularLockVal.Y;
						}
					}

					if (_lessGranularLockVal.L < i) {
						result += _lessGranularLockVal.L;
					}
					else {
						result -= _lessGranularLockVal.L;
					}

					curVal = _lessGranularLockVal;

					if (curVal.L >= _lessGranularLockVal.L) {
						if (_lessGranularLockVal.X > _lessGranularLockVal.Y) {
							result += (long) _lessGranularLockVal.Y;
						}
						else {
							result += (long) _lessGranularLockVal.X;
						}
					}
					else {
						if (_lessGranularLockVal.X > _lessGranularLockVal.Y) {
							result += (long) _lessGranularLockVal.X;
						}
						else {
							result += (long) _lessGranularLockVal.Y;
						}
					}
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);

				if (i % IterationsPerBarrier == 0) {
					lock (_lessGranularLock) {
						if (_lessGranularLockVal.X < curVal.L) {
							if (_lessGranularLockVal == curVal) _lessGranularLockVal = new Vector2(curVal.L - 1L);
							result += _lessGranularLockVal.L;
						}
						else {
							if (_lessGranularLockVal == curVal) _lessGranularLockVal = new Vector2(curVal.L + 1L);
							result += _lessGranularLockVal.L;
						}
					}

					_lessGranularLockSyncBarrier.SignalAndWait();
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);
			}

			if (result == 0L) Console.Beep(1000, 100);
		}
		#endregion

		#region Benchmark: Reader Writer Lock Slim
		ManualResetEvent _rwlsBarrier;
		List<Thread> _rwlsThreads;
		Vector2 _rwlsVal;
		ReaderWriterLockSlim _rwls;
		Barrier _rwlsSyncBarrier;

		[IterationSetup(Target = nameof(WithRWLS))]
		public void CreateRWLSContext() {
			_rwls = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
			_rwlsVal = new Vector2(-1f, -1f);
			_rwlsBarrier = new ManualResetEvent(false);
			_rwlsThreads = new List<Thread>();
			_rwlsSyncBarrier = new Barrier(NumThreads);
			BenchmarkUtils.PrepareThreads(NumThreads, _rwlsBarrier, WithRWLS_Entry, _rwlsThreads);
		}

		[Benchmark]
		public void WithRWLS() {
			BenchmarkUtils.ExecutePreparedThreads(_rwlsBarrier, _rwlsThreads);
		}

		void WithRWLS_Entry() {
			var result = 0L;

			for (var i = 0; i < NumIterations; i++) {
				_rwls.EnterReadLock();
				if (_rwlsVal.L < i) {
					result += _rwlsVal.L;
				}
				else {
					result -= _rwlsVal.L;
				}
				_rwls.ExitReadLock();

				BenchmarkUtils.SimulateContention(ContentionLevel);

				_rwls.EnterReadLock();
				var curVal = _rwlsVal;
				_rwls.ExitReadLock();

				BenchmarkUtils.SimulateContention(ContentionLevel);

				_rwls.EnterReadLock();
				if (curVal.L >= _rwlsVal.L) {
					if (_rwlsVal.X > _rwlsVal.Y) {
						result += (long) _rwlsVal.Y;
					}
					else {
						result += (long) _rwlsVal.X;
					}
				}
				else {
					if (_rwlsVal.X > _rwlsVal.Y) {
						result += (long) _rwlsVal.X;
					}
					else {
						result += (long) _rwlsVal.Y;
					}
				}

				if (_rwlsVal.L < i) {
					result += _rwlsVal.L;
				}
				else {
					result -= _rwlsVal.L;
				}

				curVal = _rwlsVal;

				if (curVal.L >= _rwlsVal.L) {
					if (_rwlsVal.X > _rwlsVal.Y) {
						result += (long) _rwlsVal.Y;
					}
					else {
						result += (long) _rwlsVal.X;
					}
				}
				else {
					if (_rwlsVal.X > _rwlsVal.Y) {
						result += (long) _rwlsVal.X;
					}
					else {
						result += (long) _rwlsVal.Y;
					}
				}
				_rwls.ExitReadLock();

				BenchmarkUtils.SimulateContention(ContentionLevel);

				if (i % IterationsPerBarrier == 0) {
					_rwls.EnterWriteLock();
					if (_rwlsVal.X < curVal.L) {
						if (_rwlsVal == curVal) _rwlsVal = new Vector2(curVal.L - 1L);
						result += _rwlsVal.L;
					}
					else {
						if (_rwlsVal == curVal) _rwlsVal = new Vector2(curVal.L + 1L);
						result += _rwlsVal.L;
					}
					_rwls.ExitWriteLock();

					_rwlsSyncBarrier.SignalAndWait();
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);
			}

			if (result == 0L) Console.Beep(1000, 100);
		}
		#endregion
	}
}