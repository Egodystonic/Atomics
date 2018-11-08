using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Egodystonic.Atomics {
	public sealed unsafe class AtomicEnumVal<T> : IAtomic<T> where T : unmanaged, Enum {
		struct OperandAndRequisiteFlagsPair : IEquatable<OperandAndRequisiteFlagsPair> {
			public readonly T Operand;
			public readonly T RequisiteFlags;

			public OperandAndRequisiteFlagsPair(T operand, T requisiteFlags) {
				Operand = operand;
				RequisiteFlags = requisiteFlags;
			}

			public bool Equals(OperandAndRequisiteFlagsPair other) {
				return Operand.Equals(other.Operand) && RequisiteFlags.Equals(other.RequisiteFlags);
			}

			public override bool Equals(object obj) {
				if (ReferenceEquals(null, obj)) return false;
				return obj is OperandAndRequisiteFlagsPair other && Equals(other);
			}

			public override int GetHashCode() {
				unchecked {
					return (Operand.GetHashCode() * 397) ^ RequisiteFlags.GetHashCode();
				}
			}

			public static bool operator ==(OperandAndRequisiteFlagsPair left, OperandAndRequisiteFlagsPair right) { return left.Equals(right); }
			public static bool operator !=(OperandAndRequisiteFlagsPair left, OperandAndRequisiteFlagsPair right) { return !left.Equals(right); }
		}

		readonly AtomicValUnmanaged<T> _asUnmanaged;

		public AtomicEnumVal() => _asUnmanaged = new AtomicValUnmanaged<T>();
		public AtomicEnumVal(T initialValue) => _asUnmanaged = new AtomicValUnmanaged<T>(initialValue);

		public T Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _asUnmanaged.Value;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _asUnmanaged.Value = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get() { return _asUnmanaged.Get(); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetUnsafe() { return _asUnmanaged.GetUnsafe(); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(T newValue) { _asUnmanaged.Set(newValue); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(T newValue) { _asUnmanaged.SetUnsafe(newValue); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Exchange(T newValue) { return _asUnmanaged.Exchange(newValue); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, T PreviousValue) TryExchange(T newValue, T comparand) { return _asUnmanaged.TryExchange(newValue, comparand); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, T PreviousValue) TryExchange(T newValue, Func<T, bool> predicate) { return _asUnmanaged.TryExchange(newValue, predicate); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, T PreviousValue) TryExchange(T newValue, Func<T, T, bool> predicate) { return _asUnmanaged.TryExchange(newValue, predicate); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (T PreviousValue, T NewValue) Exchange(Func<T, T> mapFunc) { return _asUnmanaged.Exchange(mapFunc); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(Func<T, T> mapFunc, T comparand) { return _asUnmanaged.TryExchange(mapFunc, comparand); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(Func<T, T> mapFunc, Func<T, bool> predicate) { return _asUnmanaged.TryExchange(mapFunc, predicate); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(Func<T, T> mapFunc, Func<T, T, bool> predicate) { return _asUnmanaged.TryExchange(mapFunc, predicate); }

//		[MethodImpl(MethodImplOptions.AggressiveInlining)]
//		public (T PreviousValue, T NewValue) AddFlag(T operand) => Exchange((curVar, context) => curVar | context, operand);
//
//		public (bool FlagWasAdded, T PreviousValue, T NewValue) AddFlag(T operand, T requisiteFlags) {
//			var context = new OperandAndRequisiteFlagsPair(operand, requisiteFlags);
//			return TryExchange((curVar, context) => curVar | context.Operand, (curVar, context) => (curVar & context.RequisiteFlags) == context.RequisiteFlags, context);
//		} 
//
//		[MethodImpl(MethodImplOptions.AggressiveInlining)]
//		public (bool FlagWasAdded, T PreviousValue, T NewValue) AddFlag(T operand, Func<T, bool> predicate) {
//			
//		}
//
//		[MethodImpl(MethodImplOptions.AggressiveInlining)]
//		public (bool FlagWasAdded, T PreviousValue, T NewValue) AddFlag(T operand, Func<T, T, bool> predicate) {
//			
//		}
//
//		[MethodImpl(MethodImplOptions.AggressiveInlining)]
//		public (T PreviousValue, T NewValue) RemoveFlag(T operand) {
//			
//		}
//
//		[MethodImpl(MethodImplOptions.AggressiveInlining)]
//		public (bool FlagWasAdded, T PreviousValue, T NewValue) RemoveFlag(T operand, T requisiteFlags) {
//			
//		}
//
//		[MethodImpl(MethodImplOptions.AggressiveInlining)]
//		public (bool FlagWasAdded, T PreviousValue, T NewValue) RemoveFlag(T operand, Func<T, bool> predicate) {
//			
//		}
//
//		[MethodImpl(MethodImplOptions.AggressiveInlining)]
//		public (bool FlagWasAdded, T PreviousValue, T NewValue) RemoveFlag(T operand, Func<T, T, bool> predicate) {
//			
//		}
//
//		[MethodImpl(MethodImplOptions.AggressiveInlining)]
//		public bool HasFlag(T operand) {
//			
//		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(AtomicEnumVal<T> operand) => operand.Get();
	}
}
