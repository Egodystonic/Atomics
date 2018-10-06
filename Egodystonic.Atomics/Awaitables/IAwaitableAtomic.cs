using System;
using System.Threading;
using System.Threading.Tasks;
using Egodystonic.Atomics.Numerics;

namespace Egodystonic.Atomics.Awaitables {
	public interface IAwaitableAtomic<T> : IAtomic<T> {
		// ============================ Synchronous API ============================
		(bool ValueAcquired, T Value) WaitForNext(CancellationToken cancellationToken, TimeSpan maxWaitTime);

		(bool ValueAcquired, T Value) WaitForValue(T targetValue, CancellationToken cancellationToken, TimeSpan maxWaitTime);
		(bool ValueAcquired, T Value) WaitForValue(Func<T, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime);



		// ============================ Asynchronous API ============================
		Task<(bool ValueAcquired, T Value)> WaitForNextAsync(CancellationToken cancellationToken, TimeSpan maxWaitTime);

		Task<(bool ValueAcquired, T Value)> WaitForValueAsync(T targetValue, CancellationToken cancellationToken, TimeSpan maxWaitTime);
		Task<(bool ValueAcquired, T Value)> WaitForValueAsync(Func<T, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime);



		// ============================ Low-Garbage Asynchronous API ============================
		ValueTask<T> WaitForExpectedValueAsync(T targetValue);
		ValueTask<T> WaitForExpectedValueAsync(Func<T, bool> predicate);
		ValueTask<(bool ValueAcquired, T Value)> WaitForExpectedValueAsync(T targetValue, CancellationToken cancellationToken, TimeSpan maxWaitTime);
		ValueTask<(bool ValueAcquired, T Value)> WaitForExpectedValueAsync(Func<T, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime);
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

		public static (bool ValueAcquired, T Value) WaitForValue<T>(this IAwaitableAtomic<T> @this, T targetValue) => @this.WaitForValue(targetValue, CancellationToken.None, TimeSpan.MaxValue);

		public static (bool ValueAcquired, T Value) WaitForValue<T>(this IAwaitableAtomic<T> @this, T targetValue, CancellationToken cancellationToken) => @this.WaitForValue(targetValue, cancellationToken, TimeSpan.MaxValue);

		public static (bool ValueAcquired, T Value) WaitForValue<T>(this IAwaitableAtomic<T> @this, T targetValue, TimeSpan maxWaitTime) => @this.WaitForValue(targetValue, CancellationToken.None, maxWaitTime);

		public static T WaitForValue<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate) => @this.WaitForValue(predicate, CancellationToken.None, TimeSpan.MaxValue).Value;

		public static (bool ValueAcquired, T Value) WaitForValue<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, CancellationToken cancellationToken) => @this.WaitForValue(predicate, cancellationToken, TimeSpan.MaxValue);

		public static (bool ValueAcquired, T Value) WaitForValue<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, TimeSpan maxWaitTime) => @this.WaitForValue(predicate, CancellationToken.None, maxWaitTime);


		// ============================ Asynchronous API ============================
		public static Task<T> WaitForNextAsync<T>(this IAwaitableAtomic<T> @this) => @this.WaitForNextAsync(CancellationToken.None, TimeSpan.MaxValue).ContinueWith(t => t.Result.Value);

		public static Task<(bool ValueAcquired, T Value)> WaitForNextAsync<T>(this IAwaitableAtomic<T> @this, CancellationToken cancellationToken) => @this.WaitForNextAsync(cancellationToken, TimeSpan.MaxValue);

		public static Task<(bool ValueAcquired, T Value)> WaitForNextAsync<T>(this IAwaitableAtomic<T> @this, TimeSpan maxWaitTime) => @this.WaitForNextAsync(CancellationToken.None, maxWaitTime);

		public static Task<T> WaitForValueAsync<T>(this IAwaitableAtomic<T> @this, T targetValue) => @this.WaitForValueAsync(targetValue, CancellationToken.None, TimeSpan.MaxValue).ContinueWith(t => t.Result.Value);

		public static Task<(bool ValueAcquired, T Value)> WaitForValueAsync<T>(this IAwaitableAtomic<T> @this, T targetValue, CancellationToken cancellationToken) => @this.WaitForValueAsync(targetValue, cancellationToken, TimeSpan.MaxValue);

		public static Task<(bool ValueAcquired, T Value)> WaitForValueAsync<T>(this IAwaitableAtomic<T> @this, T targetValue, TimeSpan maxWaitTime) => @this.WaitForValueAsync(targetValue, CancellationToken.None, maxWaitTime);

		public static Task<T> WaitForValueAsync<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate) => @this.WaitForValueAsync(predicate, CancellationToken.None, TimeSpan.MaxValue).ContinueWith(t => t.Result.Value);

		public static Task<(bool ValueAcquired, T Value)> WaitForValueAsync<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, CancellationToken cancellationToken) => @this.WaitForValueAsync(predicate, cancellationToken, TimeSpan.MaxValue);

		public static Task<(bool ValueAcquired, T Value)> WaitForValueAsync<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, TimeSpan maxWaitTime) => @this.WaitForValueAsync(predicate, CancellationToken.None, maxWaitTime);



		// ============================ Low-Garbage Asynchronous API ============================
		public static ValueTask<(bool ValueAcquired, T Value)> WaitForExpectedValueAsync<T>(this IAwaitableAtomic<T> @this, T targetValue, CancellationToken cancellationToken) => @this.WaitForExpectedValueAsync(targetValue, cancellationToken, TimeSpan.MaxValue);

		public static ValueTask<(bool ValueAcquired, T Value)> WaitForExpectedValueAsync<T>(this IAwaitableAtomic<T> @this, T targetValue, TimeSpan maxWaitTime) => @this.WaitForExpectedValueAsync(targetValue, CancellationToken.None, maxWaitTime);

		public static ValueTask<(bool ValueAcquired, T Value)> WaitForExpectedValueAsync<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, CancellationToken cancellationToken) => @this.WaitForExpectedValueAsync(predicate, cancellationToken, TimeSpan.MaxValue);

		public static ValueTask<(bool ValueAcquired, T Value)> WaitForExpectedValueAsync<T>(this IAwaitableAtomic<T> @this, Func<T, bool> predicate, TimeSpan maxWaitTime) => @this.WaitForExpectedValueAsync(predicate, CancellationToken.None, maxWaitTime);
	}
}