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
		[Test]
		public void API_GetSetUnsafe() {
			var target = new TTarget();
			target.SetUnsafe(Alpha);
			Assert.AreEqual(Alpha, target.GetUnsafe());

			target.SetUnsafe(Bravo);
			target.SetUnsafe(Charlie);
			Assert.AreEqual(Charlie, target.GetUnsafe());
		}

		[Test]
		public void API_GetUnsafeRef() {
			var target = new TTarget();
			ref var valueRef = ref target.GetUnsafeRef();

			valueRef = Alpha;
		}
	}
}