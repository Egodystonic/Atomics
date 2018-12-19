using System;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.UnitTests.Common;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.UnitTests.Numerics {
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
		protected override int ToInt(long operand) => (int) operand;
		protected override bool GreaterThan(long lhs, long rhs) => lhs > rhs;
		protected override bool GreaterThanOrEqualTo(long lhs, long rhs) => lhs >= rhs;
		protected override bool LessThan(long lhs, long rhs) => lhs < rhs;
		protected override bool LessThanOrEqualTo(long lhs, long rhs) => lhs <= rhs;
		#endregion Tests
	}
}