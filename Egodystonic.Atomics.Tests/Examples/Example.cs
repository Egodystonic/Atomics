// (c) Egodystonic Studios 2019
// Author: Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;
using Egodystonic.Atomics.Numerics;

namespace Egodystonic.Atomics.Tests.Examples {
	sealed class Example {
	// ReSharper disable once UseDeconstruction
	readonly OptimisticallyLockedValue<Matrix4x4> _atomicMatrix = new OptimisticallyLockedValue<Matrix4x4>();

	public void SetUpScene() {
		// Transpose the matrix if it isn't already the identity matrix; and return the previous value all as an atomic op
		var exchResult = _atomicMatrix.TryExchange(m => m.Transpose(), (current, next) => !current.IsIdentity);

		if (exchResult.ValueWasSet) {
			// If we transposed it, recalculate our scene positions using the previous value
			RecalculatePositions(exchResult.PreviousValue);
		}

		// Apply the frustum culling using the current value
		ApplyCameraFrustum(exchResult.CurrentValue);
	}

		public void RecalculatePositions(Matrix4x4 c) { }
		public void ApplyCameraFrustum(Matrix4x4 c) { }


	readonly LockFreeInt64 _index = new LockFreeInt64();

	public void ReduceIndex() {
		// Halve the current index value if it's at least 100 as an atomic op
		_index.TryMinimumExchange(i => i / 2, 100);
	}

	readonly LockFreeReference<User> _sessionUser = new LockFreeReference<User>(new User("Xenoprimate", 45067));

	public void ChangeUserName(string newUsername) {
		// Atomically swap the _sessionUser for a new User object with the new username and same ID; then print out the old name
		var previousName = _sessionUser.Exchange(u => new User(newUsername, u.ID)).PreviousValue.Name;

		Console.WriteLine($"Session user '{previousName}' has changed name to {newUsername}.");
	}

		readonly LockFreeReference<string> _currentBuffer = new LockFreeReference<string>();
	}

	class User {
		public string Name { get; set; }
		public int ID { get; set; }

		public User(string name, int id) {
			Name = name;
			ID = id;
		}
	}
}