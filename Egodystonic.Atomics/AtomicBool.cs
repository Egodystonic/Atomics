using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Egodystonic.Atomics.Numerics;

namespace Egodystonic.Atomics {
	public sealed class AtomicBool : IAtomic<bool> {
		const int True = unchecked((int) 0xFFFFFFFFU);
		const int False = 0x0;
		int _value = False;

		public bool Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Set(value);
		}

		public AtomicBool() : this(default) { }
		public AtomicBool(bool initialValue) => Set(initialValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Get() => Convert(GetValueAsInt());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		int GetValueAsInt() => Volatile.Read(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool GetUnsafe() => Convert(_value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(bool newValue) => SetValueAsInt(Convert(newValue));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void SetValueAsInt(int newValue) => Volatile.Write(ref _value, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(bool newValue) => _value = Convert(newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Exchange(bool newValue) => Convert(Interlocked.Exchange(ref _value, Convert(newValue)));

		public (bool ValueWasSet, bool PreviousValue) Exchange(bool newValue, bool comparand) {
			var newValueAsInt = Convert(newValue);
			var comparandAsInt = Convert(comparand);
			var oldIntValue = Interlocked.CompareExchange(ref _value, newValueAsInt, comparandAsInt);
			return (oldIntValue == comparandAsInt, Convert(oldIntValue));
		}

		public (bool ValueWasSet, bool PreviousValue) Exchange(bool newValue, Func<bool, bool> predicate) {
			bool trySetValue;
			bool curValue;
			var newValueAsInt = Convert(newValue);

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				var curValueAsInt = Convert(curValue);
				trySetValue = predicate(curValue);

				if (!trySetValue || Interlocked.CompareExchange(ref _value, newValueAsInt, curValueAsInt) == curValueAsInt) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue);
		}

		public (bool ValueWasSet, bool PreviousValue) Exchange(bool newValue, Func<bool, bool, bool> predicate) {
			bool trySetValue;
			bool curValue;
			var newValueAsInt = Convert(newValue);

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				var curValueAsInt = Convert(curValue);
				trySetValue = predicate(curValue, newValue);

				if (!trySetValue || Interlocked.CompareExchange(ref _value, newValueAsInt, curValueAsInt) == curValueAsInt) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue);
		}

		public (bool PreviousValue, bool NewValue) Exchange(Func<bool, bool> mapFunc) {
			bool curValue;
			bool newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				var curValueAsInt = Convert(curValue);
				newValue = mapFunc(curValue);
				var newValueAsInt = Convert(newValue);

				if (Interlocked.CompareExchange(ref _value, newValueAsInt, curValueAsInt) == curValueAsInt) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		public (bool ValueWasSet, bool PreviousValue, bool NewValue) Exchange(Func<bool, bool> mapFunc, bool comparand) {
			bool trySetValue;
			bool curValue;
			bool newValue = default;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				var curValueAsInt = Convert(curValue);
				trySetValue = comparand == curValue;

				if (!trySetValue) break;

				newValue = mapFunc(curValue);
				var newValueAsInt = Convert(newValue);

				if (Interlocked.CompareExchange(ref _value, newValueAsInt, curValueAsInt) == curValueAsInt) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue, newValue);
		}

		public (bool ValueWasSet, bool PreviousValue, bool NewValue) Exchange(Func<bool, bool> mapFunc, Func<bool, bool> predicate) {
			bool trySetValue;
			bool curValue;
			bool newValue = default;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				var curValueAsInt = Convert(curValue);
				trySetValue = predicate(curValue);

				if (!trySetValue) break;

				newValue = mapFunc(curValue);
				var newValueAsInt = Convert(newValue);

				if (Interlocked.CompareExchange(ref _value, newValueAsInt, curValueAsInt) == curValueAsInt) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue, newValue);
		}

		public (bool ValueWasSet, bool PreviousValue, bool NewValue) Exchange(Func<bool, bool> mapFunc, Func<bool, bool, bool> predicate) {
			bool trySetValue;
			bool curValue;
			bool newValue;

			var spinner = new SpinWait();

			while (true) {
				curValue = Get();
				var curValueAsInt = Convert(curValue);
				newValue = mapFunc(curValue);
				var newValueAsInt = Convert(newValue);
				trySetValue = predicate(curValue, newValue);

				if (!trySetValue) break;

				if (Interlocked.CompareExchange(ref _value, newValueAsInt, curValueAsInt) == curValueAsInt) break;
				spinner.SpinOnce();
			}

			return (trySetValue, curValue, newValue);
		}

		public (bool PreviousValue, bool NewValue) Negate() {
			var spinner = new SpinWait();
			bool curValue;
			bool newValue;

			while (true) {
				curValue = Get();
				var curValueAsInt = Convert(curValue);
				newValue = !curValue;
				var newValueAsInt = Convert(newValue);

				if (Interlocked.CompareExchange(ref _value, newValueAsInt, curValueAsInt) == curValueAsInt) break;
				spinner.SpinOnce();
			}

			return (curValue, newValue);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool Convert(int value) => value == True;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int Convert(bool value) => value ? True : False;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator bool(AtomicBool operand) => operand.Get();
	}
}