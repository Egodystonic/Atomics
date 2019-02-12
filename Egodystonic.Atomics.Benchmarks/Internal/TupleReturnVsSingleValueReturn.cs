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
	/// Benchmark used to justify FastXYZ() methods. A small but measurable difference is expected.
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

		#region Benchmark: Tuple Returns
		AtomicLong _fastAtomicLong;
		AtomicInt _fastAtomicInt;
		AtomicRef<User> _fastAtomicRef;
		AtomicVal<Val8> _fastAtomicVal8;
		AtomicVal<Val16> _fastAtomicVal16;
		AtomicVal<Val32> _fastAtomicVal32;
		AtomicVal<Val64> _fastAtomicVal64;

		[IterationSetup(Target = nameof(WithSingleValueReturns))]
		public void CreateSingleValueReturnsContext() {
			_fastAtomicLong = new AtomicLong(0L);
			_fastAtomicInt = new AtomicInt(0);
			_fastAtomicRef = new AtomicRef<User>(new User(0, ""));
			_fastAtomicVal8 = new AtomicVal<Val8>(new Val8(0L));
			_fastAtomicVal16 = new AtomicVal<Val16>(new Val16(0L));
			_fastAtomicVal32 = new AtomicVal<Val32>(new Val32(0L));
			_fastAtomicVal64 = new AtomicVal<Val64>(new Val64(0L));
		}

		[Benchmark]
		public void WithSingleValueReturns() {
			// These vars basically used to ensure the compiler doesn't optimise away the return values entirely
			var longResultVar = 0L;
			var intResultVar = 0;
			var refResultVar = "";
			var valResultVar = 0L;

			for (var i = 0; i < NumIterations; ++i) {
				longResultVar += _fastAtomicLong.FastIncrement();
				longResultVar += _fastAtomicLong.FastDecrement();
				longResultVar += _fastAtomicLong.FastExchange(i);

				intResultVar += _fastAtomicInt.FastIncrement();
				intResultVar += _fastAtomicInt.FastDecrement();
				intResultVar += _fastAtomicInt.FastTryExchange(i, i - 1) == i - 1 ? 1 : 0;

				var newUser = new User(i, "Xenoprimate");
				refResultVar = _fastAtomicRef.FastExchange(newUser).Name;
				refResultVar = _fastAtomicRef.FastTryExchange(new User(i * 2, "Ben"), newUser).LoginID == newUser.LoginID ? refResultVar : String.Empty;

				valResultVar += _fastAtomicVal8.FastExchange(new Val8(i)).A;
				valResultVar += _fastAtomicVal16.FastTryExchange(new Val16(i + 1), new Val16(i)).A == i ? 0 : 1;
				valResultVar += _fastAtomicVal32.FastExchange(new Val32(i)).A;
				valResultVar += _fastAtomicVal64.FastTryExchange(new Val64(i + 1), new Val64(i)).A == i ? 0 : 1;

				SimulateContention(ContentionLevel);
			}

			if (longResultVar != 1499996500003L || intResultVar != -728379967 || !refResultVar.Equals("Ben", StringComparison.Ordinal) || valResultVar != 999997000002L) {
				throw new ApplicationException("This will never happen; it's just here to force the compiler not to optimise away these vars. These results were measured before.");
			}
		}
		#endregion
	}
}