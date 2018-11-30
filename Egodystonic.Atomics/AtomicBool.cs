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
		public (bool PreviousValue, bool NewValue) Exchange(bool newValue) => (Convert(Interlocked.Exchange(ref _value, Convert(newValue))), newValue);

		public (bool PreviousValue, bool NewValue) Exchange<TContext>(Func<bool, TContext, bool> mapFunc, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var curValueAsInt = Convert(curValue);
				var newValue = mapFunc(curValue, context);
				var newValueAsInt = Convert(newValue);

				if (Interlocked.CompareExchange(ref _value, newValueAsInt, curValueAsInt) == curValueAsInt) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public bool SpinWaitForValue(bool targetValue) {
			var spinner = new SpinWait();
			var targetAsInt = Convert(targetValue);

			while (GetValueAsInt() != targetAsInt) spinner.SpinOnce();
			return targetValue;
		}

		public (bool PreviousValue, bool NewValue) SpinWaitForExchange(bool newValue, bool comparand) {
			var spinner = new SpinWait();
			var newValueAsInt = Convert(newValue);
			var comparandAsInt = Convert(comparand);

			while (true) {
				if (Interlocked.CompareExchange(ref _value, newValueAsInt, comparandAsInt) == comparandAsInt) return (comparand, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool PreviousValue, bool NewValue) SpinWaitForExchange<TContext>(Func<bool, TContext, bool> mapFunc, bool comparand, TContext context) {
			var spinner = new SpinWait();
			var newValue = mapFunc(comparand, context); // curValue will always be comparand when this method returns
			var newValueAsInt = Convert(newValue);
			var comparandAsInt = Convert(comparand);

			while (true) {
				if (Interlocked.CompareExchange(ref _value, newValueAsInt, comparandAsInt) == comparandAsInt) return (comparand, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool PreviousValue, bool NewValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<bool, TMapContext, bool> mapFunc, Func<bool, bool, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
			var spinner = new SpinWait();
			
			while (true) {
				var curValue = Get();
				var newValue = mapFunc(curValue, mapContext);
				if (!predicate(curValue, newValue, predicateContext)) {
					spinner.SpinOnce();
					continue;
				}
				var curValueAsInt = Convert(curValue);
				var newValueAsInt = Convert(newValue);
				if (Interlocked.CompareExchange(ref _value, newValueAsInt, curValueAsInt) == curValueAsInt) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, bool PreviousValue, bool NewValue) TryExchange(bool newValue, bool comparand) {
			var newValueAsInt = Convert(newValue);
			var comparandAsInt = Convert(comparand);
			var oldIntValue = Interlocked.CompareExchange(ref _value, newValueAsInt, comparandAsInt);
			var wasSet = oldIntValue == comparandAsInt;
			var oldValue = Convert(oldIntValue);
			return (wasSet, oldValue, wasSet ? newValue : oldValue);
		}

		public (bool ValueWasSet, bool PreviousValue, bool NewValue) TryExchange<TContext>(Func<bool, TContext, bool> mapFunc, bool comparand, TContext context) {
			var newValue = mapFunc(comparand, context); // Can just use the map func here as by the time we return, curValue MUST have been the same as comparand
			var newValueAsInt = Convert(newValue);
			var comparandAsInt = Convert(comparand);

			var prevValueAsInt = Interlocked.CompareExchange(ref _value, newValueAsInt, comparandAsInt);
			var prevValue = Convert(prevValueAsInt);
			if (prevValueAsInt == comparandAsInt) return (true, prevValue, newValue);
			else return (false, prevValue, prevValue);
		}

		public (bool ValueWasSet, bool PreviousValue, bool NewValue) TryExchange<TMapContext, TPredicateContext>(Func<bool, TMapContext, bool> mapFunc, Func<bool, bool, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = mapFunc(curValue, mapContext);
				if (!predicate(curValue, newValue, predicateContext)) return (false, curValue, curValue);

				var curValueAsInt = Convert(curValue);
				var newValueAsInt = Convert(newValue);

				if (Interlocked.CompareExchange(ref _value, newValueAsInt, curValueAsInt) == curValueAsInt) return (true, curValue, newValue);

				spinner.SpinOnce();
			}
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