// (c) Egodystonic Studios 2018


using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Egodystonic.Atomics.Tests.DummyObjects;
using Egodystonic.Atomics.Tests.Harness;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.UnitTests.Common {
	abstract class CommonLockingAtomicTestSuite<T, TTarget> : CommonAtomicTestSuite<T, TTarget> where TTarget : ILockingAtomic<T>, new() {
		[Test]
		public void API_Set() {
			var target = new TTarget();
			target.Value = Alpha;
			target.Set(Bravo, out var previousValue);
			Assert.AreEqual(previousValue, Alpha);
			target.Set(Charlie, out previousValue);
			Assert.AreEqual(previousValue, Bravo);

			Assert.AreEqual(Bravo, target.Set(x => x.Equals(Bravo) ? Delta : Bravo));
			Assert.AreEqual(Delta, target.Set(x => x.Equals(Bravo) ? Delta : Bravo));

			Assert.AreEqual(Bravo, target.Set(x => x.Equals(Bravo) ? Delta : Bravo, out previousValue));
			Assert.AreEqual(previousValue, Delta);
			Assert.AreEqual(Delta, target.Set(x => x.Equals(Bravo) ? Delta : Bravo, out previousValue));
			Assert.AreEqual(previousValue, Bravo);
		}

		[Test]
		public void API_TryGet() {
			var target = new TTarget();
			target.Value = Alpha;

			Assert.AreEqual(false, target.TryGet(v => v.Equals(Bravo), out var curVal));
			Assert.AreEqual(Alpha, curVal);
			target.Value = Bravo;
			Assert.AreEqual(true, target.TryGet(v => v.Equals(Bravo), out curVal));
			Assert.AreEqual(Bravo, curVal);
		}

		[Test]
		public void API_TrySet() {
			var target = new TTarget();
			target.Value = Alpha;

			Assert.AreEqual(true, target.TrySet(Bravo, v => v.Equals(Alpha)));
			Assert.AreEqual(Bravo, target.Value);

			Assert.AreEqual(false, target.TrySet(Charlie, v => v.Equals(Alpha)));
			Assert.AreEqual(Bravo, target.Value);

			Assert.AreEqual(true, target.TrySet(Charlie, v => v.Equals(Bravo), out var prevValue));
			Assert.AreEqual(Charlie, target.Value);
			Assert.AreEqual(Bravo, prevValue);

			Assert.AreEqual(false, target.TrySet(Delta, v => v.Equals(Bravo), out prevValue));
			Assert.AreEqual(Charlie, target.Value);
			Assert.AreEqual(Charlie, prevValue);

			Assert.AreEqual(true, target.TrySet(v => v.Equals(Charlie) ? Delta : Alpha, v => !v.Equals(Alpha)));
			Assert.AreEqual(Delta, target.Value);

			Assert.AreEqual(true, target.TrySet(v => v.Equals(Charlie) ? Delta : Alpha, v => !v.Equals(Alpha), out prevValue));
			Assert.AreEqual(Alpha, target.Value);
			Assert.AreEqual(Delta, prevValue);

			Assert.AreEqual(false, target.TrySet(v => v.Equals(Charlie) ? Delta : Alpha, v => !v.Equals(Alpha), out prevValue, out var newValue));
			Assert.AreEqual(Alpha, target.Value);
			Assert.AreEqual(Alpha, prevValue);
			Assert.AreEqual(Alpha, newValue);

			Assert.AreEqual(false, target.TrySet(v => v.Equals(Charlie) ? Delta : Bravo, v => v.Equals(Alpha), out prevValue, out newValue));
			Assert.AreEqual(Bravo, target.Value);
			Assert.AreEqual(Alpha, prevValue);
			Assert.AreEqual(Bravo, newValue);

			Assert.AreEqual(true, target.TrySet(v => v.Equals(Bravo) ? Charlie : Delta, (v, vNext) => !v.Equals(vNext)));
			Assert.AreEqual(Charlie, target.Value);

			Assert.AreEqual(true, target.TrySet(v => v.Equals(Bravo) ? Charlie : Delta, (v, vNext) => !v.Equals(vNext), out prevValue));
			Assert.AreEqual(Delta, target.Value);
			Assert.AreEqual(Charlie, prevValue);

			Assert.AreEqual(false, target.TrySet(v => v.Equals(Bravo) ? Charlie : Delta, (v, vNext) => !v.Equals(vNext), out prevValue, out newValue));
			Assert.AreEqual(Delta, target.Value);
			Assert.AreEqual(Delta, prevValue);
			Assert.AreEqual(Delta, newValue);

			Assert.AreEqual(false, target.TrySet(v => v.Equals(Bravo) ? Charlie : Alpha, (v, vNext) => !v.Equals(vNext), out prevValue, out newValue));
			Assert.AreEqual(Alpha, target.Value);
			Assert.AreEqual(Delta, prevValue);
			Assert.AreEqual(Alpha, newValue);
		}
	}
}