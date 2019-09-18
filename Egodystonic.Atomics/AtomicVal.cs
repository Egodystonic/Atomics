// (c) Egodystonic Studios 2018
// Author: Ben Bowen
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Egodystonic.Atomics {
	// TODO rename as OptimisticRWStruct<> or something
	public sealed class AtomicVal<T> : IAtomic<T> where T : struct, IEquatable<T> {
		struct BufferSlot<TSlot> where TSlot : struct {
			public long WriteCount;
			public TSlot Value;
		}

		const int NumSlots = 32;
		const int SlotMask = NumSlots - 1;
		readonly object _writeLock = new object();
		readonly BufferSlot<T>[] _slots = new BufferSlot<T>[NumSlots];
		long _lastWriteID;

		public T Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => Set(value);
		}

		public AtomicVal() : this(default) { }
		public AtomicVal(T initialValue) => Set(initialValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get() {
			if (IntPtr.Size != sizeof(long)) return Get32(); // All hail the branch predictor

			var spinner = new SpinWait();

			while (true) {
				var lastWriteID = Volatile.Read(ref _lastWriteID);
				var index = lastWriteID & SlotMask;

				var expectedWriteCount = Volatile.Read(ref _slots[index].WriteCount);
				var result = _slots[index].Value;
				Thread.MemoryBarrier();
				var actualWriteCount = _slots[index].WriteCount;

				if (expectedWriteCount == actualWriteCount) return result;

				spinner.SpinOnce();
			}
		}

		T Get32() {
			var spinner = new SpinWait();

			while (true) {
				var lastWriteID = Interlocked.Read(ref _lastWriteID);
				var index = lastWriteID & SlotMask;

				var expectedWriteCount = Interlocked.Read(ref _slots[index].WriteCount);
				var result = _slots[index].Value;
				var actualWriteCount = Interlocked.Read(ref _slots[index].WriteCount);

				if (expectedWriteCount == actualWriteCount) return result;

				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		// ReSharper disable once InconsistentlySynchronizedField Unsafe method deliberately foregoes synchronization
		public T GetUnsafe() => _slots[_lastWriteID & SlotMask].Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)] // TODO experiment with inlining off
		public void Set(T newValue) {
			lock (_writeLock) {
				var nextWriteID = _lastWriteID + 1;
				var index = nextWriteID & SlotMask;
				_slots[index].WriteCount++;
				Thread.MemoryBarrier(); // Ensure that we increment the WriteCount BEFORE we start copying over the new value. This lets readers detect changes
				_slots[index].Value = newValue;
				Thread.MemoryBarrier(); // Ensure the copy of the new value is completed before we propagate the new write ID
				_lastWriteID = nextWriteID;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		// ReSharper disable once InconsistentlySynchronizedField Unsafe method deliberately foregoes synchronization
		public void SetUnsafe(T newValue) => _slots[++_lastWriteID & SlotMask].Value = newValue;

		public T SpinWaitForValue(T targetValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue.Equals(targetValue)) return curValue;
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T FastExchange(T newValue) {
			lock (_writeLock) {
				var oldValue = GetUnsafe();
				Set(newValue);
				return oldValue;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (T PreviousValue, T CurrentValue) Exchange(T newValue) {
			lock (_writeLock) {
				var oldValue = GetUnsafe();
				Set(newValue);
				return (oldValue, newValue);
			}
		}

		public (T PreviousValue, T CurrentValue) Exchange<TContext>(Func<T, TContext, T> mapFunc, TContext context) {
			lock (_writeLock) {
				var oldValue = GetUnsafe();
				var newValue = mapFunc(oldValue, context);
				Set(newValue);
				return (oldValue, newValue);
			}
		}

		public (T PreviousValue, T CurrentValue) SpinWaitForExchange(T newValue, T comparand) {
			var spinner = new SpinWait();

			while (true) {
				var tryExchRes = TryExchange(newValue, comparand);
				if (tryExchRes.ValueWasSet) return (tryExchRes.PreviousValue, tryExchRes.CurrentValue);

				spinner.SpinOnce();
			}
		}

		public (T PreviousValue, T CurrentValue) SpinWaitForExchange<TContext>(Func<T, TContext, T> mapFunc, TContext context, T comparand) {
			return SpinWaitForExchange(mapFunc, context, (curVal, _, ctx) => curVal.Equals(ctx), comparand);
		}

		public (T PreviousValue, T CurrentValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<T, TMapContext, T> mapFunc, TMapContext mapContext, Func<T, T, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
			var spinner = new SpinWait();

			while (true) {
				var tryExchRes = TryExchange(mapFunc, mapContext, predicate, predicateContext);
				if (tryExchRes.ValueWasSet) return (tryExchRes.PreviousValue, tryExchRes.CurrentValue);

				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T FastTryExchange(T newValue, T comparand) {
			lock (_writeLock) {
				var oldValue = GetUnsafe();
				if (oldValue.Equals(comparand)) Set(newValue);
				return oldValue;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange(T newValue, T comparand) {
			lock (_writeLock) {
				var oldValue = GetUnsafe();
				if (!oldValue.Equals(comparand)) return (false, oldValue, oldValue);
				Set(newValue);
				return (true, oldValue, newValue);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange<TContext>(Func<T, TContext, T> mapFunc, TContext context, T comparand) {
			return TryExchange(mapFunc, context, (curVal, _, ctx) => curVal.Equals(ctx), comparand);
		}

		public (bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange<TMapContext, TPredicateContext>(Func<T, TMapContext, T> mapFunc, TMapContext mapContext, Func<T, T, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
			lock (_writeLock) {
				var oldValue = GetUnsafe();
				var newValue = mapFunc(oldValue, mapContext);
				if (!predicate(oldValue, newValue, predicateContext)) return (false, oldValue, oldValue);
				Set(newValue);
				return (true, oldValue, newValue);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(AtomicVal<T> operand) => operand.Get();

		public override string ToString() => Get().ToString();
	}
}
