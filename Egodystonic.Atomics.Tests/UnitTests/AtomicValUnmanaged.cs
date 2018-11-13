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
	class AtomicValUnmanagedTest : CommonAtomicValTestSuite<AtomicValUnmanaged<DummyImmutableVal>> {
		#region Test Fields
		RunnerFactory<DummyImmutableValAlphaOnlyEquatable, AtomicValUnmanaged<DummyImmutableValAlphaOnlyEquatable>> _alphaOnlyEquatableRunnerFactory;
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
		public void CheckNoCustomEqualityForUnmanagedVals() {
			var initialVal = new DummyImmutableValAlphaOnlyEquatable(10, 20);
			var atomic = new AtomicValUnmanaged<DummyImmutableValAlphaOnlyEquatable>(initialVal);
			Assert.False(atomic.TryExchange(new DummyImmutableValAlphaOnlyEquatable(), new DummyImmutableValAlphaOnlyEquatable(initialVal.Alpha, initialVal.Bravo + 20)).ValueWasSet);
		}

		[Test]
		public void ShouldThrowOnOversizedTypeParameter() {
			Assert.Throws<ArgumentException>(() => new AtomicValUnmanaged<DummySixteenByteVal>());
			Assert.DoesNotThrow(() => new AtomicValUnmanaged<long>());
		}
		#endregion Tests
	}
}