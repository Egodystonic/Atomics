// (c) Egodystonic Studios 2018

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Egodystonic.Atomics.Benchmarks.DummyObjects;
using Egodystonic.Atomics.Numerics;
using static Egodystonic.Atomics.Benchmarks.BenchmarkUtils;

namespace Egodystonic.Atomics.Benchmarks.Internal {
	/// <summary>
	/// Benchmark used to measure difference between tuple return type on various methods against a simpler but less explicit API that returns an arbitrary/nominal value.
	/// It is expected that tuple return types will be slower-than-or-equal-to less large value return types but this test measures how much difference it makes in
	/// a fairly unrealistic high-hit-rate scenario.
	/// </summary>
	[CoreJob, MemoryDiagnoser]
	public class TupleReturnVsSingleValueReturn {
		#region Parameters
		const int NumIterations = 1_000_000;

		public static object[] AllContentionLevels { get; } = { ContentionLevel.E_Maximum, ContentionLevel.C_High, ContentionLevel.A_Low };

		[ParamsSource(nameof(AllContentionLevels))]
		public ContentionLevel ContentionLevel { get; set; }
		#endregion

		#region Test Setup

		#endregion

		#region Benchmark: Tuple Returns
		AtomicLong _atomicLong;
		AtomicInt _atomicInt;
		AtomicRef<User> _atomicRef;
		AtomicVal<Val8> _atomicVal8;
		AtomicVal<Val16> _atomicVal16;
		AtomicVal<Val32> _atomicVal32;
		AtomicVal<Val64> _atomicVal64;

		[IterationSetup(Target = nameof(WithTupleReturns))]
		public void CreateTupleReturnsContext() {
			_atomicLong = new AtomicLong(0L);
			_atomicInt = new AtomicInt(0);
			_atomicRef = new AtomicRef<User>(new User(0, ""));
			_atomicVal8 = new AtomicVal<Val8>(new Val8(0L));
			_atomicVal16 = new AtomicVal<Val16>(new Val16(0L));
			_atomicVal32 = new AtomicVal<Val32>(new Val32(0L));
			_atomicVal64 = new AtomicVal<Val64>(new Val64(0L));
		}

		[Benchmark(Baseline = true)]
		public void WithTupleReturns() {
			// These vars basically used to ensure the compiler doesn't optimise away the return values entirely
			var longResultVar = 0L;
			var intResultVar = 0;
			var refResultVar = "";
			var valResultVar = 0L;

			for (var i = 0; i < NumIterations; ++i) {
				longResultVar += _atomicLong.Increment().CurrentValue;
				longResultVar += _atomicLong.Decrement().CurrentValue;
				longResultVar += _atomicLong.Exchange(i).PreviousValue;

				intResultVar += _atomicInt.Increment().CurrentValue;
				intResultVar += _atomicInt.Decrement().CurrentValue;
				intResultVar += _atomicInt.TryExchange(i, i - 1).ValueWasSet ? 1 : 0;

				var newUser = new User(i, "Xenoprimate");
				refResultVar = _atomicRef.Exchange(newUser).PreviousValue.Name;
				refResultVar = _atomicRef.TryExchange(new User(i * 2, "Ben"), newUser).ValueWasSet ? refResultVar : String.Empty;

				valResultVar += _atomicVal8.Exchange(new Val8(i)).PreviousValue.A;
				valResultVar += _atomicVal16.TryExchange(new Val16(i + 1), new Val16(i)).ValueWasSet ? 0 : 1;
				valResultVar += _atomicVal32.Exchange(new Val32(i)).PreviousValue.A;
				valResultVar += _atomicVal64.TryExchange(new Val64(i + 1), new Val64(i)).ValueWasSet ? 0 : 1;

				SimulateContention(ContentionLevel);
			}

			if (longResultVar != 1499996500003L || intResultVar != -728379967 || !refResultVar.Equals("Ben", StringComparison.Ordinal) || valResultVar != 999997000002L) {
				throw new ApplicationException("This will never happen; it's just here to force the compiler not to optimise away these vars. These results were measured before.");
			}
		}
		#endregion

		#region Benchmark: Simple Returns
		NonTupledAtomicLong _nonTupledAtomicLong;
		NonTupledAtomicInt _nonTupledAtomicInt;
		NonTupledAtomicRef<User> _nonTupledAtomicRef;
		NonTupledAtomicVal<Val8> _nonTupledAtomicVal8;
		NonTupledAtomicVal<Val16> _nonTupledAtomicVal16;
		NonTupledAtomicVal<Val32> _nonTupledAtomicVal32;
		NonTupledAtomicVal<Val64> _nonTupledAtomicVal64;

		[IterationSetup(Target = nameof(WithNonTupleReturns))]
		public void CreateNonTupleReturnsContext() {
			_nonTupledAtomicLong = new NonTupledAtomicLong(0L);
			_nonTupledAtomicInt = new NonTupledAtomicInt(0);
			_nonTupledAtomicRef = new NonTupledAtomicRef<User>(new User(0, ""));
			_nonTupledAtomicVal8 = new NonTupledAtomicVal<Val8>(new Val8(0L));
			_nonTupledAtomicVal16 = new NonTupledAtomicVal<Val16>(new Val16(0L));
			_nonTupledAtomicVal32 = new NonTupledAtomicVal<Val32>(new Val32(0L));
			_nonTupledAtomicVal64 = new NonTupledAtomicVal<Val64>(new Val64(0L));
		}

		[Benchmark]
		public void WithNonTupleReturns() {
			// These vars basically used to ensure the compiler doesn't optimise away the return values entirely
			var longResultVar = 0L;
			var intResultVar = 0;
			var refResultVar = "";
			var valResultVar = 0L;

			for (var i = 0; i < NumIterations; ++i) {
				longResultVar += _nonTupledAtomicLong.Increment();
				longResultVar += _nonTupledAtomicLong.Decrement();
				longResultVar += _nonTupledAtomicLong.Exchange(i);

				intResultVar += _nonTupledAtomicInt.Increment();
				intResultVar += _nonTupledAtomicInt.Decrement();
				intResultVar += _nonTupledAtomicInt.TryExchange(i, i - 1) ? 1 : 0;

				var newUser = new User(i, "Xenoprimate");
				refResultVar = _nonTupledAtomicRef.Exchange(newUser).Name;
				refResultVar = _nonTupledAtomicRef.TryExchange(new User(i * 2, "Ben"), newUser) ? refResultVar : String.Empty;

				valResultVar += _nonTupledAtomicVal8.Exchange(new Val8(i)).A;
				valResultVar += _nonTupledAtomicVal16.TryExchange(new Val16(i + 1), new Val16(i)) ? 0 : 1;
				valResultVar += _nonTupledAtomicVal32.Exchange(new Val32(i)).A;
				valResultVar += _nonTupledAtomicVal64.TryExchange(new Val64(i + 1), new Val64(i)) ? 0 : 1;

				SimulateContention(ContentionLevel);
			}

			if (longResultVar != 1499996500003L || intResultVar != -728379967 || !refResultVar.Equals("Ben", StringComparison.Ordinal) || valResultVar != 999997000002L) {
				throw new ApplicationException("This will never happen; it's just here to force the compiler not to optimise away these vars. These results were measured before.");
			}
		}
		#endregion
	}
}