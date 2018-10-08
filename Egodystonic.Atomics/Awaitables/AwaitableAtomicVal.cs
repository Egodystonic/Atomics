using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Egodystonic.Atomics.Awaitables {
	/// <summary>
	/// TODO mention that multi-consumer/multi-producer scenarios don't have consistent ordering. The point of this structure is not to be a queue, but to allow waiting for any particular value that matches the target/predicate
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class AwaitableAtomicVal<T> : IAwaitableAtomic<T> where T : struct, IEquatable<T> {
		public const int RecommendedDefaultBufferSize = 128;
		// ReSharper disable once StaticMemberInGenericType The value will be the same for each independent reification
		static readonly double NumStopwatchTicksPerSec = Stopwatch.Frequency;
		readonly AtomicVal<T> _value;
		readonly AwaitableConsumerValueQueuePool<T> _consumerQueuePool;
		readonly GarbageAndLockFreeBag<AwaitableConsumerValueQueue<T>> _activeConsumerQueues = new GarbageAndLockFreeBag<AwaitableConsumerValueQueue<T>>();

		public T Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => Set(value);
		}

		public AwaitableAtomicVal() : this(default) { }
		public AwaitableAtomicVal(T initialValue) : this(initialValue, RecommendedDefaultBufferSize) { }
		public AwaitableAtomicVal(T initialValue, int minBufferSize) {
			_value = new AtomicVal<T>(initialValue);
			_consumerQueuePool = new AwaitableConsumerValueQueuePool<T>(minBufferSize);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get() => _value.Get();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetUnsafe() => _value.GetUnsafe();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(T newValue) {
			_value.Set(newValue);
			PublishNewValue(newValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(T newValue) {
			_value.SetUnsafe(newValue);
			PublishNewValue(newValue);
		}

		public T Exchange(T newValue) {
			var result = _value.Exchange(newValue);
			PublishNewValue(result);
			return result;
		}

		public (bool ValueWasSet, T PreviousValue) TryExchange(T newValue, T comparand) {
			var result = _value.TryExchange(newValue, comparand);
			if (result.ValueWasSet) PublishNewValue(newValue);
			return result;
		}

		public (bool ValueWasSet, T PreviousValue) TryExchange(T newValue, Func<T, bool> predicate) {
			var result = _value.TryExchange(newValue, predicate);
			if (result.ValueWasSet) PublishNewValue(newValue);
			return result;
		}

		public (bool ValueWasSet, T PreviousValue) TryExchange(T newValue, Func<T, T, bool> predicate) {
			var result = _value.TryExchange(newValue, predicate);
			if (result.ValueWasSet) PublishNewValue(newValue);
			return result;
		}

		public (T PreviousValue, T NewValue) Exchange(Func<T, T> mapFunc) {
			var result = _value.Exchange(mapFunc);
			PublishNewValue(result.NewValue);
			return result;
		}

		public (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(Func<T, T> mapFunc, T comparand) {
			var result = _value.TryExchange(mapFunc, comparand);
			if (result.ValueWasSet) PublishNewValue(result.NewValue);
			return result;
		}

		public (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(Func<T, T> mapFunc, Func<T, bool> predicate) {
			var result = _value.TryExchange(mapFunc, predicate);
			if (result.ValueWasSet) PublishNewValue(result.NewValue);
			return result;
		}

		public (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(Func<T, T> mapFunc, Func<T, T, bool> predicate) {
			var result = _value.TryExchange(mapFunc, predicate);
			if (result.ValueWasSet) PublishNewValue(result.NewValue);
			return result;
		}

		// ============================ Waiter API ============================
		public (bool ValueAcquired, T Value) WaitForNext(CancellationToken cancellationToken, TimeSpan maxWaitTime) {
			var queue = GetConsumerQueue();
			var result = queue.Dequeue(cancellationToken, maxWaitTime);
			RelinquishConsumerQueue(queue);
			return result;
		}

		public (bool ValueAcquired, T Value) WaitForValue(T targetValue, CancellationToken cancellationToken, TimeSpan maxWaitTime) {
			var queue = GetConsumerQueue();

			do {
				var dequeueStartTime = GetTimestamp();
				var dequeueResult = queue.Dequeue(cancellationToken, maxWaitTime);
				if (!dequeueResult.ValueAcquired) break;

				if (ValueMatchesTarget(dequeueResult.Value, targetValue)) {
					RelinquishConsumerQueue(queue);
					return (true, dequeueResult.Value);
				}

				maxWaitTime -= GetTimeSinceTimestamp(dequeueStartTime);
			} while (maxWaitTime > TimeSpan.Zero);

			RelinquishConsumerQueue(queue);
			return (false, default);
		}

		public (bool ValueAcquired, T Value) WaitForValue(Func<T, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime) {
			var queue = GetConsumerQueue();

			do {
				var dequeueStartTime = GetTimestamp();
				var dequeueResult = queue.Dequeue(cancellationToken, maxWaitTime);
				if (!dequeueResult.ValueAcquired) break;

				if (ValueSatisfiesPredicate(dequeueResult.Value, predicate)) {
					RelinquishConsumerQueue(queue);
					return (true, dequeueResult.Value);
				}

				maxWaitTime -= GetTimeSinceTimestamp(dequeueStartTime);
			} while (maxWaitTime > TimeSpan.Zero);

			RelinquishConsumerQueue(queue);
			return (false, default);
		}

		public async Task<(bool ValueAcquired, T Value)> WaitForNextAsync(CancellationToken cancellationToken, TimeSpan maxWaitTime) {
			var queue = GetConsumerQueue();
			var result = await queue.DequeueAsync(cancellationToken, maxWaitTime);
			RelinquishConsumerQueue(queue);
			return result;
		}

		public async Task<(bool ValueAcquired, T Value)> WaitForValueAsync(T targetValue, CancellationToken cancellationToken, TimeSpan maxWaitTime) {
			var queue = GetConsumerQueue();

			do {
				var dequeueStartTime = GetTimestamp();
				var dequeueResult = await queue.DequeueAsync(cancellationToken, maxWaitTime);
				if (!dequeueResult.ValueAcquired) break;

				if (ValueMatchesTarget(dequeueResult.Value, targetValue)) {
					RelinquishConsumerQueue(queue);
					return (true, dequeueResult.Value);
				}

				maxWaitTime -= GetTimeSinceTimestamp(dequeueStartTime);
			} while (maxWaitTime > TimeSpan.Zero);

			RelinquishConsumerQueue(queue);
			return (false, default);
		}

		public async Task<(bool ValueAcquired, T Value)> WaitForValueAsync(Func<T, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime) {
			var queue = GetConsumerQueue();

			do {
				var dequeueStartTime = GetTimestamp();
				var dequeueResult = await queue.DequeueAsync(cancellationToken, maxWaitTime);
				if (!dequeueResult.ValueAcquired) break;

				if (ValueSatisfiesPredicate(dequeueResult.Value, predicate)) {
					RelinquishConsumerQueue(queue);
					return (true, dequeueResult.Value);
				}

				maxWaitTime -= GetTimeSinceTimestamp(dequeueStartTime);
			} while (maxWaitTime > TimeSpan.Zero);

			RelinquishConsumerQueue(queue);
			return (false, default);
		}

		public ValueTask<T> WaitForExpectedValueAsync(T targetValue) {
			var queue = GetConsumerQueue();
			var quickReadResult = queue.AttemptQuickRead();
			RelinquishConsumerQueue(queue);

			if (quickReadResult.ValueAcquired && ValueMatchesTarget(quickReadResult.Value, targetValue)) {
				return new ValueTask<T>(quickReadResult.Value);
			}
			else return new ValueTask<T>(this.WaitForValueAsync(targetValue));
		}

		public ValueTask<T> WaitForExpectedValueAsync(Func<T, bool> predicate) {
			var queue = GetConsumerQueue();
			var quickReadResult = queue.AttemptQuickRead();
			RelinquishConsumerQueue(queue);

			if (quickReadResult.ValueAcquired && ValueSatisfiesPredicate(quickReadResult.Value, predicate)) {
				return new ValueTask<T>(quickReadResult.Value);
			}
			else return new ValueTask<T>(this.WaitForValueAsync(predicate));
		}

		public ValueTask<(bool ValueAcquired, T Value)> WaitForExpectedValueAsync(T targetValue, CancellationToken cancellationToken, TimeSpan maxWaitTime) {
			var queue = GetConsumerQueue();
			var quickReadResult = queue.AttemptQuickRead();
			RelinquishConsumerQueue(queue);

			if (quickReadResult.ValueAcquired && ValueMatchesTarget(quickReadResult.Value, targetValue)) {
				return new ValueTask<(bool ValueAcquired, T Value)>((true, quickReadResult.Value));
			}
			else return new ValueTask<(bool ValueAcquired, T Value)>(WaitForValueAsync(targetValue, cancellationToken, maxWaitTime));
		}

		public ValueTask<(bool ValueAcquired, T Value)> WaitForExpectedValueAsync(Func<T, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime) {
			var queue = GetConsumerQueue();
			var quickReadResult = queue.AttemptQuickRead();
			RelinquishConsumerQueue(queue);

			if (quickReadResult.ValueAcquired && ValueSatisfiesPredicate(quickReadResult.Value, predicate)) {
				return new ValueTask<(bool ValueAcquired, T Value)>((true, quickReadResult.Value));
			}
			else return new ValueTask<(bool ValueAcquired, T Value)>(WaitForValueAsync(predicate, cancellationToken, maxWaitTime));
		}

		void PublishNewValue(T newValue) {
			foreach (var queue in _activeConsumerQueues) queue.Enqueue(newValue);
		}

		AwaitableConsumerValueQueue<T> GetConsumerQueue() {
			var result = _consumerQueuePool.BorrowOne();
			result.Enqueue(_value);
			_activeConsumerQueues.Add(result);
			return result;
		}

		void RelinquishConsumerQueue(AwaitableConsumerValueQueue<T> queueToRelinquish) {
			_activeConsumerQueues.Remove(queueToRelinquish);
			_consumerQueuePool.ReturnOne(queueToRelinquish);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool ValueMatchesTarget(T value, T target) => value.Equals(target);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool ValueSatisfiesPredicate(T value, Func<T, bool> predicate) => predicate(value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static long GetTimestamp() => Stopwatch.GetTimestamp();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static TimeSpan GetTimeSinceTimestamp(long timestamp) => TimeSpan.FromSeconds((GetTimestamp() - timestamp) / NumStopwatchTicksPerSec);
	}
}