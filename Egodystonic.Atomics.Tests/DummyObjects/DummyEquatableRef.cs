// (c) Egodystonic Studios 2018


using System;

namespace Egodystonic.Atomics.Tests.DummyObjects {
	class DummyEquatableRef : IEquatable<DummyEquatableRef> {
		public string StringProp { get; }
		public long LongProp { get; }
		public DummyEquatableRef RefProp { get; }

		public DummyEquatableRef() { }

		public DummyEquatableRef(string stringProp) { StringProp = stringProp; }

		public DummyEquatableRef(long longProp) { LongProp = longProp; }

		public DummyEquatableRef(DummyEquatableRef refProp) { RefProp = refProp; }

		public DummyEquatableRef(string stringProp, long longProp) {
			StringProp = stringProp;
			LongProp = longProp;
		}

		public DummyEquatableRef(string stringProp, DummyEquatableRef refProp) {
			StringProp = stringProp;
			RefProp = refProp;
		}

		public DummyEquatableRef(long longProp, DummyEquatableRef refProp) {
			LongProp = longProp;
			RefProp = refProp;
		}

		public DummyEquatableRef(string stringProp, long longProp, DummyEquatableRef refProp) {
			StringProp = stringProp;
			LongProp = longProp;
			RefProp = refProp;
		}

		public override string ToString() => $"{LongProp} / \"{StringProp}\" / {(RefProp != null ? "Ref is set" : "Ref is null")}";

		public bool Equals(DummyEquatableRef other) {
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(StringProp, other.StringProp) && LongProp == other.LongProp && Equals(RefProp, other.RefProp);
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((DummyEquatableRef) obj);
		}

		public override int GetHashCode() {
			unchecked {
				var hashCode = (StringProp != null ? StringProp.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ LongProp.GetHashCode();
				hashCode = (hashCode * 397) ^ (RefProp != null ? RefProp.GetHashCode() : 0);
				return hashCode;
			}
		}

		public static bool operator ==(DummyEquatableRef left, DummyEquatableRef right) { return Equals(left, right); }
		public static bool operator !=(DummyEquatableRef left, DummyEquatableRef right) { return !Equals(left, right); }
	}
}