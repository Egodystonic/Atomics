// (c) Egodystonic Studios 2018

using System;
using System.Collections.Generic;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Egodystonic.Atomics.Benchmarks.DummyObjects;
using Egodystonic.Atomics.Numerics;

namespace Egodystonic.Atomics.Benchmarks.Internal {
	/// <summary>
	/// Benchmark comparing methods that take map/predicate funcs. Compares non-contextualized with contextualized,
	/// and also compares the func methods against just writing manual loops using Get()/Set().
	/// </summary>
	[CoreJob, MemoryDiagnoser]
	public class FuncOverheads {
		#region Parameters
		const int NumIterations = 300_000;

		public static object[] ThreadCounts { get; } = BenchmarkUtils.ThreadCounts;
		public static object[] AllContentionLevels { get; } = BenchmarkUtils.AllContentionLevels;

		[ParamsSource(nameof(ThreadCounts))]
		public int NumThreads { get; set; }

		[ParamsSource(nameof(AllContentionLevels))]
		public ContentionLevel ContentionLevel { get; set; }
		#endregion

		#region Test Setup

		#endregion

		#region Benchmark: ClosureCapturing Funcs
		ManualResetEvent _closureCapturingFuncsBarrier;
		List<Thread> _closureCapturingFuncsThreads;
		AtomicRef<User> _closureCapturingFuncsUser;
		AtomicInt64 _closureCapturingFuncsInt64;

		[IterationSetup(Target = nameof(WithClosureCapturingFuncs))]
		public void CreateClosureCapturingFuncsContext() {
			_closureCapturingFuncsBarrier = new ManualResetEvent(false);
			_closureCapturingFuncsThreads = new List<Thread>();
			_closureCapturingFuncsUser = new AtomicRef<User>(new User(0, ""));
			_closureCapturingFuncsInt64 = new AtomicInt64(0L);
			BenchmarkUtils.PrepareThreads(NumThreads, _closureCapturingFuncsBarrier, WithClosureCapturingFuncs_Entry, _closureCapturingFuncsThreads);
		}

		[Benchmark(Baseline = true)]
		public void WithClosureCapturingFuncs() {
			BenchmarkUtils.ExecutePreparedThreads(_closureCapturingFuncsBarrier, _closureCapturingFuncsThreads);
		}

		void WithClosureCapturingFuncs_Entry() {
			var usernameA = "aaaa";
			var usernameB = "bbbb";
			for (var i = 0; i < NumIterations; i++) {
				_closureCapturingFuncsUser.Exchange(u => new User(u.LoginID, (i & 1) == 0 ? usernameA : usernameB));
				_closureCapturingFuncsUser.TryExchange(u => new User(u.LoginID, (i & 1) == 0 ? usernameA : usernameB), (cur, next) => cur.Name == usernameA || next.Name == usernameA);

				_closureCapturingFuncsInt64.TryBoundedExchange(l => l + i, 0L, NumIterations);
				_closureCapturingFuncsInt64.TryMinimumExchange(l => l + i, 0L);
				_closureCapturingFuncsInt64.TryMaximumExchange(l => l + i, NumIterations);

				BenchmarkUtils.SimulateContention(ContentionLevel);
			}
		}
		#endregion

		#region Benchmark: Contextual Funcs
		ManualResetEvent _contextualFuncsBarrier;
		List<Thread> _contextualFuncsThreads;
		AtomicRef<User> _contextualFuncsUser;
		AtomicInt64 _contextualFuncsInt64;

		[IterationSetup(Target = nameof(WithContextualFuncs))]
		public void CreateContextualFuncsContext() {
			_contextualFuncsBarrier = new ManualResetEvent(false);
			_contextualFuncsThreads = new List<Thread>();
			_contextualFuncsUser = new AtomicRef<User>(new User(0, ""));
			_contextualFuncsInt64 = new AtomicInt64(0L);
			BenchmarkUtils.PrepareThreads(NumThreads, _contextualFuncsBarrier, WithContextualFuncs_Entry, _contextualFuncsThreads);
		}

		[Benchmark]
		public void WithContextualFuncs() {
			BenchmarkUtils.ExecutePreparedThreads(_contextualFuncsBarrier, _contextualFuncsThreads);
		}

		void WithContextualFuncs_Entry() {
			var usernameA = "aaaa";
			var usernameB = "bbbb";
			for (var i = 0; i < NumIterations; i++) {
				_contextualFuncsUser.Exchange((u, ctx) => new User(u.LoginID, ctx), (i & 1) == 0 ? usernameA : usernameB);
				_contextualFuncsUser.TryExchange((u, ctx) => new User(u.LoginID, ctx), (i & 1) == 0 ? usernameA : usernameB, (cur, next, ctx) => cur.Name == ctx || next.Name == ctx, usernameA);

				_contextualFuncsInt64.TryBoundedExchange((l, ctx) => l + ctx, i, 0L, NumIterations);
				_contextualFuncsInt64.TryMinimumExchange((l, ctx) => l + ctx, i, 0L);
				_contextualFuncsInt64.TryMaximumExchange((l, ctx) => l + ctx, i, NumIterations);

				BenchmarkUtils.SimulateContention(ContentionLevel);
			}
		}
		#endregion

		#region Benchmark: Manual Loops
		ManualResetEvent _manualLoopsBarrier;
		List<Thread> _manualLoopsThreads;
		AtomicRef<User> _manualLoopsUser;
		AtomicInt64 _manualLoopsInt64;

		[IterationSetup(Target = nameof(WithManualLoops))]
		public void CreateManualLoopsContext() {
			_manualLoopsBarrier = new ManualResetEvent(false);
			_manualLoopsThreads = new List<Thread>();
			_manualLoopsUser = new AtomicRef<User>(new User(0, ""));
			_manualLoopsInt64 = new AtomicInt64(0L);
			BenchmarkUtils.PrepareThreads(NumThreads, _manualLoopsBarrier, WithManualLoops_Entry, _manualLoopsThreads);
		}

		[Benchmark]
		public void WithManualLoops() {
			BenchmarkUtils.ExecutePreparedThreads(_manualLoopsBarrier, _manualLoopsThreads);
		}

		void WithManualLoops_Entry() {
			var usernameA = "aaaa";
			var usernameB = "bbbb";
			for (var i = 0; i < NumIterations; i++) {
				ExchangeUser((i & 1) == 0 ? usernameA : usernameB);
				TryExchangeUser((i & 1) == 0 ? usernameA : usernameB, usernameA);

				TryBoundedExchange(i, 0L, NumIterations);
				TryMinExchange(i, 0L);
				TryMaxExchange(i, NumIterations);

				BenchmarkUtils.SimulateContention(ContentionLevel);
			}
		}

		void ExchangeUser(string context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = _manualLoopsUser.Get();
				var newValue = new User(curValue.LoginID, context);

				if (_manualLoopsUser.TryExchange(newValue, curValue).ValueWasSet) return;
				spinner.SpinOnce();
			}
		}

		void TryExchangeUser(string mapContext, string predicateContext) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = _manualLoopsUser.Get();
				var newValue = new User(curValue.LoginID, mapContext);

				if (!(curValue.Name == predicateContext || newValue.Name == predicateContext)) return;

				if (_manualLoopsUser.TryExchange(newValue, curValue).ValueWasSet) return;
				spinner.SpinOnce();
			}
		}

		void TryBoundedExchange(long context, long lowerBoundInc, long upperBoundEx) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = _manualLoopsInt64.Get();
				if (curValue < lowerBoundInc || curValue >= upperBoundEx) return;
				var newValue = curValue + context;
				if (_manualLoopsInt64.TryExchange(newValue, curValue).ValueWasSet) return;
				spinner.SpinOnce();
			}
		}

		void TryMinExchange(long context, long min) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = _manualLoopsInt64.Get();
				if (curValue < min) return;
				var newValue = curValue + context;
				if (_manualLoopsInt64.TryExchange(newValue, curValue).ValueWasSet) return;
				spinner.SpinOnce();
			}
		}

		void TryMaxExchange(long context, long max) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = _manualLoopsInt64.Get();
				if (curValue > max) return;
				var newValue = curValue + context;
				if (_manualLoopsInt64.TryExchange(newValue, curValue).ValueWasSet) return;
				spinner.SpinOnce();
			}
		}
		#endregion
	}
}