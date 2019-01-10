using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;
using Egodystonic.Atomics.Tests.Harness;
using Egodystonic.Atomics.Tests.UnitTests.Common;
using NUnit.Framework;
using static Egodystonic.Atomics.Tests.Harness.ConcurrentTestCaseRunner;
using ImmutableVal = Egodystonic.Atomics.Tests.DummyObjects.DummyImmutableVal;
using EquatableVal = Egodystonic.Atomics.Tests.DummyObjects.DummyImmutableValAlphaOnlyEquatable;
using SixteenVal = Egodystonic.Atomics.Tests.DummyObjects.DummySixteenByteVal;

namespace Egodystonic.Atomics.Tests.UnitTests {
	[TestFixture]
	class AtomicValTest : CommonAtomicValTestSuite<AtomicVal<ImmutableVal>> {
		#region Test Fields
		RunnerFactory<EquatableVal, AtomicVal<EquatableVal>> _alphaOnlyEquatableRunnerFactory;
		RunnerFactory<SixteenVal, AtomicVal<SixteenVal>> _sixteenByteRunnerFactory;

		protected override ImmutableVal Alpha { get; } = new ImmutableVal(1, 1);
		protected override ImmutableVal Bravo { get; } = new ImmutableVal(2, 2);
		protected override ImmutableVal Charlie { get; } = new ImmutableVal(3, 3);
		protected override ImmutableVal Delta { get; } = new ImmutableVal(4, 4);
		protected override bool AreEqual(ImmutableVal lhs, ImmutableVal rhs) => lhs == rhs;
		#endregion

		#region Test Setup
		[OneTimeSetUp]
		public void SetUpClass() {
			_alphaOnlyEquatableRunnerFactory = new RunnerFactory<EquatableVal, AtomicVal<EquatableVal>>();
			_sixteenByteRunnerFactory = new RunnerFactory<SixteenVal, AtomicVal<SixteenVal>>();
		}

		[OneTimeTearDown]
		public void TearDownClass() { }

		[SetUp]
		public void SetUpTest() { }

		[TearDown]
		public void TearDownTest() { }
		#endregion

		#region Custom Equality Tests
		[Test]
		public void API_SpinWaitForValue_CustomEquality() {
			var target = new AtomicVal<EquatableVal>(new EquatableVal(0, 0));
			var task = Task.Run(() => target.SpinWaitForValue(new EquatableVal(1, 1)));
			target.Set(new EquatableVal(1, 100));
			Assert.AreEqual(new EquatableVal(1, 100), task.Result);
		}

		[Test]
		public void API_SpinWaitForExchange_CustomEquality() {
			var target = new AtomicVal<EquatableVal>(new EquatableVal(0, 0));
			var task = Task.Run(() => target.SpinWaitForExchange(new EquatableVal(1, 1), new EquatableVal(10, 10)));
			target.Set(new EquatableVal(10, 100));
			Assert.AreEqual((new EquatableVal(10, 100), new EquatableVal(1, 1)), task.Result);

			task = Task.Run(() => target.SpinWaitForExchange((c, ctx) => new EquatableVal(c.Alpha + ctx, c.Bravo + ctx), new EquatableVal(100, 100), 1));
			target.Set(new EquatableVal(100, 1000));
			Assert.AreEqual((new EquatableVal(100, 1000), new EquatableVal(101, 1001)), task.Result);
		}

		[Test]
		public void API_TryExchange_CustomEquality() {
			var target = new AtomicVal<EquatableVal>(new EquatableVal(0, 0));
			Assert.AreEqual((false, new EquatableVal(0, 0), new EquatableVal(0, 0)), target.TryExchange(new EquatableVal(10, 10), new EquatableVal(1, 1)));
			target.Set(new EquatableVal(1, 0));
			Assert.AreEqual((true, new EquatableVal(1, 0), new EquatableVal(10, 10)), target.TryExchange(new EquatableVal(10, 10), new EquatableVal(1, 1)));

			target.Set(new EquatableVal(0, 0));
			Assert.AreEqual((false, new EquatableVal(0, 0), new EquatableVal(0, 0)), target.TryExchange((c, ctx) => new EquatableVal(10, c.Bravo + ctx), new EquatableVal(1, 1), 10));
			target.Set(new EquatableVal(1, 0));
			Assert.AreEqual((true, new EquatableVal(1, 0), new EquatableVal(10, 10)), target.TryExchange((c, ctx) => new EquatableVal(10, c.Bravo + ctx), new EquatableVal(1, 1), 10));
		}
		#endregion

		[Test]
		public void Stub2() {
			Assert.Fail("Need to implement tests for 16-byte val");
		}

		[Test]
		public void Stub3() {
			Assert.Fail("Need to implement tests for borrow methods");
		}
	}
}