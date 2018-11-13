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
	class AtomicBoolTest : CommonAtomicRefTestSuite<AtomicRef<DummyImmutableRef>> {
		#region Test Fields
		RunnerFactory<DummyEquatableRef, AtomicRef<DummyEquatableRef>> _equatableRefRunnerFactory;
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
		
		#endregion Tests
	}
}