// (c) Egodystonic Studios 2018

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Egodystonic.Atomics.Numerics;
using static Egodystonic.Atomics.Benchmarks.BenchmarkUtils;

namespace Egodystonic.Atomics.Benchmarks.Internal {
	/// <summary>
	/// Benchmark used to justify indirection of all delegate-consuming functions via extension methods to one 'master' that takes all parameters.
	/// Although the code performance *is* worse than manually implementing all variants of TryExchange, the delegate-consuming functions are not
	/// considered 'hot path' as the slowdown is already implied by using Funcs or Actions. Users of the library writing high-speed loops should
	/// use the comparand-version of TryExchange or similar methods and implement those loops manually.
	/// It may be that when C# implements function pointers we should revisit this part of the API.
	/// </summary>
	[CoreJob, MemoryDiagnoser]
	public class DelegatedTryExchangeVsDirect {
		#region Parameters
		const int NumIterations = 1_000_000;

		public static object[] ThreadCounts { get; } = BenchmarkUtils.ThreadCounts;

		[ParamsSource(nameof(ThreadCounts))]
		public int NumThreads { get; set; }
		#endregion

		#region Test Setup
		
		#endregion

		#region Benchmark: Delegated TryExchange
		ManualResetEvent _delegatedBarrier;
		List<Thread> _delegatedThreads;
		User _delegatedUser;

		[IterationSetup(Target = nameof(DelegatedTryExchange))]
		public void CreateDelegatedTryExchangeContext() {
			_delegatedBarrier = new ManualResetEvent(false);
			_delegatedThreads = new List<Thread>();
			_delegatedUser = new User(0, String.Empty);
			PrepareThreads(NumThreads, _delegatedBarrier, DelegatedTryExchange_Entry, _delegatedThreads);
		}

		[Benchmark]
		public void DelegatedTryExchange() {
			ExecutePreparedThreads(_delegatedBarrier, _delegatedThreads);
		}

		void DelegatedTryExchange_Entry() {
			for (var i = 0; i < NumIterations; ++i) {
				var curValue = Volatile.Read(ref _delegatedUser);
				var res = DelegatedTryExchange(new User(curValue.LoginID + 1, String.Empty), c => c.LoginID == curValue.LoginID);
				if (res.ValueWasSet) Assert(res.PreviousValue.LoginID == curValue.LoginID);
				else Assert(res.PreviousValue.LoginID != curValue.LoginID);
				SimulateContention(ContentionLevel.C_High);
			}
		}

		(bool ValueWasSet, User PreviousValue) DelegatedTryExchange(User newValue, Func<User, bool> predicate) {
			var res = DelegatedTryExchange<User, object>((_, ctx) => ctx, (cur, _, __) => predicate(cur), newValue, null);
			return (res.ValueWasSet, res.PreviousValue);
		}

		(bool ValueWasSet, User PreviousValue, User NewValue) DelegatedTryExchange<TMapContext, TPredicateContext>(Func<User, TMapContext, User> mapFunc, Func<User, User, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
			bool trySetValue;
			User curValue;
			User newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Volatile.Read(ref _delegatedUser);
				newValue = mapFunc(curValue, mapContext);
				trySetValue = predicate(curValue, newValue, predicateContext);

				if (!trySetValue) break;

				if (Interlocked.CompareExchange(ref _delegatedUser, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue, newValue);
		}
		#endregion

		#region Benchmark: Direct TryExchange
		ManualResetEvent _directBarrier;
		List<Thread> _directThreads;
		User _directUser;

		[IterationSetup(Target = nameof(DirectTryExchange))]
		public void CreateDirectTryExchangeContext() {
			_directBarrier = new ManualResetEvent(false);
			_directThreads = new List<Thread>();
			_directUser = new User(0, String.Empty);
			PrepareThreads(NumThreads, _directBarrier, DirectTryExchange_Entry, _directThreads);
		}

		[Benchmark]
		public void DirectTryExchange() {
			ExecutePreparedThreads(_directBarrier, _directThreads);
		}

		void DirectTryExchange_Entry() {
			for (var i = 0; i < NumIterations; ++i) {
				var curValue = Volatile.Read(ref _directUser);
				var res = DirectTryExchange(new User(curValue.LoginID + 1, String.Empty), c => c.LoginID == curValue.LoginID);
				if (res.ValueWasSet) Assert(res.PreviousValue.LoginID == curValue.LoginID);
				else Assert(res.PreviousValue.LoginID != curValue.LoginID);
				SimulateContention(ContentionLevel.C_High);
			}
		}

		(bool ValueWasSet, User PreviousValue) DirectTryExchange(User newValue, Func<User, bool> predicate) {
			bool trySetValue;
			User curValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Volatile.Read(ref _directUser);
				trySetValue = predicate(curValue);

				if (!trySetValue || Interlocked.CompareExchange(ref _directUser, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue);
		}
		#endregion

		#region Benchmark: Comparand TryExchange
		ManualResetEvent _comparandBarrier;
		List<Thread> _comparandThreads;
		User _comparandUser;

		[IterationSetup(Target = nameof(ComparandTryExchange))]
		public void CreateComparandTryExchangeContext() {
			_comparandBarrier = new ManualResetEvent(false);
			_comparandThreads = new List<Thread>();
			_comparandUser = new User(0, String.Empty);
			PrepareThreads(NumThreads, _comparandBarrier, ComparandTryExchange_Entry, _comparandThreads);
		}

		[Benchmark(Baseline = true)]
		public void ComparandTryExchange() {
			ExecutePreparedThreads(_comparandBarrier, _comparandThreads);
		}

		void ComparandTryExchange_Entry() {
			for (var i = 0; i < NumIterations; ++i) {
				var curValue = Volatile.Read(ref _comparandUser);
				var res = ComparandTryExchange(new User(curValue.LoginID + 1, String.Empty), curValue);
				if (res.ValueWasSet) Assert(res.PreviousValue.LoginID == curValue.LoginID);
				else Assert(res.PreviousValue.LoginID != curValue.LoginID);
				SimulateContention(ContentionLevel.C_High);
			}
		}

		(bool ValueWasSet, User PreviousValue) ComparandTryExchange(User newValue, User comparand) {
			var oldVal = Interlocked.CompareExchange(ref _comparandUser, newValue, comparand);
			return (oldVal == comparand, oldVal);
		}
		#endregion
	}
}