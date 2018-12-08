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
	class CopyOnReadRefTest : CommonAtomicTestSuite<DummyEquatableRef, CopyOnReadRefDefaultCopyWrapper> {
		#region Test Fields
		RunnerFactory<DummyEquatableRef, AtomicRef<DummyEquatableRef>> _equatableRefRunnerFactory;

		protected override DummyEquatableRef Alpha { get; } = new DummyEquatableRef("Alpha", 111L, null);
		protected override DummyEquatableRef Bravo { get; } = new DummyEquatableRef("Bravo", 222L, null);
		protected override DummyEquatableRef Charlie { get; } = new DummyEquatableRef("Charlie", 333L, null);
		protected override DummyEquatableRef Delta { get; } = new DummyEquatableRef("Delta", 444L, null);
		protected override bool AreEqual(DummyEquatableRef lhs, DummyEquatableRef rhs) => lhs.Equals(rhs);
		#endregion

		#region Test Setup
		[OneTimeSetUp]
		public void SetUpClass() => _equatableRefRunnerFactory = new RunnerFactory<DummyEquatableRef, AtomicRef<DummyEquatableRef>>();

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
			Assert.Fail("Tests for everything unfortunately, as we need to test equatable type and that there are copies");
		}

		[Test]
		public void Stub2() {
			Assert.Fail("Tests specifically for methods that branch over non-equatable");
		}
		#endregion Tests
	}
}