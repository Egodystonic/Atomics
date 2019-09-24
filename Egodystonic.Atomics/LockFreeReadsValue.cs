// (c) Egodystonic Studios 2018
// Author: Ben Bowen
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Egodystonic.Atomics {
	public sealed class LockFreeReadsValue<T> : INonLockingAtomic<T> where T : struct, IEquatable<T> {
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

		public LockFreeReadsValue() : this(default) { }
		public LockFreeReadsValue(T initialValue) => Set(initialValue);

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

		public ref T GetUnsafeRef() => ref _slots[_lastWriteID & SlotMask].Value;

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
		public void SetUnsafe(T newValue) => _slots[_lastWriteID & SlotMask].Value = newValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Swap(T newValue) {
			lock (_writeLock) {
				var oldValue = GetUnsafe();
				Set(newValue);
				return oldValue;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T TrySwap(T newValue, T comparand) {
			lock (_writeLock) {
				var oldValue = GetUnsafe();
				if (oldValue.Equals(comparand)) Set(newValue);
				return oldValue;
			}
		}

		public override string ToString() => Get().ToString();

		#region Equality
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(T other) => Get().Equals(other);

		public override bool Equals(object obj) {
			if (obj is T value) return Equals(value);
			return ReferenceEquals(this, obj);
		}

		// ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode Base GetHashCode() is appropriate here.
		public override int GetHashCode() => base.GetHashCode();

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(LockFreeReadsValue<T> left, T right) => left?.Equals(right) ?? false;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(LockFreeReadsValue<T> left, T right) => !(left == right);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(T left, LockFreeReadsValue<T> right) => right?.Equals(left) ?? false;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(T left, LockFreeReadsValue<T> right) => !(right == left);
		#endregion
	}
}
