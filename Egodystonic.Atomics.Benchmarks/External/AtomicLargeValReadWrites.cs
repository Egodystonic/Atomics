// (c) Egodystonic Studios 2018

using System;
using System.Collections.Generic;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Egodystonic.Atomics.Benchmarks.DummyObjects;
using Egodystonic.Atomics.Numerics;

namespace Egodystonic.Atomics.Benchmarks.External {
	/// <summary>
	/// Benchmark comparing various atomic value methods for scenarios where reads outnumber writes, for large structs.
	/// </summary>
	[CoreJob, MemoryDiagnoser]
	public class AtomicLargeValReadWrites {
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
		AtomicVal<Val64> _atomicVal;
		Barrier _atomicValSyncBarrier;

		[IterationSetup(Target = nameof(WithAtomicVal))]
		public void CreateAtomicValContext() {
			_atomicVal = new AtomicVal<Val64>(new Val64(-1L));
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
				if (_atomicVal.Value.A < i) {
					result += _atomicVal.Get().A;
				}
				else {
					result -= _atomicVal.Get().A;
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);

				var curVal = _atomicVal.Get();

				BenchmarkUtils.SimulateContention(ContentionLevel);

				if (curVal.A >= _atomicVal.Get().A) {
					if (_atomicVal.Get().B > _atomicVal.Get().C) {
						result += _atomicVal.Get().C;
					}
					else {
						result += _atomicVal.Get().B;
					}
				}
				else {
					if (_atomicVal.Get().B > _atomicVal.Get().C) {
						result += _atomicVal.Get().B;
					}
					else {
						result += _atomicVal.Get().C;
					}
				}

				if (_atomicVal.Value.A < i) {
					result += _atomicVal.Get().A;
				}
				else {
					result -= _atomicVal.Get().A;
				}

				curVal = _atomicVal.Get();

				if (curVal.A >= _atomicVal.Get().A) {
					if (_atomicVal.Get().B > _atomicVal.Get().C) {
						result += _atomicVal.Get().C;
					}
					else {
						result += _atomicVal.Get().B;
					}
				}
				else {
					if (_atomicVal.Get().B > _atomicVal.Get().C) {
						result += _atomicVal.Get().B;
					}
					else {
						result += _atomicVal.Get().C;
					}
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);

				if (i % IterationsPerBarrier == 0) {
					if (_atomicVal.Get().B < curVal.A) {
						result += _atomicVal.TryExchange(new Val64(curVal.A - 1L), curVal).CurrentValue.A;
					}
					else {
						result += _atomicVal.TryExchange(new Val64(curVal.A + 1L), curVal).CurrentValue.A;
					}
					_atomicValSyncBarrier.SignalAndWait();
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);
			}

			if (result == 0L) Console.Beep(1000, 100);
		}
		#endregion

		#region Benchmark: Lock
		ManualResetEvent _lockBarrier;
		List<Thread> _lockThreads;
		Val64 _lockVal;
		object _lock;
		Barrier _lockSyncBarrier;

		[IterationSetup(Target = nameof(WithLock))]
		public void CreateLockContext() {
			_lock = new object();
			_lockVal = new Val64(-1L);
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
					if (_lockVal.A < i) {
						result += _lockVal.A;
					}
					else {
						result -= _lockVal.A;
					}
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);

				Val64 curVal;
				lock (_lock) {
					curVal = _lockVal;
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);

				lock (_lock) {
					if (curVal.A >= _lockVal.A) {
						if (_lockVal.B > _lockVal.C) {
							result += _lockVal.C;
						}
						else {
							result += _lockVal.B;
						}
					}
					else {
						if (_lockVal.B > _lockVal.C) {
							result += _lockVal.B;
						}
						else {
							result += _lockVal.C;
						}
					}

					if (_lockVal.A < i) {
						result += _lockVal.A;
					}
					else {
						result -= _lockVal.A;
					}

					curVal = _lockVal;

					if (curVal.A >= _lockVal.A) {
						if (_lockVal.B > _lockVal.C) {
							result += _lockVal.C;
						}
						else {
							result += _lockVal.B;
						}
					}
					else {
						if (_lockVal.B > _lockVal.C) {
							result += _lockVal.B;
						}
						else {
							result += _lockVal.C;
						}
					}
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);

				if (i % IterationsPerBarrier == 0) {
					lock (_lock) {
						if (_lockVal.B < curVal.A) {
							if (_lockVal == curVal) _lockVal = new Val64(curVal.A - 1L);
							result += _lockVal.A;
						}
						else {
							if (_lockVal == curVal) _lockVal = new Val64(curVal.A + 1L);
							result += _lockVal.A;
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
		Val64 _lessGranularLockVal;
		object _lessGranularLock;
		Barrier _lessGranularLockSyncBarrier;

		[IterationSetup(Target = nameof(WithLessGranularLock))]
		public void CreateLessGranularLockContext() {
			_lessGranularLock = new object();
			_lessGranularLockVal = new Val64(-1L);
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
				Val64 curVal;

				lock (_lessGranularLock) {
					if (_lessGranularLockVal.A < i) {
						result += _lessGranularLockVal.A;
					}
					else {
						result -= _lessGranularLockVal.A;
					}

					BenchmarkUtils.SimulateContention(ContentionLevel);

					curVal = _lessGranularLockVal;

					BenchmarkUtils.SimulateContention(ContentionLevel);

					if (curVal.A >= _lessGranularLockVal.A) {
						if (_lessGranularLockVal.B > _lessGranularLockVal.C) {
							result += _lessGranularLockVal.C;
						}
						else {
							result += _lessGranularLockVal.B;
						}
					}
					else {
						if (_lessGranularLockVal.B > _lessGranularLockVal.C) {
							result += _lessGranularLockVal.B;
						}
						else {
							result += _lessGranularLockVal.C;
						}
					}

					if (_lessGranularLockVal.A < i) {
						result += _lessGranularLockVal.A;
					}
					else {
						result -= _lessGranularLockVal.A;
					}

					curVal = _lessGranularLockVal;

					if (curVal.A >= _lessGranularLockVal.A) {
						if (_lessGranularLockVal.B > _lessGranularLockVal.C) {
							result += _lessGranularLockVal.C;
						}
						else {
							result += _lessGranularLockVal.B;
						}
					}
					else {
						if (_lessGranularLockVal.B > _lessGranularLockVal.C) {
							result += _lessGranularLockVal.B;
						}
						else {
							result += _lessGranularLockVal.C;
						}
					}
				}

				BenchmarkUtils.SimulateContention(ContentionLevel);

				if (i % IterationsPerBarrier == 0) {
					lock (_lessGranularLock) {
						if (_lessGranularLockVal.B < curVal.A) {
							if (_lessGranularLockVal == curVal) _lessGranularLockVal = new Val64(curVal.A - 1L);
							result += _lessGranularLockVal.A;
						}
						else {
							if (_lessGranularLockVal == curVal) _lessGranularLockVal = new Val64(curVal.A + 1L);
							result += _lessGranularLockVal.A;
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
		Val64 _rwlsVal;
		ReaderWriterLockSlim _rwls;
		Barrier _rwlsSyncBarrier;

		[IterationSetup(Target = nameof(WithRWLS))]
		public void CreateRWLSContext() {
			_rwls = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
			_rwlsVal = new Val64(-1L);
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
				if (_rwlsVal.A < i) {
					result += _rwlsVal.A;
				}
				else {
					result -= _rwlsVal.A;
				}
				_rwls.ExitReadLock();

				BenchmarkUtils.SimulateContention(ContentionLevel);

				_rwls.EnterReadLock();
				var curVal = _rwlsVal;
				_rwls.ExitReadLock();

				BenchmarkUtils.SimulateContention(ContentionLevel);

				_rwls.EnterReadLock();
				if (curVal.A >= _rwlsVal.A) {
					if (_rwlsVal.B > _rwlsVal.C) {
						result += _rwlsVal.C;
					}
					else {
						result += _rwlsVal.B;
					}
				}
				else {
					if (_rwlsVal.B > _rwlsVal.C) {
						result += _rwlsVal.B;
					}
					else {
						result += _rwlsVal.C;
					}
				}

				if (_rwlsVal.A < i) {
					result += _rwlsVal.A;
				}
				else {
					result -= _rwlsVal.A;
				}

				curVal = _rwlsVal;

				if (curVal.A >= _rwlsVal.A) {
					if (_rwlsVal.B > _rwlsVal.C) {
						result += _rwlsVal.C;
					}
					else {
						result += _rwlsVal.B;
					}
				}
				else {
					if (_rwlsVal.B > _rwlsVal.C) {
						result += _rwlsVal.B;
					}
					else {
						result += _rwlsVal.C;
					}
				}
				_rwls.ExitReadLock();

				BenchmarkUtils.SimulateContention(ContentionLevel);

				if (i % IterationsPerBarrier == 0) {
					_rwls.EnterWriteLock();
					if (_rwlsVal.B < curVal.A) {
						if (_rwlsVal == curVal) _rwlsVal = new Val64(curVal.A - 1L);
						result += _rwlsVal.A;
					}
					else {
						if (_rwlsVal == curVal) _rwlsVal = new Val64(curVal.A + 1L);
						result += _rwlsVal.A;
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