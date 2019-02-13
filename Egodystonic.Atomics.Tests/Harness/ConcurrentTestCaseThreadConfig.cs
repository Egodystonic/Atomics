// (c) Egodystonic Studios 2018


using System;

namespace Egodystonic.Atomics.Tests.Harness {
	struct ConcurrentTestCaseThreadConfig : IEquatable<ConcurrentTestCaseThreadConfig> {
		public readonly int WriterThreadCount;
		public readonly int ReaderThreadCount;

		public int TotalThreadCount => WriterThreadCount + ReaderThreadCount;

		public ConcurrentTestCaseThreadConfig(int writerThreadCount, int readerThreadCount) {
			WriterThreadCount = writerThreadCount;
			ReaderThreadCount = readerThreadCount;
		}

		public int ThreadCount(ThreadType threadType) => threadType == ThreadType.WriterThread ? WriterThreadCount : ReaderThreadCount;

		public bool Equals(ConcurrentTestCaseThreadConfig other) {
			return WriterThreadCount == other.WriterThreadCount && ReaderThreadCount == other.ReaderThreadCount;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			return obj is ConcurrentTestCaseThreadConfig other && Equals(other);
		}

		public override int GetHashCode() {
			unchecked {
				return (WriterThreadCount * 397) ^ ReaderThreadCount;
			}
		}

		public static bool operator ==(ConcurrentTestCaseThreadConfig left, ConcurrentTestCaseThreadConfig right) { return left.Equals(right); }
		public static bool operator !=(ConcurrentTestCaseThreadConfig left, ConcurrentTestCaseThreadConfig right) { return !left.Equals(right); }
	}
}