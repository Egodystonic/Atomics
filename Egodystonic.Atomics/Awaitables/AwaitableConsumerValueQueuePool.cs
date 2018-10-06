using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Egodystonic.Atomics.Awaitables {
	class AwaitableConsumerValueQueuePool<T> {
		const int MinMinBufferSize = 4;
		const int MaxPowerOfTwo = 29;

		[ThreadStatic]
		static Queue<AwaitableConsumerValueQueue<T>> _threadLocalUnusedObjectQueue;

		readonly int _bufferSize;

		public AwaitableConsumerValueQueuePool(int minBufferSize) {
			if (minBufferSize < MinMinBufferSize) minBufferSize = MinMinBufferSize;

			var minBufferSizeMSB = 0;
			while (minBufferSize > 0) {
				minBufferSize >>= 1;
				++minBufferSizeMSB;
				if (minBufferSizeMSB == MaxPowerOfTwo) break;
			}

			_bufferSize = 1 << minBufferSizeMSB;
		}

		public AwaitableConsumerValueQueue<T> BorrowOne() {
			if (_threadLocalUnusedObjectQueue == null) _threadLocalUnusedObjectQueue = new Queue<AwaitableConsumerValueQueue<T>>();

			return _threadLocalUnusedObjectQueue.Count > 0 ? _threadLocalUnusedObjectQueue.Dequeue() : new AwaitableConsumerValueQueue<T>(_bufferSize);
		}

		public void ReturnOne(AwaitableConsumerValueQueue<T> buffer) {
			if (_threadLocalUnusedObjectQueue == null) _threadLocalUnusedObjectQueue = new Queue<AwaitableConsumerValueQueue<T>>();

			buffer.Clear();
			_threadLocalUnusedObjectQueue.Enqueue(buffer);
		}
	}
}
