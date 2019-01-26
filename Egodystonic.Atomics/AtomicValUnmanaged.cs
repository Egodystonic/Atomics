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
			return ReadFromLong(&valueCopy);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		long GetLong() => Interlocked.Read(ref _valueAsLong);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetUnsafe() {
			var valueCopy = _valueAsLong;
			return ReadFromLong(&valueCopy);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(T CurrentValue) {
			long CurrentValueAsLong;
			WriteToLong(&CurrentValueAsLong, CurrentValue);
			SetLong(CurrentValueAsLong);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void SetLong(long CurrentValueAsLong) => Interlocked.Exchange(ref _valueAsLong, CurrentValueAsLong);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(T CurrentValue) {
			long CurrentValueAsLong;
			WriteToLong(&CurrentValueAsLong, CurrentValue);
			_valueAsLong = CurrentValueAsLong;
		}

		public T SpinWaitForValue(T targetValue) {
			var spinner = new SpinWait();
			long targetValueAsLong;
			WriteToLong(&targetValueAsLong, targetValue);

			while (true) {
				var curValueAsLong = GetLong();
				if (curValueAsLong == targetValueAsLong) return ReadFromLong(&curValueAsLong);
				spinner.SpinOnce();
			}
		}

		public (T PreviousValue, T CurrentValue) Exchange(T CurrentValue) {
			long CurrentValueAsLong;
			WriteToLong(&CurrentValueAsLong, CurrentValue);
			var previousValueAsLong = Interlocked.Exchange(ref _valueAsLong, CurrentValueAsLong);
			return (ReadFromLong(&previousValueAsLong), CurrentValue);
		}

		public (T PreviousValue, T CurrentValue) Exchange<TContext>(Func<T, TContext, T> mapFunc, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValueAsLong = GetLong();
				var CurrentValue = mapFunc(ReadFromLong(&curValueAsLong), context);
				long CurrentValueAsLong;
				WriteToLong(&CurrentValueAsLong, CurrentValue);

				if (Interlocked.CompareExchange(ref _valueAsLong, CurrentValueAsLong, curValueAsLong) == curValueAsLong) return (ReadFromLong(&curValueAsLong), CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange(T CurrentValue, T comparand) {
			long CurrentValueAsLong, comparandAsLong;
			WriteToLong(&CurrentValueAsLong, CurrentValue);
			WriteToLong(&comparandAsLong, comparand);
			var previousValueAsLong = Interlocked.CompareExchange(ref _valueAsLong, CurrentValueAsLong, comparandAsLong);
			var previousValue = ReadFromLong(&previousValueAsLong);

			var wasSet = previousValueAsLong == comparandAsLong;
			return (wasSet, previousValue, wasSet ? CurrentValue : previousValue);
		}

		public (T PreviousValue, T CurrentValue) SpinWaitForExchange(T CurrentValue, T comparand) {
			var spinner = new SpinWait();
			long CurrentValueAsLong, comparandAsLong;
			WriteToLong(&CurrentValueAsLong, CurrentValue);
			WriteToLong(&comparandAsLong, comparand);

			while (true) {
				if (Interlocked.CompareExchange(ref _valueAsLong, CurrentValueAsLong, comparandAsLong) == comparandAsLong) return (comparand, CurrentValue);
				spinner.SpinOnce();
			}
		}
		public (T PreviousValue, T CurrentValue) SpinWaitForExchange<TContext>(Func<T, TContext, T> mapFunc, TContext context, T comparand) {
			var spinner = new SpinWait();
			var CurrentValue = mapFunc(comparand, context); // curValue will always be comparand when this method returns
			long CurrentValueAsLong, comparandAsLong;
			WriteToLong(&CurrentValueAsLong, CurrentValue);
			WriteToLong(&comparandAsLong, comparand);

			while (true) {
				if (Interlocked.CompareExchange(ref _valueAsLong, CurrentValueAsLong, comparandAsLong) == comparandAsLong) return (comparand, CurrentValue);
				spinner.SpinOnce();
			}
		}
		public (T PreviousValue, T CurrentValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<T, TMapContext, T> mapFunc, TMapContext mapContext, Func<T, T, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
			var spinner = new SpinWait();
			
			while (true) {
				var curValue = Get();
				var CurrentValue = mapFunc(curValue, mapContext);
				if (!predicate(curValue, CurrentValue, predicateContext)) {
					spinner.SpinOnce();
					continue;
				}

				long curValueAsLong, CurrentValueAsLong;
				WriteToLong(&curValueAsLong, curValue);
				WriteToLong(&CurrentValueAsLong, CurrentValue);

				if (Interlocked.CompareExchange(ref _valueAsLong, CurrentValueAsLong, curValueAsLong) == curValueAsLong) return (curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange<TContext>(Func<T, TContext, T> mapFunc, TContext context, T comparand) {
			long comparandAsLong, CurrentValueAsLong;
			WriteToLong(&comparandAsLong, comparand);
			var CurrentValue = mapFunc(comparand, context); // Comparand will always be curValue if the interlocked call passes
			WriteToLong(&CurrentValueAsLong, CurrentValue);

			var prevValueAsLong = Interlocked.CompareExchange(ref _valueAsLong, CurrentValueAsLong, comparandAsLong);
			var prevValue = ReadFromLong(&prevValueAsLong);
			if (prevValueAsLong == comparandAsLong) return (true, prevValue, CurrentValue);
			else return (false, prevValue, prevValue);
		}

		public (bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchange<TMapContext, TPredicateContext>(Func<T, TMapContext, T> mapFunc, TMapContext mapContext, Func<T, T, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				long curValueAsLong;
				WriteToLong(&curValueAsLong, curValue);
				var CurrentValue = mapFunc(curValue, mapContext);
				if (!predicate(curValue, CurrentValue, predicateContext)) return (false, curValue, curValue);

				long CurrentValueAsLong;
				WriteToLong(&CurrentValueAsLong, CurrentValue);

				if (Interlocked.CompareExchange(ref _valueAsLong, CurrentValueAsLong, curValueAsLong) == curValueAsLong) return (true, curValue, CurrentValue);
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void WriteToLong(long* target, T val) => Buffer.MemoryCopy(&val, target, sizeof(long), sizeof(T)); // TODO replace with faster solution

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static T ReadFromLong(long* src) { // TODO replace with faster solution
			T result;
			Buffer.MemoryCopy(src, &result, sizeof(T), sizeof(T));
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(AtomicValUnmanaged<T> operand) => operand.Get();
	}
}
