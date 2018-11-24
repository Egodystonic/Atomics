using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Egodystonic.Atomics {
	public sealed class CopyOnReadRef<T> : IAtomic<T> where T : class {
		static readonly bool TargetTypeIsEquatable = typeof(IEquatable<T>).IsAssignableFrom(typeof(T));
		readonly Func<T, T> _copyFunc;
		T _value;

		public T Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => Set(value);
		}

		public CopyOnReadRef(Func<T, T> copyFunc) : this(copyFunc, default) { }
		public CopyOnReadRef(Func<T, T> copyFunc, T initialValue) {
			_copyFunc = copyFunc;
			Set(initialValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get() => _copyFunc(Volatile.Read(ref _value)); // fence is useless on its own but will synchronize with other operations

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetWithoutCopy() => Volatile.Read(ref _value); // fence is useless on its own but will synchronize with other operations

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetUnsafe() => _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(T newValue) => Volatile.Write(ref _value, newValue); // fence is useless on its own but will synchronize with other operations

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(T newValue) => _value = newValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Exchange(T newValue) => _copyFunc(Interlocked.Exchange(ref _value, newValue));

		public (bool ValueWasSet, T PreviousValue) TryExchange(T newValue, T comparand) {
			if (!TargetTypeIsEquatable) {
				var oldValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
				return (oldValue == comparand, _copyFunc(oldValue));
			}

			var comparandAsIEquatable = (IEquatable<T>)comparand;
			bool trySetValue;
			T curValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				trySetValue = comparandAsIEquatable.Equals(curValue);

				if (!trySetValue || Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, _copyFunc(curValue));
		}

		public (bool ValueWasSet, T PreviousValue) TryExchange(T newValue, Func<T, T, bool> predicate) {
			bool trySetValue;
			T curValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = GetWithoutCopy();
				trySetValue = predicate(curValue, newValue);

				if (!trySetValue || Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, _copyFunc(curValue));
		}

		public (T PreviousValue, T NewValue) Exchange(Func<T, T> mapFunc) {
			T curValue;
			T newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = GetWithoutCopy();
				newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (_copyFunc(curValue), _copyFunc(newValue));
		}

		public (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(Func<T, T> mapFunc, T comparand) {
			bool trySetValue;
			T curValue;
			T newValue = default;

			var spinner = new SpinWait();

			while (true) {
				curValue = GetWithoutCopy();
				trySetValue = TargetTypeIsEquatable ? ((IEquatable<T>) comparand).Equals(curValue) : comparand == curValue;

				if (!trySetValue) break;

				newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, _copyFunc(curValue), _copyFunc(newValue));
		}

		public (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(Func<T, T> mapFunc, Func<T, T, bool> predicate) {
			bool trySetValue;
			T curValue;
			T newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = GetWithoutCopy();
				newValue = mapFunc(curValue);
				trySetValue = predicate(curValue, newValue);

				if (!trySetValue) break;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (trySetValue, _copyFunc(curValue), _copyFunc(newValue));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(CopyOnReadRef<T> operand) => operand.Get();
	}
}
