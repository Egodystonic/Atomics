﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Egodystonic.Atomics.Numerics;
using Egodystonic.Atomics.Tests.DummyObjects;
using Egodystonic.Atomics.Tests.Harness;
using Egodystonic.Atomics.Tests.UnitTests.Common;
using NUnit.Framework;
using static Egodystonic.Atomics.Tests.Harness.ConcurrentTestCaseRunner;

namespace Egodystonic.Atomics.Tests.UnitTests {
	[TestFixture]
	class AtomicDelegateTest : CommonAtomicTestSuite<Action, AtomicDelegate<Action>> {
		#region Test Fields
		RunnerFactory<Action, AtomicDelegate<Action>> _atomicDelegateRunnerFactory;

		protected override Action Alpha { get; } = () => { };
		protected override Action Bravo { get; } = () => { };
		protected override Action Charlie { get; } = () => { };
		protected override Action Delta { get; } = () => { };
		protected override bool AreEqual(Action lhs, Action rhs) => ReferenceEquals(lhs, rhs);
		#endregion

		#region Test Setup
		[OneTimeSetUp]
		public void SetUpClass() => _atomicDelegateRunnerFactory = new RunnerFactory<Action, AtomicDelegate<Action>>();

		[OneTimeTearDown]
		public void TearDownClass() { }

		[SetUp]
		public void SetUpTest() { }

		[TearDown]
		public void TearDownTest() { }
		#endregion

		#region Tests
		[Test]
		public void Stub1() {
			Assert.Fail("MT testing for API surface");
		}

		[Test]
		public void API_CombineRemoveRemoveAll() {
			var lastRecordedInput = new AtomicRef<string>();
			Func<string, string> initialValue = s => { lastRecordedInput.Set(s); return s.ToUpper(); };

			var target = new AtomicDelegate<Func<string, string>>(initialValue);

			var combineRes = target.Combine(s => s.ToLower());
			Assert.AreEqual(initialValue, combineRes.PreviousValue);
			Assert.AreEqual("TEST", combineRes.PreviousValue("Test"));
			Assert.AreEqual("test", combineRes.NewValue("Test"));

			Assert.AreEqual("Test", lastRecordedInput.Value);

			target.TryInvoke("Input");
			Assert.AreEqual("Input", lastRecordedInput.Value);

			combineRes = target.Combine(s => s + s);
			Assert.AreEqual("abcabc", target.Value("abc"));
			Assert.AreEqual("abc", lastRecordedInput.Value);
			Assert.AreEqual(combineRes.NewValue, target.Value);

			var removeRes = target.Remove(initialValue);
			Assert.AreEqual("qwertyqwerty", removeRes.PreviousValue("qwerty"));
			Assert.AreEqual("qwerty", lastRecordedInput.Value);
			Assert.AreEqual("ijklijkl", removeRes.NewValue("ijkl"));
			Assert.AreEqual("qwerty", lastRecordedInput.Value);
			Assert.AreEqual(removeRes.NewValue, target.Value);

			removeRes = target.Remove(c => "this delegate was never added");
			Assert.AreEqual(removeRes.NewValue, removeRes.PreviousValue);
			Assert.AreEqual(target.Value, removeRes.PreviousValue);

			var invocationCounter = new AtomicInt();
			Func<string, string> newValue = s => { invocationCounter.Increment(); return s[0].ToString(); };
			target.Combine(newValue);
			target.Combine(newValue);
			target.Combine(newValue);

			Assert.AreEqual((true, "r"), target.TryInvoke("rrrrr"));
			Assert.AreEqual(3, invocationCounter.Value);

			target.Remove(newValue);
			Assert.AreEqual((true, "r"), target.TryInvoke("rrrrr"));
			Assert.AreEqual(5, invocationCounter.Value);

			removeRes = target.RemoveAll(newValue);
			Assert.AreEqual((true, "rrrrrrrrrr"), target.TryInvoke("rrrrr"));
			Assert.AreEqual(5, invocationCounter.Value);
			Assert.AreEqual("rrrrrrrrrr", removeRes.NewValue("rrrrr"));
			Assert.AreEqual("r", removeRes.PreviousValue("rrrrr"));

			removeRes = target.RemoveAll(newValue);
			Assert.AreEqual((true, "rrrrrrrrrr"), target.TryInvoke("rrrrr"));
			Assert.AreEqual(7, invocationCounter.Value);
			Assert.AreEqual("rrrrrrrrrr", removeRes.NewValue("rrrrr"));
			Assert.AreEqual(removeRes.NewValue, removeRes.PreviousValue);
		}

		[Test]
		public void API_TryDynamicInvoke() {
			var atomicInt = new AtomicInt();

			var targetA = new AtomicDelegate<Action<int>>();
			Assert.AreEqual((false, (object) null), targetA.TryDynamicInvoke(10));
			
			targetA.Set(i => atomicInt.Add(i));
			Assert.AreEqual((true, (object) null), targetA.TryDynamicInvoke(300));
			Assert.Throws<TargetParameterCountException>(() => targetA.TryDynamicInvoke());
			Assert.Throws<ArgumentException>(() => targetA.TryDynamicInvoke(""));
			Assert.Throws<TargetParameterCountException>(() => targetA.TryDynamicInvoke(3, 3));

			var targetB = new AtomicDelegate<Func<int, int>>();
			Assert.AreEqual((false, (object) null), targetB.TryDynamicInvoke(10));
			
			targetB.Set(i => atomicInt.Add(i).NewValue);
			var invokeRes = targetB.TryDynamicInvoke(300);
			Assert.AreEqual(true, invokeRes.DelegateWasInvoked);
			Assert.AreEqual(600, (int) invokeRes.Result);

			Assert.Throws<TargetParameterCountException>(() => targetB.TryDynamicInvoke());
			Assert.Throws<ArgumentException>(() => targetB.TryDynamicInvoke(""));
			Assert.Throws<TargetParameterCountException>(() => targetB.TryDynamicInvoke(3, 3));
		}

		[Test]
		public void API_TryWrappedInvoke() {
			var atomicInt = new AtomicInt();

			var target = new AtomicDelegate<Func<int, int>>();
			Assert.AreEqual((false, default(int)), target.TryWrappedInvoke(f => f(10)));

			target.Set(i => atomicInt.Add(i).NewValue);
			Assert.AreEqual((true, 10), target.TryWrappedInvoke(f => f(10)));
			Assert.AreEqual(10, atomicInt.Value);

			Assert.AreEqual(true, target.TryWrappedInvoke((Action<Func<int, int>>) (f => f(10))));
			Assert.AreEqual(20, atomicInt.Value);
			target.Set(null);
			Assert.AreEqual(false, target.TryWrappedInvoke((Action<Func<int, int>>) (f => f(10))));
			Assert.AreEqual(20, atomicInt.Value);
		}

		[Test]
		public void API_TryInvoke() {
			// Someone smarter than me could automate this with a for loop and dynamic code gen. But all those smart people are off building things that are actually useful, so here we are.
			// If you're reading this and want to take a crack at it though...
			var actionTarget = new AtomicInt();

			Assert.AreEqual(false, new AtomicDelegate<Action>().TryInvoke());
			Assert.AreEqual(true, new AtomicDelegate<Action>(() => actionTarget.Set(3)).TryInvoke());
			Assert.AreEqual(3, actionTarget.Value);

			Assert.AreEqual(false, new AtomicDelegate<Action<int>>().TryInvoke(1));
			Assert.AreEqual(true, new AtomicDelegate<Action<int>>(n1 => actionTarget.Set(n1)).TryInvoke(1));
			Assert.AreEqual(1, actionTarget.Value);

			Assert.AreEqual(false, new AtomicDelegate<Action<int, int>>().TryInvoke(1, 2));
			Assert.AreEqual(true, new AtomicDelegate<Action<int, int>>((n1, n2) => actionTarget.Set(n1 + n2)).TryInvoke(1, 2));
			Assert.AreEqual(3, actionTarget.Value);

			Assert.AreEqual(false, new AtomicDelegate<Action<int, int, int>>().TryInvoke(1, 2, 3));
			Assert.AreEqual(true, new AtomicDelegate<Action<int, int, int>>((n1, n2, n3) => actionTarget.Set(n1 + n2 + n3)).TryInvoke(1, 2, 3));
			Assert.AreEqual(6, actionTarget.Value);

			Assert.AreEqual(false, new AtomicDelegate<Action<int, int, int, int>>().TryInvoke(1, 2, 3, 4));
			Assert.AreEqual(true, new AtomicDelegate<Action<int, int, int, int>>((n1, n2, n3, n4) => actionTarget.Set(n1 + n2 + n3 + n4)).TryInvoke(1, 2, 3, 4));
			Assert.AreEqual(10, actionTarget.Value);

			Assert.AreEqual(false, new AtomicDelegate<Action<int, int, int, int, int>>().TryInvoke(1, 2, 3, 4, 5));
			Assert.AreEqual(true, new AtomicDelegate<Action<int, int, int, int, int>>((n1, n2, n3, n4, n5) => actionTarget.Set(n1 + n2 + n3 + n4 + n5)).TryInvoke(1, 2, 3, 4, 5));
			Assert.AreEqual(15, actionTarget.Value);

			Assert.AreEqual(false, new AtomicDelegate<Action<int, int, int, int, int, int>>().TryInvoke(1, 2, 3, 4, 5, 6));
			Assert.AreEqual(true, new AtomicDelegate<Action<int, int, int, int, int, int>>((n1, n2, n3, n4, n5, n6) => actionTarget.Set(n1 + n2 + n3 + n4 + n5 + n6)).TryInvoke(1, 2, 3, 4, 5, 6));
			Assert.AreEqual(21, actionTarget.Value);

			Assert.AreEqual(false, new AtomicDelegate<Action<int, int, int, int, int, int, int>>().TryInvoke(1, 2, 3, 4, 5, 6, 7));
			Assert.AreEqual(true, new AtomicDelegate<Action<int, int, int, int, int, int, int>>((n1, n2, n3, n4, n5, n6, n7) => actionTarget.Set(n1 + n2 + n3 + n4 + n5 + n6 + n7)).TryInvoke(1, 2, 3, 4, 5, 6, 7));
			Assert.AreEqual(28, actionTarget.Value);

			Assert.AreEqual(false, new AtomicDelegate<Action<int, int, int, int, int, int, int, int>>().TryInvoke(1, 2, 3, 4, 5, 6, 7, 8));
			Assert.AreEqual(true, new AtomicDelegate<Action<int, int, int, int, int, int, int, int>>((n1, n2, n3, n4, n5, n6, n7, n8) => actionTarget.Set(n1 + n2 + n3 + n4 + n5 + n6 + n7 + n8)).TryInvoke(1, 2, 3, 4, 5, 6, 7, 8));
			Assert.AreEqual(36, actionTarget.Value);

			Assert.AreEqual(false, new AtomicDelegate<Action<int, int, int, int, int, int, int, int, int>>().TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9));
			Assert.AreEqual(true, new AtomicDelegate<Action<int, int, int, int, int, int, int, int, int>>((n1, n2, n3, n4, n5, n6, n7, n8, n9) => actionTarget.Set(n1 + n2 + n3 + n4 + n5 + n6 + n7 + n8 + n9)).TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9));
			Assert.AreEqual(45, actionTarget.Value);

			Assert.AreEqual(false, new AtomicDelegate<Action<int, int, int, int, int, int, int, int, int, int>>().TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10));
			Assert.AreEqual(true, new AtomicDelegate<Action<int, int, int, int, int, int, int, int, int, int>>((n1, n2, n3, n4, n5, n6, n7, n8, n9, n10) => actionTarget.Set(n1 + n2 + n3 + n4 + n5 + n6 + n7 + n8 + n9 + n10)).TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10));
			Assert.AreEqual(55, actionTarget.Value);

			Assert.AreEqual(false, new AtomicDelegate<Action<int, int, int, int, int, int, int, int, int, int, int>>().TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11));
			Assert.AreEqual(true, new AtomicDelegate<Action<int, int, int, int, int, int, int, int, int, int, int>>((n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11) => actionTarget.Set(n1 + n2 + n3 + n4 + n5 + n6 + n7 + n8 + n9 + n10 + n11)).TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11));
			Assert.AreEqual(66, actionTarget.Value);

			Assert.AreEqual(false, new AtomicDelegate<Action<int, int, int, int, int, int, int, int, int, int, int, int>>().TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12));
			Assert.AreEqual(true, new AtomicDelegate<Action<int, int, int, int, int, int, int, int, int, int, int, int>>((n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11, n12) => actionTarget.Set(n1 + n2 + n3 + n4 + n5 + n6 + n7 + n8 + n9 + n10 + n11 + n12)).TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12));
			Assert.AreEqual(78, actionTarget.Value);

			Assert.AreEqual(false, new AtomicDelegate<Action<int, int, int, int, int, int, int, int, int, int, int, int, int>>().TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13));
			Assert.AreEqual(true, new AtomicDelegate<Action<int, int, int, int, int, int, int, int, int, int, int, int, int>>((n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11, n12, n13) => actionTarget.Set(n1 + n2 + n3 + n4 + n5 + n6 + n7 + n8 + n9 + n10 + n11 + n12 + n13)).TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13));
			Assert.AreEqual(91, actionTarget.Value);

			Assert.AreEqual(false, new AtomicDelegate<Action<int, int, int, int, int, int, int, int, int, int, int, int, int, int>>().TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14));
			Assert.AreEqual(true, new AtomicDelegate<Action<int, int, int, int, int, int, int, int, int, int, int, int, int, int>>((n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11, n12, n13, n14) => actionTarget.Set(n1 + n2 + n3 + n4 + n5 + n6 + n7 + n8 + n9 + n10 + n11 + n12 + n13 + n14)).TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14));
			Assert.AreEqual(105, actionTarget.Value);

			Assert.AreEqual(false, new AtomicDelegate<Action<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int>>().TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15));
			Assert.AreEqual(true, new AtomicDelegate<Action<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int>>((n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11, n12, n13, n14, n15) => actionTarget.Set(n1 + n2 + n3 + n4 + n5 + n6 + n7 + n8 + n9 + n10 + n11 + n12 + n13 + n14 + n15)).TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15));
			Assert.AreEqual(120, actionTarget.Value);

			Assert.AreEqual(false, new AtomicDelegate<Action<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int>>().TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16));
			Assert.AreEqual(true, new AtomicDelegate<Action<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int>>((n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11, n12, n13, n14, n15, n16) => actionTarget.Set(n1 + n2 + n3 + n4 + n5 + n6 + n7 + n8 + n9 + n10 + n11 + n12 + n13 + n14 + n15 + n16)).TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16));
			Assert.AreEqual(136, actionTarget.Value);

			Assert.AreEqual(false, new AtomicDelegate<Func<int>>().TryInvoke());
			Assert.AreEqual((true, 0), new AtomicDelegate<Func<int>>(() => 0).TryInvoke());

			Assert.AreEqual(false, new AtomicDelegate<Func<int, int>>().TryInvoke(1));
			Assert.AreEqual((true, 1), new AtomicDelegate<Func<int, int>>(n1 => n1).TryInvoke(1));

			Assert.AreEqual(false, new AtomicDelegate<Func<int, int, int>>().TryInvoke(1, 2));
			Assert.AreEqual((true, 3), new AtomicDelegate<Func<int, int, int>>((n1, n2) => n1 + n2).TryInvoke(1, 2));

			Assert.AreEqual(false, new AtomicDelegate<Func<int, int, int, int>>().TryInvoke(1, 2, 3));
			Assert.AreEqual((true, 6), new AtomicDelegate<Func<int, int, int, int>>((n1, n2, n3) => n1 + n2 + n3).TryInvoke(1, 2, 3));

			Assert.AreEqual(false, new AtomicDelegate<Func<int, int, int, int, int>>().TryInvoke(1, 2, 3, 4));
			Assert.AreEqual((true, 10), new AtomicDelegate<Func<int, int, int, int, int>>((n1, n2, n3, n4) => n1 + n2 + n3 + n4).TryInvoke(1, 2, 3, 4));

			Assert.AreEqual(false, new AtomicDelegate<Func<int, int, int, int, int, int>>().TryInvoke(1, 2, 3, 4, 5));
			Assert.AreEqual((true, 15), new AtomicDelegate<Func<int, int, int, int, int, int>>((n1, n2, n3, n4, n5) => n1 + n2 + n3 + n4 + n5).TryInvoke(1, 2, 3, 4, 5));

			Assert.AreEqual(false, new AtomicDelegate<Func<int, int, int, int, int, int, int>>().TryInvoke(1, 2, 3, 4, 5, 6));
			Assert.AreEqual((true, 21), new AtomicDelegate<Func<int, int, int, int, int, int, int>>((n1, n2, n3, n4, n5, n6) => n1 + n2 + n3 + n4 + n5 + n6).TryInvoke(1, 2, 3, 4, 5, 6));

			Assert.AreEqual(false, new AtomicDelegate<Func<int, int, int, int, int, int, int, int>>().TryInvoke(1, 2, 3, 4, 5, 6, 7));
			Assert.AreEqual((true, 28), new AtomicDelegate<Func<int, int, int, int, int, int, int, int>>((n1, n2, n3, n4, n5, n6, n7) => n1 + n2 + n3 + n4 + n5 + n6 + n7).TryInvoke(1, 2, 3, 4, 5, 6, 7));

			Assert.AreEqual(false, new AtomicDelegate<Func<int, int, int, int, int, int, int, int, int>>().TryInvoke(1, 2, 3, 4, 5, 6, 7, 8));
			Assert.AreEqual((true, 36), new AtomicDelegate<Func<int, int, int, int, int, int, int, int, int>>((n1, n2, n3, n4, n5, n6, n7, n8) => n1 + n2 + n3 + n4 + n5 + n6 + n7 + n8).TryInvoke(1, 2, 3, 4, 5, 6, 7, 8));

			Assert.AreEqual(false, new AtomicDelegate<Func<int, int, int, int, int, int, int, int, int, int>>().TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9));
			Assert.AreEqual((true, 45), new AtomicDelegate<Func<int, int, int, int, int, int, int, int, int, int>>((n1, n2, n3, n4, n5, n6, n7, n8, n9) => n1 + n2 + n3 + n4 + n5 + n6 + n7 + n8 + n9).TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9));

			Assert.AreEqual(false, new AtomicDelegate<Func<int, int, int, int, int, int, int, int, int, int, int>>().TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10));
			Assert.AreEqual((true, 55), new AtomicDelegate<Func<int, int, int, int, int, int, int, int, int, int, int>>((n1, n2, n3, n4, n5, n6, n7, n8, n9, n10) => n1 + n2 + n3 + n4 + n5 + n6 + n7 + n8 + n9 + n10).TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10));

			Assert.AreEqual(false, new AtomicDelegate<Func<int, int, int, int, int, int, int, int, int, int, int, int>>().TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11));
			Assert.AreEqual((true, 66), new AtomicDelegate<Func<int, int, int, int, int, int, int, int, int, int, int, int>>((n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11) => n1 + n2 + n3 + n4 + n5 + n6 + n7 + n8 + n9 + n10 + n11).TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11));

			Assert.AreEqual(false, new AtomicDelegate<Func<int, int, int, int, int, int, int, int, int, int, int, int, int>>().TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12));
			Assert.AreEqual((true, 78), new AtomicDelegate<Func<int, int, int, int, int, int, int, int, int, int, int, int, int>>((n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11, n12) => n1 + n2 + n3 + n4 + n5 + n6 + n7 + n8 + n9 + n10 + n11 + n12).TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12));

			Assert.AreEqual(false, new AtomicDelegate<Func<int, int, int, int, int, int, int, int, int, int, int, int, int, int>>().TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13));
			Assert.AreEqual((true, 91), new AtomicDelegate<Func<int, int, int, int, int, int, int, int, int, int, int, int, int, int>>((n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11, n12, n13) => n1 + n2 + n3 + n4 + n5 + n6 + n7 + n8 + n9 + n10 + n11 + n12 + n13).TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13));

			Assert.AreEqual(false, new AtomicDelegate<Func<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int>>().TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14));
			Assert.AreEqual((true, 105), new AtomicDelegate<Func<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int>>((n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11, n12, n13, n14) => n1 + n2 + n3 + n4 + n5 + n6 + n7 + n8 + n9 + n10 + n11 + n12 + n13 + n14).TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14));

			Assert.AreEqual(false, new AtomicDelegate<Func<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int>>().TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15));
			Assert.AreEqual((true, 120), new AtomicDelegate<Func<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int>>((n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11, n12, n13, n14, n15) => n1 + n2 + n3 + n4 + n5 + n6 + n7 + n8 + n9 + n10 + n11 + n12 + n13 + n14 + n15).TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15));

			Assert.AreEqual(false, new AtomicDelegate<Func<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int>>().TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16));
			Assert.AreEqual((true, 136), new AtomicDelegate<Func<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int>>((n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11, n12, n13, n14, n15, n16) => n1 + n2 + n3 + n4 + n5 + n6 + n7 + n8 + n9 + n10 + n11 + n12 + n13 + n14 + n15 + n16).TryInvoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16));
		}
		#endregion Tests
	}
}