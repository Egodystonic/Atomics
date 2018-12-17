using System;
using System.Diagnostics;
using System.Threading;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;
using Egodystonic.Atomics.Tests.Harness;
using Egodystonic.Atomics.Tests.UnitTests.Common;
using NUnit.Framework;
using static Egodystonic.Atomics.Tests.Harness.ConcurrentTestCaseRunner;

namespace Egodystonic.Atomics.Tests.UnitTests {
	[TestFixture]
	class AtomicValTest : CommonAtomicValTestSuite<AtomicVal<DummyImmutableVal>> {
		#region Test Fields
		RunnerFactory<DummyImmutableValAlphaOnlyEquatable, AtomicValUnmanaged<DummyImmutableValAlphaOnlyEquatable>> _alphaOnlyEquatableRunnerFactory;

		protected override DummyImmutableVal Alpha { get; } = new DummyImmutableVal(1, 1);
		protected override DummyImmutableVal Bravo { get; } = new DummyImmutableVal(2, 2);
		protected override DummyImmutableVal Charlie { get; } = new DummyImmutableVal(3, 3);
		protected override DummyImmutableVal Delta { get; } = new DummyImmutableVal(4, 4);
		protected override bool AreEqual(DummyImmutableVal lhs, DummyImmutableVal rhs) => lhs == rhs;
		#endregion

		#region Test Setup
		[OneTimeSetUp]
		public void SetUpClass() => _alphaOnlyEquatableRunnerFactory = new RunnerFactory<DummyImmutableValAlphaOnlyEquatable, AtomicValUnmanaged<DummyImmutableValAlphaOnlyEquatable>>();

		[OneTimeTearDown]
		public void TearDownClass() { }

		[SetUp]
		public void SetUpTest() { }

		[TearDown]
		public void TearDownTest() { }
		#endregion

		#region Tests
		[Test]
		public void Stub1() {
			Assert.Fail("Need to implement tests for custom equality");
		}

		[Test]
		public void Stub2() {
			Assert.Fail("Need to implement tests for 16-byte val");
		}

		[Test]
		public void Stub3() {
			Assert.Fail("Need to implement tests for borrow methods");
		}
		#endregion Tests
	}
}