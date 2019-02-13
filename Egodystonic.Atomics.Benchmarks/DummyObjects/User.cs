// (c) Egodystonic Studios 2018


using System;

namespace Egodystonic.Atomics.Benchmarks.DummyObjects {
	public sealed class User {
		public int LoginID { get; }
		public string Name { get; }

		public User(int loginID, string name) {
			LoginID = loginID;
			Name = name;
		}
	}
}