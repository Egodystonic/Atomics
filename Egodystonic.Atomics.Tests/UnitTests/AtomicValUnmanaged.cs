﻿using System;
using Egodystonic.Atomics.Tests.DummyObjects;
using Egodystonic.Atomics.Tests.UnitTests.Common;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.UnitTests {
	[TestFixture]
	class AtomicValUnmanagedTest : CommonAtomicValTestSuite<LockFreeValue<DummyImmutableVal>> {
		#region Test Fields
		protected override DummyImmutableVal Alpha { get; } = new DummyImmutableVal(1, 1);
		protected override DummyImmutableVal Bravo { get; } = new DummyImmutableVal(2, 2);
		protected override DummyImmutableVal Charlie { get; } = new DummyImmutableVal(3, 3);
		protected override DummyImmutableVal Delta { get; } = new DummyImmutableVal(4, 4);
		protected override bool AreEqual(DummyImmutableVal lhs, DummyImmutableVal rhs) => lhs == rhs;
		#endregion

		#region Test Setup
		[OneTimeSetUp]
		public void SetUpClass() { }

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
			var atomic = new LockFreeValue<DummyImmutableValAlphaOnlyEquatable>(initialVal);
			Assert.False(atomic.TryExchange(new DummyImmutableValAlphaOnlyEquatable(), new DummyImmutableValAlphaOnlyEquatable(initialVal.Alpha, initialVal.Bravo + 20)).ValueWasSet);
		}

		[Test]
		public void ShouldThrowOnOversizedTypeParameter() {
			Assert.Throws<ArgumentException>(() => new LockFreeValue<DummySixteenByteVal>());
			Assert.DoesNotThrow(() => new LockFreeValue<long>());
		}
		#endregion Tests
	}
}