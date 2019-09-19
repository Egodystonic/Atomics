// (c) Egodystonic Studios 2018

using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace Egodystonic.Atomics.Benchmarks.Internal {
	/// <summary>
	/// Benchmark used to determine the fastest way to copy to/from a long* (used by <see cref="LockFreeValue{T}"/>).
	/// </summary>
	[CoreJob, MemoryDiagnoser]
	public unsafe class ReadWriteLongPtrMethods {
		#region Parameters
		const int NumIterations = 100_000_000;
		#endregion

		#region Test Setup
		
		#endregion

		#region Benchmark: Buffer.MemoryCopy
		[Benchmark]
		public void WithBufferMemCopy() {
			long dest;
			var ptr = &dest;

			for (var i = 0; i < NumIterations; ++i) {
				WriteToLongWithBufferMemCopy(ptr, (byte) i);
				if (ReadFromLongWithBufferMemCopy<byte>(ptr) != (byte) i) throw new ApplicationException();

				WriteToLongWithBufferMemCopy(ptr, (short) i);
				if (ReadFromLongWithBufferMemCopy<short>(ptr) != (short) i) throw new ApplicationException();

				WriteToLongWithBufferMemCopy(ptr, 0 - i);
				if (ReadFromLongWithBufferMemCopy<int>(ptr) != 0 - i) throw new ApplicationException();

				WriteToLongWithBufferMemCopy(ptr, 0L - i);
				if (ReadFromLongWithBufferMemCopy<long>(ptr) != 0L - i) throw new ApplicationException();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void WriteToLongWithBufferMemCopy<T>(long* target, T val) where T : unmanaged => Buffer.MemoryCopy(&val, target, sizeof(long), sizeof(T));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static T ReadFromLongWithBufferMemCopy<T>(long* src) where T : unmanaged {
			T result;
			Buffer.MemoryCopy(src, &result, sizeof(T), sizeof(T));
			return result;
		}
		#endregion

		#region Benchmark: Byte-by-Byte Loop
		[Benchmark]
		public void WithByteLoopCopy() {
			long dest;
			var ptr = &dest;

			for (var i = 0; i < NumIterations; ++i) {
				WriteToLongWithByteLoopCopy(ptr, (byte) i);
				if (ReadFromLongWithByteLoopCopy<byte>(ptr) != (byte) i) throw new ApplicationException();

				WriteToLongWithByteLoopCopy(ptr, (short) i);
				if (ReadFromLongWithByteLoopCopy<short>(ptr) != (short) i) throw new ApplicationException();

				WriteToLongWithByteLoopCopy(ptr, 0 - i);
				if (ReadFromLongWithByteLoopCopy<int>(ptr) != 0 - i) throw new ApplicationException();

				WriteToLongWithByteLoopCopy(ptr, 0L - i);
				if (ReadFromLongWithByteLoopCopy<long>(ptr) != 0L - i) throw new ApplicationException();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void WriteToLongWithByteLoopCopy<T>(long* target, T val) where T : unmanaged {
			var valRef = (byte*) &val;
			for (var i = 0; i < sizeof(T); ++i) {
				((byte*) target)[i] = valRef[i];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static T ReadFromLongWithByteLoopCopy<T>(long* src) where T : unmanaged {
			T result;
			var resRef = (byte*) &result;
			for (var i = 0; i < sizeof(T); ++i) {
				resRef[i] = ((byte*) src)[i];
			}
			return result;
		}
		#endregion

		#region Benchmark: Span<T>
		[Benchmark]
		public void WithSpan() {
			long dest;
			var ptr = &dest;

			for (var i = 0; i < NumIterations; ++i) {
				WriteToLongWithSpan(ptr, (byte) i);
				if (ReadFromLongWithSpan<byte>(ptr) != (byte) i) throw new ApplicationException();

				WriteToLongWithSpan(ptr, (short) i);
				if (ReadFromLongWithSpan<short>(ptr) != (short) i) throw new ApplicationException();

				WriteToLongWithSpan(ptr, 0 - i);
				if (ReadFromLongWithSpan<int>(ptr) != 0 - i) throw new ApplicationException();

				WriteToLongWithSpan(ptr, 0L - i);
				if (ReadFromLongWithSpan<long>(ptr) != 0L - i) throw new ApplicationException();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void WriteToLongWithSpan<T>(long* target, T val) where T : unmanaged {
			var valRef = &val;
			var srcSpan = new Span<byte>(valRef, sizeof(T));
			var dstSpan = new Span<byte>(target, sizeof(T));
			srcSpan.CopyTo(dstSpan);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static T ReadFromLongWithSpan<T>(long* src) where T : unmanaged {
			T result;
			var resRef = &result;
			var srcSpan = new Span<byte>(src, sizeof(T));
			var dstSpan = new Span<byte>(resRef, sizeof(T));
			srcSpan.CopyTo(dstSpan);
			return result;
		}
		#endregion

		#region Benchmark: Pointer Copy
		[Benchmark(Baseline = true)]
		public void WithPointerCopy() {
			long dest;
			var ptr = &dest;

			for (var i = 0; i < NumIterations; ++i) {
				WriteToLongWithPointerCopy(ptr, (byte) i);
				if (ReadFromLongWithPointerCopy<byte>(ptr) != (byte) i) throw new ApplicationException();

				WriteToLongWithPointerCopy(ptr, (short) i);
				if (ReadFromLongWithPointerCopy<short>(ptr) != (short) i) throw new ApplicationException();

				WriteToLongWithPointerCopy(ptr, 0 - i);
				if (ReadFromLongWithPointerCopy<int>(ptr) != 0 - i) throw new ApplicationException();

				WriteToLongWithPointerCopy(ptr, 0L - i);
				if (ReadFromLongWithPointerCopy<long>(ptr) != 0L - i) throw new ApplicationException();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void WriteToLongWithPointerCopy<T>(long* target, T val) where T : unmanaged {
			*((T*) target) = val;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static T ReadFromLongWithPointerCopy<T>(long* src) where T : unmanaged {
			return *((T*) src);
		}
		#endregion

		#region Benchmark: Specialized Copy Logic
		[Benchmark]
		public void WithSpecializedLogic() {
			long dest;
			var ptr = &dest;

			for (var i = 0; i < NumIterations; ++i) {
				WriteToLongWithSpecializedLogic(ptr, (byte) i);
				if (ReadFromLongWithSpecializedLogic<byte>(ptr) != (byte) i) throw new ApplicationException();

				WriteToLongWithSpecializedLogic(ptr, (short) i);
				if (ReadFromLongWithSpecializedLogic<short>(ptr) != (short) i) throw new ApplicationException();

				WriteToLongWithSpecializedLogic(ptr, 0 - i);
				if (ReadFromLongWithSpecializedLogic<int>(ptr) != 0 - i) throw new ApplicationException();

				WriteToLongWithSpecializedLogic(ptr, 0L - i);
				if (ReadFromLongWithSpecializedLogic<long>(ptr) != 0L - i) throw new ApplicationException();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static void WriteToLongWithSpecializedLogic<T>(long* target, T val) where T : unmanaged {
			SpecializedLogicCopy<T>((byte*) &val, (byte*) target);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static T ReadFromLongWithSpecializedLogic<T>(long* src) where T : unmanaged {
			T result;
			SpecializedLogicCopy<T>((byte*) src, (byte*) &result);
			return result;
		}

		static void SpecializedLogicCopy<T>(byte* src, byte* dest) where T : unmanaged {
			var len = sizeof(T);
			var srcEnd = src + len;
			var destEnd = dest + len;

			if (len != 8) goto BRANCH1;
			*(long*) dest = *(long*) src;
			*(long*) (destEnd - 8) = *(long*) (srcEnd - 8);
			return;

			BRANCH1:
			if ((len & 4) == 0) goto BRANCH2;
			*(int*) dest = *(int*) src;
			*(int*) (destEnd - 4) = *(int*) (srcEnd - 4);
			return;


			BRANCH2:
			*dest = *src;
			if ((len & 2) == 0) return;
			*(short*) (destEnd - 2) = *(short*) (srcEnd - 2);
			return;
		}
		#endregion
	}
}