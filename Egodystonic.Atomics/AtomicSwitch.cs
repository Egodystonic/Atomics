// (c) Egodystonic Studios 2019
// Author: Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using IntBool = Egodystonic.Atomics.AtomicUtils.Union<int, bool>;

namespace Egodystonic.Atomics {
	public sealed class AtomicSwitch : IScalableAtomic<bool> {
		IntBool _value;

		public bool IsFlipped {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => VolatileRead();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => VolatileWrite(value);
		}

		bool IAtomic<bool>.Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => IsFlipped;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => IsFlipped = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AtomicSwitch() : this(default) { }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AtomicSwitch(bool isFlipped) => VolatileWrite(isFlipped);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool VolatileRead() => (IntBool) Volatile.Read(ref _value.AsTypeOne);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void VolatileWrite(bool newValue) => Volatile.Write(ref _value.AsTypeOne, (IntBool) newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool CompareExchange(bool newValue, bool comparand) => (IntBool) Interlocked.CompareExchange(ref _value.AsTypeOne, (IntBool) newValue, (IntBool) comparand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool Exchange(bool newValue) => (IntBool) Interlocked.Exchange(ref _value.AsTypeOne, (IntBool) newValue);

		// AtomicSwitch implementation
		public bool FlipAndGet() {
			var spinner = new SpinWait();
			while (true) {
				if (TryFlip(true)) return true;
				if (TryFlip(false)) return false;
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryFlip(bool desiredFlipState) => CompareExchange(desiredFlipState, !desiredFlipState);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool SetAndGetPreviousFlipState(bool newFlipState) => Exchange(newFlipState);

		// Interface implementation
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Get() => VolatileRead();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(bool isFlipped) => VolatileWrite(isFlipped);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool GetUnsafe() => _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(bool isFlipped) => _value = isFlipped;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref bool GetUnsafeRef() => ref _value.AsTypeTwo;

		// Note: Hidden because it's slightly confusing in this class. "Swap" clashes too strongly with "flip", but does something different.
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool IScalableAtomic<bool>.Swap(bool isFlipped) => Exchange(isFlipped);

		// Note: Hidden because it's confusing as fuck. The return value is the previously set value (like usual) but on this class in particular
		// it's easy to misunderstand and assume it's some kind of "was set successfully" return value.
		// Also... I'm not really sure what you'd ever want this for (watch someone complain now...).
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool IScalableAtomic<bool>.TrySwap(bool newValue, bool comparand) => CompareExchange(newValue, comparand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString() => IsFlipped ? "Switch (Flipped)" : "Switch (Not flipped)";

		#region Equality
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(bool other) => other == Get();

		public override bool Equals(object obj) {
			if (obj is bool value) return Equals(value);
			return ReferenceEquals(this, obj);
		}

		// ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode Base GetHashCode() is appropriate here.
		public override int GetHashCode() => base.GetHashCode();

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(AtomicSwitch left, bool right) => left?.Equals(right) ?? false;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(AtomicSwitch left, bool right) => !(left == right);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(bool left, AtomicSwitch right) => right?.Equals(left) ?? false;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(bool left, AtomicSwitch right) => !(right == left);
		#endregion
	}
}