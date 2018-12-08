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
	class AtomicDelegateTest : CommonAtomicTestSuite<Action, AtomicDelegate<Action>> {
		#region Test Fields
		RunnerFactory<Action, AtomicDelegate<Action>> _atomicDelegateRunnerFactory;

		protected override Action Alpha { get; } = () => { };
		protected override Action Bravo { get; } = () => { };
		protected override Action Charlie { get; } = () => { };
		protected override Action Delta { get; } = () => { };
		protected override bool AreEqual(Action lhs, Action rhs) => ReferenceEquals(lhs, rhs);
		#endregion

		#region Test Setup
		[OneTimeSetUp]
		public void SetUpClass() => _atomicDelegateRunnerFactory = new RunnerFactory<Action, AtomicDelegate<Action>>();

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
			Assert.Fail("Need all tests");
		}
		#endregion Tests
	}
}