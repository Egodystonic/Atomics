using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Egodystonic.Atomics {
	public sealed class AtomicDelegate<T> : IAtomic<T> where T : Delegate {
		T _value;

		public T Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => Set(value);
		}

		public AtomicDelegate() : this(default) { }
		public AtomicDelegate(T initialValue) => Set(initialValue);

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

		public (bool ValueWasSet, T PreviousValue) Exchange(T newValue, T comparand) {
			var oldValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			return (oldValue == comparand, oldValue);
		}

		public (bool ValueWasSet, T PreviousValue) Exchange(T newValue, Func<T, bool> predicate) {
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

		public (bool ValueWasSet, T PreviousValue) Exchange(T newValue, Func<T, T, bool> predicate) {
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

		public (bool ValueWasSet, T PreviousValue, T NewValue) Exchange(Func<T, T> mapFunc, T comparand) {
			bool trySetValue;
			T curValue;
			T newValue = default;

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

		public (bool ValueWasSet, T PreviousValue, T NewValue) Exchange(Func<T, T> mapFunc, Func<T, bool> predicate) {
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

		public (bool ValueWasSet, T PreviousValue, T NewValue) Exchange(Func<T, T> mapFunc, Func<T, T, bool> predicate) {
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

		public (T PreviousValue, T NewValue) Combine(T operand) {
			T curValue;
			T newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				newValue = (T)Delegate.Combine(curValue, operand);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		public (T PreviousValue, T NewValue) Remove(T operand) {
			T curValue;
			T newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				newValue = (T)Delegate.Remove(curValue, operand);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		public (T PreviousValue, T NewValue) RemoveAll(T operand) {
			T curValue;
			T newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				newValue = (T)Delegate.RemoveAll(curValue, operand);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		public (bool DelegateWasInvoked, object Result) TryDynamicInvoke(params object[] args) {
			var curValue = Get();
			if (curValue is null) return (false, default);
			else return (true, curValue.DynamicInvoke(args));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(AtomicDelegate<T> operand) => operand.Get();
	}

	public static class AtomicDelegateExtensions {
		public static bool TryInvoke(this AtomicDelegate<Action> @this) {
			var curValue = @this.Value;
			if (curValue is null) return false;
			curValue();
			return true;
		}

		public static bool TryInvoke<T1>(this AtomicDelegate<Action<T1>> @this, T1 arg1) {
			var curValue = @this.Value;
			if (curValue is null) return false;
			curValue(arg1);
			return true;
		}

		public static bool TryInvoke<T1, T2>(this AtomicDelegate<Action<T1, T2>> @this, T1 arg1, T2 arg2) {
			var curValue = @this.Value;
			if (curValue is null) return false;
			curValue(arg1, arg2);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3>(this AtomicDelegate<Action<T1, T2, T3>> @this, T1 arg1, T2 arg2, T3 arg3) {
			var curValue = @this.Value;
			if (curValue is null) return false;
			curValue(arg1, arg2, arg3);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4>(this AtomicDelegate<Action<T1, T2, T3, T4>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
			var curValue = @this.Value;
			if (curValue is null) return false;
			curValue(arg1, arg2, arg3, arg4);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5>(this AtomicDelegate<Action<T1, T2, T3, T4, T5>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
			var curValue = @this.Value;
			if (curValue is null) return false;
			curValue(arg1, arg2, arg3, arg4, arg5);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6>(this AtomicDelegate<Action<T1, T2, T3, T4, T5, T6>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) {
			var curValue = @this.Value;
			if (curValue is null) return false;
			curValue(arg1, arg2, arg3, arg4, arg5, arg6);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7>(this AtomicDelegate<Action<T1, T2, T3, T4, T5, T6, T7>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) {
			var curValue = @this.Value;
			if (curValue is null) return false;
			curValue(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8>(this AtomicDelegate<Action<T1, T2, T3, T4, T5, T6, T7, T8>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) {
			var curValue = @this.Value;
			if (curValue is null) return false;
			curValue(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this AtomicDelegate<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9) {
			var curValue = @this.Value;
			if (curValue is null) return false;
			curValue(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this AtomicDelegate<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10) {
			var curValue = @this.Value;
			if (curValue is null) return false;
			curValue(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this AtomicDelegate<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11) {
			var curValue = @this.Value;
			if (curValue is null) return false;
			curValue(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this AtomicDelegate<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12) {
			var curValue = @this.Value;
			if (curValue is null) return false;
			curValue(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this AtomicDelegate<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13) {
			var curValue = @this.Value;
			if (curValue is null) return false;
			curValue(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this AtomicDelegate<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14) {
			var curValue = @this.Value;
			if (curValue is null) return false;
			curValue(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this AtomicDelegate<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15) {
			var curValue = @this.Value;
			if (curValue is null) return false;
			curValue(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this AtomicDelegate<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16) {
			var curValue = @this.Value;
			if (curValue is null) return false;
			curValue(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
			return true;
		}

		public static (bool DelegateWasInvoked, TOut Result) TryInvoke<TOut>(this AtomicDelegate<Func<TOut>> @this) {
			var curValue = @this.Value;
			if (curValue is null) return (false, default);
			else return (true, curValue());
		}

		public static (bool DelegateWasInvoked, TOut Result) TryInvoke<T1, TOut>(this AtomicDelegate<Func<T1, TOut>> @this, T1 arg1) {
			var curValue = @this.Value;
			if (curValue is null) return (false, default);
			else return (true, curValue(arg1));
		}

		public static (bool DelegateWasInvoked, TOut Result) TryInvoke<T1, T2, TOut>(this AtomicDelegate<Func<T1, T2, TOut>> @this, T1 arg1, T2 arg2) {
			var curValue = @this.Value;
			if (curValue is null) return (false, default);
			else return (true, curValue(arg1, arg2));
		}

		public static (bool DelegateWasInvoked, TOut Result) TryInvoke<T1, T2, T3, TOut>(this AtomicDelegate<Func<T1, T2, T3, TOut>> @this, T1 arg1, T2 arg2, T3 arg3) {
			var curValue = @this.Value;
			if (curValue is null) return (false, default);
			else return (true, curValue(arg1, arg2, arg3));
		}

		public static (bool DelegateWasInvoked, TOut Result) TryInvoke<T1, T2, T3, T4, TOut>(this AtomicDelegate<Func<T1, T2, T3, T4, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
			var curValue = @this.Value;
			if (curValue is null) return (false, default);
			else return (true, curValue(arg1, arg2, arg3, arg4));
		}

		public static (bool DelegateWasInvoked, TOut Result) TryInvoke<T1, T2, T3, T4, T5, TOut>(this AtomicDelegate<Func<T1, T2, T3, T4, T5, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
			var curValue = @this.Value;
			if (curValue is null) return (false, default);
			else return (true, curValue(arg1, arg2, arg3, arg4, arg5));
		}

		public static (bool DelegateWasInvoked, TOut Result) TryInvoke<T1, T2, T3, T4, T5, T6, TOut>(this AtomicDelegate<Func<T1, T2, T3, T4, T5, T6, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) {
			var curValue = @this.Value;
			if (curValue is null) return (false, default);
			else return (true, curValue(arg1, arg2, arg3, arg4, arg5, arg6));
		}

		public static (bool DelegateWasInvoked, TOut Result) TryInvoke<T1, T2, T3, T4, T5, T6, T7, TOut>(this AtomicDelegate<Func<T1, T2, T3, T4, T5, T6, T7, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) {
			var curValue = @this.Value;
			if (curValue is null) return (false, default);
			else return (true, curValue(arg1, arg2, arg3, arg4, arg5, arg6, arg7));
		}

		public static (bool DelegateWasInvoked, TOut Result) TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, TOut>(this AtomicDelegate<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) {
			var curValue = @this.Value;
			if (curValue is null) return (false, default);
			else return (true, curValue(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8));
		}

		public static (bool DelegateWasInvoked, TOut Result) TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOut>(this AtomicDelegate<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9) {
			var curValue = @this.Value;
			if (curValue is null) return (false, default);
			else return (true, curValue(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9));
		}

		public static (bool DelegateWasInvoked, TOut Result) TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOut>(this AtomicDelegate<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10) {
			var curValue = @this.Value;
			if (curValue is null) return (false, default);
			else return (true, curValue(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10));
		}

		public static (bool DelegateWasInvoked, TOut Result) TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOut>(this AtomicDelegate<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11) {
			var curValue = @this.Value;
			if (curValue is null) return (false, default);
			else return (true, curValue(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11));
		}

		public static (bool DelegateWasInvoked, TOut Result) TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOut>(this AtomicDelegate<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12) {
			var curValue = @this.Value;
			if (curValue is null) return (false, default);
			else return (true, curValue(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12));
		}

		public static (bool DelegateWasInvoked, TOut Result) TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOut>(this AtomicDelegate<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13) {
			var curValue = @this.Value;
			if (curValue is null) return (false, default);
			else return (true, curValue(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13));
		}

		public static (bool DelegateWasInvoked, TOut Result) TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOut>(this AtomicDelegate<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14) {
			var curValue = @this.Value;
			if (curValue is null) return (false, default);
			else return (true, curValue(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14));
		}

		public static (bool DelegateWasInvoked, TOut Result) TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOut>(this AtomicDelegate<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15) {
			var curValue = @this.Value;
			if (curValue is null) return (false, default);
			else return (true, curValue(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15));
		}

		public static (bool DelegateWasInvoked, TOut Result) TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TOut>(this AtomicDelegate<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16) {
			var curValue = @this.Value;
			if (curValue is null) return (false, default);
			else return (true, curValue(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16));
		}
	}
}
