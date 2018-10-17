// Created on 2018-09-19 by Ben Bowen
// (c) Amber Kinetics 2018


using System;
using System.Collections.Generic;
using System.Linq;

namespace Egodystonic.Atomics.Numerics {
	public interface INumericAtomic<T> : IAtomic<T> {
		(T PreviousValue, T NewValue) Increment();
		(T PreviousValue, T NewValue) Decrement();

		(T PreviousValue, T NewValue) Add(T operand);
		(T PreviousValue, T NewValue) Subtract(T operand);
		(T PreviousValue, T NewValue) MultiplyBy(T operand);
		(T PreviousValue, T NewValue) DivideBy(T operand);
	}

	public interface IFloatingPointAtomic<T> : INumericAtomic<T> {
		void SpinWaitForValue(T targetValue, T maxDelta);

		(bool ValueWasSet, T PreviousValue) TryExchange(T newValue, T comparand, T maxDelta);
		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchange(Func<T, T> mapFunc, T comparand, T maxDelta);
//		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchange<TContext>(Func<T, TContext, T> mapFunc, T comparand, T maxDelta, TContext context);
	}
}