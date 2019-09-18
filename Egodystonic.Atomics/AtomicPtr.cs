﻿// (c) Egodystonic Studios 2018
// Author: Ben Bowen
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Egodystonic.Atomics {
	public sealed unsafe class AtomicPtr<T> : IScalableAtomic<IntPtr> where T : unmanaged {
		[StructLayout(LayoutKind.Explicit)]
		struct PtrUnion {
			// Note: Don't use AsTypedPtr except for the unsafe methods. It's only necessary to allow creating an unsafe ref
			// to a variable of type T*. The memory barriers only sync up with each other when accessing the same variable
			// (from the point of view of the compiler). The CPU will see these two union vars as the same memory address but I'm not
			// sure that C# will. I'm also fairly sure that C# orders around memfences according to their emission in program order
			// and not the variable they access... But why risk it?
			[FieldOffset(0)]
			public T* AsTypedPtr; 
			[FieldOffset(0)]
			public IntPtr AsIntPtr;
		}
		PtrUnion _value;

		public T* Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => Set(value);
		}
		IntPtr IAtomic<IntPtr>.Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetAsIntPtr();
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => SetAsIntPtr(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public AtomicPtr() : this(default(T*)) { }
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public AtomicPtr(IntPtr initialValue) => SetAsIntPtr(initialValue);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public AtomicPtr(T* initialValue) => Set(initialValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)] IntPtr GetAsIntPtr() => Volatile.Read(ref _value.AsIntPtr);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] void SetAsIntPtr(IntPtr v) => Volatile.Write(ref _value.AsIntPtr, v);

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public T* Get() => (T*) GetAsIntPtr();
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public void Set(T* newValue) => SetAsIntPtr((IntPtr) newValue);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] IntPtr IAtomic<IntPtr>.Get() => GetAsIntPtr();
		[MethodImpl(MethodImplOptions.AggressiveInlining)] void IAtomic<IntPtr>.Set(IntPtr newValue) => SetAsIntPtr(newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public T* GetUnsafe() => _value.AsTypedPtr;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public void SetUnsafe(T* newValue) => _value.AsTypedPtr = newValue;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] IntPtr IScalableAtomic<IntPtr>.GetUnsafe() => _value.AsIntPtr;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] void IScalableAtomic<IntPtr>.SetUnsafe(IntPtr newValue) => _value.AsIntPtr = newValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public ref T* GetUnsafeRef() => ref _value.AsTypedPtr;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] ref IntPtr IScalableAtomic<IntPtr>.GetUnsafeRef() => ref _value.AsIntPtr;

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public T* Swap(T* newValue) => (T*) Interlocked.Exchange(ref _value.AsIntPtr, (IntPtr) newValue);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] IntPtr IScalableAtomic<IntPtr>.Swap(IntPtr newValue) => Interlocked.Exchange(ref _value.AsIntPtr, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public T* TrySwap(T* newValue, T* comparand) => (T*) Interlocked.CompareExchange(ref _value.AsIntPtr, (IntPtr) newValue, (IntPtr) comparand);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] IntPtr IScalableAtomic<IntPtr>.TrySwap(IntPtr newValue, IntPtr comparand) => Interlocked.CompareExchange(ref _value.AsIntPtr, newValue, comparand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(AtomicPtr<T> other) => Equals((IAtomic<IntPtr>) other);
		public bool Equals(IAtomic<IntPtr> other) {
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.Value);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(T* other) => Value == other;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(void* other) => Value == other;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(IntPtr other) => GetAsIntPtr() == other;

		public override bool Equals(object obj) {
			return ReferenceEquals(this, obj)
				|| obj is AtomicPtr<T> atomic && Equals(atomic)
				|| obj is IAtomic<IntPtr> atomicInterface && Equals(atomicInterface)
				|| obj is IntPtr value && Equals(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() => GetAsIntPtr().GetHashCode();

		public override string ToString() => GetAsIntPtr().ToString("x");
	}
}
