// (c) Egodystonic Studios 2019
// Author: Ben Bowen

using System;

namespace Egodystonic.Atomics {
	public static class LockingAtomicNumberExtensions {
		#region SByte
		public static SByte Increment(this ILockingAtomic<SByte> @this) => @this.Set(i => ++i);
		public static SByte Increment(this ILockingAtomic<SByte> @this, out SByte previousValue) => @this.Set(i => ++i, out previousValue);

		public static SByte Decrement(this ILockingAtomic<SByte> @this) => @this.Set(i => --i);
		public static SByte Decrement(this ILockingAtomic<SByte> @this, out SByte previousValue) => @this.Set(i => --i, out previousValue);

		public static SByte Add(this ILockingAtomic<SByte> @this, SByte operand) => @this.Set(i => (SByte) (i + operand));
		public static SByte Add(this ILockingAtomic<SByte> @this, SByte operand, out SByte previousValue) => @this.Set(i => (SByte) (i + operand), out previousValue);

		public static SByte Subtract(this ILockingAtomic<SByte> @this, SByte operand) => @this.Set(i => (SByte) (i - operand));
		public static SByte Subtract(this ILockingAtomic<SByte> @this, SByte operand, out SByte previousValue) => @this.Set(i => (SByte) (i - operand), out previousValue);

		public static SByte Multiply(this ILockingAtomic<SByte> @this, SByte operand) => @this.Set(i => (SByte) (i * operand));
		public static SByte Multiply(this ILockingAtomic<SByte> @this, SByte operand, out SByte previousValue) => @this.Set(i => (SByte) (i * operand), out previousValue);

		public static SByte Divide(this ILockingAtomic<SByte> @this, SByte operand) => @this.Set(i => (SByte) (i / operand));
		public static SByte Divide(this ILockingAtomic<SByte> @this, SByte operand, out SByte previousValue) => @this.Set(i => (SByte) (i / operand), out previousValue);
		#endregion

		#region Byte
		public static Byte Increment(this ILockingAtomic<Byte> @this) => @this.Set(i => ++i);
		public static Byte Increment(this ILockingAtomic<Byte> @this, out Byte previousValue) => @this.Set(i => ++i, out previousValue);

		public static Byte Decrement(this ILockingAtomic<Byte> @this) => @this.Set(i => --i);
		public static Byte Decrement(this ILockingAtomic<Byte> @this, out Byte previousValue) => @this.Set(i => --i, out previousValue);

		public static Byte Add(this ILockingAtomic<Byte> @this, Byte operand) => @this.Set(i => (Byte) (i + operand));
		public static Byte Add(this ILockingAtomic<Byte> @this, Byte operand, out Byte previousValue) => @this.Set(i => (Byte) (i + operand), out previousValue);

		public static Byte Subtract(this ILockingAtomic<Byte> @this, Byte operand) => @this.Set(i => (Byte) (i - operand));
		public static Byte Subtract(this ILockingAtomic<Byte> @this, Byte operand, out Byte previousValue) => @this.Set(i => (Byte) (i - operand), out previousValue);

		public static Byte Multiply(this ILockingAtomic<Byte> @this, Byte operand) => @this.Set(i => (Byte) (i * operand));
		public static Byte Multiply(this ILockingAtomic<Byte> @this, Byte operand, out Byte previousValue) => @this.Set(i => (Byte) (i * operand), out previousValue);

		public static Byte Divide(this ILockingAtomic<Byte> @this, Byte operand) => @this.Set(i => (Byte) (i / operand));
		public static Byte Divide(this ILockingAtomic<Byte> @this, Byte operand, out Byte previousValue) => @this.Set(i => (Byte) (i / operand), out previousValue);
		#endregion

		#region Int16
		public static Int16 Increment(this ILockingAtomic<Int16> @this) => @this.Set(i => ++i);
		public static Int16 Increment(this ILockingAtomic<Int16> @this, out Int16 previousValue) => @this.Set(i => ++i, out previousValue);

		public static Int16 Decrement(this ILockingAtomic<Int16> @this) => @this.Set(i => --i);
		public static Int16 Decrement(this ILockingAtomic<Int16> @this, out Int16 previousValue) => @this.Set(i => --i, out previousValue);

		public static Int16 Add(this ILockingAtomic<Int16> @this, Int16 operand) => @this.Set(i => (Int16) (i + operand));
		public static Int16 Add(this ILockingAtomic<Int16> @this, Int16 operand, out Int16 previousValue) => @this.Set(i => (Int16) (i + operand), out previousValue);

		public static Int16 Subtract(this ILockingAtomic<Int16> @this, Int16 operand) => @this.Set(i => (Int16) (i - operand));
		public static Int16 Subtract(this ILockingAtomic<Int16> @this, Int16 operand, out Int16 previousValue) => @this.Set(i => (Int16) (i - operand), out previousValue);

		public static Int16 Multiply(this ILockingAtomic<Int16> @this, Int16 operand) => @this.Set(i => (Int16) (i * operand));
		public static Int16 Multiply(this ILockingAtomic<Int16> @this, Int16 operand, out Int16 previousValue) => @this.Set(i => (Int16) (i * operand), out previousValue);

		public static Int16 Divide(this ILockingAtomic<Int16> @this, Int16 operand) => @this.Set(i => (Int16) (i / operand));
		public static Int16 Divide(this ILockingAtomic<Int16> @this, Int16 operand, out Int16 previousValue) => @this.Set(i => (Int16) (i / operand), out previousValue);
		#endregion

		#region UInt16
		public static UInt16 Increment(this ILockingAtomic<UInt16> @this) => @this.Set(i => ++i);
		public static UInt16 Increment(this ILockingAtomic<UInt16> @this, out UInt16 previousValue) => @this.Set(i => ++i, out previousValue);

		public static UInt16 Decrement(this ILockingAtomic<UInt16> @this) => @this.Set(i => --i);
		public static UInt16 Decrement(this ILockingAtomic<UInt16> @this, out UInt16 previousValue) => @this.Set(i => --i, out previousValue);

		public static UInt16 Add(this ILockingAtomic<UInt16> @this, UInt16 operand) => @this.Set(i => (UInt16) (i + operand));
		public static UInt16 Add(this ILockingAtomic<UInt16> @this, UInt16 operand, out UInt16 previousValue) => @this.Set(i => (UInt16) (i + operand), out previousValue);

		public static UInt16 Subtract(this ILockingAtomic<UInt16> @this, UInt16 operand) => @this.Set(i => (UInt16) (i - operand));
		public static UInt16 Subtract(this ILockingAtomic<UInt16> @this, UInt16 operand, out UInt16 previousValue) => @this.Set(i => (UInt16) (i - operand), out previousValue);

		public static UInt16 Multiply(this ILockingAtomic<UInt16> @this, UInt16 operand) => @this.Set(i => (UInt16) (i * operand));
		public static UInt16 Multiply(this ILockingAtomic<UInt16> @this, UInt16 operand, out UInt16 previousValue) => @this.Set(i => (UInt16) (i * operand), out previousValue);

		public static UInt16 Divide(this ILockingAtomic<UInt16> @this, UInt16 operand) => @this.Set(i => (UInt16) (i / operand));
		public static UInt16 Divide(this ILockingAtomic<UInt16> @this, UInt16 operand, out UInt16 previousValue) => @this.Set(i => (UInt16) (i / operand), out previousValue);
		#endregion

		#region Int32
		public static Int32 Increment(this ILockingAtomic<Int32> @this) => @this.Set(i => ++i);
		public static Int32 Increment(this ILockingAtomic<Int32> @this, out Int32 previousValue) => @this.Set(i => ++i, out previousValue);

		public static Int32 Decrement(this ILockingAtomic<Int32> @this) => @this.Set(i => --i);
		public static Int32 Decrement(this ILockingAtomic<Int32> @this, out Int32 previousValue) => @this.Set(i => --i, out previousValue);

		public static Int32 Add(this ILockingAtomic<Int32> @this, Int32 operand) => @this.Set(i => i + operand);
		public static Int32 Add(this ILockingAtomic<Int32> @this, Int32 operand, out Int32 previousValue) => @this.Set(i => i + operand, out previousValue);

		public static Int32 Subtract(this ILockingAtomic<Int32> @this, Int32 operand) => @this.Set(i => i - operand);
		public static Int32 Subtract(this ILockingAtomic<Int32> @this, Int32 operand, out Int32 previousValue) => @this.Set(i => i - operand, out previousValue);

		public static Int32 Multiply(this ILockingAtomic<Int32> @this, Int32 operand) => @this.Set(i => i * operand);
		public static Int32 Multiply(this ILockingAtomic<Int32> @this, Int32 operand, out Int32 previousValue) => @this.Set(i => i * operand, out previousValue);

		public static Int32 Divide(this ILockingAtomic<Int32> @this, Int32 operand) => @this.Set(i => i / operand);
		public static Int32 Divide(this ILockingAtomic<Int32> @this, Int32 operand, out Int32 previousValue) => @this.Set(i => i / operand, out previousValue);
		#endregion

		#region UInt32
		public static UInt32 Increment(this ILockingAtomic<UInt32> @this) => @this.Set(i => ++i);
		public static UInt32 Increment(this ILockingAtomic<UInt32> @this, out UInt32 previousValue) => @this.Set(i => ++i, out previousValue);

		public static UInt32 Decrement(this ILockingAtomic<UInt32> @this) => @this.Set(i => --i);
		public static UInt32 Decrement(this ILockingAtomic<UInt32> @this, out UInt32 previousValue) => @this.Set(i => --i, out previousValue);

		public static UInt32 Add(this ILockingAtomic<UInt32> @this, UInt32 operand) => @this.Set(i => i + operand);
		public static UInt32 Add(this ILockingAtomic<UInt32> @this, UInt32 operand, out UInt32 previousValue) => @this.Set(i => i + operand, out previousValue);

		public static UInt32 Subtract(this ILockingAtomic<UInt32> @this, UInt32 operand) => @this.Set(i => i - operand);
		public static UInt32 Subtract(this ILockingAtomic<UInt32> @this, UInt32 operand, out UInt32 previousValue) => @this.Set(i => i - operand, out previousValue);

		public static UInt32 Multiply(this ILockingAtomic<UInt32> @this, UInt32 operand) => @this.Set(i => i * operand);
		public static UInt32 Multiply(this ILockingAtomic<UInt32> @this, UInt32 operand, out UInt32 previousValue) => @this.Set(i => i * operand, out previousValue);

		public static UInt32 Divide(this ILockingAtomic<UInt32> @this, UInt32 operand) => @this.Set(i => i / operand);
		public static UInt32 Divide(this ILockingAtomic<UInt32> @this, UInt32 operand, out UInt32 previousValue) => @this.Set(i => i / operand, out previousValue);
		#endregion

		#region Int64
		public static Int64 Increment(this ILockingAtomic<Int64> @this) => @this.Set(i => ++i);
		public static Int64 Increment(this ILockingAtomic<Int64> @this, out Int64 previousValue) => @this.Set(i => ++i, out previousValue);

		public static Int64 Decrement(this ILockingAtomic<Int64> @this) => @this.Set(i => --i);
		public static Int64 Decrement(this ILockingAtomic<Int64> @this, out Int64 previousValue) => @this.Set(i => --i, out previousValue);

		public static Int64 Add(this ILockingAtomic<Int64> @this, Int64 operand) => @this.Set(i => i + operand);
		public static Int64 Add(this ILockingAtomic<Int64> @this, Int64 operand, out Int64 previousValue) => @this.Set(i => i + operand, out previousValue);

		public static Int64 Subtract(this ILockingAtomic<Int64> @this, Int64 operand) => @this.Set(i => i - operand);
		public static Int64 Subtract(this ILockingAtomic<Int64> @this, Int64 operand, out Int64 previousValue) => @this.Set(i => i - operand, out previousValue);

		public static Int64 Multiply(this ILockingAtomic<Int64> @this, Int64 operand) => @this.Set(i => i * operand);
		public static Int64 Multiply(this ILockingAtomic<Int64> @this, Int64 operand, out Int64 previousValue) => @this.Set(i => i * operand, out previousValue);

		public static Int64 Divide(this ILockingAtomic<Int64> @this, Int64 operand) => @this.Set(i => i / operand);
		public static Int64 Divide(this ILockingAtomic<Int64> @this, Int64 operand, out Int64 previousValue) => @this.Set(i => i / operand, out previousValue);
		#endregion

		#region UInt64
		public static UInt64 Increment(this ILockingAtomic<UInt64> @this) => @this.Set(i => ++i);
		public static UInt64 Increment(this ILockingAtomic<UInt64> @this, out UInt64 previousValue) => @this.Set(i => ++i, out previousValue);

		public static UInt64 Decrement(this ILockingAtomic<UInt64> @this) => @this.Set(i => --i);
		public static UInt64 Decrement(this ILockingAtomic<UInt64> @this, out UInt64 previousValue) => @this.Set(i => --i, out previousValue);

		public static UInt64 Add(this ILockingAtomic<UInt64> @this, UInt64 operand) => @this.Set(i => i + operand);
		public static UInt64 Add(this ILockingAtomic<UInt64> @this, UInt64 operand, out UInt64 previousValue) => @this.Set(i => i + operand, out previousValue);

		public static UInt64 Subtract(this ILockingAtomic<UInt64> @this, UInt64 operand) => @this.Set(i => i - operand);
		public static UInt64 Subtract(this ILockingAtomic<UInt64> @this, UInt64 operand, out UInt64 previousValue) => @this.Set(i => i - operand, out previousValue);

		public static UInt64 Multiply(this ILockingAtomic<UInt64> @this, UInt64 operand) => @this.Set(i => i * operand);
		public static UInt64 Multiply(this ILockingAtomic<UInt64> @this, UInt64 operand, out UInt64 previousValue) => @this.Set(i => i * operand, out previousValue);

		public static UInt64 Divide(this ILockingAtomic<UInt64> @this, UInt64 operand) => @this.Set(i => i / operand);
		public static UInt64 Divide(this ILockingAtomic<UInt64> @this, UInt64 operand, out UInt64 previousValue) => @this.Set(i => i / operand, out previousValue);
		#endregion

		#region Single
		public static Single Increment(this ILockingAtomic<Single> @this) => @this.Set(i => ++i);
		public static Single Increment(this ILockingAtomic<Single> @this, out Single previousValue) => @this.Set(i => ++i, out previousValue);

		public static Single Decrement(this ILockingAtomic<Single> @this) => @this.Set(i => --i);
		public static Single Decrement(this ILockingAtomic<Single> @this, out Single previousValue) => @this.Set(i => --i, out previousValue);

		public static Single Add(this ILockingAtomic<Single> @this, Single operand) => @this.Set(i => i + operand);
		public static Single Add(this ILockingAtomic<Single> @this, Single operand, out Single previousValue) => @this.Set(i => i + operand, out previousValue);

		public static Single Subtract(this ILockingAtomic<Single> @this, Single operand) => @this.Set(i => i - operand);
		public static Single Subtract(this ILockingAtomic<Single> @this, Single operand, out Single previousValue) => @this.Set(i => i - operand, out previousValue);

		public static Single Multiply(this ILockingAtomic<Single> @this, Single operand) => @this.Set(i => i * operand);
		public static Single Multiply(this ILockingAtomic<Single> @this, Single operand, out Single previousValue) => @this.Set(i => i * operand, out previousValue);

		public static Single Divide(this ILockingAtomic<Single> @this, Single operand) => @this.Set(i => i / operand);
		public static Single Divide(this ILockingAtomic<Single> @this, Single operand, out Single previousValue) => @this.Set(i => i / operand, out previousValue);
		#endregion

		#region Double
		public static Double Increment(this ILockingAtomic<Double> @this) => @this.Set(i => ++i);
		public static Double Increment(this ILockingAtomic<Double> @this, out Double previousValue) => @this.Set(i => ++i, out previousValue);

		public static Double Decrement(this ILockingAtomic<Double> @this) => @this.Set(i => --i);
		public static Double Decrement(this ILockingAtomic<Double> @this, out Double previousValue) => @this.Set(i => --i, out previousValue);

		public static Double Add(this ILockingAtomic<Double> @this, Double operand) => @this.Set(i => i + operand);
		public static Double Add(this ILockingAtomic<Double> @this, Double operand, out Double previousValue) => @this.Set(i => i + operand, out previousValue);

		public static Double Subtract(this ILockingAtomic<Double> @this, Double operand) => @this.Set(i => i - operand);
		public static Double Subtract(this ILockingAtomic<Double> @this, Double operand, out Double previousValue) => @this.Set(i => i - operand, out previousValue);

		public static Double Multiply(this ILockingAtomic<Double> @this, Double operand) => @this.Set(i => i * operand);
		public static Double Multiply(this ILockingAtomic<Double> @this, Double operand, out Double previousValue) => @this.Set(i => i * operand, out previousValue);

		public static Double Divide(this ILockingAtomic<Double> @this, Double operand) => @this.Set(i => i / operand);
		public static Double Divide(this ILockingAtomic<Double> @this, Double operand, out Double previousValue) => @this.Set(i => i / operand, out previousValue);
		#endregion
	}
}