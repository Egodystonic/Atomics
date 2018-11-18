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
	class AtomicIntTest : CommonAtomicNumericTestSuite<int, AtomicInt> {
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
		protected override int Zero { get; } = 0;
		protected override int One { get; } = 1;
		protected override int Convert(int operand) => operand;
		protected override int Add(int lhs, int rhs) => lhs + rhs;
		protected override int Sub(int lhs, int rhs) => lhs - rhs;
		protected override int Mul(int lhs, int rhs) => lhs * rhs;
		protected override int Div(int lhs, int rhs) => lhs / rhs;
		#endregion Tests
	}
}