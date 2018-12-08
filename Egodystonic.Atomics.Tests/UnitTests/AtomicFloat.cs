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
	class AtomicFloatTest : CommonAtomicFloatingPointTestSuite<float, AtomicFloat> {
		#region Test Fields
		protected override float Alpha { get; } = 111f;
		protected override float Bravo { get; } = 222f;
		protected override float Charlie { get; } = 333f;
		protected override float Delta { get; } = 444f;
		// ReSharper disable once CompareOfFloatsByEqualityOperator Direct comparison is deliberate
		protected override bool AreEqual(float lhs, float rhs) => lhs == rhs;
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
		protected override float Zero { get; } = 0f;
		protected override float One { get; } = 1f;
		protected override float Convert(int operand) => operand;
		protected override float Add(float lhs, float rhs) => lhs + rhs;
		protected override float Sub(float lhs, float rhs) => lhs - rhs;
		protected override float Mul(float lhs, float rhs) => lhs * rhs;
		protected override float Div(float lhs, float rhs) => lhs / rhs;
		protected override float Convert(float operand) => operand;
		protected override float Abs(float operand) => MathF.Abs(operand);
		#endregion Tests
	}
}