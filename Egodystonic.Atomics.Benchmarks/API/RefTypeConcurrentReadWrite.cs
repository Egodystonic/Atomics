// (c) Egodystonic Studios 2018

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BenchmarkDotNet.Attributes;
using static Egodystonic.Atomics.Benchmarks.BenchmarkUtils;

namespace Egodystonic.Atomics.Benchmarks.API {
	/// <summary>
	/// Demonstration of AtomicRef speed for simple exchange/read versus standard lock and ReaderWriterLockSlim.
	/// </summary>
	[CoreJob, MemoryDiagnoser]
	public class RefTypeConcurrentReadWrite {
		#region Parameters
		const int NumIterations = 10_000;

		public static object[] ThreadCounts { get; } = BenchmarkUtils.ThreadCounts;
		public static object[] AllContentionLevels { get; } = BenchmarkUtils.AllContentionLevels;

		[ParamsSource(nameof(ThreadCounts))]
		public int NumThreads { get; set; }

		public int NumWriters => NumThreads;
		public int NumReaders => NumWriters;

		[ParamsSource(nameof(AllContentionLevels))]
		public ContentionLevel ContentionLevel { get; set; }
		#endregion

		#region Test Setup
		string[] _usernames;

		[GlobalSetup]
		public void CreateDummyData() {
			const int MaxUserNameLength = 14;
			const int NumLettersToUse = 26;
			const int CharArrayLen = NumLettersToUse * 100;

			var rng = new Random();
			_usernames = new string[NumIterations * (int) ThreadCounts[ThreadCounts.Length - 1] + 1];
			var builder = new StringBuilder(MaxUserNameLength);
			var letters = Enumerable.Range('a', NumLettersToUse).Select(i => (char)i).ToArray();
			var chars = new char[CharArrayLen];
			for (var i = 0; i < CharArrayLen / NumLettersToUse; ++i) Buffer.BlockCopy(letters, 0, chars, i * NumLettersToUse, NumLettersToUse);
			var curCharArrIndex = 0;

			for (var i = 0; i < _usernames.Length; ++i) {
				var firstNameLen = rng.Next(3, MaxUserNameLength + 1);
				var secondNameLen = rng.Next(3, MaxUserNameLength + 1);
				var totalLen = firstNameLen + secondNameLen;
				if ((CharArrayLen - 1) - curCharArrIndex < totalLen) curCharArrIndex = 0;

				builder.Append(chars, curCharArrIndex, firstNameLen);
				builder.Append(" ");
				builder.Append(chars, curCharArrIndex, secondNameLen);
				curCharArrIndex += totalLen;

				_usernames[i] = builder.ToString();
				builder.Clear();
			}
		}
		#endregion

		#region Benchmark: Atomic Ref
		ManualResetEvent _atomicRefBarrier;
		List<Thread> _atomicRefThreads;
		AtomicRef<User> _atomicUser;

		[IterationSetup(Target = nameof(WithAtomicRef))]
		public void CreateAtomicRefContext() {
			_atomicUser = new AtomicRef<User>(new User(0, _usernames[0]));
			_atomicRefBarrier = new ManualResetEvent(false);
			_atomicRefThreads = new List<Thread>();
			PrepareThreads(NumWriters, _atomicRefBarrier, WithAtomicRef_WriterEntry, _atomicRefThreads);
			PrepareThreads(NumReaders, _atomicRefBarrier, WithAtomicRef_ReaderEntry, _atomicRefThreads);
		}

		[Benchmark(Baseline = true)]
		public void WithAtomicRef() {
			ExecutePreparedThreads(_atomicRefBarrier, _atomicRefThreads);
		}

		void WithAtomicRef_WriterEntry() {
			for (var i = 0; i < NumIterations; ++i) {
				_atomicUser.Exchange(cur => new User(cur.LoginID + 1, _usernames[cur.LoginID + 1]));
				SimulateContention(ContentionLevel);
			}
		}
		void WithAtomicRef_ReaderEntry() {
			for (var i = 0; i < NumIterations; ++i) {
				var curUser = _atomicUser.Value;
				Assert(curUser.Name == _usernames[curUser.LoginID]);
				SimulateContention(ContentionLevel);
			}
		}
		#endregion

		#region Benchmark: Standard Lock
		ManualResetEvent _standardLockBarrier;
		List<Thread> _standardLockThreads;
		object _lockObject;
		User _standardLockUser;

		[IterationSetup(Target = nameof(WithStandardLock))]
		public void CreateStandardLockContext() {
			_lockObject = new object();
			_standardLockUser = new User(0, _usernames[0]);
			_standardLockBarrier = new ManualResetEvent(false);
			_standardLockThreads = new List<Thread>();
			PrepareThreads(NumWriters, _standardLockBarrier, WithStandardLock_WriterEntry, _standardLockThreads);
			PrepareThreads(NumReaders, _standardLockBarrier, WithStandardLock_ReaderEntry, _standardLockThreads);
		}

		[Benchmark]
		public void WithStandardLock() {
			ExecutePreparedThreads(_standardLockBarrier, _standardLockThreads);
		}

		void WithStandardLock_WriterEntry() {
			for (var i = 0; i < NumIterations; ++i) {
				lock (_lockObject) {
					var nextID = _standardLockUser.LoginID + 1;
					_standardLockUser = new User(nextID, _usernames[nextID]);
				}
				SimulateContention(ContentionLevel);
			}
		}
		void WithStandardLock_ReaderEntry() {
			for (var i = 0; i < NumIterations; ++i) {
				User curUser;
				lock (_lockObject) curUser = _standardLockUser;
				Assert(curUser.Name == _usernames[curUser.LoginID]);
				SimulateContention(ContentionLevel);
			}
		}
		#endregion

		#region Benchmark: RWLS
		ManualResetEvent _rwlsBarrier;
		List<Thread> _rwlsThreads;
		ReaderWriterLockSlim _rwls;
		User _rwlsUser;

		[IterationSetup(Target = nameof(WithRWLS))]
		public void CreateRWLSContext() {
			_rwls = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
			_rwlsUser = new User(0, _usernames[0]);
			_rwlsBarrier = new ManualResetEvent(false);
			_rwlsThreads = new List<Thread>();
			PrepareThreads(NumWriters, _rwlsBarrier, WithRWLS_WriterEntry, _rwlsThreads);
			PrepareThreads(NumReaders, _rwlsBarrier, WithRWLS_ReaderEntry, _rwlsThreads);
		}

		[Benchmark]
		public void WithRWLS() {
			ExecutePreparedThreads(_rwlsBarrier, _rwlsThreads);
		}

		void WithRWLS_WriterEntry() {
			for (var i = 0; i < NumIterations; ++i) {
				_rwls.EnterWriteLock();
				var nextID = _rwlsUser.LoginID + 1;
				_rwlsUser = new User(nextID, _usernames[nextID]);
				_rwls.ExitWriteLock();
				SimulateContention(ContentionLevel);
			}
		}
		void WithRWLS_ReaderEntry() {
			for (var i = 0; i < NumIterations; ++i) {
				_rwls.EnterReadLock();
				var curUser = _rwlsUser;
				_rwls.ExitReadLock();
				Assert(curUser.Name == _usernames[curUser.LoginID]);
				SimulateContention(ContentionLevel);
			}
		}
		#endregion
	}
}