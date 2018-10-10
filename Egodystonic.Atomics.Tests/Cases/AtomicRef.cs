using System;
using Egodystonic.Atomics.Tests.Dummies;
using Egodystonic.Atomics.Tests.Harness;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.Cases {
	[TestFixture]
	class AtomicRefTest {
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
		[Test]
		public void MappedExchange() {
			const int NumIterations = 300_000;

			var runner = NewRunner(new DummyImmutableRef(0));
			runner.AllThreadsTearDown = atomicRef => Assert.AreEqual(NumIterations, atomicRef.Value.IntProp);

			runner.ExecuteFreeThreadedTest(atomicRef => atomicRef.Exchange(dummy => new DummyImmutableRef(dummy.IntProp + 1)), NumIterations);

			runner.ExecuteContinuousCoherencyTest(
				atomicRef => atomicRef.Exchange(dummy => new DummyImmutableRef(dummy.IntProp + 1)),
				NumIterations,
				atomicRef => atomicRef.Value,
				(prev, cur) => Assert.True(cur.IntProp >= prev.IntProp),
				dummy => dummy.IntProp == NumIterations
			);

			// TODO same again with equatable... Test the equality functionality somehow
		}

		static ConcurrentTestCaseRunner<AtomicRef<T>> NewRunner<T>() where T : class => new ConcurrentTestCaseRunner<AtomicRef<T>> { ContextFactoryFunc = () => new AtomicRef<T>() };
		static ConcurrentTestCaseRunner<AtomicRef<T>> NewRunner<T>(T initialValue) where T : class => new ConcurrentTestCaseRunner<AtomicRef<T>> { ContextFactoryFunc = () => new AtomicRef<T>(), GlobalSetUp = ctx => ctx.Set(initialValue) };
		#endregion Tests
	}
}