using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Egodystonic.Atomics {
	public sealed class NonTupledAtomicVal<T> where T : struct, IEquatable<T> {
		// Use this strategy rather than a RWLS because:
		// > RWLS implements IDisposable() and I didn't want to have to make this class also implement IDisposable()
		// > RWLS has some extra stuff we don't want, such as upgrading from a spinwait to a proper lock when enough time has passed, timeout tracking, optional re-entrancy support, etc.
		int _readerCount = 0; // 0 = no ongoing access, positive = N readers, -1 = 1 writer
		T _value;

		public NonTupledAtomicVal(T initialValue) => Set(initialValue);

		public T Get() { // TODO inline?
			EnterLockAsReader();
			var result = _value;
			ExitLockAsReader();
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetUnsafe() => _value;

		public void Set(T newValue) {
			EnterLockAsWriter();
			_value = newValue;
			ExitLockAsWriter();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(T newValue) => _value = newValue;

		public T Exchange(T newValue) {
			EnterLockAsWriter();
			var oldValue = _value;
			_value = newValue;
			ExitLockAsWriter();
			return oldValue;
		}

		public bool TryExchange(T newValue, T comparand) {
			EnterLockAsWriter();
			var oldValue = _value;
			if (oldValue.Equals(comparand)) {
				_value = newValue;
				ExitLockAsWriter();
				return true;
			}
			else {
				ExitLockAsWriter();
				return false;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(NonTupledAtomicVal<T> operand) => operand.Get();

		// Enter/Exit Lock functions:
		// > All functions emit memfences via Interlocked calls. If Interlocked calls are ever removed, explicit memfences must be added.
		// > This is because the memfences are required for these lock functions to provide correct ordering expectations to their users
		//		(for example, it is expected that any read/write after a call to EnterLockAsReader() can not be less recent than that call;
		//		and similar for the other Enter/Exit funcs)

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void EnterLockAsReader() => EnterLockAsReader(new SpinWait());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void EnterLockAsReader(SpinWait spinner) {
			while (true) {
				var curReaderCount = Volatile.Read(ref _readerCount); // TODO... Is this fence really necessary? The CMPXCHG below will likely emit a full fence... Benchmark the difference and see if it's even worth taking out though

				if (curReaderCount >= 0 && Interlocked.CompareExchange(ref _readerCount, curReaderCount + 1, curReaderCount) == curReaderCount) return;

				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void ExitLockAsReader() {
			Interlocked.Decrement(ref _readerCount);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void EnterLockAsWriter() => EnterLockAsWriter(new SpinWait());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void EnterLockAsWriter(SpinWait spinner) {
			while (true) {
				if (Interlocked.CompareExchange(ref _readerCount, -1, 0) == 0) return;
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void ExitLockAsWriter() {
			Interlocked.Increment(ref _readerCount);
		}
	}
}
