using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Egodystonic.Atomics {
	/// <summary>
	/// TODO make note that all comparisons are reference comparison; and that if actual equality overrides are required AtomicObj should be used.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class AtomicRef<T> : IAtomic<T> where T : class {
		static readonly bool TargetTypeIsEquatable = typeof(IEquatable<T>).IsAssignableFrom(typeof(T));
		T _value;

		public T Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => Set(value);
		}

		public AtomicRef() : this(default) { }
		public AtomicRef(T initialValue) => Set(initialValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get() => Volatile.Read(ref _value); // fence is useless on its own but will synchronize with other operations

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetUnsafe() => _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(T newValue) => Volatile.Write(ref _value, newValue); // fence is useless on its own but will synchronize with other operations

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(T newValue) => _value = newValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Exchange(T newValue) => Interlocked.Exchange(ref _value, newValue);

		public (bool ValueWasSet, T PreviousValue) TryExchange(T newValue, T comparand) {
			if (TargetTypeIsEquatable) return TryExchange(newValue, (cur, @new) => ((IEquatable<T>)cur).Equals(@new));

			var oldValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			return (oldValue == comparand, oldValue);
		}

		public (bool ValueWasSet, T PreviousValue) TryExchange(T newValue, Func<T, bool> predicate) {
			bool trySetValue;
			T curValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				trySetValue = predicate(curValue);

				if (!trySetValue || Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue);
		}

		public (bool ValueWasSet, T PreviousValue) TryExchange(T newValue, Func<T, T, bool> predicate) {
			bool trySetValue;
			T curValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				trySetValue = predicate(curValue, newValue);

				if (!trySetValue || Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue);
		}

		public (T PreviousValue, T NewValue) Exchange(Func<T, T> mapFunc) {
			T curValue;
			T newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		public (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(Func<T, T> mapFunc, T comparand) {
			bool trySetValue;
			T curValue;
			T newValue = default;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				trySetValue = TargetTypeIsEquatable ? ((IEquatable<T>) comparand).Equals(curValue) : comparand == curValue;

				if (!trySetValue) break;

				newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue, newValue);
		}

		public (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(Func<T, T> mapFunc, Func<T, bool> predicate) {
			bool trySetValue;
			T curValue;
			T newValue = default;

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

		public (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(Func<T, T> mapFunc, Func<T, T, bool> predicate) {
			bool trySetValue;
			T curValue;
			T newValue;

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
		public static implicit operator T(AtomicRef<T> operand) => operand.Get();
	}
}
