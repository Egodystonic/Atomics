using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Egodystonic.Atomics.Awaitables {
	class AwaitableConsumerValueQueue<T> {
		readonly ManualResetEventSlim _writeNotifyEvent = new ManualResetEventSlim();
		readonly T[] _buffer;
		readonly int _bufferSize;
		readonly int _slotMask;
		TaskCompletionSource<T> _currentAsyncReadCompletionSource;
		int _writeReserveHead;
		int _writeHead;
		int _readHead;

		public AwaitableConsumerValueQueue(int bufferSize) {
			_bufferSize = bufferSize;
			_buffer = new T[bufferSize];
			_slotMask = bufferSize - 1;
			Clear();
		}

		public void Enqueue(T newVal) {
			unchecked {
				var spinner = new SpinWait();
				var reservedSlot = Interlocked.Increment(ref _writeReserveHead);
				var prevReservedSlot = reservedSlot - 1;
				var readHeadLocal = _readHead; // No memfence here because Interlocked.Increment implies one

				// Check whether the difference between the next write and the current read head is >= the buffer size. If yes,
				// we have to block here because one of the readers is being too slow. In future we might want to replace this with some kind
				// of synchronization mechanism rather than a busy-wait but in theory this should not happen anyway:
				// This being hit indicates either that the user needs to specify a larger buffer size to 'ride out' larger-scale fluctuations
				// in throughput; or that the writers are continuously pushing values faster than the readers can handle them,
				// which probably indicates either that a phenomenal amount of writes are occurring or that one or more readers' predicate funcs
				// are too slow. In which case we have no choice but to slow down the writer. If this really is the case the user probably ought
				// to look in to using something more specialized anyway (like the LMAX disruptor).
				while (reservedSlot - readHeadLocal >= _bufferSize) {
					spinner.SpinOnce();
					readHeadLocal = Volatile.Read(ref _readHead);
				}

				_buffer[reservedSlot & _slotMask] = newVal;

				while (Interlocked.CompareExchange(ref _writeHead, reservedSlot, prevReservedSlot) != prevReservedSlot) {
					spinner.SpinOnce();
				}

				_writeNotifyEvent.Set(); /* Important: By the time this is set, the new write head must have been written. 
										  * This is because wait handles emit membars: https://docs.microsoft.com/en-gb/windows/desktop/Sync/synchronization-and-multiprocessor-issues.
				                          *	It also helps make sure the following read of the TaskCompletionSource<> is fresher than the write. */

				_currentAsyncReadCompletionSource?.TrySetResult(newVal); // C# compiler rewrites this to take a local variable first; so it's safe
			}
		}

		public (bool ValueAcquired, T Value) Dequeue(CancellationToken cancellationToken, TimeSpan maxWaitTime) {
			while (true) {
				_writeNotifyEvent.Reset();

				var quickReadResult = AttemptQuickRead();
				if (quickReadResult.ValueAcquired) return quickReadResult;

				try {
					var writerWake = _writeNotifyEvent.Wait(maxWaitTime, cancellationToken);
					if (!writerWake) return (false, default);
				}
				catch (OperationCanceledException) {
					return (false, default);
				}
			}
		}

		public async Task<(bool ValueAcquired, T Value)> DequeueAsync(CancellationToken cancellationToken, TimeSpan maxWaitTime) {
			while (true) {
				var newCompletionSource = new TaskCompletionSource<T>();
				Volatile.Write(ref _currentAsyncReadCompletionSource, newCompletionSource); // This data structure is single-reader; so there's no issue with just replacing the old one

				var quickReadResult = AttemptQuickRead();
				if (quickReadResult.ValueAcquired) return quickReadResult;

				var waitOrCancelTask = Task.Delay(maxWaitTime, cancellationToken);
				var writerWake = await Task.WhenAny(newCompletionSource.Task, waitOrCancelTask) == newCompletionSource.Task;
				if (!writerWake) return (false, default);
			}			
		}

		public (bool ValueAcquired, T Value) AttemptQuickRead() {
			unchecked {
				readStart:
				var newReadHead = Interlocked.Increment(ref _readHead);
				var writeHeadLocal = Volatile.Read(ref _writeHead);
				if (writeHeadLocal - newReadHead >= 0) return (true, _buffer[newReadHead & _slotMask]);

				// No new value. Rewind the read head and spinwait for the write head to advance, or eventually fall back to the event if it doesn't
				Interlocked.Decrement(ref _readHead);
				var spinner = new SpinWait();
				while (!spinner.NextSpinWillYield) {
					spinner.SpinOnce();
					writeHeadLocal = Volatile.Read(ref _writeHead);
					if (writeHeadLocal - newReadHead >= 0) goto readStart;
				}

				return (false, default);
			}
		}

		public void Clear() {
			_readHead = 0;
			_writeReserveHead = 0;
			_writeNotifyEvent.Set();
			Volatile.Write(ref _writeHead, 0);
		}
	}
}
