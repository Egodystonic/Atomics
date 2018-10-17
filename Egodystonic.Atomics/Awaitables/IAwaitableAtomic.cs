using System;
using System.Threading;
using System.Threading.Tasks;
using Egodystonic.Atomics.Numerics;

namespace Egodystonic.Atomics.Awaitables {
	public interface IAwaitableAtomic<T> : IAtomic<T> {
		// ============================ Synchronous API ============================
		T WaitForNext();
//		(bool ValueAcquired, T Value) WaitForNext(TimeSpan maxWaitTime);
//		(bool ValueAcquired, T Value) WaitForNext(CancellationToken cancellationToken);
//		(bool ValueAcquired, T Value) WaitForNext(TimeSpan maxWaitTime, CancellationToken cancellationToken);
//
//		T WaitForValue(T targetValue);
//		(bool ValueAcquired, T Value) WaitForValue(T targetValue, TimeSpan maxWaitTime);
//		(bool ValueAcquired, T Value) WaitForValue(T targetValue, CancellationToken cancellationToken);
//		(bool ValueAcquired, T Value) WaitForValue(T targetValue, TimeSpan maxWaitTime, CancellationToken cancellationToken);
//
//		T WaitForValue(Func<T, bool> predicate);
//		(bool ValueAcquired, T Value) WaitForValue(Func<T, bool> predicate, TimeSpan maxWaitTime);
//		(bool ValueAcquired, T Value) WaitForValue(Func<T, bool> predicate, CancellationToken cancellationToken);
//		(bool ValueAcquired, T Value) WaitForValue(Func<T, bool> predicate, TimeSpan maxWaitTime, CancellationToken cancellationToken);
//
//		T WaitForValue<TContext>(Func<T, TContext, bool> predicate, TContext context);
//		(bool ValueAcquired, T Value) WaitForValue<TContext>(Func<T, TContext, bool> predicate, TimeSpan maxWaitTime, TContext context);
//		(bool ValueAcquired, T Value) WaitForValue<TContext>(Func<T, TContext, bool> predicate, CancellationToken cancellationToken, TContext context);
//		(bool ValueAcquired, T Value) WaitForValue<TContext>(Func<T, TContext, bool> predicate, TimeSpan maxWaitTime, CancellationToken cancellationToken, TContext context);
//
//
//
//		// ============================ Asynchronous API ============================
//		Task<T> WaitForNextAsync();
//		Task<(bool ValueAcquired, T Value)> WaitForNextAsync(CancellationToken cancellationToken = default, TimeSpan maxWaitTime = default);
//
//		Task<T> WaitForValueAsync(T targetValue);
//		Task<(bool ValueAcquired, T Value)> WaitForValueAsync(T targetValue, CancellationToken cancellationToken, TimeSpan maxWaitTime);
//
//		Task<T> WaitForValueAsync(Func<T, bool> predicate);
//		Task<(bool ValueAcquired, T Value)> WaitForValueAsync(Func<T, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime);
//
//		Task<T> WaitForValueAsync<TContext>(Func<T, TContext, bool> predicate, TContext context);
//		Task<(bool ValueAcquired, T Value)> WaitForValueAsync<TContext>(Func<T, TContext, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime, TContext context);
//
//
//
//		// ============================ Low-Garbage Asynchronous API ============================
//		ValueTask<T> WaitForExpectedValueAsync(T targetValue);
//		ValueTask<(bool ValueAcquired, T Value)> WaitForExpectedValueAsync(T targetValue, CancellationToken cancellationToken, TimeSpan maxWaitTime);
//
//		ValueTask<T> WaitForExpectedValueAsync(Func<T, bool> predicate);
//		ValueTask<(bool ValueAcquired, T Value)> WaitForExpectedValueAsync(Func<T, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime);
//
//		ValueTask<T> WaitForExpectedValueAsync<TContext>(Func<T, TContext, bool> predicate, TContext context);
//		ValueTask<(bool ValueAcquired, T Value)> WaitForExpectedValueAsync<TContext>(Func<T, TContext, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime, TContext context);
	}

	public interface IAwaitableNumericAtomic<T> : IAwaitableAtomic<T>, INumericAtomic<T> { }
}