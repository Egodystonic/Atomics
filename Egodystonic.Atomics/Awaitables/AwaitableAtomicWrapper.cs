using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Egodystonic.Atomics.Awaitables {
	class AwaitableAtomicWrapper<T> : IAwaitableAtomic<T> {
		static readonly bool TargetTypeIsEquatable = typeof(IEquatable<T>).IsAssignableFrom(typeof(T));
		readonly AutoResetEvent _valuePublishEvent = new AutoResetEvent(false);
		AwaitableValueNode<T> _publishedValueLinkedListHead;

		public T Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => Set(value);
		}

		public AtomicValueBackstop CurrentValueBackstop {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _value.Get().Backstop;
		}

		public AwaitableAtomicWrapper() : this(default) { }
		public AwaitableAtomicWrapper(T initialValue) {
			_value = new AtomicVal<SequencedValue<T>>(SequencedValue<T>.Initial(initialValue));
			var initialLinkedListHead = _nodePool.Borrow();
			initialLinkedListHead.Set(_value);
			Interlocked.Exchange(ref _publishedValueLinkedListHead, initialLinkedListHead);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get() => _value.Get().Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetUnsafe() => _value.GetUnsafe().Value;

		public void Set(T newValue) { // TODO essentially we should make a linked-list. Only difficulty is gonna be: How do we release borrowed nodes?
			SequencedValue<T> newSeqVal;

			var spinner = new SpinWait();
			while (true) {
				var curSeqVal = _value.GetUnsafe();
				newSeqVal = curSeqVal.Increment(newValue);

				if (_value.TryExchange(newSeqVal, curSeqVal).ValueWasSet) break;
				spinner.SpinOnce();
			}

			PublishNewValue(newSeqVal);
		}

		public void SetUnsafe(T newValue) {
			var newSeqVal = _value.GetUnsafe().Increment(newValue);
			_value.SetUnsafe(newSeqVal);
			PublishNewValue(newSeqVal);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SpinWaitForValue(T targetValue) {
			var spinner = new SpinWait();
			while (true) {
				if (_value.Get().Value.Equals(targetValue)) return;
				spinner.SpinOnce();
			}
		}

		// TODO these funcs; don't forget to call PublishNewValue
		public T Exchange(T newValue) { return default; }
		public (bool ValueWasSet, T PreviousValue) TryExchange(T newValue, T comparand) { return (false, default); }
		public (bool ValueWasSet, T PreviousValue) TryExchange(T newValue, Func<T, bool> predicate) { return (false, default); }
		public (bool ValueWasSet, T PreviousValue) TryExchange(T newValue, Func<T, T, bool> predicate) { return (false, default); }
		public (T PreviousValue, T NewValue) Exchange(Func<T, T> mapFunc) { return (default, default); }
		public (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(Func<T, T> mapFunc, T comparand) { return (false, default, default); }
		public (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(Func<T, T> mapFunc, Func<T, bool> predicate) { return (false, default, default); }
		public (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(Func<T, T> mapFunc, Func<T, T, bool> predicate) { return (false, default, default); }

		// ============================ Synchronous API ============================
		public (bool ValueAcquired, T Value) WaitForNext(CancellationToken cancellationToken, TimeSpan maxWaitTime) {
			// TODO just enregister for updates and get next
		}

		public bool WaitForValue(T targetValue, CancellationToken cancellationToken, TimeSpan maxWaitTime, AtomicValueBackstop backstop) {
			// TODO for this and all waitforvalues, we need to check all values from current and onwards. Be careful not to check current and then miss any updates between then and enregistering for updates
		}

		public (bool ValueAcquired, T Value) WaitForValue(Func<T, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime, AtomicValueBackstop backstop) {
			
		}


		// ============================ Asynchronous API ============================
		public Task<(bool ValueAcquired, T Value)> WaitForNextAsync(CancellationToken cancellationToken, TimeSpan maxWaitTime) {
			
		}

		public async Task<bool> WaitForValueAsync(T targetValue, CancellationToken cancellationToken, TimeSpan maxWaitTime, AtomicValueBackstop backstop) {

		}

		public async Task<(bool ValueAcquired, T Value)> WaitForValueAsync(Func<T, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime, AtomicValueBackstop backstop) {
			
		}


		// ============================ Low-Garbage Asynchronous API ============================
		public ValueTask WaitForExpectedValueAsync(T targetValue, AtomicValueBackstop backstop) {

		}

		public ValueTask<T> WaitForExpectedValueAsync(Func<T, bool> targetValue, AtomicValueBackstop backstop) {

		}

		public ValueTask<bool> WaitForExpectedValueAsync(T targetValue, CancellationToken cancellationToken, TimeSpan maxWaitTime, AtomicValueBackstop backstop) {

		}

		public ValueTask<(bool ValueAcquired, T Value)> WaitForExpectedValueAsync(Func<T, bool> predicate, CancellationToken cancellationToken, TimeSpan maxWaitTime, AtomicValueBackstop backstop) {

		}

		void PublishNewValue(SequencedValue<T> newSeqVal) {
			// TODO... We need to make sure values are published in-order somehow... 2 PublishNewValue()s could be called OoO WRT each other; but WaitForNext() will expect ordering (as will other things)
			// TODO ordering IS required because otherwise the backstops will leak the misordering... Could get a backstop for B that is 'later' than A, but B would be published first
		}

		void RegisterAsListener() {
			Interlocked.Increment(ref _listenerCount);
		}

		void UnregisterAsListener() {
			Interlocked.Decrement(ref _listenerCount);
		}

		bool SearchForValue(T target) {
			Thread.MemoryBarrier();
			var curNode = _publishedValueLinkedListHead;
			Thread.MemoryBarrier();

			while (curNode != null) {
				var nextNode = curNode.Next;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool ValueMatchesTarget(SequencedValue<T> seqVal, AtomicValueBackstop backstop, T target) {
			if (seqVal.Backstop <= backstop) return false;

			return TargetTypeIsEquatable ? ((IEquatable<T>) seqVal.Value).Equals(target) : seqVal.Equals(target);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool ValueSatisfiesPredicate(SequencedValue<T> seqVal, AtomicValueBackstop backstop, Func<T, bool> predicate) => seqVal.Backstop > backstop && predicate(seqVal.Value);
	}
}