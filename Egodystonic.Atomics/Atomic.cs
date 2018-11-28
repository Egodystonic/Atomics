using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Egodystonic.Atomics {
	public static class Atomic { // TODO update this to latest IAtomic<T> API
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Get<T>(ref T @ref) where T : class => Volatile.Read(ref @ref); // fence is useless on its own but will synchronize with other operations

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetUnsafe<T>(ref T @ref) where T : class => @ref;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Set<T>(ref T @ref, T newValue) where T : class => Volatile.Write(ref @ref, newValue); // fence is useless on its own but will synchronize with other operations

		// ReSharper disable once RedundantAssignment It doesn't realise it's a ref field
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetUnsafe<T>(ref T @ref, T newValue) where T : class => @ref = newValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Exchange<T>(ref T @ref, T newValue) where T : class => Interlocked.Exchange(ref @ref, newValue);

		public static (bool ValueWasSet, T PreviousValue) TryExchange<T>(ref T @ref, T newValue, T comparand) where T : class {
			if (!TargetTypeIsEquatable<T>()) {
				var oldValue = Interlocked.CompareExchange(ref @ref, newValue, comparand);
				return (oldValue == comparand, oldValue);
			}

			var comparandAsIEquatable = (IEquatable<T>)comparand;
			bool trySetValue;
			T curValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get(ref @ref);
				trySetValue = comparandAsIEquatable.Equals(curValue);

				if (!trySetValue || Interlocked.CompareExchange(ref @ref, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue);
		}

		public static (bool ValueWasSet, T PreviousValue) TryExchange<T>(ref T @ref, T newValue, Func<T, bool> predicate) where T : class {
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

		public static (bool ValueWasSet, T PreviousValue) TryExchange<T>(ref T @ref, T newValue, Func<T, T, bool> predicate) where T : class {
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

		public static (T PreviousValue, T NewValue) Exchange<T>(ref T @ref, Func<T, T> mapFunc) where T : class {
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

		public static (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<T>(ref T @ref, Func<T, T> mapFunc, T comparand) where T : class {
			bool trySetValue;
			T curValue;
			T newValue = default;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get(ref @ref);
				trySetValue = TargetTypeIsEquatable<T>() ? ((IEquatable<T>) comparand).Equals(curValue) : comparand == curValue;

				if (!trySetValue) break;

				newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref @ref, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue, newValue);
		}

		public static (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<T>(ref T @ref, Func<T, T> mapFunc, Func<T, bool> predicate) where T : class {
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

		public static (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<T>(ref T @ref, Func<T, T> mapFunc, Func<T, T, bool> predicate) where T : class {
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool TargetTypeIsEquatable<T>() where T : class => typeof(IEquatable<T>).IsAssignableFrom(typeof(T));
	}
}
