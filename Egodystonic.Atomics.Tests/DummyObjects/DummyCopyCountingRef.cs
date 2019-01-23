// (c) Egodystonic Studios 2018


using System;

namespace Egodystonic.Atomics.Tests.DummyObjects {
	class DummyCopyCountingRef : DummyEquatableRef, IEquatable<DummyCopyCountingRef> {
		public int InstanceIndex { get; }

		public DummyCopyCountingRef() { }
		public DummyCopyCountingRef(string stringProp) : base(stringProp) { }
		public DummyCopyCountingRef(long longProp) : base(longProp) { }
		public DummyCopyCountingRef(DummyEquatableRef refProp) : base(refProp) { }
		public DummyCopyCountingRef(string stringProp, long longProp) : base(stringProp, longProp) { }
		public DummyCopyCountingRef(string stringProp, DummyEquatableRef refProp) : base(stringProp, refProp) { }
		public DummyCopyCountingRef(long longProp, DummyEquatableRef refProp) : base(longProp, refProp) { }
		public DummyCopyCountingRef(string stringProp, long longProp, DummyEquatableRef refProp) : base(stringProp, longProp, refProp) { }
		public DummyCopyCountingRef(int instanceIndex) { InstanceIndex = instanceIndex; }
		public DummyCopyCountingRef(string stringProp, int instanceIndex) : base(stringProp) { InstanceIndex = instanceIndex; }
		public DummyCopyCountingRef(long longProp, int instanceIndex) : base(longProp) { InstanceIndex = instanceIndex; }
		public DummyCopyCountingRef(DummyEquatableRef refProp, int instanceIndex) : base(refProp) { InstanceIndex = instanceIndex; }
		public DummyCopyCountingRef(string stringProp, long longProp, int instanceIndex) : base(stringProp, longProp) { InstanceIndex = instanceIndex; }
		public DummyCopyCountingRef(string stringProp, DummyEquatableRef refProp, int instanceIndex) : base(stringProp, refProp) { InstanceIndex = instanceIndex; }
		public DummyCopyCountingRef(long longProp, DummyEquatableRef refProp, int instanceIndex) : base(longProp, refProp) { InstanceIndex = instanceIndex; }
		public DummyCopyCountingRef(string stringProp, long longProp, DummyEquatableRef refProp, int instanceIndex) : base(stringProp, longProp, refProp) { InstanceIndex = instanceIndex; }

		public DummyCopyCountingRef Copy() => new DummyCopyCountingRef(StringProp, LongProp, RefProp, InstanceIndex + 1);

		public bool Equals(DummyCopyCountingRef other) => Equals(other as DummyEquatableRef);

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((DummyCopyCountingRef) obj);
		}

		public override int GetHashCode() { return base.GetHashCode(); }
		public static bool operator ==(DummyCopyCountingRef left, DummyCopyCountingRef right) { return Equals(left, right); }
		public static bool operator !=(DummyCopyCountingRef left, DummyCopyCountingRef right) { return !Equals(left, right); }

		public override string ToString() => $"{base.ToString()} #{InstanceIndex}";
	}
}