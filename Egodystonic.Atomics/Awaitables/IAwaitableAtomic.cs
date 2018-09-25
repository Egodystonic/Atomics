using System;
using System.Threading;
using System.Threading.Tasks;
using Egodystonic.Atomics.Numerics;

namespace Egodystonic.Atomics.Awaitables {
	public interface IAwaitableAtomic<T> : IAtomic<T> {
		AtomicValueBackstop CurrentValueBackstop { get; }

		// ============================ Synchronous API ============================
		(bool ValueAcquired, T Value) WaitForNext(CancellationToken cancellationToken, TimeSpan maxWaitTime);

		bool WaitForValue(T targetValue, CancellationToken cancellationToken, TimeSpan maxWaitTime, AtomicValueBackstop backstop);
		(bool ValueAcquired, T Value) WaitForValue(Func<T, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime, AtomicValueBackstop backstop);



		// ============================ Asynchronous API ============================
		Task<(bool ValueAcquired, T Value)> WaitForNextAsync(CancellationToken cancellationToken, TimeSpan maxWaitTime);

		Task<bool> WaitForValueAsync(T targetValue, CancellationToken cancellationToken, TimeSpan maxWaitTime, AtomicValueBackstop backstop);
		Task<(bool ValueAcquired, T Value)> WaitForValueAsync(Func<T, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime, AtomicValueBackstop backstop);



		// ============================ Low-Garbage Asynchronous API ============================
		ValueTask WaitForExpectedValueAsync(T targetValue, AtomicValueBackstop backstop);
		ValueTask<T> WaitForExpectedValueAsync(Func<T, bool> targetValue, AtomicValueBackstop backstop);
		ValueTask<bool> WaitForExpectedValueAsync(T targetValue, CancellationToken cancellationToken, TimeSpan maxWaitTime, AtomicValueBackstop backstop);
		ValueTask<(bool ValueAcquired, T Value)> WaitForExpectedValueAsync(Func<T, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime, AtomicValueBackstop backstop);
	}

	public interface IAwaitableNumericAtomic<T> : IAwaitableAtomic<T>, INumericAtomic<T> { }

	public static class AwaitableAtomicExtensions {
		// These are used instead of default parameters because:
		//	1) The value we use for the 'default' TimeSpan is MaxValue, not default(TimeSpan),
		//	2) We can specialize the return types to something simpler occasionally; which also helps effectively
		//		communicate to users the possible termination states of any of these calls (i.e. no 'bool ValueAcquired'
		//		indicates the function will block until the target is set/predicate is satisfied)

		// ============================ Synchronous API ============================
		public static T WaitForNext<T>(this IAwaitableAtomic<T> @this) => @this.WaitForNext(CancellationToken.None, TimeSpan.MaxValue).Value;

		public static (bool ValueAcquired, T Value) WaitForNext<T>(this IAwaitableAtomic<T> @this, CancellationToken cancellationToken) => @this.WaitForNext(cancellationToken, TimeSpan.MaxValue);

		public static (bool ValueAcquired, T Value) WaitForNext<T>(this IAwaitableAtomic<T> @this, TimeSpan maxWaitTime) => @this.WaitForNext(CancellationToken.None, maxWaitTime);

		public static bool WaitForValue<T>(this IAwaitableAtomic<T> @this, T targetValue) => @this.WaitForValue(targetValue, CancellationToken.None, TimeSpan.MaxValue, AtomicValueBackstop.None);

		public static bool WaitForValue<T>(this IAwaitableAtomic<T> @this, T targetValue, CancellationToken cancellationToken) => @this.WaitForValue(targetValue, cancellationToken, TimeSpan.MaxValue, AtomicValueBackstop.None);

		public static bool WaitForValue<T>(this IAwaitableAtomic<T> @this, T targetValue, TimeSpan maxWaitTime) => @this.WaitForValue(targetValue, CancellationToken.None, maxWaitTime, AtomicValueBackstop.None);

		public static bool WaitForValue<T>(this IAwaitableAtomic<T> @this, T targetValue, AtomicValueBackstop backstop) => @this.WaitForValue(targetValue, CancellationToken.None, TimeSpan.MaxValue, backstop);

		public static bool WaitForValue<T>(this IAwaitableAtomic<T> @this, T targetValue, CancellationToken cancellationToken, TimeSpan maxWaitTime) => @this.WaitForValue(targetValue, cancellationToken, maxWaitTime, AtomicValueBackstop.None);

		public static bool WaitForValue<T>(this IAwaitableAtomic<T> @this, T targetValue, CancellationToken cancellationToken, AtomicValueBackstop backstop) => @this.WaitForValue(targetValue, cancellationToken, TimeSpan.MaxValue, backstop);

		public static bool WaitForValue<T>(this IAwaitableAtomic<T> @this, T targetValue, TimeSpan maxWaitTime, AtomicValueBackstop backstop) => @this.WaitForValue(targetValue, CancellationToken.None, maxWaitTime, backstop);

		public static bool WaitForValue<T>(this IAwaitableAtomic<T> @this, T targetValue, CancellationToken cancellationToken, TimeSpan maxWaitTime, AtomicValueBackstop backstop) => @this.WaitForValue(targetValue, cancellationToken, maxWaitTime, backstop);

		public static T WaitForValue<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate) => @this.WaitForValue(predicate, CancellationToken.None, TimeSpan.MaxValue, AtomicValueBackstop.None).Value;

		public static (bool ValueAcquired, T Value) WaitForValue<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, CancellationToken cancellationToken) => @this.WaitForValue(predicate, cancellationToken, TimeSpan.MaxValue, AtomicValueBackstop.None);

		public static (bool ValueAcquired, T Value) WaitForValue<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, TimeSpan maxWaitTime) => @this.WaitForValue(predicate, CancellationToken.None, maxWaitTime, AtomicValueBackstop.None);

		public static T WaitForValue<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, AtomicValueBackstop backstop) => @this.WaitForValue(predicate, CancellationToken.None, TimeSpan.MaxValue, backstop).Value;

		public static (bool ValueAcquired, T Value) WaitForValue<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime) => @this.WaitForValue(predicate, cancellationToken, maxWaitTime, AtomicValueBackstop.None);

		public static (bool ValueAcquired, T Value) WaitForValue<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, CancellationToken cancellationToken, AtomicValueBackstop backstop) => @this.WaitForValue(predicate, cancellationToken, TimeSpan.MaxValue, backstop);

		public static (bool ValueAcquired, T Value) WaitForValue<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, TimeSpan maxWaitTime, AtomicValueBackstop backstop) => @this.WaitForValue(predicate, CancellationToken.None, maxWaitTime, backstop);

		public static (bool ValueAcquired, T Value) WaitForValue<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime, AtomicValueBackstop backstop) => @this.WaitForValue(predicate, cancellationToken, maxWaitTime, backstop);


		// ============================ Asynchronous API ============================
		public static Task<T> WaitForNextAsync<T>(this IAwaitableAtomic<T> @this) => @this.WaitForNextAsync(CancellationToken.None, TimeSpan.MaxValue).ContinueWith(t => t.Result.Value);

		public static Task<(bool ValueAcquired, T Value)> WaitForNextAsync<T>(this IAwaitableAtomic<T> @this, CancellationToken cancellationToken) => @this.WaitForNextAsync(cancellationToken, TimeSpan.MaxValue);

		public static Task<(bool ValueAcquired, T Value)> WaitForNextAsync<T>(this IAwaitableAtomic<T> @this, TimeSpan maxWaitTime) => @this.WaitForNextAsync(CancellationToken.None, maxWaitTime);

		public static Task WaitForValueAsync<T>(this IAwaitableAtomic<T> @this, T targetValue) => @this.WaitForValueAsync(targetValue, CancellationToken.None, TimeSpan.MaxValue, AtomicValueBackstop.None);

		public static Task<bool> WaitForValueAsync<T>(this IAwaitableAtomic<T> @this, T targetValue, CancellationToken cancellationToken) => @this.WaitForValueAsync(targetValue, cancellationToken, TimeSpan.MaxValue, AtomicValueBackstop.None);

		public static Task<bool> WaitForValueAsync<T>(this IAwaitableAtomic<T> @this, T targetValue, TimeSpan maxWaitTime) => @this.WaitForValueAsync(targetValue, CancellationToken.None, maxWaitTime, AtomicValueBackstop.None);

		public static Task<bool> WaitForValueAsync<T>(this IAwaitableAtomic<T> @this, T targetValue, AtomicValueBackstop backstop) => @this.WaitForValueAsync(targetValue, CancellationToken.None, TimeSpan.MaxValue, backstop);

		public static Task<bool> WaitForValueAsync<T>(this IAwaitableAtomic<T> @this, T targetValue, CancellationToken cancellationToken, TimeSpan maxWaitTime) => @this.WaitForValueAsync(targetValue, cancellationToken, maxWaitTime, AtomicValueBackstop.None);

		public static Task<bool> WaitForValueAsync<T>(this IAwaitableAtomic<T> @this, T targetValue, CancellationToken cancellationToken, AtomicValueBackstop backstop) => @this.WaitForValueAsync(targetValue, cancellationToken, TimeSpan.MaxValue, backstop);

		public static Task<bool> WaitForValueAsync<T>(this IAwaitableAtomic<T> @this, T targetValue, TimeSpan maxWaitTime, AtomicValueBackstop backstop) => @this.WaitForValueAsync(targetValue, CancellationToken.None, maxWaitTime, backstop);

		public static Task<bool> WaitForValueAsync<T>(this IAwaitableAtomic<T> @this, T targetValue, CancellationToken cancellationToken, TimeSpan maxWaitTime, AtomicValueBackstop backstop) => @this.WaitForValueAsync(targetValue, cancellationToken, maxWaitTime, backstop);

		public static Task<T> WaitForValueAsync<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate) => @this.WaitForValueAsync(predicate, CancellationToken.None, TimeSpan.MaxValue, AtomicValueBackstop.None).ContinueWith(t => t.Result.Value);

		public static Task<(bool ValueAcquired, T Value)> WaitForValueAsync<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, CancellationToken cancellationToken) => @this.WaitForValueAsync(predicate, cancellationToken, TimeSpan.MaxValue, AtomicValueBackstop.None);

		public static Task<(bool ValueAcquired, T Value)> WaitForValueAsync<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, TimeSpan maxWaitTime) => @this.WaitForValueAsync(predicate, CancellationToken.None, maxWaitTime, AtomicValueBackstop.None);

		public static Task<T> WaitForValueAsync<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, AtomicValueBackstop backstop) => @this.WaitForValueAsync(predicate, CancellationToken.None, TimeSpan.MaxValue, backstop).ContinueWith(t => t.Result.Value);

		public static Task<(bool ValueAcquired, T Value)> WaitForValueAsync<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime) => @this.WaitForValueAsync(predicate, cancellationToken, maxWaitTime, AtomicValueBackstop.None);

		public static Task<(bool ValueAcquired, T Value)> WaitForValueAsync<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, CancellationToken cancellationToken, AtomicValueBackstop backstop) => @this.WaitForValueAsync(predicate, cancellationToken, TimeSpan.MaxValue, backstop);

		public static Task<(bool ValueAcquired, T Value)> WaitForValueAsync<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, TimeSpan maxWaitTime, AtomicValueBackstop backstop) => @this.WaitForValueAsync(predicate, CancellationToken.None, maxWaitTime, backstop);

		public static Task<(bool ValueAcquired, T Value)> WaitForValueAsync<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime, AtomicValueBackstop backstop) => @this.WaitForValueAsync(predicate, cancellationToken, maxWaitTime, backstop);


		// ============================ Low-Garbage Asynchronous API ============================
		public static ValueTask WaitForExpectedValueAsync<T>(this IAwaitableAtomic<T> @this, T targetValue) => @this.WaitForExpectedValueAsync(targetValue, AtomicValueBackstop.None);

		public static ValueTask<bool> WaitForExpectedValueAsync<T>(this IAwaitableAtomic<T> @this, T targetValue, CancellationToken cancellationToken) => @this.WaitForExpectedValueAsync(targetValue, cancellationToken, TimeSpan.MaxValue, AtomicValueBackstop.None);

		public static ValueTask<bool> WaitForExpectedValueAsync<T>(this IAwaitableAtomic<T> @this, T targetValue, TimeSpan maxWaitTime) => @this.WaitForExpectedValueAsync(targetValue, CancellationToken.None, maxWaitTime, AtomicValueBackstop.None);

		public static ValueTask<bool> WaitForExpectedValueAsync<T>(this IAwaitableAtomic<T> @this, T targetValue, CancellationToken cancellationToken, TimeSpan maxWaitTime) => @this.WaitForExpectedValueAsync(targetValue, cancellationToken, maxWaitTime, AtomicValueBackstop.None);

		public static ValueTask<bool> WaitForExpectedValueAsync<T>(this IAwaitableAtomic<T> @this, T targetValue, CancellationToken cancellationToken, AtomicValueBackstop backstop) => @this.WaitForExpectedValueAsync(targetValue, cancellationToken, TimeSpan.MaxValue, backstop);

		public static ValueTask<bool> WaitForExpectedValueAsync<T>(this IAwaitableAtomic<T> @this, T targetValue, TimeSpan maxWaitTime, AtomicValueBackstop backstop) => @this.WaitForExpectedValueAsync(targetValue, CancellationToken.None, maxWaitTime, backstop);

		public static ValueTask<bool> WaitForExpectedValueAsync<T>(this IAwaitableAtomic<T> @this, T targetValue, CancellationToken cancellationToken, TimeSpan maxWaitTime, AtomicValueBackstop backstop) => @this.WaitForExpectedValueAsync(targetValue, cancellationToken, maxWaitTime, backstop);

		public static ValueTask<T> WaitForExpectedValueAsync<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate) => @this.WaitForExpectedValueAsync(predicate, AtomicValueBackstop.None);

		public static ValueTask<(bool ValueAcquired, T Value)> WaitForExpectedValueAsync<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, CancellationToken cancellationToken) => @this.WaitForExpectedValueAsync(predicate, cancellationToken, TimeSpan.MaxValue, AtomicValueBackstop.None);

		public static ValueTask<(bool ValueAcquired, T Value)> WaitForExpectedValueAsync<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, TimeSpan maxWaitTime) => @this.WaitForExpectedValueAsync(predicate, CancellationToken.None, maxWaitTime, AtomicValueBackstop.None);

		public static ValueTask<(bool ValueAcquired, T Value)> WaitForExpectedValueAsync<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime) => @this.WaitForExpectedValueAsync(predicate, cancellationToken, maxWaitTime, AtomicValueBackstop.None);

		public static ValueTask<(bool ValueAcquired, T Value)> WaitForExpectedValueAsync<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, CancellationToken cancellationToken, AtomicValueBackstop backstop) => @this.WaitForExpectedValueAsync(predicate, cancellationToken, TimeSpan.MaxValue, backstop);

		public static ValueTask<(bool ValueAcquired, T Value)> WaitForExpectedValueAsync<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, TimeSpan maxWaitTime, AtomicValueBackstop backstop) => @this.WaitForExpectedValueAsync(predicate, CancellationToken.None, maxWaitTime, backstop);

		public static ValueTask<(bool ValueAcquired, T Value)> WaitForExpectedValueAsync<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime, AtomicValueBackstop backstop) => @this.WaitForExpectedValueAsync(predicate, cancellationToken, maxWaitTime, backstop);
	}
}