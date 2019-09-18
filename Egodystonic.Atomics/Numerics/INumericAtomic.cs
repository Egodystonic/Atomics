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
	// previous value is not always possible for the caller of this API. I'm not
	// too bothered about offering overloads that provide a single return value
	// because the multiply/divide op is probably going to shadow the performance
	// loss from return 2 values anyway. And if they're desperate they can use
	// GetRefUnsafe and do it themselves.
	public interface IScalableNumericAtomic<T> : IAtomic<T> {
		T IncrementAndGet();
		T DecrementAndGet();
		T AddAndGet(T operand);
		T SubtractAndGet(T operand);

		ExchangeResult<T> Multiply(T operand);
		ExchangeResult<T> Divide(T operand);
	}

	// There is a tradeoff here. There may be call for GetAndXYZ variants of these
	// for extreme performance scenarios at some point. But I think this API
	// works best in terms of consistency with how IScalableNumericAtomic<T> is
	// laid out and still giving an option of a fast path and a path that doesn't
	// lose any information.
	public interface IScalableIntegerAtomic<T> : IScalableNumericAtomic<T> {
		T BitwiseAndAndGet(T operand);
		ExchangeResult<T> BitwiseAnd(T operand);

		T BitwiseOrAndGet(T operand);
		ExchangeResult<T> BitwiseOr(T operand);

		T BitwiseExclusiveOrAndGet(T operand);
		ExchangeResult<T> BitwiseExclusiveOr(T operand);

		T BitwiseNegateAndGet();
		ExchangeResult<T> BitwiseNegate();

		T BitwiseLeftShiftAndGet(int operand);
		ExchangeResult<T> BitwiseLeftShift(int operand);

		T BitwiseRightShiftAndGet(int operand);
		ExchangeResult<T> BitwiseRightShift(int operand);
	}

	public interface INonScalableFloatingPointAtomic<T> : IScalableNumericAtomic<T> {
		T TrySwap(T newValue, T comparand, T maxDelta);
	}
}