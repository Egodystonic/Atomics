using System;

namespace Egodystonic.Atomics.Awaitables {
	public static class AwaitableAtomicFactory {
		public static IAwaitableAtomic<T> CreateAtomicRef<T>() where T : class => new AwaitableAtomicWrapper<T>(new AtomicRef<T>());
		public static IAwaitableAtomic<T> CreateAtomicRef<T>(T initialValue) where T : class => new AwaitableAtomicWrapper<T>(new AtomicRef<T>(initialValue));

		public static IAwaitableAtomic<T> CreateAtomicVal<T>() where T : struct, IEquatable<T> => new AwaitableAtomicWrapper<T>(new AtomicVal<T>());
		public static IAwaitableAtomic<T> CreateAtomicVal<T>(T initialValue) where T : struct, IEquatable<T> => new AwaitableAtomicWrapper<T>(new AtomicVal<T>(initialValue));

		public static IAwaitableNumericAtomic<int> CreateAtomicInt() =>
		public static IAwaitableNumericAtomic<int> CreateAtomicInt(int initialValue) =>
		public static IAwaitableNumericAtomic<long> CreateAtomicLong() =>
		public static IAwaitableNumericAtomic<long> CreateAtomicLong(long initialValue) =>
		public static IAwaitableNumericAtomic<float> CreateAtomicFloat() =>
		public static IAwaitableNumericAtomic<float> CreateAtomicFloat(float initialValue) =>
		public static IAwaitableNumericAtomic<double> CreateAtomicDouble() =>
		public static IAwaitableNumericAtomic<double> CreateAtomicDouble(double initialValue) =>
	}
}