// (c) Egodystonic Studios 2018


using System;
using System.Collections.Generic;
using System.Linq;

namespace Egodystonic.Atomics.Benchmarks {
	public sealed class User {
		public int LoginID { get; }
		public string Name { get; }

		public User(int loginID, string name) {
			LoginID = loginID;
			Name = name;
		}
	}
}