using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Egodystonic.Atomics {
	/// <summary>
	/// TODO document the max struct size and also the fact that IEquatable overrides are not used here (if they are required, AtomicVal should be used)
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed unsafe class AtomicValUnmanaged<T> : IAtomic<T> where T : unmanaged {
		long _valueAsLong;

		public T Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => Set(value);
		}

		public AtomicValUnmanaged() : this(default) { }
		public AtomicValUnmanaged(T initialValue) {
			if (sizeof(T) > sizeof(long)) {
				throw new ArgumentException($"Generic type parameter in {typeof(AtomicValUnmanaged<>).Name} must not exceed {sizeof(long)} bytes. " +
											$"Given type '{typeof(T)}' has a size of {sizeof(T)} bytes. " +
											$"Use {typeof(AtomicVal<>).Name} instead for large unmanaged types.");
			}
			Set(initialValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get() {
			var valueCopy = GetLong();
			return *(T*) &valueCopy;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		long GetLong() => Interlocked.Read(ref _valueAsLong);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetUnsafe() {
			var valueCopy = _valueAsLong;
			return *(T*) &valueCopy;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(T newValue) {
			SetLong(*(long*)&newValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void SetLong(long newValueAsLong) => Interlocked.Exchange(ref _valueAsLong, newValueAsLong);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(T newValue) {
			_valueAsLong = *(long*)&newValue;
		}

		public T Exchange(T newValue) {
			var newValueAsLong = *(long*)&newValue;
			var previousValueAsLong = Interlocked.Exchange(ref _valueAsLong, newValueAsLong);
			return *(T*) &previousValueAsLong;
		}

		public (bool ValueWasSet, T PreviousValue) TryExchange(T newValue, T comparand) {
			var newValueAsLong = *(long*)&newValue;
			var comparandAsLong = *(long*)&comparand;
			var previousValueAsLong = Interlocked.CompareExchange(ref _valueAsLong, newValueAsLong, comparandAsLong);

			return (previousValueAsLong == comparandAsLong, *(T*) &previousValueAsLong);
		}

		public (bool ValueWasSet, T PreviousValue) TryExchange(T newValue, Func<T, bool> predicate) {
			bool trySetValue;
			T curValue;
			var newValueAsLong = *(long*)&newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				var curValueAsLong = *(long*)&curValue;
				trySetValue = predicate(curValue);

				if (!trySetValue || Interlocked.CompareExchange(ref _valueAsLong, newValueAsLong, curValueAsLong) == curValueAsLong) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue);
		}

		public (bool ValueWasSet, T PreviousValue) TryExchange(T newValue, Func<T, T, bool> predicate) {
			bool trySetValue;
			T curValue;
			var newValueAsLong = *(long*)&newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				var curValueAsLong = *(long*)&curValue;
				trySetValue = predicate(curValue, newValue);

				if (!trySetValue || Interlocked.CompareExchange(ref _valueAsLong, newValueAsLong, curValueAsLong) == curValueAsLong) break;
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
				var curValueAsLong = *(long*)&curValue;
				newValue = mapFunc(curValue);
				var newValueAsLong = *(long*)&newValue;

				if (Interlocked.CompareExchange(ref _valueAsLong, curValueAsLong, newValueAsLong) == curValueAsLong) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		public (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(Func<T, T> mapFunc, T comparand) {
			bool trySetValue;
			T curValue;
			T newValue = default;
			var comparandAsLong = *(long*)&comparand; // Converting the comparand to a long for comparisons rather than comparing as type T. Reasons:
													  //	a) There is no 'where T : IEquatable<T>' constraint because it's impossible to consistently use custom equality
													  //	   throughout this class without just turning it in to a clone of AtomicVal<T>. Therefore randomly using
													  //	   non-binary-equality here would be inconsistent;
													  //	b) Even if we were okay with the inconsistency, because there is no 'where T : IEquatable<T>' a comparison via
													  //	   some form of Equals() would almost certainly require casting our comparands to object; introducing boxing.

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				var curValueAsLong = *(long*)&curValue;
				trySetValue = comparandAsLong == curValueAsLong;

				if (!trySetValue) break;

				newValue = mapFunc(curValue);
				var newValueAsLong = *(long*)&newValue;

				if (Interlocked.CompareExchange(ref _valueAsLong, newValueAsLong, curValueAsLong) == curValueAsLong) break;
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
				var curValueAsLong = *(long*)&curValue;
				trySetValue = predicate(curValue);

				if (!trySetValue) break;

				newValue = mapFunc(curValue);
				var newValueAsLong = *(long*)&newValue;

				if (Interlocked.CompareExchange(ref _valueAsLong, newValueAsLong, curValueAsLong) == curValueAsLong) break;
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
				var curValueAsLong = *(long*)&curValue;
				newValue = mapFunc(curValue);
				var newValueAsLong = *(long*)&newValue;
				trySetValue = predicate(curValue, newValue);

				if (!trySetValue) break;

				if (Interlocked.CompareExchange(ref _valueAsLong, newValueAsLong, curValueAsLong) == curValueAsLong) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue, newValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(AtomicValUnmanaged<T> operand) => operand.Get();
	}
}
