using System;
using System.Diagnostics;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;
using Egodystonic.Atomics.Tests.Harness;
using NUnit.Framework;

namespace Egodystonic.Atomics.Tests.UnitTests {
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
		public void GetAndSet() {
			const int NumIterations = 1_000_000;
			var atomicLong = new AtomicLong(0L);
			var runner = NewRunner(new DummyImmutableRef(0L));

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				atomicRef => atomicRef.Set(new DummyImmutableRef(atomicLong.Increment().NewValue)),
				NumIterations,
				atomicRef => atomicRef.Get(),
				(prev, cur) => Assert.True(prev.LongProp <= cur.LongProp)
			);
		}

		[Test]
		public void Exchange() {
			const int NumIterations = 300_000;
			var atomicLong = new AtomicLong(0L);
			var runner = NewRunner(new DummyImmutableRef(0L));
			runner.GlobalSetUp = _ => atomicLong.Set(0L);

			runner.ExecuteContinuousSingleWriterCoherencyTests(
				atomicRef => {
					var newLongValue = atomicLong.Increment().NewValue;
					var prev = atomicRef.Exchange(new DummyImmutableRef(newLongValue));
					Assert.AreEqual(prev.LongProp, newLongValue - 1L);
				},
				NumIterations,
				atomicRef => atomicRef.Value,
				(prev, cur) => Assert.True(cur.LongProp >= prev.LongProp)
			);
		}

		[Test]
		public void TryExchange() {
			const int NumIterations = 100_000;

			var equatableRunner = NewRunner(new DummyEquatableRef(0L));

			equatableRunner.ExecuteFreeThreadedTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					var newValue = new DummyEquatableRef(curValue.LongProp + 1L);
					var (wasSet, prevValue) = atomicRef.TryExchange(newValue, curValue);
					if (wasSet) Assert.AreEqual(curValue, prevValue);
					else Assert.AreNotEqual(curValue, prevValue);
				},
				NumIterations
			);

			equatableRunner.ExecuteContinuousCoherencyTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					var newValue = new DummyEquatableRef(curValue.LongProp + 1L);
					atomicRef.TryExchange(newValue, curValue);
				},
				NumIterations,
				atomicRef => atomicRef.Value,
				(prev, cur) => Assert.True(cur.LongProp >= prev.LongProp)
			);

			var immutableRunner = NewRunner(new DummyImmutableRef(0L));

			immutableRunner.ExecuteFreeThreadedTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
					var (wasSet, prevValue) = atomicRef.TryExchange(newValue, curValue);
					if (wasSet) Assert.AreEqual(curValue, prevValue);
					else Assert.AreNotEqual(curValue, prevValue);
				},
				NumIterations
			);

			immutableRunner.ExecuteContinuousCoherencyTests(
				atomicRef => {
					var curValue = atomicRef.Value;
					var newValue = new DummyImmutableRef(curValue.LongProp + 1L);
					atomicRef.TryExchange(newValue, curValue);
				},
				NumIterations,
				atomicRef => atomicRef.Value,
				(prev, cur) => Assert.True(cur.LongProp >= prev.LongProp)
			);

			var dummyImmutableA = new DummyImmutableRef("Xenoprimate Rules");
			var dummyImmutableB = new DummyImmutableRef("Xenoprimate Rules");
			var dummyEquatableA = new DummyEquatableRef("Xenoprimate Rules");
			var dummyEquatableB = new DummyEquatableRef("Xenoprimate Rules");
			var atomicImmutable = new AtomicRef<DummyImmutableRef>();
			var atomicEquatable = new AtomicRef<DummyEquatableRef>();

			atomicImmutable.Set(dummyImmutableA);
			Assert.AreEqual(false, atomicImmutable.TryExchange(new DummyImmutableRef(), dummyImmutableB).ValueWasSet);
			atomicEquatable.Set(dummyEquatableA);
			Assert.AreEqual(true, atomicEquatable.TryExchange(new DummyEquatableRef(), dummyEquatableB).ValueWasSet);
		}

		[Test]
		public void PredicatedTryExchange() {
			
		}

		[Test]
		public void MappedExchange() {
			const int NumIterations = 300_000;

			var runner = NewRunner(new DummyImmutableRef(0L));
			runner.AllThreadsTearDown = atomicRef => Assert.AreEqual(NumIterations, atomicRef.Value.LongProp);

			runner.ExecuteFreeThreadedTests(
				atomicRef => {
					var (prev, cur) = atomicRef.Exchange(dummy => new DummyImmutableRef(dummy.LongProp + 1L));
					Assert.AreEqual(prev.LongProp, cur.LongProp - 1L);
				}, 
				NumIterations
			);

			runner.ExecuteContinuousCoherencyTests(
				atomicRef => atomicRef.Exchange(dummy => new DummyImmutableRef(dummy.LongProp + 1L)),
				NumIterations,
				atomicRef => atomicRef.Value,
				(prev, cur) => Assert.True(cur.LongProp >= prev.LongProp),
				dummy => dummy.LongProp == NumIterations
			);
		}

		static ConcurrentTestCaseRunner<AtomicRef<T>> NewRunner<T>() where T : class => new ConcurrentTestCaseRunner<AtomicRef<T>>(() => new AtomicRef<T>());
		static ConcurrentTestCaseRunner<AtomicRef<T>> NewRunner<T>(T initialValue) where T : class => new ConcurrentTestCaseRunner<AtomicRef<T>>(() => new AtomicRef<T>(initialValue));
		#endregion Tests
	}
}