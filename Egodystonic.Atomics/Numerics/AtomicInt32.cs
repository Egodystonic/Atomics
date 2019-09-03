// (c) Egodystonic Studios 2018
// Author: Ben Bowen
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Egodystonic.Atomics.Numerics {
	public sealed class AtomicInt32 : IIntegerAtomic<int>, IFormattable {
		int _value;

		public int Value {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Get();
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Set(value);
		}

		public AtomicInt32() : this(default) { }
		public AtomicInt32(int initialValue) => Set(initialValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Get() => Volatile.Read(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetUnsafe() => _value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(int newValue) => Volatile.Write(ref _value, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetUnsafe(int newValue) => _value = newValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int FastExchange(int newValue) => Interlocked.Exchange(ref _value, newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int PreviousValue, int CurrentValue) Exchange(int newValue) => (Interlocked.Exchange(ref _value, newValue), newValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int SpinWaitForValue(int targetValue) {
			var spinner = new SpinWait();
			while (Get() != targetValue) spinner.SpinOnce();
			return targetValue;
		}

		public (int PreviousValue, int CurrentValue) Exchange<TContext>(Func<int, TContext, int> mapFunc, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForExchange(int newValue, int comparand) {
			var spinner = new SpinWait();

			while (true) {
				if (Interlocked.CompareExchange(ref _value, newValue, comparand) == comparand) return (comparand, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForExchange<TContext>(Func<int, TContext, int> mapFunc, TContext context, int comparand) {
			var spinner = new SpinWait();
			var newValue = mapFunc(comparand, context); // curValue will always be comparand when this method returns

			while (true) {
				if (Interlocked.CompareExchange(ref _value, newValue, comparand) == comparand) return (comparand, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForExchange<TMapContext, TPredicateContext>(Func<int, TMapContext, int> mapFunc, TMapContext mapContext, Func<int, int, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = mapFunc(curValue, mapContext);
				if (!predicate(curValue, newValue, predicateContext)) {
					spinner.SpinOnce();
					continue;
				}

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int FastTryExchange(int newValue, int comparand) => Interlocked.CompareExchange(ref _value, newValue, comparand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (bool ValueWasSet, int PreviousValue, int CurrentValue) TryExchange(int newValue, int comparand) {
			var oldValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			var wasSet = oldValue == comparand;
			return (wasSet, oldValue, wasSet ? newValue : oldValue);
		}

		public (bool ValueWasSet, int PreviousValue, int CurrentValue) TryExchange<TContext>(Func<int, TContext, int> mapFunc, TContext context, int comparand) {
			var newValue = mapFunc(comparand, context); // Comparand will always be curValue if the interlocked call passes
			var prevValue = Interlocked.CompareExchange(ref _value, newValue, comparand);
			if (prevValue == comparand) return (true, prevValue, newValue);
			else return (false, prevValue, prevValue);
		}

		public (bool ValueWasSet, int PreviousValue, int CurrentValue) TryExchange<TMapContext, TPredicateContext>(Func<int, TMapContext, int> mapFunc, TMapContext mapContext, Func<int, int, TPredicateContext, bool> predicate, TPredicateContext predicateContext) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = mapFunc(curValue, mapContext);
				if (!predicate(curValue, newValue, predicateContext)) return (false, curValue, curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);

				spinner.SpinOnce();
			}
		}

		// ============================ Numeric API ============================

		public int SpinWaitForMinimumValue(int minValue) {
			var spinner = new SpinWait();
			while (true) {
				var curVal = Get();
				if (curVal >= minValue) return curVal;
				spinner.SpinOnce();
			}
		}

		public int SpinWaitForMaximumValue(int maxValue) {
			var spinner = new SpinWait();
			while (true) {
				var curVal = Get();
				if (curVal <= maxValue) return curVal;
				spinner.SpinOnce();
			}
		}

		public int SpinWaitForBoundedValue(int lowerBoundInclusive, int upperBoundExclusive) {
			var spinner = new SpinWait();
			while (true) {
				var curVal = Get();
				if (curVal >= lowerBoundInclusive && curVal < upperBoundExclusive) return curVal;
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForMinimumExchange(int newValue, int minValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) {
					spinner.SpinOnce();
					continue;
				}

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForMinimumExchange(Func<int, int> mapFunc, int minValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) {
					spinner.SpinOnce();
					continue;
				}

				var newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForMinimumExchange<TContext>(Func<int, TContext, int> mapFunc, int minValue, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) {
					spinner.SpinOnce();
					continue;
				}

				var newValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForMaximumExchange(int newValue, int maxValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) {
					spinner.SpinOnce();
					continue;
				}

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForMaximumExchange(Func<int, int> mapFunc, int maxValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) {
					spinner.SpinOnce();
					continue;
				}

				var newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForMaximumExchange<TContext>(Func<int, TContext, int> mapFunc, int maxValue, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) {
					spinner.SpinOnce();
					continue;
				}

				var newValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForBoundedExchange(int newValue, int lowerBoundInclusive, int upperBoundExclusive) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) {
					spinner.SpinOnce();
					continue;
				}

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForBoundedExchange(Func<int, int> mapFunc, int lowerBoundInclusive, int upperBoundExclusive) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) {
					spinner.SpinOnce();
					continue;
				}

				var newValue = mapFunc(curValue);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) SpinWaitForBoundedExchange<TContext>(Func<int, TContext, int> mapFunc, int lowerBoundInclusive, int upperBoundExclusive, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) {
					spinner.SpinOnce();
					continue;
				}

				var newValue = mapFunc(curValue, context);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int CurrentValue) TryMinimumExchange(int newValue, int minValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) return (false, curValue, curValue);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int CurrentValue) TryMinimumExchange(Func<int, int> mapFunc, int minValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) return (false, curValue, curValue);
				var newValue = mapFunc(curValue);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int CurrentValue) TryMinimumExchange<TContext>(Func<int, TContext, int> mapFunc, int minValue, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < minValue) return (false, curValue, curValue);
				var newValue = mapFunc(curValue, context);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int CurrentValue) TryMaximumExchange(int newValue, int maxValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) return (false, curValue, curValue);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int CurrentValue) TryMaximumExchange(Func<int, int> mapFunc, int maxValue) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) return (false, curValue, curValue);
				var newValue = mapFunc(curValue);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int CurrentValue) TryMaximumExchange<TContext>(Func<int, TContext, int> mapFunc, int maxValue, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue > maxValue) return (false, curValue, curValue);
				var newValue = mapFunc(curValue, context);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int CurrentValue) TryBoundedExchange(int newValue, int lowerBoundInclusive, int upperBoundExclusive) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) return (false, curValue, curValue);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int CurrentValue) TryBoundedExchange(Func<int, int> mapFunc, int lowerBoundInclusive, int upperBoundExclusive) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) return (false, curValue, curValue);
				var newValue = mapFunc(curValue);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (bool ValueWasSet, int PreviousValue, int CurrentValue) TryBoundedExchange<TContext>(Func<int, TContext, int> mapFunc, int lowerBoundInclusive, int upperBoundExclusive, TContext context) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				if (curValue < lowerBoundInclusive || curValue >= upperBoundExclusive) return (false, curValue, curValue);
				var newValue = mapFunc(curValue, context);
				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (true, curValue, newValue);
				spinner.SpinOnce();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int FastIncrement() => Interlocked.Increment(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int FastDecrement() => Interlocked.Decrement(ref _value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int FastAdd(int operand) => Interlocked.Add(ref _value, operand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int FastSubtract(int operand) => Interlocked.Add(ref _value, -operand);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int PreviousValue, int CurrentValue) Increment() {
			var newVal = Interlocked.Increment(ref _value);
			return (newVal - 1, newVal);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int PreviousValue, int CurrentValue) Decrement() {
			var newVal = Interlocked.Decrement(ref _value);
			return (newVal + 1, newVal);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int PreviousValue, int CurrentValue) Add(int operand) {
			var newVal = Interlocked.Add(ref _value, operand);
			return (newVal - operand, newVal);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public (int PreviousValue, int CurrentValue) Subtract(int operand) {
			var newVal = Interlocked.Add(ref _value, -operand);
			return (newVal + operand, newVal);
		}

		IScalableAtomic<int> GetSA() { }

		ref int Test() => ref _value;

		public (int PreviousValue, int CurrentValue) MultiplyBy(int operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = curValue * operand;

				var s = GetSA();
				ref var f = ref s.GetRefUnsafe();
				Interlocked.Exchange(ref f, 3);
				s.SetRefUnsafe(ref f);

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) DivideBy(int operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = curValue / operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		// ============================ Integer API ============================

		public (int PreviousValue, int CurrentValue) BitwiseAnd(int operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = curValue & operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) BitwiseOr(int operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = curValue | operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) BitwiseExclusiveOr(int operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = curValue ^ operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) BitwiseNegate() {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = ~curValue;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) BitwiseLeftShift(int operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = curValue << operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public (int PreviousValue, int CurrentValue) BitwiseRightShift(int operand) {
			var spinner = new SpinWait();

			while (true) {
				var curValue = Get();
				var newValue = curValue >> operand;

				if (Interlocked.CompareExchange(ref _value, newValue, curValue) == curValue) return (curValue, newValue);
				spinner.SpinOnce();
			}
		}

		public int FastBitwiseAnd(int operand) { return 0; }
		public int FastBitwiseOr(int operand) { return 0; }
		public int FastBitwiseExclusiveOr(int operand) { return 0; }
		public int FastBitwiseNegate() { return 0; }
		public int FastBitwiseLeftShift(int operand) { return 0; }
		public int FastBitwiseRightShift(int operand) { return 0; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator int(AtomicInt32 operand) => operand.Get();

		public override string ToString() => Get().ToString();
		public string ToString(IFormatProvider provider) => Get().ToString(provider);
		public string ToString(string format) => Get().ToString(format);
		public string ToString(string format, IFormatProvider provider) => Get().ToString(format, provider);
	}
}