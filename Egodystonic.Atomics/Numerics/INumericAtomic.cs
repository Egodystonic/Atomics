﻿// Created on 2018-09-19 by Ben Bowen
// (c) Amber Kinetics 2018


using System;
using System.Collections.Generic;
using System.Linq;

namespace Egodystonic.Atomics.Numerics {
	public interface INumericAtomic<T> : IAtomic<T> {
		T SpinWaitForBoundedValue(T lowerBound, T upperBound);

		(T PreviousValue, T NewValue) SpinWaitForBoundedExchange(T newValue, T lowerBound, T upperBound);
		(T PreviousValue, T NewValue) SpinWaitForBoundedExchange(Func<T, T> mapFunc, T lowerBound, T upperBound);
		(T PreviousValue, T NewValue) SpinWaitForBoundedExchange<TContext>(Func<T, TContext, T> mapFunc, T lowerBound, T upperBound, TContext context);

		(T PreviousValue, T NewValue) Increment();
		(T PreviousValue, T NewValue) Decrement();

		(T PreviousValue, T NewValue) Add(T operand);
		(T PreviousValue, T NewValue) Subtract(T operand);
		(T PreviousValue, T NewValue) MultiplyBy(T operand);
		(T PreviousValue, T NewValue) DivideBy(T operand);
	}

	public interface IFloatingPointAtomic<T> : INumericAtomic<T> {
		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchangeWithMaxDelta(T newValue, T comparand, T maxDelta);
		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchangeWithMaxDelta(Func<T, T> mapFunc, T comparand, T maxDelta);
		(bool ValueWasSet, T PreviousValue, T NewValue) TryExchangeWithMaxDelta<TContext>(Func<T, TContext, T> mapFunc, T comparand, T maxDelta, TContext context);
	}
}