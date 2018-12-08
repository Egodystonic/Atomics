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
	class AtomicLongTest : CommonAtomicNumericTestSuite<long, AtomicLong> {
		#region Test Fields
		protected override long Alpha { get; } = 111L;
		protected override long Bravo { get; } = 222L;
		protected override long Charlie { get; } = 333L;
		protected override long Delta { get; } = 444L;
		protected override bool AreEqual(long lhs, long rhs) => lhs == rhs;
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
		protected override long Zero { get; } = 0;
		protected override long One { get; } = 1;
		protected override long Convert(int operand) => operand;
		protected override long Add(long lhs, long rhs) => lhs + rhs;
		protected override long Sub(long lhs, long rhs) => lhs - rhs;
		protected override long Mul(long lhs, long rhs) => lhs * rhs;
		protected override long Div(long lhs, long rhs) => lhs / rhs;
		#endregion Tests
	}
}