using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Egodystonic.Atomics.Numerics;

namespace Egodystonic.Atomics {
	public sealed unsafe class AtomicPtr<T> : IAtomic<IntPtr> where T : unmanaged {
		public struct TypedPtrTryExchangeRes : IEquatable<TypedPtrTryExchangeRes> {
			public readonly bool ValueWasSet;
			public readonly T* PreviousValue;
			public readonly T* NewValue;

			public (bool ValueWasSet, IntPtr PreviousValue, IntPtr NewValue) AsUntyped => (ValueWasSet, (IntPtr) PreviousValue, (IntPtr) NewValue);

			public TypedPtrTryExchangeRes(bool valueWasSet, T* previousValue, T* newValue) {
				ValueWasSet = valueWasSet;
				PreviousValue = previousValue;
				NewValue = newValue;
			}

			public void Deconstruct(out bool valueWasSet, out T* previousValue, out T* newValue) {
				valueWasSet = ValueWasSet;
				previousValue = PreviousValue;
				newValue = NewValue;
			}

			public bool Equals(TypedPtrTryExchangeRes other) {
				return ValueWasSet == other.ValueWasSet && PreviousValue == other.PreviousValue && NewValue == other.NewValue;
			}

			public override bool Equals(object obj) {
				if (ReferenceEquals(null, obj)) return false;
				return obj is TypedPtrTryExchangeRes other && Equals(other);
			}

			public override int GetHashCode() {
				unchecked {
					var hashCode = ValueWasSet.GetHashCode();
					hashCode = (hashCode * 397) ^ unchecked((int) (long) PreviousValue);
					hashCode = (hashCode * 397) ^ unchecked((int) (long) NewValue);
					return hashCode;
				}
			}

			public static bool operator ==(TypedPtrTryExchangeRes left, TypedPtrTryExchangeRes right) { return left.Equals(right); }
			public static bool operator !=(TypedPtrTryExchangeRes left, TypedPtrTryExchangeRes right) { return !left.Equals(right); }
		}

		public struct TypedPtrExchangeRes : IEquatable<TypedPtrExchangeRes> {
			public readonly T* PreviousValue;
			public readonly T* NewValue;

			public (IntPtr PreviousValue, IntPtr NewValue) AsUntyped => ((IntPtr) PreviousValue, (IntPtr) NewValue);

			public TypedPtrExchangeRes(T* previousValue, T* newValue) {
				PreviousValue = previousValue;
				NewValue = newValue;
			}

			public bool Equals(TypedPtrExchangeRes other) {
				return PreviousValue == other.PreviousValue && NewValue == other.NewValue;
			}

			public override bool Equals(object obj) {
				if (ReferenceEquals(null, obj)) return false;
				return obj is TypedPtrExchangeRes other && Equals(other);
			}

			public override int GetHashCode() {
				unchecked {
					return (unchecked((int) (long) PreviousValue) * 397) ^ unchecked((int) (long) NewValue);
				}
			}

			public void Deconstruct(out T* previousValue, out T* newValue) {
				previousValue = PreviousValue;
				newValue = NewValue;
			}

			public static bool operator ==(TypedPtrExchangeRes left, TypedPtrExchangeRes right) { return left.Equals(right); }
			public static bool operator !=(TypedPtrExchangeRes left, TypedPtrExchangeRes right) { return !left.Equals(right); }
		}

		public delegate bool AtomicPtrPredicate(T* curValue, T* newValue);
		public delegate bool AtomicPtrPredicate<in TContext>(T* curValue, T* newValue, TContext context);
		public delegate T* AtomicPtrMap(T* curValue);
		public delegate T* AtomicPtrMap<in TContext>(T* curValue, TContext context);

		IntPtr _value;

		public T* Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => Set(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* Get() => (T*) Volatile.Read(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IntPtr GetAsIntPtr() => Volatile.Read(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* GetUnsafe() => (T*) _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(T* newValue) => Volatile.Write(ref _value, (IntPtr) newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(T* newValue) => _value = (IntPtr) newValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetAsIntPtr(IntPtr newValue) => Volatile.Write(ref _value, newValue);

		public TypedPtrExchangeRes Exchange(T* newValue) => new TypedPtrExchangeRes((T*) Interlocked.Exchange(ref _value, (IntPtr) newValue), newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* SpinWaitForValue(T* targetValue) {
			var spinner = new SpinWait();
			while (Get() != targetValue) spinner.SpinOnce();
			return targetValue;
		}

		public TypedPtrExchangeRes Exchange<TContext>(AtomicPtrMap<TContext> mapFunc, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, (IntPtr) newValue, (IntPtr) curValue) == (IntPtr) curValue) return new TypedPtrExchangeRes(curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public TypedPtrExchangeRes SpinWaitForExchange(T* newValue, T* comparand) {
			var spinner = new SpinWait();

			while (true) {
				if (Interlocked.CompareExchange(ref _value, (IntPtr) newValue, (IntPtr) comparand) == (IntPtr) comparand) return new TypedPtrExchangeRes(comparand, newValue);
				spinner.SpinOnce();
			}
		}

		public TypedPtrExchangeRes SpinWaitForExchange<TContext>(AtomicPtrMap<TContext> mapFunc, T* comparand, TContext context) {
			var spinner = new SpinWait();
			var newValue = mapFunc(comparand, context); // curValue will always be comparand when this method returns

			while (true) {
				if (Interlocked.CompareExchange(ref _value, (IntPtr) newValue, (IntPtr) comparand) == (IntPtr) comparand) return new TypedPtrExchangeRes(comparand, newValue);
				spinner.SpinOnce();
			}
		}

		public TypedPtrExchangeRes SpinWaitForExchange<TMapContext, TPredicateContext>(AtomicPtrMap<TMapContext> mapFunc, AtomicPtrPredicate<TPredicateContext> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = mapFunc(curValue, mapContext);
				if (!predicate(curValue, newValue, predicateContext)) {
					spinner.SpinOnce();
					continue;
				}

				if (Interlocked.CompareExchange(ref _value, (IntPtr) newValue, (IntPtr) curValue) == (IntPtr) curValue) return new TypedPtrExchangeRes(curValue, newValue);
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TypedPtrTryExchangeRes TryExchange(T* newValue, T* comparand) {
			var oldValue = (T*) Interlocked.CompareExchange(ref _value, (IntPtr) newValue, (IntPtr) comparand);
			var wasSet = oldValue == comparand;
			return new TypedPtrTryExchangeRes(wasSet, oldValue, wasSet ? newValue : oldValue);
		}

		public TypedPtrTryExchangeRes TryExchange<TContext>(AtomicPtrMap<TContext> mapFunc, T* comparand, TContext context) {
			var newValue = mapFunc(comparand, context); // Comparand will always be curValue if the interlocked call passes
			var prevValue = (T*) Interlocked.CompareExchange(ref _value, (IntPtr) newValue, (IntPtr) comparand);
			if (prevValue == comparand) return new TypedPtrTryExchangeRes(true, prevValue, newValue);
			else return new TypedPtrTryExchangeRes(false, prevValue, prevValue);
		}

		public TypedPtrTryExchangeRes TryExchange<TMapContext, TPredicateContext>(AtomicPtrMap<TMapContext> mapFunc, AtomicPtrPredicate<TPredicateContext> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = mapFunc(curValue, mapContext);
				if (!predicate(curValue, newValue, predicateContext)) return new TypedPtrTryExchangeRes(false, curValue, curValue);

				if ((T*) Interlocked.CompareExchange(ref _value, (IntPtr) newValue, (IntPtr) curValue) == curValue) return new TypedPtrTryExchangeRes(true, curValue, newValue);

				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TypedPtrExchangeRes Increment() => Add(new IntPtr(1L));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TypedPtrExchangeRes Decrement() => Subtract(new IntPtr(-1L));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TypedPtrExchangeRes Add(int operand) => Add(new IntPtr(operand));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TypedPtrExchangeRes Add(long operand) => Add(new IntPtr(operand));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TypedPtrExchangeRes Subtract(int operand) => Subtract(new IntPtr(operand));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TypedPtrExchangeRes Subtract(long operand) => Subtract(new IntPtr(operand));

		public TypedPtrExchangeRes Add(IntPtr operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = curValue + operand.ToInt64();
				var oldValue = (T*) Interlocked.CompareExchange(ref _value, (IntPtr) newValue, (IntPtr) curValue);
				if (oldValue == curValue) return new TypedPtrExchangeRes(oldValue, newValue);
				spinner.SpinOnce();
			}
		}

		public TypedPtrExchangeRes Subtract(IntPtr operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = curValue - operand.ToInt64();
				var oldValue = (T*) Interlocked.CompareExchange(ref _value, (IntPtr) newValue, (IntPtr) curValue);
				if (oldValue == curValue) return new TypedPtrExchangeRes(oldValue, newValue);
				spinner.SpinOnce();
			}
		}

		// ============================ IAtomic<IntPtr> API ============================

		IntPtr IAtomic<IntPtr>.Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => (IntPtr) Value;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Value = (T*) value;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)] IntPtr IAtomic<IntPtr>.Get() => GetAsIntPtr();
		[MethodImpl(MethodImplOptions.AggressiveInlining)] IntPtr IAtomic<IntPtr>.GetUnsafe() => (IntPtr) GetUnsafe();
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public void Set(IntPtr newValue) => SetAsIntPtr(newValue);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public void SetUnsafe(IntPtr newValue) => SetUnsafe((T*) newValue);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public (IntPtr PreviousValue, IntPtr NewValue) Exchange(IntPtr newValue) => Exchange((T*) newValue).AsUntyped;

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public IntPtr SpinWaitForValue(IntPtr targetValue) => (IntPtr) SpinWaitForValue((T*) targetValue);

		public (IntPtr PreviousValue, IntPtr NewValue) Exchange<TContext>(Func<IntPtr, TContext, IntPtr> mapFunc, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = GetAsIntPtr();
				var newValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public (IntPtr PreviousValue, IntPtr NewValue) SpinWaitForExchange(IntPtr newValue, IntPtr comparand) => SpinWaitForExchange((T*) newValue, (T*) comparand).AsUntyped;

		public (IntPtr PreviousValue, IntPtr NewValue) SpinWaitForExchange<TContext>(Func<IntPtr, TContext, IntPtr> mapFunc, IntPtr comparand, TContext context) {
			var spinner = new SpinWait();
			var newValue = mapFunc(comparand, context); // curValue will always be comparand when this method returns

			while (true) {
				if (Interlocked.CompareExchange(ref _value, newValue, comparand) == comparand) return (comparand, newValue);
				spinner.SpinOnce();
			}
		}

		public (IntPtr PreviousValue, IntPtr NewValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<IntPtr, TMapContext, IntPtr> mapFunc, Func<IntPtr, IntPtr, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = GetAsIntPtr();
				var newValue = mapFunc(curValue, mapContext);
				if (!predicate(curValue, newValue, predicateContext)) {
					spinner.SpinOnce();
					continue;
				}

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public (bool ValueWasSet, IntPtr PreviousValue, IntPtr NewValue) TryExchange(IntPtr newValue, IntPtr comparand) => TryExchange((T*) newValue, (T*) comparand).AsUntyped;

		public (bool ValueWasSet, IntPtr PreviousValue, IntPtr NewValue) TryExchange<TContext>(Func<IntPtr, TContext, IntPtr> mapFunc, IntPtr comparand, TContext context) {
			var newValue = mapFunc(comparand, context); // Comparand will always be curValue if the interlocked call passes
			var prevValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			if (prevValue == comparand) return (true, prevValue, newValue);
			else return (false, prevValue, prevValue);
		}

		public (bool ValueWasSet, IntPtr PreviousValue, IntPtr NewValue) TryExchange<TMapContext, TPredicateContext>(Func<IntPtr, TMapContext, IntPtr> mapFunc, Func<IntPtr, IntPtr, TPredicateContext, bool> predicate, TMapContext mapContext, TPredicateContext predicateContext) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = GetAsIntPtr();
				var newValue = mapFunc(curValue, mapContext);
				if (!predicate(curValue, newValue, predicateContext)) return (false, curValue, curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);

				spinner.SpinOnce();
			}
		}

		// ============================ Missing extension methods for IAtomic<T*> ============================

		public delegate bool AtomicPtrSpinWaitPredicate(T* curValue);
		public delegate bool AtomicPtrSpinWaitPredicate<in TContext>(T* curValue, TContext context);

		public T* SpinWaitForValue(AtomicPtrSpinWaitPredicate predicate) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (predicate(curValue)) return curValue;
				spinner.SpinOnce();
			}
		}

		public T* SpinWaitForValue<TContext>(AtomicPtrSpinWaitPredicate<TContext> predicate, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (predicate(curValue, context)) return curValue;
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TypedPtrExchangeRes Exchange(AtomicPtrMap mapFunc) => Exchange((curVal, ctx) => ctx(curVal), mapFunc);

		public TypedPtrExchangeRes SpinWaitForExchange(T* newValue, AtomicPtrPredicate predicate) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (!predicate(curValue, newValue)) {
					spinner.SpinOnce();
					continue;
				}

				if (Interlocked.CompareExchange(ref _value, (IntPtr) newValue, (IntPtr) curValue) == (IntPtr) curValue) return new TypedPtrExchangeRes(curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public TypedPtrExchangeRes SpinWaitForExchange<TContext>(T* newValue, AtomicPtrPredicate<TContext> predicate, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (!predicate(curValue, newValue, context)) {
					spinner.SpinOnce();
					continue;
				}

				if (Interlocked.CompareExchange(ref _value, (IntPtr) newValue, (IntPtr) curValue) == (IntPtr) curValue) return new TypedPtrExchangeRes(curValue, newValue);
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TypedPtrExchangeRes SpinWaitForExchange(AtomicPtrMap mapFunc, T* comparand) {
			return SpinWaitForExchange((curVal, ctx) => ctx(curVal), comparand, mapFunc);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TypedPtrExchangeRes SpinWaitForExchange(AtomicPtrMap mapFunc, AtomicPtrPredicate predicate) {
			return SpinWaitForExchange((curVal, ctx) => ctx(curVal), (curVal, newVal, ctx) => ctx(curVal, newVal), mapFunc, predicate);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TypedPtrExchangeRes SpinWaitForExchange<TContext>(AtomicPtrMap<TContext> mapFunc, AtomicPtrPredicate predicate, TContext context) {
			return SpinWaitForExchange(mapFunc, (curVal, newVal, ctx) => ctx(curVal, newVal), context, predicate);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TypedPtrExchangeRes SpinWaitForExchange<TContext>(AtomicPtrMap mapFunc, AtomicPtrPredicate<TContext> predicate, TContext context) {
			return SpinWaitForExchange((curVal, ctx) => ctx(curVal), predicate, mapFunc, context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TypedPtrExchangeRes SpinWaitForExchange<TContext>(AtomicPtrMap<TContext> mapFunc, AtomicPtrPredicate<TContext> predicate, TContext context) {
			return SpinWaitForExchange(mapFunc, predicate, context, context);
		}

		public TypedPtrTryExchangeRes TryExchange(T* newValue, AtomicPtrPredicate predicate) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (!predicate(curValue, newValue)) return new TypedPtrTryExchangeRes(false, curValue, curValue);

				if ((T*) Interlocked.CompareExchange(ref _value, (IntPtr) newValue, (IntPtr) curValue) == curValue) return new TypedPtrTryExchangeRes(true, curValue, newValue);

				spinner.SpinOnce();
			}
		}

		public TypedPtrTryExchangeRes TryExchange<TContext>(T* newValue, AtomicPtrPredicate<TContext> predicate, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (!predicate(curValue, newValue, context)) return new TypedPtrTryExchangeRes(false, curValue, curValue);

				if ((T*) Interlocked.CompareExchange(ref _value, (IntPtr) newValue, (IntPtr) curValue) == curValue) return new TypedPtrTryExchangeRes(true, curValue, newValue);

				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TypedPtrTryExchangeRes TryExchange(AtomicPtrMap mapFunc, T* comparand) {
			return TryExchange((curVal, ctx) => ctx(curVal), comparand, mapFunc);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TypedPtrTryExchangeRes TryExchange(AtomicPtrMap mapFunc, AtomicPtrPredicate predicate) {
			return TryExchange((curVal, ctx) => ctx(curVal), (curVal, newVal, ctx) => ctx(curVal, newVal), mapFunc, predicate);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TypedPtrTryExchangeRes TryExchange<TContext>(AtomicPtrMap<TContext> mapFunc, AtomicPtrPredicate predicate, TContext context) {
			return TryExchange(mapFunc, (curVal, newVal, ctx) => ctx(curVal, newVal), context, predicate);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TypedPtrTryExchangeRes TryExchange<TContext>(AtomicPtrMap mapFunc, AtomicPtrPredicate<TContext> predicate, TContext context) {
			return TryExchange((curVal, ctx) => ctx(curVal), predicate, mapFunc, context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TypedPtrTryExchangeRes TryExchange<TContext>(AtomicPtrMap<TContext> mapFunc, AtomicPtrPredicate<TContext> predicate, TContext context) {
			return TryExchange(mapFunc, predicate, context, context);
		}
	}
}
