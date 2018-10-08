using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Egodystonic.Atomics.Numerics;

namespace Egodystonic.Atomics.Awaitables {
	/// <summary>
	/// TODO mention that multi-consumer/multi-producer scenarios don't have consistent ordering. The point of this structure is not to be a queue, but to allow waiting for any particular value that matches the target/predicate
	/// </summary>
	public class AwaitableAtomicInt : IAwaitableNumericAtomic<int> {
		public const int RecommendedDefaultBufferSize = 128;
		// ReSharper disable once StaticMemberInGenericType The value will be the same for each independent reification
		static readonly double NumStopwatchTicksPerSec = Stopwatch.Frequency;
		readonly AtomicInt _value;
		readonly AwaitableConsumerValueQueuePool<int> _consumerQueuePool;
		readonly GarbageAndLockFreeBag<AwaitableConsumerValueQueue<int>> _activeConsumerQueues = new GarbageAndLockFreeBag<AwaitableConsumerValueQueue<int>>();

		public int Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => Set(value);
		}

		public AwaitableAtomicInt() : this(default) { }
		public AwaitableAtomicInt(int initialValue) : this(initialValue, RecommendedDefaultBufferSize) { }
		public AwaitableAtomicInt(int initialValue, int minBufferSize) {
			_value = new AtomicInt(initialValue);
			_consumerQueuePool = new AwaitableConsumerValueQueuePool<int>(minBufferSize);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Get() => _value.Get();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetUnsafe() => _value.GetUnsafe();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(int newValue) {
			_value.Set(newValue);
			PublishNewValue(newValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(int newValue) {
			_value.SetUnsafe(newValue);
			PublishNewValue(newValue);
		}

		public int Exchange(int newValue) {
			var result = _value.Exchange(newValue);
			PublishNewValue(result);
			return result;
		}

		public (bool ValueWasSet, int PreviousValue) TryExchange(int newValue, int comparand) {
			var result = _value.TryExchange(newValue, comparand);
			if (result.ValueWasSet) PublishNewValue(newValue);
			return result;
		}

		public (bool ValueWasSet, int PreviousValue) TryExchange(int newValue, Func<int, bool> predicate) {
			var result = _value.TryExchange(newValue, predicate);
			if (result.ValueWasSet) PublishNewValue(newValue);
			return result;
		}

		public (bool ValueWasSet, int PreviousValue) TryExchange(int newValue, Func<int, int, bool> predicate) {
			var result = _value.TryExchange(newValue, predicate);
			if (result.ValueWasSet) PublishNewValue(newValue);
			return result;
		}

		public (int PreviousValue, int NewValue) Exchange(Func<int, int> mapFunc) {
			var result = _value.Exchange(mapFunc);
			PublishNewValue(result.NewValue);
			return result;
		}

		public (bool ValueWasSet, int PreviousValue, int NewValue) TryExchange(Func<int, int> mapFunc, int comparand) {
			var result = _value.TryExchange(mapFunc, comparand);
			if (result.ValueWasSet) PublishNewValue(result.NewValue);
			return result;
		}

		public (bool ValueWasSet, int PreviousValue, int NewValue) TryExchange(Func<int, int> mapFunc, Func<int, bool> predicate) {
			var result = _value.TryExchange(mapFunc, predicate);
			if (result.ValueWasSet) PublishNewValue(result.NewValue);
			return result;
		}

		public (bool ValueWasSet, int PreviousValue, int NewValue) TryExchange(Func<int, int> mapFunc, Func<int, int, bool> predicate) {
			var result = _value.TryExchange(mapFunc, predicate);
			if (result.ValueWasSet) PublishNewValue(result.NewValue);
			return result;
		}


		// ============================ Numeric API ============================
		public (int PreviousValue, int NewValue) Increment() {
			var result = _value.Increment();
			PublishNewValue(result.NewValue);
			return result;
		}
		public (int PreviousValue, int NewValue) Decrement() {
			var result = _value.Decrement();
			PublishNewValue(result.NewValue);
			return result;
		}
		public (int PreviousValue, int NewValue) Add(int operand) {
			var result = _value.Add(operand);
			PublishNewValue(result.NewValue);
			return result;
		}
		public (int PreviousValue, int NewValue) Subtract(int operand) {
			var result = _value.Subtract(operand);
			PublishNewValue(result.NewValue);
			return result;
		}
		public (int PreviousValue, int NewValue) MultiplyBy(int operand) {
			var result = _value.MultiplyBy(operand);
			PublishNewValue(result.NewValue);
			return result;
		}
		public (int PreviousValue, int NewValue) DivideBy(int operand) {
			var result = _value.DivideBy(operand);
			PublishNewValue(result.NewValue);
			return result;
		}


		// ============================ Waiter API ============================
		public (bool ValueAcquired, int Value) WaitForNext(CancellationToken cancellationToken, TimeSpan maxWaitTime) {
			var queue = GetConsumerQueue();
			var result = queue.Dequeue(cancellationToken, maxWaitTime);
			RelinquishConsumerQueue(queue);
			return result;
		}

		public (bool ValueAcquired, int Value) WaitForValue(int targetValue, CancellationToken cancellationToken, TimeSpan maxWaitTime) {
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

		public (bool ValueAcquired, int Value) WaitForValue(Func<int, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime) {
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

		public async Task<(bool ValueAcquired, int Value)> WaitForNextAsync(CancellationToken cancellationToken, TimeSpan maxWaitTime) {
			var queue = GetConsumerQueue();
			var result = await queue.DequeueAsync(cancellationToken, maxWaitTime);
			RelinquishConsumerQueue(queue);
			return result;
		}

		public async Task<(bool ValueAcquired, int Value)> WaitForValueAsync(int targetValue, CancellationToken cancellationToken, TimeSpan maxWaitTime) {
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

		public async Task<(bool ValueAcquired, int Value)> WaitForValueAsync(Func<int, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime) {
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

		public ValueTask<int> WaitForExpectedValueAsync(int targetValue) {
			var queue = GetConsumerQueue();
			var quickReadResult = queue.AttemptQuickRead();
			RelinquishConsumerQueue(queue);

			if (quickReadResult.ValueAcquired && ValueMatchesTarget(quickReadResult.Value, targetValue)) {
				return new ValueTask<int>(quickReadResult.Value);
			}
			else return new ValueTask<int>(this.WaitForValueAsync(targetValue));
		}

		public ValueTask<int> WaitForExpectedValueAsync(Func<int, bool> predicate) {
			var queue = GetConsumerQueue();
			var quickReadResult = queue.AttemptQuickRead();
			RelinquishConsumerQueue(queue);

			if (quickReadResult.ValueAcquired && ValueSatisfiesPredicate(quickReadResult.Value, predicate)) {
				return new ValueTask<int>(quickReadResult.Value);
			}
			else return new ValueTask<int>(this.WaitForValueAsync(predicate));
		}

		public ValueTask<(bool ValueAcquired, int Value)> WaitForExpectedValueAsync(int targetValue, CancellationToken cancellationToken, TimeSpan maxWaitTime) {
			var queue = GetConsumerQueue();
			var quickReadResult = queue.AttemptQuickRead();
			RelinquishConsumerQueue(queue);

			if (quickReadResult.ValueAcquired && ValueMatchesTarget(quickReadResult.Value, targetValue)) {
				return new ValueTask<(bool ValueAcquired, int Value)>((true, quickReadResult.Value));
			}
			else return new ValueTask<(bool ValueAcquired, int Value)>(WaitForValueAsync(targetValue, cancellationToken, maxWaitTime));
		}

		public ValueTask<(bool ValueAcquired, int Value)> WaitForExpectedValueAsync(Func<int, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime) {
			var queue = GetConsumerQueue();
			var quickReadResult = queue.AttemptQuickRead();
			RelinquishConsumerQueue(queue);

			if (quickReadResult.ValueAcquired && ValueSatisfiesPredicate(quickReadResult.Value, predicate)) {
				return new ValueTask<(bool ValueAcquired, int Value)>((true, quickReadResult.Value));
			}
			else return new ValueTask<(bool ValueAcquired, int Value)>(WaitForValueAsync(predicate, cancellationToken, maxWaitTime));
		}

		void PublishNewValue(int newValue) {
			foreach (var queue in _activeConsumerQueues) queue.Enqueue(newValue);
		}

		AwaitableConsumerValueQueue<int> GetConsumerQueue() {
			var result = _consumerQueuePool.BorrowOne();
			result.Enqueue(_value);
			_activeConsumerQueues.Add(result);
			return result;
		}

		void RelinquishConsumerQueue(AwaitableConsumerValueQueue<int> queueToRelinquish) {
			_activeConsumerQueues.Remove(queueToRelinquish);
			_consumerQueuePool.ReturnOne(queueToRelinquish);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool ValueMatchesTarget(int value, int target) => value == target;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool ValueSatisfiesPredicate(int value, Func<int, bool> predicate) => predicate(value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static long GetTimestamp() => Stopwatch.GetTimestamp();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static TimeSpan GetTimeSinceTimestamp(long timestamp) => TimeSpan.FromSeconds((GetTimestamp() - timestamp) / NumStopwatchTicksPerSec);
	}
}