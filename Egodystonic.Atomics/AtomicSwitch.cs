// (c) Egodystonic Studios 2019
// Author: Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Convertible = Egodystonic.Atomics.AtomicUtils.Union<int, bool>;

namespace Egodystonic.Atomics {
	public sealed class AtomicSwitch : IScalableAtomic<bool> {
		Convertible _value;

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
		bool VolatileRead() => (Convertible) Volatile.Read(ref _value.AsTypeOne);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void VolatileWrite(bool newValue) => Volatile.Write(ref _value.AsTypeOne, (Convertible) newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool CompareExchange(bool newValue, bool comparand) => (Convertible) Interlocked.CompareExchange(ref _value.AsTypeOne, (Convertible) newValue, (Convertible) comparand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool Exchange(bool newValue) => (Convertible) Interlocked.Exchange(ref _value.AsTypeOne, (Convertible) newValue);

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
		public override string ToString() => IsFlipped ? "Switch (Flipped)" : "Switch (Non-flipped)";

		#region Equality
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(AtomicSwitch other) => Equals((IAtomic<bool>) other);
		public bool Equals(IAtomic<bool> other) {
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.Value);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(bool other) => IsFlipped = other;

		public override bool Equals(object obj) {
			return ReferenceEquals(this, obj)
				|| obj is AtomicSwitch atomic && Equals(atomic)
				|| obj is IAtomic<bool> atomicInterface && Equals(atomicInterface)
				|| obj is bool value && Equals(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() => IsFlipped.GetHashCode();

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(AtomicSwitch left, AtomicSwitch right) => Equals(left, right);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(AtomicSwitch left, AtomicSwitch right) => !Equals(left, right);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(AtomicSwitch left, IAtomic<bool> right) => Equals(left, right);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(AtomicSwitch left, IAtomic<bool> right) => !Equals(left, right);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(IAtomic<bool> left, AtomicSwitch right) => Equals(left, right);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(IAtomic<bool> left, AtomicSwitch right) => !Equals(left, right);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(AtomicSwitch left, bool right) => left?.Equals(right) ?? false;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(AtomicSwitch left, bool right) => !(left?.Equals(right) ?? false);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(bool left, AtomicSwitch right) => right?.Equals(left) ?? false;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(bool left, AtomicSwitch right) => !(right?.Equals(left) ?? false);
		#endregion
	}
}