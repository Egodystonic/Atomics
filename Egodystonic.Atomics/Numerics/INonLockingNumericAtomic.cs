// (c) Egodystonic Studios 2018
// Author: Ben Bowen
using System;

namespace Egodystonic.Atomics.Numerics {
	// Notes on this interface:
	// Increment, Decrement, Add and Subtract are all reversible operations
	// Therefore offering only the fast path (XAndGet) makes sense. If the user
	// needs the 'previous' value they can just reverse the operation manually
	// (which is all we'd be doing for them anyway, it's not like we have some
	// 'faster' API internally to do that).
	// On the other hand, multiply and divide can be irreversible (multiplication
	// that overflows and any division that has a remainder > 0). So knowing the
	// previous value is not always possible for the caller of this API, I'm not
	// too bothered about offering overloads that provide a single return value
	// because the multiply/divide op is probably going to overshadow the performance
	// loss from returning 2 values anyway. And if they're desperate they can use
	// GetUnsafeRef and do it themselves.
	public interface INonLockingNumericAtomic<T> : INonLockingAtomic<T> {
		T IncrementAndGet();
		T DecrementAndGet();
		T AddAndGet(T operand);
		T SubtractAndGet(T operand);

		ExchangeResult<T> Multiply(T operand);
		ExchangeResult<T> Divide(T operand);
	}

	// I did consider putting BitwiseXyzAndGet() overloads but the actual implementations for
	// these operations tend to be CAS loops with at least one conditional so the overhead of
	// returning a slightly larger type tends to be overshadowed anyway.
	public interface INonLockingIntegerAtomic<T> : INonLockingNumericAtomic<T> {
		ExchangeResult<T> BitwiseAnd(T operand);

		ExchangeResult<T> BitwiseOr(T operand);

		ExchangeResult<T> BitwiseExclusiveOr(T operand);

		ExchangeResult<T> BitwiseNegate();

		ExchangeResult<T> BitwiseLeftShift(int operand);

		ExchangeResult<T> BitwiseRightShift(int operand);
	}

	public interface INonLockingFloatingPointAtomic<T> : INonLockingNumericAtomic<T> {
		TryExchangeResult<T> TrySwap(T newValue, T comparand, T maxDelta);
	}
}