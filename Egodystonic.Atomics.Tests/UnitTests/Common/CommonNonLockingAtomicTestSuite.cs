// (c) Egodystonic Studios 2018


using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Egodystonic.Atomics.Tests.DummyObjects;
using Egodystonic.Atomics.Tests.Harness;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.UnitTests.Common {
	abstract class CommonNonLockingAtomicTestSuite<T, TTarget> : CommonAtomicTestSuite<T, TTarget> where TTarget : INonLockingAtomic<T>, new() {
		protected CommonNonLockingAtomicTestSuite(Func<T, T, bool> equalityFunc, T alpha, T bravo, T charlie, T delta) : base(equalityFunc, alpha, bravo, charlie, delta) { }

		public void Assert_API_GetSetUnsafe() {
			var target = new TTarget();
			target.SetUnsafe(Alpha);
			Assert.AreEqual(Alpha, target.GetUnsafe());

			target.SetUnsafe(Bravo);
			target.SetUnsafe(Charlie);
			Assert.AreEqual(Charlie, target.GetUnsafe());
		}

		public void Assert_API_GetUnsafeRef() {
			var target = new TTarget();
			ref var valueRef = ref target.GetUnsafeRef();

			valueRef = Alpha;
			Assert.AreEqual(Alpha, target.Get());
			valueRef = Bravo;
			Assert.AreEqual(Bravo, valueRef);
			target.Set(Charlie);
			Assert.AreEqual(Charlie, valueRef);
			valueRef = Delta;
			Assert.AreEqual(Delta, target.GetUnsafe());

			Assert.AreEqual(valueRef, target.GetUnsafeRef());
		}

		public void Assert_API_Swap() {
			var target = new TTarget();

			target.Value = Alpha;
			Assert.AreEqual(Alpha, target.Swap(Bravo));
			target.Set(Bravo);
			Assert.AreEqual(Charlie, target.Swap(Charlie));
			Assert.AreEqual(Charlie, target.Swap(Delta));
			Assert.AreEqual(Delta, target.Get());
		}

		public void Assert_API_TrySwap() {
			var target = new TTarget();

			target.Value = Alpha;

			Assert.AreEqual(Alpha, target.TrySwap(Bravo, Alpha));
			Assert.AreEqual(Bravo, target.Value);

			Assert.AreEqual(Bravo, target.TrySwap(Charlie, Alpha));
			Assert.AreEqual(Bravo, target.Value);

			Assert.AreEqual(Bravo, target.TrySwap(Bravo, Bravo));
			Assert.AreEqual(Bravo, target.Value);

			Assert.AreEqual(Delta, target.TrySwap(Delta, Bravo));
			Assert.AreEqual(Delta, target.Value);
		}
	}
}