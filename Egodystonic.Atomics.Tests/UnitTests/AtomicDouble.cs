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
	class AtomicDoubleTest : CommonAtomicFloatingPointTestSuite<double, AtomicDouble> {
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
		protected override double Zero { get; } = 0d;
		protected override double One { get; } = 1d;
		protected override double Convert(int operand) => operand;
		protected override double Add(double lhs, double rhs) => lhs + rhs;
		protected override double Sub(double lhs, double rhs) => lhs - rhs;
		protected override double Mul(double lhs, double rhs) => lhs * rhs;
		protected override double Div(double lhs, double rhs) => lhs / rhs;
		protected override double Convert(float operand) => operand;
		protected override double Abs(double operand) => Math.Abs(operand);
		#endregion Tests
	}
}