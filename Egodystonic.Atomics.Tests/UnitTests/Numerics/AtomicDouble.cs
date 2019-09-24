using System;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.UnitTests.Common;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.UnitTests.Numerics {
	[TestFixture]
	class AtomicDoubleTest : CommonAtomicFloatingPointTestSuite<double, LockFreeDouble> {
		#region Test Fields
		protected override double Alpha { get; } = 111d;
		protected override double Bravo { get; } = 222d;
		protected override double Charlie { get; } = 333d;
		protected override double Delta { get; } = 444d;
		// ReSharper disable once CompareOfFloatsByEqualityOperator Direct comparison is deliberate
		protected override bool AreEqual(double lhs, double rhs) => lhs == rhs;
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
		protected override double Zero { get; } = 0d;
		protected override double One { get; } = 1d;
		protected override double Convert(int operand) => operand;
		protected override double Add(double lhs, double rhs) => lhs + rhs;
		protected override double Sub(double lhs, double rhs) => lhs - rhs;
		protected override double Mul(double lhs, double rhs) => lhs * rhs;
		protected override double Div(double lhs, double rhs) => lhs / rhs;
		protected override double Convert(float operand) => operand;
		protected override double AsDouble(double operand) => operand;
		protected override double Abs(double operand) => Math.Abs(operand);
		protected override int ToInt(double operand) => (int) operand;
		protected override bool GreaterThan(double lhs, double rhs) => lhs > rhs;
		protected override bool GreaterThanOrEqualTo(double lhs, double rhs) => lhs >= rhs;
		protected override bool LessThan(double lhs, double rhs) => lhs < rhs;
		protected override bool LessThanOrEqualTo(double lhs, double rhs) => lhs <= rhs;
		#endregion Tests
	}
}