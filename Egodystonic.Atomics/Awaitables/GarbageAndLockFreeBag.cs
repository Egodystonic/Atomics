using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Egodystonic.Atomics.Awaitables {
	// TODO experiment with chunking to improve cache locality: Maybe a flag in each chunk that locks it for a single writer; other writers skip past, readers skip past also but attempt to return later
	// TODO It's essentially a striped lock linked list of arrays I guess
	class GarbageAndLockFreeBag<T> : IEnumerable<T> {
		class Node {
			public NodeTail Tail;
			public T Value;
		}

		class NodeTail {
			public Node Next;
			public bool DeletionFlag;
		}

		public struct Enumerator : IEnumerator<T> {
			readonly GarbageAndLockFreeBag<T> _owner;
			Node _curParent;
			Node _curNode;

			public Enumerator(GarbageAndLockFreeBag<T> owner) : this() { _owner = owner; }

			public bool MoveNext() {
				_curParent = _curNode;
				Thread.MemoryBarrier(); // Just to ensure a relative amount of 'freshness'

				getParentTail:
				_curNode = _curParent == null ? _owner._head : _curParent.Tail.Next;

				if (_curNode == null) return false;
				if (!_curNode.Tail.DeletionFlag) return true;

				_owner.PhysicalDeleteNodes(_curParent);
				goto getParentTail;
			}

			public void Reset() {
				_curParent = null;
				_curNode = null;
			}

			object IEnumerator.Current => Current;

			public void Dispose() { }

			public T Current => _curNode.Value;
		}

		[ThreadStatic]
		static Queue<Node> _threadLocalUnusedNodeQueue;
		[ThreadStatic]
		static Queue<NodeTail> _threadLocalUnusedNodeTailQueue;
		readonly Node _deletionMarkerNode = new Node();
		Node _head;

		public void Add(T item) {
			if (_threadLocalUnusedNodeQueue == null) _threadLocalUnusedNodeQueue = new Queue<Node>();
			if (_threadLocalUnusedNodeTailQueue == null) _threadLocalUnusedNodeTailQueue = new Queue<NodeTail>();

			var node = _threadLocalUnusedNodeQueue.Count > 0 ? _threadLocalUnusedNodeQueue.Dequeue() : new Node();
			var tail = _threadLocalUnusedNodeTailQueue.Count > 0 ? _threadLocalUnusedNodeTailQueue.Dequeue() : new NodeTail();
			node.Value = item;
			node.Tail = tail;
			Volatile.Write(ref tail.DeletionFlag, false);

			var spinner = new SpinWait();
			while (true) {
				var curReaderListNode = _head;
				tail.Next = curReaderListNode;
				if (Interlocked.CompareExchange(ref _head, node, curReaderListNode) == curReaderListNode) return;
				spinner.SpinOnce();
			}
		}

		public void Remove(T item) {
			if (_threadLocalUnusedNodeTailQueue == null) _threadLocalUnusedNodeTailQueue = new Queue<NodeTail>();

			var spinner = new SpinWait();

			// Step 1: Find node to relinquish in linked list
			findTargetNodeInLinkedList:
			Node parentNode = null;
			Node currentNode = Volatile.Read(ref _head);
			while (currentNode == null || !currentNode.Value.Equals(item)) {
				if (currentNode == null) {
					spinner.SpinOnce();
					goto findTargetNodeInLinkedList; // Start again from the head. Previous volatile read of head will force next go around to be fresher
				}

				var curNodeTail = currentNode.Tail;
				if (curNodeTail.DeletionFlag) {
					PhysicalDeleteNodes(parentNode);
					spinner.SpinOnce();
					goto findTargetNodeInLinkedList; // Start again from the head. Previous volatile read of head will force next go around to be fresher
				}

				parentNode = currentNode;
				currentNode = currentNode.Tail.Next;
			}

			// Step 2: Logically delete this node then attempt to physically delete
			var curTail = currentNode.Tail;
			var newTail = _threadLocalUnusedNodeTailQueue.Count > 0 ? _threadLocalUnusedNodeTailQueue.Dequeue() : new NodeTail();
			newTail.Next = curTail.Next;
			newTail.DeletionFlag = true;
			if (Interlocked.CompareExchange(ref currentNode.Tail, newTail, curTail) == curTail) PhysicalDeleteNodes(parentNode);
		}

		void PhysicalDeleteNodes(Node parentStart) {
			if (_threadLocalUnusedNodeQueue == null) _threadLocalUnusedNodeQueue = new Queue<Node>();
			if (_threadLocalUnusedNodeTailQueue == null) _threadLocalUnusedNodeTailQueue = new Queue<NodeTail>();

			var curParent = parentStart;
			var curNode = curParent == null ? Volatile.Read(ref _head) : curParent.Tail.Next;
			while (curNode != null) {
				if (curNode.Tail.DeletionFlag) {
					var deletionSuccess = Interlocked.CompareExchange(ref (curParent == null ? ref _head : ref curParent.Tail.Next), curNode.Tail.Next, curNode) == curNode;
					if (deletionSuccess) {
						_threadLocalUnusedNodeQueue.Enqueue(curNode);
						_threadLocalUnusedNodeTailQueue.Enqueue(curNode.Tail);
					}
				}

				curParent = curNode;
				curNode = curNode.Tail.Next;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
		Enumerator GetEnumerator() => new Enumerator(this);
	}
}
