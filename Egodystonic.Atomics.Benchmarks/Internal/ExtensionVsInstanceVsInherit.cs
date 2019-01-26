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
	interface IAtomicStub<T> {
		T Get();
		void Set(T CurrentValue);
	}

	abstract class AtomicStubBase<T> {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public abstract T BaseGet();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public abstract void BaseSet(T CurrentValue);

		public T BaseFrobnicate<TContext>(Func<T, TContext, bool> predicate, TContext context) {
			SimulateContention(ContentionLevel.D_VeryHigh);
			BaseSet(BaseGet());
			return default;
		}
	}

	sealed class AtomicStub : AtomicStubBase<User>, IAtomicStub<User> {
		User _value;

		public AtomicStub(User value) => Set(value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public sealed override User BaseGet() => Volatile.Read(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public sealed override void BaseSet(User CurrentValue) => Volatile.Write(ref _value, CurrentValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public User Get() => Volatile.Read(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(User CurrentValue) => Volatile.Write(ref _value, CurrentValue);

		public User InstanceFrobnicate<TContext>(Func<User, TContext, bool> predicate, TContext context) {
			SimulateContention(ContentionLevel.D_VeryHigh);
			Set(Get());
			return default;
		}
	}

	static class AtomicStubExtensions {
		public static T ExtensionFrobnicate<T, TContext>(this IAtomicStub<T> @this, Func<T, TContext, bool> predicate, TContext context) {
			SimulateContention(ContentionLevel.D_VeryHigh);
			@this.Set(@this.Get());
			return default;
		}
	}

	/// <summary>
	/// Benchmark used to show
	/// </summary>
	[CoreJob, MemoryDiagnoser]
	public class ExtensionVsInstanceVsInherit {
		#region Parameters
		const int NumIterations = 500_000;

		public static object[] ThreadCounts { get; } = { 1 };

		[ParamsSource(nameof(ThreadCounts))]
		public int NumThreads { get; set; }
		#endregion

		#region Test Setup
		
		#endregion

		#region Benchmark: Instance
		AtomicStub _instanceUser;

		[Benchmark]
		public void WithInstance() {
			_instanceUser = new AtomicStub(new User(0, ""));
			for (var i = 0; i < NumIterations; ++i) {
				_instanceUser.InstanceFrobnicate((u, idx) => u.LoginID == idx, i - 1);
			}
		}
		#endregion

		#region Benchmark: Extension
		AtomicStub _extensionUser;

		[Benchmark]
		public void WithExtension() {
			_extensionUser = new AtomicStub(new User(0, ""));
			for (var i = 0; i < NumIterations; ++i) {
				_extensionUser.ExtensionFrobnicate((u, idx) => u.LoginID == idx, i - 1);
			}
		}
		#endregion

		#region Benchmark: Inherit
		AtomicStub _baseUser;

		[Benchmark]
		public void WithBase() {
			_baseUser = new AtomicStub(new User(0, ""));
			for (var i = 0; i < NumIterations; ++i) {
				_baseUser.BaseFrobnicate((u, idx) => u.LoginID == idx, i - 1);
			}
		}
		#endregion
	}
}