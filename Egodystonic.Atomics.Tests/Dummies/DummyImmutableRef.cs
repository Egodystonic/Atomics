// (c) Egodystonic Studios 2018


using System;
using System.Collections.Generic;
using System.Linq;

namespace Egodystonic.Atomics.Tests.Dummies {
	class DummyImmutableRef {
		public string StringProp { get; }
		public int IntProp { get; }
		public DummyImmutableRef RefProp { get; }

		public DummyImmutableRef() { }

		public DummyImmutableRef(string stringProp) { StringProp = stringProp; }

		public DummyImmutableRef(int intProp) { IntProp = intProp; }

		public DummyImmutableRef(DummyImmutableRef refProp) { RefProp = refProp; }

		public DummyImmutableRef(string stringProp, int intProp) {
			StringProp = stringProp;
			IntProp = intProp;
		}

		public DummyImmutableRef(string stringProp, DummyImmutableRef refProp) {
			StringProp = stringProp;
			RefProp = refProp;
		}

		public DummyImmutableRef(int intProp, DummyImmutableRef refProp) {
			IntProp = intProp;
			RefProp = refProp;
		}

		public DummyImmutableRef(string stringProp, int intProp, DummyImmutableRef refProp) {
			StringProp = stringProp;
			IntProp = intProp;
			RefProp = refProp;
		}
	}
}