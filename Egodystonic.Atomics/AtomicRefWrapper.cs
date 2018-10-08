using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Egodystonic.Atomics {
	public sealed class AtomicRefWrapper<T> where T : class {
		static readonly bool TargetTypeIsEquatable = typeof(IEquatable<T>).IsAssignableFrom(typeof(T));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get(ref T @ref) => Volatile.Read(ref @ref); // fence is useless on its own but will synchronize with other operations

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetUnsafe(ref T @ref) => @ref;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(ref T @ref, T newValue) => Volatile.Write(ref @ref, newValue); // fence is useless on its own but will synchronize with other operations

		// ReSharper disable once RedundantAssignment It doesn't realise it's a ref field
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(ref T @ref, T newValue) => @ref = newValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Exchange(ref T @ref, T newValue) => Interlocked.Exchange(ref @ref, newValue);

		public (bool ValueWasSet, T PreviousValue) TryExchange(ref T @ref, T newValue, T comparand) {
			if (TargetTypeIsEquatable) return TryExchange(ref @ref, newValue, (cur, @new) => ((IEquatable<T>)cur).Equals(@new));

			var oldValue = Interlocked.CompareExchange(ref @ref, newValue, comparand);
			return (oldValue == comparand, oldValue);
		}

		public (bool ValueWasSet, T PreviousValue) TryExchange(ref T @ref, T newValue, Func<T, bool> predicate) {
			bool trySetValue;
			T curValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get(ref @ref);
				trySetValue = predicate(curValue);

				if (!trySetValue || Interlocked.CompareExchange(ref @ref, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue);
		}

		public (bool ValueWasSet, T PreviousValue) TryExchange(ref T @ref, T newValue, Func<T, T, bool> predicate) {
			bool trySetValue;
			T curValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get(ref @ref);
				trySetValue = predicate(curValue, newValue);

				if (!trySetValue || Interlocked.CompareExchange(ref @ref, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue);
		}

		public (T PreviousValue, T NewValue) Exchange(ref T @ref, Func<T, T> mapFunc) {
			T curValue;
			T newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get(ref @ref);
				newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref @ref, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		public (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(ref T @ref, Func<T, T> mapFunc, T comparand) {
			bool trySetValue;
			T curValue;
			T newValue = default;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get(ref @ref);
				trySetValue = TargetTypeIsEquatable ? ((IEquatable<T>) comparand).Equals(curValue) : comparand == curValue;

				if (!trySetValue) break;

				newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref @ref, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue, newValue);
		}

		public (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(ref T @ref, Func<T, T> mapFunc, Func<T, bool> predicate) {
			bool trySetValue;
			T curValue;
			T newValue = default;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get(ref @ref);
				trySetValue = predicate(curValue);

				if (!trySetValue) break;

				newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref @ref, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue, newValue);
		}

		public (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(ref T @ref, Func<T, T> mapFunc, Func<T, T, bool> predicate) {
			bool trySetValue;
			T curValue;
			T newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get(ref @ref);
				newValue = mapFunc(curValue);
				trySetValue = predicate(curValue, newValue);

				if (!trySetValue) break;

				if (Interlocked.CompareExchange(ref @ref, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue, newValue);
		}
	}
}
