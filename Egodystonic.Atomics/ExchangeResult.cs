// (c) Egodystonic Studios 2019
// Author: Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;

namespace Egodystonic.Atomics {
	public struct ExchangeResult<T> : IEquatable<ExchangeResult<T>> {
		public readonly T PreviousValue;
		public readonly T NewValue;

		public ExchangeResult(T previousValue, T newValue) {
			PreviousValue = previousValue;
			NewValue = newValue;
		}

		public override string ToString() {
			return $"{(PreviousValue != null ? PreviousValue.ToString() : "<null>")} => " +
				   $"{(NewValue != null ? NewValue.ToString() : "<null>")}";
		}

		public bool Equals(ExchangeResult<T> other) {
			return EqualityComparer<T>.Default.Equals(PreviousValue, other.PreviousValue) && EqualityComparer<T>.Default.Equals(NewValue, other.NewValue);
		}

		public override bool Equals(object obj) {
			return obj is ExchangeResult<T> other && Equals(other);
		}

		public override int GetHashCode() {
			unchecked {
				return (EqualityComparer<T>.Default.GetHashCode(PreviousValue) * 397) ^ EqualityComparer<T>.Default.GetHashCode(NewValue);
			}
		}

		public static bool operator ==(ExchangeResult<T> left, ExchangeResult<T> right) => left.Equals(right);
		public static bool operator !=(ExchangeResult<T> left, ExchangeResult<T> right) => !left.Equals(right);
	}
}