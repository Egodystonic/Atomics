using System;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.UnitTests.Common;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.UnitTests.Numerics {
	[TestFixture]
	class AtomicInt32Test : CommonAtomicNumericTestSuite<int, AtomicInt32> {
		#region Test Fields
		protected override int Alpha { get; } = 111;
		protected override int Bravo { get; } = 222;
		protected override int Charlie { get; } = 333;
		protected override int Delta { get; } = 444;
		protected override bool AreEqual(int lhs, int rhs) => lhs == rhs;
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
		protected override int Zero { get; } = 0;
		protected override int One { get; } = 1;
		protected override int Convert(int operand) => operand;
		protected override int Add(int lhs, int rhs) => lhs + rhs;
		protected override int Sub(int lhs, int rhs) => lhs - rhs;
		protected override int Mul(int lhs, int rhs) => lhs * rhs;
		protected override int Div(int lhs, int rhs) => lhs / rhs;
		protected override int ToInt(int operand) => operand;
		protected override bool GreaterThan(int lhs, int rhs) => lhs > rhs;
		protected override bool GreaterThanOrEqualTo(int lhs, int rhs) => lhs >= rhs;
		protected override bool LessThan(int lhs, int rhs) => lhs < rhs;
		protected override bool LessThanOrEqualTo(int lhs, int rhs) => lhs <= rhs;
		#endregion Tests
	}
}