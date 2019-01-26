// Created on 2018-09-19 by Ben Bowen
// (c) Amber Kinetics 2018


using System;
using System.Collections.Generic;
using System.Linq;

namespace Egodystonic.Atomics.Numerics {
	public interface INumericAtomic<T> : IAtomic<T> {
		T SpinWaitForMinimumValue(T minValue);
		T SpinWaitForMaximumValue(T maxValue);
		T SpinWaitForBoundedValue(T lowerBoundInclusive, T upperBoundExclusive);

		(T PreviousValue, T CurrentValue) SpinWaitForMinimumExchange(T CurrentValue, T minValue);
		(T PreviousValue, T CurrentValue) SpinWaitForMinimumExchange(Func<T, T> mapFunc, T minValue);
		(T PreviousValue, T CurrentValue) SpinWaitForMinimumExchange<TContext>(Func<T, TContext, T> mapFunc, T minValue, TContext context);
		(T PreviousValue, T CurrentValue) SpinWaitForMaximumExchange(T CurrentValue, T maxValue);
		(T PreviousValue, T CurrentValue) SpinWaitForMaximumExchange(Func<T, T> mapFunc, T maxValue);
		(T PreviousValue, T CurrentValue) SpinWaitForMaximumExchange<TContext>(Func<T, TContext, T> mapFunc, T maxValue, TContext context);
		(T PreviousValue, T CurrentValue) SpinWaitForBoundedExchange(T CurrentValue, T lowerBoundInclusive, T upperBoundExclusive);
		(T PreviousValue, T CurrentValue) SpinWaitForBoundedExchange(Func<T, T> mapFunc, T lowerBoundInclusive, T upperBoundExclusive);
		(T PreviousValue, T CurrentValue) SpinWaitForBoundedExchange<TContext>(Func<T, TContext, T> mapFunc, T lowerBoundInclusive, T upperBoundExclusive, TContext context);

		(bool ValueWasSet, T PreviousValue, T CurrentValue) TryMinimumExchange(T CurrentValue, T minValue);
		(bool ValueWasSet, T PreviousValue, T CurrentValue) TryMinimumExchange(Func<T, T> mapFunc, T minValue);
		(bool ValueWasSet, T PreviousValue, T CurrentValue) TryMinimumExchange<TContext>(Func<T, TContext, T> mapFunc, T minValue, TContext context);
		(bool ValueWasSet, T PreviousValue, T CurrentValue) TryMaximumExchange(T CurrentValue, T maxValue);
		(bool ValueWasSet, T PreviousValue, T CurrentValue) TryMaximumExchange(Func<T, T> mapFunc, T maxValue);
		(bool ValueWasSet, T PreviousValue, T CurrentValue) TryMaximumExchange<TContext>(Func<T, TContext, T> mapFunc, T maxValue, TContext context);
		(bool ValueWasSet, T PreviousValue, T CurrentValue) TryBoundedExchange(T CurrentValue, T lowerBoundInclusive, T upperBoundExclusive);
		(bool ValueWasSet, T PreviousValue, T CurrentValue) TryBoundedExchange(Func<T, T> mapFunc, T lowerBoundInclusive, T upperBoundExclusive);
		(bool ValueWasSet, T PreviousValue, T CurrentValue) TryBoundedExchange<TContext>(Func<T, TContext, T> mapFunc, T lowerBoundInclusive, T upperBoundExclusive, TContext context);

		(T PreviousValue, T CurrentValue) Increment();
		(T PreviousValue, T CurrentValue) Decrement();

		(T PreviousValue, T CurrentValue) Add(T operand);
		(T PreviousValue, T CurrentValue) Subtract(T operand);
		(T PreviousValue, T CurrentValue) MultiplyBy(T operand);
		(T PreviousValue, T CurrentValue) DivideBy(T operand);
	}

	public interface IFloatingPointAtomic<T> : INumericAtomic<T> {
		T SpinWaitForValueWithMaxDelta(T targetValue, T maxDelta);

		(T PreviousValue, T CurrentValue) SpinWaitForExchangeWithMaxDelta(T CurrentValue, T comparand, T maxDelta);
		(T PreviousValue, T CurrentValue) SpinWaitForExchangeWithMaxDelta(Func<T, T> mapFunc, T comparand, T maxDelta);
		(T PreviousValue, T CurrentValue) SpinWaitForExchangeWithMaxDelta<TContext>(Func<T, TContext, T> mapFunc, T comparand, T maxDelta, TContext context);

		(bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchangeWithMaxDelta(T CurrentValue, T comparand, T maxDelta);
		(bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchangeWithMaxDelta(Func<T, T> mapFunc, T comparand, T maxDelta);
		(bool ValueWasSet, T PreviousValue, T CurrentValue) TryExchangeWithMaxDelta<TContext>(Func<T, TContext, T> mapFunc, T comparand, T maxDelta, TContext context);
	}
}