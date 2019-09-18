// (c) Egodystonic Studios 2019
// Author: Ben Bowen

using System;

// Note: Although these could arguably be in the "Numerics" folder, I wanted to keep them in the same namespace as Atomic<T>
namespace Egodystonic.Atomics {
	public static class NonScalableAtomicNumberExtensions {
		#region SByte
		public static SByte Increment(this INonScalableAtomic<SByte> @this) => @this.Set(i => ++i);
		public static SByte Increment(this INonScalableAtomic<SByte> @this, out SByte previousValue) => @this.Set(i => ++i, out previousValue);

		public static SByte Decrement(this INonScalableAtomic<SByte> @this) => @this.Set(i => --i);
		public static SByte Decrement(this INonScalableAtomic<SByte> @this, out SByte previousValue) => @this.Set(i => --i, out previousValue);

		public static SByte Add(this INonScalableAtomic<SByte> @this, SByte operand) => @this.Set(i => (SByte) (i + operand));
		public static SByte Add(this INonScalableAtomic<SByte> @this, SByte operand, out SByte previousValue) => @this.Set(i => (SByte) (i + operand), out previousValue);

		public static SByte Subtract(this INonScalableAtomic<SByte> @this, SByte operand) => @this.Set(i => (SByte) (i - operand));
		public static SByte Subtract(this INonScalableAtomic<SByte> @this, SByte operand, out SByte previousValue) => @this.Set(i => (SByte) (i - operand), out previousValue);

		public static SByte Multiply(this INonScalableAtomic<SByte> @this, SByte operand) => @this.Set(i => (SByte) (i * operand));
		public static SByte Multiply(this INonScalableAtomic<SByte> @this, SByte operand, out SByte previousValue) => @this.Set(i => (SByte) (i * operand), out previousValue);

		public static SByte Divide(this INonScalableAtomic<SByte> @this, SByte operand) => @this.Set(i => (SByte) (i / operand));
		public static SByte Divide(this INonScalableAtomic<SByte> @this, SByte operand, out SByte previousValue) => @this.Set(i => (SByte) (i / operand), out previousValue);
		#endregion

		#region Byte
		public static Byte Increment(this INonScalableAtomic<Byte> @this) => @this.Set(i => ++i);
		public static Byte Increment(this INonScalableAtomic<Byte> @this, out Byte previousValue) => @this.Set(i => ++i, out previousValue);

		public static Byte Decrement(this INonScalableAtomic<Byte> @this) => @this.Set(i => --i);
		public static Byte Decrement(this INonScalableAtomic<Byte> @this, out Byte previousValue) => @this.Set(i => --i, out previousValue);

		public static Byte Add(this INonScalableAtomic<Byte> @this, Byte operand) => @this.Set(i => (Byte) (i + operand));
		public static Byte Add(this INonScalableAtomic<Byte> @this, Byte operand, out Byte previousValue) => @this.Set(i => (Byte) (i + operand), out previousValue);

		public static Byte Subtract(this INonScalableAtomic<Byte> @this, Byte operand) => @this.Set(i => (Byte) (i - operand));
		public static Byte Subtract(this INonScalableAtomic<Byte> @this, Byte operand, out Byte previousValue) => @this.Set(i => (Byte) (i - operand), out previousValue);

		public static Byte Multiply(this INonScalableAtomic<Byte> @this, Byte operand) => @this.Set(i => (Byte) (i * operand));
		public static Byte Multiply(this INonScalableAtomic<Byte> @this, Byte operand, out Byte previousValue) => @this.Set(i => (Byte) (i * operand), out previousValue);

		public static Byte Divide(this INonScalableAtomic<Byte> @this, Byte operand) => @this.Set(i => (Byte) (i / operand));
		public static Byte Divide(this INonScalableAtomic<Byte> @this, Byte operand, out Byte previousValue) => @this.Set(i => (Byte) (i / operand), out previousValue);
		#endregion

		#region Int16
		public static Int16 Increment(this INonScalableAtomic<Int16> @this) => @this.Set(i => ++i);
		public static Int16 Increment(this INonScalableAtomic<Int16> @this, out Int16 previousValue) => @this.Set(i => ++i, out previousValue);

		public static Int16 Decrement(this INonScalableAtomic<Int16> @this) => @this.Set(i => --i);
		public static Int16 Decrement(this INonScalableAtomic<Int16> @this, out Int16 previousValue) => @this.Set(i => --i, out previousValue);

		public static Int16 Add(this INonScalableAtomic<Int16> @this, Int16 operand) => @this.Set(i => (Int16) (i + operand));
		public static Int16 Add(this INonScalableAtomic<Int16> @this, Int16 operand, out Int16 previousValue) => @this.Set(i => (Int16) (i + operand), out previousValue);

		public static Int16 Subtract(this INonScalableAtomic<Int16> @this, Int16 operand) => @this.Set(i => (Int16) (i - operand));
		public static Int16 Subtract(this INonScalableAtomic<Int16> @this, Int16 operand, out Int16 previousValue) => @this.Set(i => (Int16) (i - operand), out previousValue);

		public static Int16 Multiply(this INonScalableAtomic<Int16> @this, Int16 operand) => @this.Set(i => (Int16) (i * operand));
		public static Int16 Multiply(this INonScalableAtomic<Int16> @this, Int16 operand, out Int16 previousValue) => @this.Set(i => (Int16) (i * operand), out previousValue);

		public static Int16 Divide(this INonScalableAtomic<Int16> @this, Int16 operand) => @this.Set(i => (Int16) (i / operand));
		public static Int16 Divide(this INonScalableAtomic<Int16> @this, Int16 operand, out Int16 previousValue) => @this.Set(i => (Int16) (i / operand), out previousValue);
		#endregion

		#region UUInt16
		public static UInt16 Increment(this INonScalableAtomic<UInt16> @this) => @this.Set(i => ++i);
		public static UInt16 Increment(this INonScalableAtomic<UInt16> @this, out UInt16 previousValue) => @this.Set(i => ++i, out previousValue);

		public static UInt16 Decrement(this INonScalableAtomic<UInt16> @this) => @this.Set(i => --i);
		public static UInt16 Decrement(this INonScalableAtomic<UInt16> @this, out UInt16 previousValue) => @this.Set(i => --i, out previousValue);

		public static UInt16 Add(this INonScalableAtomic<UInt16> @this, UInt16 operand) => @this.Set(i => (UInt16) (i + operand));
		public static UInt16 Add(this INonScalableAtomic<UInt16> @this, UInt16 operand, out UInt16 previousValue) => @this.Set(i => (UInt16) (i + operand), out previousValue);

		public static UInt16 Subtract(this INonScalableAtomic<UInt16> @this, UInt16 operand) => @this.Set(i => (UInt16) (i - operand));
		public static UInt16 Subtract(this INonScalableAtomic<UInt16> @this, UInt16 operand, out UInt16 previousValue) => @this.Set(i => (UInt16) (i - operand), out previousValue);

		public static UInt16 Multiply(this INonScalableAtomic<UInt16> @this, UInt16 operand) => @this.Set(i => (UInt16) (i * operand));
		public static UInt16 Multiply(this INonScalableAtomic<UInt16> @this, UInt16 operand, out UInt16 previousValue) => @this.Set(i => (UInt16) (i * operand), out previousValue);

		public static UInt16 Divide(this INonScalableAtomic<UInt16> @this, UInt16 operand) => @this.Set(i => (UInt16) (i / operand));
		public static UInt16 Divide(this INonScalableAtomic<UInt16> @this, UInt16 operand, out UInt16 previousValue) => @this.Set(i => (UInt16) (i / operand), out previousValue);
		#endregion

		#region Int32
		public static Int32 Increment(this INonScalableAtomic<Int32> @this) => @this.Set(i => ++i);
		public static Int32 Increment(this INonScalableAtomic<Int32> @this, out Int32 previousValue) => @this.Set(i => ++i, out previousValue);

		public static Int32 Decrement(this INonScalableAtomic<Int32> @this) => @this.Set(i => --i);
		public static Int32 Decrement(this INonScalableAtomic<Int32> @this, out Int32 previousValue) => @this.Set(i => --i, out previousValue);

		public static Int32 Add(this INonScalableAtomic<Int32> @this, Int32 operand) => @this.Set(i => i + operand);
		public static Int32 Add(this INonScalableAtomic<Int32> @this, Int32 operand, out Int32 previousValue) => @this.Set(i => i + operand, out previousValue);

		public static Int32 Subtract(this INonScalableAtomic<Int32> @this, Int32 operand) => @this.Set(i => i - operand);
		public static Int32 Subtract(this INonScalableAtomic<Int32> @this, Int32 operand, out Int32 previousValue) => @this.Set(i => i - operand, out previousValue);

		public static Int32 Multiply(this INonScalableAtomic<Int32> @this, Int32 operand) => @this.Set(i => i * operand);
		public static Int32 Multiply(this INonScalableAtomic<Int32> @this, Int32 operand, out Int32 previousValue) => @this.Set(i => i * operand, out previousValue);

		public static Int32 Divide(this INonScalableAtomic<Int32> @this, Int32 operand) => @this.Set(i => i / operand);
		public static Int32 Divide(this INonScalableAtomic<Int32> @this, Int32 operand, out Int32 previousValue) => @this.Set(i => i / operand, out previousValue);
		#endregion

		#region UUInt32
		public static UInt32 Increment(this INonScalableAtomic<UInt32> @this) => @this.Set(i => ++i);
		public static UInt32 Increment(this INonScalableAtomic<UInt32> @this, out UInt32 previousValue) => @this.Set(i => ++i, out previousValue);

		public static UInt32 Decrement(this INonScalableAtomic<UInt32> @this) => @this.Set(i => --i);
		public static UInt32 Decrement(this INonScalableAtomic<UInt32> @this, out UInt32 previousValue) => @this.Set(i => --i, out previousValue);

		public static UInt32 Add(this INonScalableAtomic<UInt32> @this, UInt32 operand) => @this.Set(i => i + operand);
		public static UInt32 Add(this INonScalableAtomic<UInt32> @this, UInt32 operand, out UInt32 previousValue) => @this.Set(i => i + operand, out previousValue);

		public static UInt32 Subtract(this INonScalableAtomic<UInt32> @this, UInt32 operand) => @this.Set(i => i - operand);
		public static UInt32 Subtract(this INonScalableAtomic<UInt32> @this, UInt32 operand, out UInt32 previousValue) => @this.Set(i => i - operand, out previousValue);

		public static UInt32 Multiply(this INonScalableAtomic<UInt32> @this, UInt32 operand) => @this.Set(i => i * operand);
		public static UInt32 Multiply(this INonScalableAtomic<UInt32> @this, UInt32 operand, out UInt32 previousValue) => @this.Set(i => i * operand, out previousValue);

		public static UInt32 Divide(this INonScalableAtomic<UInt32> @this, UInt32 operand) => @this.Set(i => i / operand);
		public static UInt32 Divide(this INonScalableAtomic<UInt32> @this, UInt32 operand, out UInt32 previousValue) => @this.Set(i => i / operand, out previousValue);
		#endregion

		#region Int64
		public static Int64 Increment(this INonScalableAtomic<Int64> @this) => @this.Set(i => ++i);
		public static Int64 Increment(this INonScalableAtomic<Int64> @this, out Int64 previousValue) => @this.Set(i => ++i, out previousValue);

		public static Int64 Decrement(this INonScalableAtomic<Int64> @this) => @this.Set(i => --i);
		public static Int64 Decrement(this INonScalableAtomic<Int64> @this, out Int64 previousValue) => @this.Set(i => --i, out previousValue);

		public static Int64 Add(this INonScalableAtomic<Int64> @this, Int64 operand) => @this.Set(i => i + operand);
		public static Int64 Add(this INonScalableAtomic<Int64> @this, Int64 operand, out Int64 previousValue) => @this.Set(i => i + operand, out previousValue);

		public static Int64 Subtract(this INonScalableAtomic<Int64> @this, Int64 operand) => @this.Set(i => i - operand);
		public static Int64 Subtract(this INonScalableAtomic<Int64> @this, Int64 operand, out Int64 previousValue) => @this.Set(i => i - operand, out previousValue);

		public static Int64 Multiply(this INonScalableAtomic<Int64> @this, Int64 operand) => @this.Set(i => i * operand);
		public static Int64 Multiply(this INonScalableAtomic<Int64> @this, Int64 operand, out Int64 previousValue) => @this.Set(i => i * operand, out previousValue);

		public static Int64 Divide(this INonScalableAtomic<Int64> @this, Int64 operand) => @this.Set(i => i / operand);
		public static Int64 Divide(this INonScalableAtomic<Int64> @this, Int64 operand, out Int64 previousValue) => @this.Set(i => i / operand, out previousValue);
		#endregion

		#region UUInt64
		public static UInt64 Increment(this INonScalableAtomic<UInt64> @this) => @this.Set(i => ++i);
		public static UInt64 Increment(this INonScalableAtomic<UInt64> @this, out UInt64 previousValue) => @this.Set(i => ++i, out previousValue);

		public static UInt64 Decrement(this INonScalableAtomic<UInt64> @this) => @this.Set(i => --i);
		public static UInt64 Decrement(this INonScalableAtomic<UInt64> @this, out UInt64 previousValue) => @this.Set(i => --i, out previousValue);

		public static UInt64 Add(this INonScalableAtomic<UInt64> @this, UInt64 operand) => @this.Set(i => i + operand);
		public static UInt64 Add(this INonScalableAtomic<UInt64> @this, UInt64 operand, out UInt64 previousValue) => @this.Set(i => i + operand, out previousValue);

		public static UInt64 Subtract(this INonScalableAtomic<UInt64> @this, UInt64 operand) => @this.Set(i => i - operand);
		public static UInt64 Subtract(this INonScalableAtomic<UInt64> @this, UInt64 operand, out UInt64 previousValue) => @this.Set(i => i - operand, out previousValue);

		public static UInt64 Multiply(this INonScalableAtomic<UInt64> @this, UInt64 operand) => @this.Set(i => i * operand);
		public static UInt64 Multiply(this INonScalableAtomic<UInt64> @this, UInt64 operand, out UInt64 previousValue) => @this.Set(i => i * operand, out previousValue);

		public static UInt64 Divide(this INonScalableAtomic<UInt64> @this, UInt64 operand) => @this.Set(i => i / operand);
		public static UInt64 Divide(this INonScalableAtomic<UInt64> @this, UInt64 operand, out UInt64 previousValue) => @this.Set(i => i / operand, out previousValue);
		#endregion
	}
}