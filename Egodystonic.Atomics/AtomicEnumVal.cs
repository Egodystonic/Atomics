using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Egodystonic.Atomics {
	public sealed unsafe class AtomicEnumVal<T> : IAtomic<T> where T : unmanaged, Enum {
		long _valueAsLong;

		public T Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Set(value);
		}

		public AtomicEnumVal() : this(default) { }
		public AtomicEnumVal(T initialValue) {
			Set(initialValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get() {
			var valueCopy = GetLong();
			return *(T*)&valueCopy;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		long GetLong() => Interlocked.Read(ref _valueAsLong);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetUnsafe() {
			var valueCopy = _valueAsLong;
			return *(T*)&valueCopy;
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
			return *(T*)&previousValueAsLong;
		}

		public (bool ValueWasSet, T PreviousValue) TryExchange(T newValue, T comparand) {
			var newValueAsLong = *(long*)&newValue;
			var comparandAsLong = *(long*)&comparand;
			var previousValueAsLong = Interlocked.CompareExchange(ref _valueAsLong, newValueAsLong, comparandAsLong);

			return (previousValueAsLong == comparandAsLong, *(T*)&previousValueAsLong);
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
			var comparandAsLong = *(long*)&comparand; 

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

		public (T PreviousValue, T NewValue) AddFlag(T operand) {
			T curValue;
			T newValue;
			var operandAsLong = *(long*)&operand;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				var curValueAsLong = *(long*)&curValue;
				var newValueAsLong = curValueAsLong | operandAsLong;
				newValue = *(T*)newValueAsLong;

				if (Interlocked.CompareExchange(ref _valueAsLong, curValueAsLong, newValueAsLong) == curValueAsLong) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		public (bool FlagWasAdded, T PreviousValue, T NewValue) AddFlag(T operand, T requisiteFlags) {
			bool trySetValue;
			T curValue;
			T newValue = default;
			var operandAsLong = *(long*)&operand;
			var requisiteFlagsAsLong = *(long*)&requisiteFlags;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				var curValueAsLong = *(long*)&curValue;
				trySetValue = (curValueAsLong & requisiteFlagsAsLong) == requisiteFlagsAsLong;
				if (!trySetValue) break;
				var newValueAsLong = curValueAsLong | operandAsLong;
				newValue = *(T*)newValueAsLong;

				if (Interlocked.CompareExchange(ref _valueAsLong, curValueAsLong, newValueAsLong) == curValueAsLong) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue, newValue);
		}

		public (bool FlagWasAdded, T PreviousValue, T NewValue) AddFlag(T operand, Func<T, bool> predicate) {
			bool trySetValue;
			T curValue;
			T newValue = default;
			var operandAsLong = *(long*)&operand;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				var curValueAsLong = *(long*)&curValue;
				trySetValue = predicate(curValue);
				if (!trySetValue) break;
				var newValueAsLong = curValueAsLong | operandAsLong;
				newValue = *(T*)newValueAsLong;

				if (Interlocked.CompareExchange(ref _valueAsLong, curValueAsLong, newValueAsLong) == curValueAsLong) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue, newValue);
		}

		public (bool FlagWasAdded, T PreviousValue, T NewValue) AddFlag(T operand, Func<T, T, bool> predicate) {
			bool trySetValue;
			T curValue;
			T newValue;
			var operandAsLong = *(long*)&operand;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				var curValueAsLong = *(long*)&curValue;
				var newValueAsLong = curValueAsLong | operandAsLong;
				newValue = *(T*)newValueAsLong;
				trySetValue = predicate(curValue, newValue);
				if (!trySetValue) break;

				if (Interlocked.CompareExchange(ref _valueAsLong, curValueAsLong, newValueAsLong) == curValueAsLong) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue, newValue);
		}

		public (T PreviousValue, T NewValue) RemoveFlag(T operand) {
			T curValue;
			T newValue;
			var operandAsLong = *(long*)&operand;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				var curValueAsLong = *(long*)&curValue;
				var newValueAsLong = curValueAsLong & ~operandAsLong;
				newValue = *(T*)newValueAsLong;

				if (Interlocked.CompareExchange(ref _valueAsLong, curValueAsLong, newValueAsLong) == curValueAsLong) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		public (bool FlagWasAdded, T PreviousValue, T NewValue) RemoveFlag(T operand, T requisiteFlags) {
			bool trySetValue;
			T curValue;
			T newValue = default;
			var operandAsLong = *(long*)&operand;
			var requisiteFlagsAsLong = *(long*)&requisiteFlags;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				var curValueAsLong = *(long*)&curValue;
				trySetValue = (curValueAsLong & requisiteFlagsAsLong) == requisiteFlagsAsLong;
				if (!trySetValue) break;
				var newValueAsLong = curValueAsLong & ~operandAsLong;
				newValue = *(T*)newValueAsLong;

				if (Interlocked.CompareExchange(ref _valueAsLong, curValueAsLong, newValueAsLong) == curValueAsLong) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue, newValue);
		}

		public (bool FlagWasAdded, T PreviousValue, T NewValue) RemoveFlag(T operand, Func<T, bool> predicate) {
			bool trySetValue;
			T curValue;
			T newValue = default;
			var operandAsLong = *(long*)&operand;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				var curValueAsLong = *(long*)&curValue;
				trySetValue = predicate(curValue);
				if (!trySetValue) break;
				var newValueAsLong = curValueAsLong & ~operandAsLong;
				newValue = *(T*)newValueAsLong;

				if (Interlocked.CompareExchange(ref _valueAsLong, curValueAsLong, newValueAsLong) == curValueAsLong) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue, newValue);
		}

		public (bool FlagWasAdded, T PreviousValue, T NewValue) RemoveFlag(T operand, Func<T, T, bool> predicate) {
			bool trySetValue;
			T curValue;
			T newValue;
			var operandAsLong = *(long*)&operand;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				var curValueAsLong = *(long*)&curValue;
				var newValueAsLong = curValueAsLong & ~operandAsLong;
				newValue = *(T*)newValueAsLong;
				trySetValue = predicate(curValue, newValue);
				if (!trySetValue) break;

				if (Interlocked.CompareExchange(ref _valueAsLong, curValueAsLong, newValueAsLong) == curValueAsLong) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue, newValue);
		}

		public bool HasFlag(T operand) {
			var operandAsLong = *(long*)&operand;
			return (GetLong() & operandAsLong) == operandAsLong;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(AtomicEnumVal<T> operand) => operand.Get();
	}
}
