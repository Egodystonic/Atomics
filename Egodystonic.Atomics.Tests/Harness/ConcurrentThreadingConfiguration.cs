using System;

namespace Egodystonic.Atomics.Tests.Harness {
	enum ConcurrentThreadingConfiguration {
		None = 0,
		SingleThread,
		SingleWriterSingleReader,
		SingleWriterMultipleReader,
		MultipleWriterSingleReader,
		MultipleWriterMultipleReader,
		FreeThreaded
	}
}