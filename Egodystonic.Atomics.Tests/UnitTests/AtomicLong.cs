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