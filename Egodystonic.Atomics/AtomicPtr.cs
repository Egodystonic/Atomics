using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Egodystonic.Atomics {
	public sealed class AtomicPtr : IAtomic<IntPtr> {
		IntPtr _value;

		public IntPtr Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => Set(value);
		}

		public AtomicPtr() : this(default) { }
		public AtomicPtr(IntPtr initialValue) => Set(initialValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IntPtr Get() => Volatile.Read(ref _value); // fence is useless on its own but will synchronize with other operations

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IntPtr GetUnsafe() => _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(IntPtr newValue) => Volatile.Write(ref _value, newValue); // fence is useless on its own but will synchronize with other operations

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(IntPtr newValue) => _value = newValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IntPtr Exchange(IntPtr newValue) => Interlocked.Exchange(ref _value, newValue);

		public (bool ValueWasSet, IntPtr PreviousValue) TryExchange(IntPtr newValue, IntPtr comparand) {
			var oldValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			return (oldValue == comparand, oldValue);
		}

		public (bool ValueWasSet, IntPtr PreviousValue) TryExchange(IntPtr newValue, Func<IntPtr, bool> predicate) {
			bool trySetValue;
			IntPtr curValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				trySetValue = predicate(curValue);

				if (!trySetValue || Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue);
		}

		public (bool ValueWasSet, IntPtr PreviousValue) TryExchange(IntPtr newValue, Func<IntPtr, IntPtr, bool> predicate) {
			bool trySetValue;
			IntPtr curValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				trySetValue = predicate(curValue, newValue);

				if (!trySetValue || Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue);
		}

		public (IntPtr PreviousValue, IntPtr NewValue) Exchange(Func<IntPtr, IntPtr> mapFunc) {
			IntPtr curValue;
			IntPtr newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		public (bool ValueWasSet, IntPtr PreviousValue, IntPtr NewValue) TryExchange(Func<IntPtr, IntPtr> mapFunc, IntPtr comparand) {
			bool trySetValue;
			IntPtr curValue;
			IntPtr newValue = default;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				trySetValue = comparand == curValue;

				if (!trySetValue) break;

				newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue, newValue);
		}

		public (bool ValueWasSet, IntPtr PreviousValue, IntPtr NewValue) TryExchange(Func<IntPtr, IntPtr> mapFunc, Func<IntPtr, bool> predicate) {
			bool trySetValue;
			IntPtr curValue;
			IntPtr newValue = default;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				trySetValue = predicate(curValue);

				if (!trySetValue) break;

				newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue, newValue);
		}

		public (bool ValueWasSet, IntPtr PreviousValue, IntPtr NewValue) TryExchange(Func<IntPtr, IntPtr> mapFunc, Func<IntPtr, IntPtr, bool> predicate) {
			bool trySetValue;
			IntPtr curValue;
			IntPtr newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				newValue = mapFunc(curValue);
				trySetValue = predicate(curValue, newValue);

				if (!trySetValue) break;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue, newValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator IntPtr(AtomicPtr operand) => operand.Get();
	}
}
