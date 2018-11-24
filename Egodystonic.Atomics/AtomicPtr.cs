using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Egodystonic.Atomics.Numerics;

namespace Egodystonic.Atomics {
	public sealed unsafe class AtomicPtr<T> : INumericAtomic<IntPtr> where T : unmanaged {
		public struct TypedPtrTryExchangeRes : IEquatable<TypedPtrTryExchangeRes> {
			public readonly bool ValueWasSet;
			public readonly T* PreviousValue;

			public TypedPtrTryExchangeRes(bool valueWasSet, T* previousValue) {
				ValueWasSet = valueWasSet;
				PreviousValue = previousValue;
			}

			public bool Equals(TypedPtrTryExchangeRes other) {
				return ValueWasSet == other.ValueWasSet && PreviousValue == other.PreviousValue;
			}

			public override bool Equals(object obj) {
				if (ReferenceEquals(null, obj)) return false;
				return obj is TypedPtrTryExchangeRes other && Equals(other);
			}

			public override int GetHashCode() {
				unchecked {
					return (ValueWasSet.GetHashCode() * 397) ^ unchecked((int) (long) PreviousValue);
				}
			}

			public void Deconstruct(out bool valueWasSet, out IntPtr previousValue) {
				valueWasSet = ValueWasSet;
				previousValue = (IntPtr) PreviousValue;
			}

			public static bool operator ==(TypedPtrTryExchangeRes left, TypedPtrTryExchangeRes right) { return left.Equals(right); }
			public static bool operator !=(TypedPtrTryExchangeRes left, TypedPtrTryExchangeRes right) { return !left.Equals(right); }
		}

		public struct TypedPtrTryExchangeMappedRes : IEquatable<TypedPtrTryExchangeMappedRes> {
			public readonly bool ValueWasSet;
			public readonly T* PreviousValue;
			public readonly T* NewValue;

			public TypedPtrTryExchangeMappedRes(bool valueWasSet, T* previousValue, T* newValue) {
				ValueWasSet = valueWasSet;
				PreviousValue = previousValue;
				NewValue = newValue;
			}

			public bool Equals(TypedPtrTryExchangeMappedRes other) {
				return ValueWasSet == other.ValueWasSet && PreviousValue == other.PreviousValue && NewValue == other.NewValue;
			}

			public override bool Equals(object obj) {
				if (ReferenceEquals(null, obj)) return false;
				return obj is TypedPtrTryExchangeMappedRes other && Equals(other);
			}

			public override int GetHashCode() {
				unchecked {
					var hashCode = ValueWasSet.GetHashCode();
					hashCode = (hashCode * 397) ^ unchecked((int) (long) PreviousValue);
					hashCode = (hashCode * 397) ^ unchecked((int) (long) NewValue);
					return hashCode;
				}
			}

			public void Deconstruct(out bool valueWasSet, out T* previousValue, out T* newValue) {
				valueWasSet = ValueWasSet;
				previousValue = PreviousValue;
				newValue = NewValue;
			}

			public static bool operator ==(TypedPtrTryExchangeMappedRes left, TypedPtrTryExchangeMappedRes right) { return left.Equals(right); }
			public static bool operator !=(TypedPtrTryExchangeMappedRes left, TypedPtrTryExchangeMappedRes right) { return !left.Equals(right); }
		}

		public struct TypedPtrExchangeRes : IEquatable<TypedPtrExchangeRes> {
			public readonly T* PreviousValue;
			public readonly T* NewValue;

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
		public delegate bool AtomicPtrPredicateContextual<in TContext>(T* curValue, T* newValue, TContext context);
		public delegate T* AtomicPtrMapper(T* curValue);

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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T* Exchange(T* newValue) => (T*) Interlocked.Exchange(ref _value, (IntPtr)newValue);

		public TypedPtrTryExchangeRes TryExchange(T* newValue, T* comparand) {
			var oldValue = Interlocked.CompareExchange(ref _value, (IntPtr) newValue, (IntPtr) comparand);
			return new TypedPtrTryExchangeRes((T*) oldValue == comparand, (T*) oldValue);
		}

		public TypedPtrTryExchangeRes TryExchange(T* newValue, AtomicPtrPredicate predicate) {
			bool trySetValue;
			IntPtr curValueAsIntPtr;

			var spinner = new SpinWait();

			while (true) {
				curValueAsIntPtr = GetAsIntPtr();
				trySetValue = predicate((T*) curValueAsIntPtr, newValue);

				if (!trySetValue || Interlocked.CompareExchange(ref _value, (IntPtr) newValue, curValueAsIntPtr) == curValueAsIntPtr) break;
				spinner.SpinOnce();
			}

			return new TypedPtrTryExchangeRes(trySetValue, (T*) curValueAsIntPtr);
		}

		public TypedPtrExchangeRes Exchange(AtomicPtrMapper mapFunc) {
			IntPtr curValueAsIntPtr;
			IntPtr newValueAsIntPtr;

			var spinner = new SpinWait();

			while (true) {
				curValueAsIntPtr = GetAsIntPtr();
				newValueAsIntPtr = (IntPtr) mapFunc((T*) curValueAsIntPtr);

				if (Interlocked.CompareExchange(ref _value, newValueAsIntPtr, curValueAsIntPtr) == curValueAsIntPtr) break;
				spinner.SpinOnce();
			}

			return new TypedPtrExchangeRes((T*) curValueAsIntPtr, (T*) newValueAsIntPtr);
		}

		public TypedPtrTryExchangeMappedRes TryExchange(AtomicPtrMapper mapFunc, T* comparand) {
			bool trySetValue;
			IntPtr curValueAsIntPtr;
			IntPtr newValueAsIntPtr = default;

			var spinner = new SpinWait();

			while (true) {
				curValueAsIntPtr = GetAsIntPtr();
				trySetValue = comparand == (T*) curValueAsIntPtr;

				if (!trySetValue) break;

				newValueAsIntPtr = (IntPtr) mapFunc((T*) curValueAsIntPtr);

				if (Interlocked.CompareExchange(ref _value, newValueAsIntPtr, curValueAsIntPtr) == curValueAsIntPtr) break;
				spinner.SpinOnce();
			}

			return new TypedPtrTryExchangeMappedRes(trySetValue, (T*) curValueAsIntPtr, (T*) newValueAsIntPtr);
		}

		public TypedPtrTryExchangeMappedRes TryExchange(AtomicPtrMapper mapFunc, AtomicPtrPredicate predicate) {
			bool trySetValue;
			IntPtr curValueAsIntPtr;
			IntPtr newValueAsIntPtr;

			var spinner = new SpinWait();

			while (true) {
				curValueAsIntPtr = GetAsIntPtr();
				newValueAsIntPtr = (IntPtr) mapFunc((T*) curValueAsIntPtr);
				trySetValue = predicate((T*) curValueAsIntPtr, (T*) newValueAsIntPtr);

				if (!trySetValue) break;

				if (Interlocked.CompareExchange(ref _value, newValueAsIntPtr, curValueAsIntPtr) == curValueAsIntPtr) break;
				spinner.SpinOnce();
			}

			return new TypedPtrTryExchangeMappedRes(trySetValue, (T*) curValueAsIntPtr, (T*) newValueAsIntPtr);
		}

		public TypedPtrExchangeRes Increment() {
			var spinner = new SpinWait();

			while (true) {
				var curValue = GetAsIntPtr();
				var newValue = curValue + 1;
				var oldValue = Interlocked.CompareExchange(ref _value, newValue, curValue);
				if (oldValue == curValue) return new TypedPtrExchangeRes((T*) oldValue, (T*) newValue);
				spinner.SpinOnce();
			}
		}

		public TypedPtrExchangeRes Decrement() {
			var spinner = new SpinWait();

			while (true) {
				var curValue = GetAsIntPtr();
				var newValue = curValue - 1;
				var oldValue = Interlocked.CompareExchange(ref _value, newValue, curValue);
				if (oldValue == curValue) return new TypedPtrExchangeRes((T*) oldValue, (T*) newValue);
				spinner.SpinOnce();
			}
		}

		public TypedPtrExchangeRes Add(IntPtr operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = GetAsIntPtr();
				var newValue = AddPointers(curValue, operand);
				var oldValue = Interlocked.CompareExchange(ref _value, newValue, curValue);
				if (oldValue == curValue) return new TypedPtrExchangeRes((T*) oldValue, (T*) newValue);
				spinner.SpinOnce();
			}

			IntPtr AddPointers(IntPtr lhs, IntPtr rhs) {
				switch (IntPtr.Size) {
					case 4: return (IntPtr) (lhs.ToInt32() + rhs.ToInt32());
					case 8: return (IntPtr) (lhs.ToInt64() + rhs.ToInt64());
					default: throw new InvalidOperationException($"Unexpected pointer size of {IntPtr.Size} bytes.");
				}
			}
		}

		public TypedPtrExchangeRes Subtract(IntPtr operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = GetAsIntPtr();
				var newValue = SubtractPointers(curValue, operand);
				var oldValue = Interlocked.CompareExchange(ref _value, newValue, curValue);
				if (oldValue == curValue) return new TypedPtrExchangeRes((T*) oldValue, (T*) newValue);
				spinner.SpinOnce();
			}

			IntPtr SubtractPointers(IntPtr lhs, IntPtr rhs) {
				switch (IntPtr.Size) {
					case 4: return (IntPtr) (lhs.ToInt32() - rhs.ToInt32());
					case 8: return (IntPtr) (lhs.ToInt64() - rhs.ToInt64());
					default: throw new InvalidOperationException($"Unexpected pointer size of {IntPtr.Size} bytes.");
				}
			}
		}

		public TypedPtrExchangeRes MultiplyBy(IntPtr operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = GetAsIntPtr();
				var newValue = MultiplyPointers(curValue, operand);
				var oldValue = Interlocked.CompareExchange(ref _value, newValue, curValue);
				if (oldValue == curValue) return new TypedPtrExchangeRes((T*) oldValue, (T*) newValue);
				spinner.SpinOnce();
			}

			IntPtr MultiplyPointers(IntPtr lhs, IntPtr rhs) {
				switch (IntPtr.Size) {
					case 4: return (IntPtr) (lhs.ToInt32() * rhs.ToInt32());
					case 8: return (IntPtr) (lhs.ToInt64() * rhs.ToInt64());
					default: throw new InvalidOperationException($"Unexpected pointer size of {IntPtr.Size} bytes.");
				}
			}
		}

		public TypedPtrExchangeRes DivideBy(IntPtr operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = GetAsIntPtr();
				var newValue = DividePointers(curValue, operand);
				var oldValue = Interlocked.CompareExchange(ref _value, newValue, curValue);
				if (oldValue == curValue) return new TypedPtrExchangeRes((T*) oldValue, (T*) newValue);
				spinner.SpinOnce();
			}

			IntPtr DividePointers(IntPtr lhs, IntPtr rhs) {
				switch (IntPtr.Size) {
					case 4: return (IntPtr) (lhs.ToInt32() / rhs.ToInt32());
					case 8: return (IntPtr) (lhs.ToInt64() / rhs.ToInt64());
					default: throw new InvalidOperationException($"Unexpected pointer size of {IntPtr.Size} bytes.");
				}
			}
		}

		IntPtr IAtomic<IntPtr>.Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetAsIntPtr();
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => SetAsIntPtr(value);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)] IntPtr IAtomic<IntPtr>.Get() => GetAsIntPtr();
		[MethodImpl(MethodImplOptions.AggressiveInlining)] IntPtr IAtomic<IntPtr>.GetUnsafe() => (IntPtr) GetUnsafe();
		[MethodImpl(MethodImplOptions.AggressiveInlining)] void IAtomic<IntPtr>.Set(IntPtr newValue) => SetAsIntPtr(newValue);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] void IAtomic<IntPtr>.SetUnsafe(IntPtr newValue) => SetUnsafe((T*) newValue);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] IntPtr IAtomic<IntPtr>.Exchange(IntPtr newValue) => (IntPtr) Exchange((T*) newValue);

		(bool ValueWasSet, IntPtr PreviousValue) IAtomic<IntPtr>.TryExchange(IntPtr newValue, IntPtr comparand) {
			var res = TryExchange((T*) newValue, (T*) comparand);
			return (res.ValueWasSet, (IntPtr) res.PreviousValue);
		}

		(bool ValueWasSet, IntPtr PreviousValue) IAtomic<IntPtr>.TryExchange(IntPtr newValue, Func<IntPtr, IntPtr, bool> predicate) {
			var res = TryExchange((T*) newValue, (curPtr, newPtr) => predicate((IntPtr) curPtr, (IntPtr) newPtr));
			return (res.ValueWasSet, (IntPtr) res.PreviousValue);
		}

		(IntPtr PreviousValue, IntPtr NewValue) IAtomic<IntPtr>.Exchange(Func<IntPtr, IntPtr> mapFunc) {
			var res = Exchange(ptr => (T*) mapFunc((IntPtr) ptr));
			return ((IntPtr) res.PreviousValue, (IntPtr) res.NewValue);
		}

		(bool ValueWasSet, IntPtr PreviousValue, IntPtr NewValue) IAtomic<IntPtr>.TryExchange(Func<IntPtr, IntPtr> mapFunc, IntPtr comparand) {
			var res = TryExchange(ptr => (T*) mapFunc((IntPtr) ptr), (T*) comparand);
			return (res.ValueWasSet, (IntPtr) res.PreviousValue, (IntPtr) res.NewValue);
		}

		(bool ValueWasSet, IntPtr PreviousValue, IntPtr NewValue) IAtomic<IntPtr>.TryExchange(Func<IntPtr, IntPtr> mapFunc, Func<IntPtr, IntPtr, bool> predicate) {
			var res = TryExchange(ptr => (T*) mapFunc((IntPtr) ptr), (curPtr, newPtr) => predicate((IntPtr) curPtr, (IntPtr) newPtr));
			return (res.ValueWasSet, (IntPtr) res.PreviousValue, (IntPtr) res.NewValue);
		}

		(IntPtr PreviousValue, IntPtr NewValue) INumericAtomic<IntPtr>.Increment() {
			var res = Increment();
			return ((IntPtr) res.PreviousValue, (IntPtr) res.NewValue);
		}

		(IntPtr PreviousValue, IntPtr NewValue) INumericAtomic<IntPtr>.Decrement() {
			var res = Decrement();
			return ((IntPtr) res.PreviousValue, (IntPtr) res.NewValue);
		}

		(IntPtr PreviousValue, IntPtr NewValue) INumericAtomic<IntPtr>.Add(IntPtr operand) {
			var res = Add(operand);
			return ((IntPtr) res.PreviousValue, (IntPtr) res.NewValue);
		}

		(IntPtr PreviousValue, IntPtr NewValue) INumericAtomic<IntPtr>.Subtract(IntPtr operand) {
			var res = Subtract(operand);
			return ((IntPtr) res.PreviousValue, (IntPtr) res.NewValue);
		}

		(IntPtr PreviousValue, IntPtr NewValue) INumericAtomic<IntPtr>.MultiplyBy(IntPtr operand) {
			var res = MultiplyBy(operand);
			return ((IntPtr) res.PreviousValue, (IntPtr) res.NewValue);
		}

		(IntPtr PreviousValue, IntPtr NewValue) INumericAtomic<IntPtr>.DivideBy(IntPtr operand) {
			var res = DivideBy(operand);
			return ((IntPtr) res.PreviousValue, (IntPtr) res.NewValue);
		}
	}
}
