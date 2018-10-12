// (c) Egodystonic Studios 2018


using System;

namespace Egodystonic.Atomics.Tests.DummyObjects {
	class DummyImmutableRef {
		public string StringProp { get; }
		public long LongProp { get; }
		public DummyImmutableRef RefProp { get; }

		public DummyImmutableRef() { }

		public DummyImmutableRef(string stringProp) { StringProp = stringProp; }

		public DummyImmutableRef(long longProp) { LongProp = longProp; }

		public DummyImmutableRef(DummyImmutableRef refProp) { RefProp = refProp; }

		public DummyImmutableRef(string stringProp, long longProp) {
			StringProp = stringProp;
			LongProp = longProp;
		}

		public DummyImmutableRef(string stringProp, DummyImmutableRef refProp) {
			StringProp = stringProp;
			RefProp = refProp;
		}

		public DummyImmutableRef(long longProp, DummyImmutableRef refProp) {
			LongProp = longProp;
			RefProp = refProp;
		}

		public DummyImmutableRef(string stringProp, long longProp, DummyImmutableRef refProp) {
			StringProp = stringProp;
			LongProp = longProp;
			RefProp = refProp;
		}

		public override string ToString() => $"{LongProp} / \"{StringProp}\" / {(RefProp != null ? "Ref is set" : "Ref is null")}";
	}
}