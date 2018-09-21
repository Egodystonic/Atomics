using System;
using System.Threading;
using System.Threading.Tasks;

namespace Egodystonic.Atomics.Awaitables {
	public interface IAwaitableAtomic<T> : IAtomic<T> {
		// ============================ Synchronous API ============================
		T WaitForNext();

		(bool ValueAcquired, T Value) WaitForNext(TimeSpan maxWaitTime);
		(bool ValueAcquired, T Value) WaitForNext(CancellationToken cancellationToken);

		T WaitForValue(AtomicValueBackstop backstop);
		T WaitForValue(Func<T, bool> predicate);
		T WaitForValue(AtomicValueBackstop backstop, Func<T, bool> predicate);

		(bool ValueAcquired, T Value) WaitForValue(AtomicValueBackstop backstop, TimeSpan maxWaitTime);
		(bool ValueAcquired, T Value) WaitForValue(AtomicValueBackstop backstop, CancellationToken cancellationToken);

		(bool ValueAcquired, T Value) WaitForValue(Func<T, bool> predicate, TimeSpan maxWaitTime);
		(bool ValueAcquired, T Value) WaitForValue(Func<T, bool> predicate, CancellationToken cancellationToken);

		(bool ValueAcquired, T Value) WaitForValue(AtomicValueBackstop backstop, Func<T, bool> predicate, TimeSpan maxWaitTime);
		(bool ValueAcquired, T Value) WaitForValue(AtomicValueBackstop backstop, Func<T, bool> predicate, CancellationToken cancellationToken);



		// ============================ Asynchronous API ============================
		T WaitForNextAsync();

		Task<(bool ValueAcquired, T Value)> WaitForNextAsync(TimeSpan maxWaitTime);
		Task<(bool ValueAcquired, T Value)> WaitForNextAsync(CancellationToken cancellationToken);

		Task<T> WaitForValueAsync(Func<T, bool> predicate);

		Task<(bool ValueAcquired, T Value)> WaitForValueAsync(Func<T, bool> predicate, TimeSpan maxWaitTime);
		Task<(bool ValueAcquired, T Value)> WaitForValueAsync(Func<T, bool> predicate, CancellationToken cancellationToken);



		// ============================ Low-Garbage Asynchronous API ============================
		ValueTask<T> TryGetImmediateValueAsync(Func<T, bool> predicate);

		ValueTask<(bool ValueAcquired, T Value)> TryGetImmediateValueAsync(Func<T, bool> predicate, TimeSpan maxWaitTime);
		ValueTask<(bool ValueAcquired, T Value)> TryGetImmediateValueAsync(Func<T, bool> predicate, CancellationToken cancellationToken);
	}
}