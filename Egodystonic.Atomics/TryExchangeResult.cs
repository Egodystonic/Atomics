// (c) Egodystonic Studios 2019
// Author: Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;

namespace Egodystonic.Atomics {
	public struct TryExchangeResult<T> : IEquatable<TryExchangeResult<T>> {
		public readonly bool ExchangeSuccess;
		public readonly T PreviousValue;
		public readonly T NewValue;

		public TryExchangeResult(bool exchangeSuccess, T previousValue, T newValue) {
			ExchangeSuccess = exchangeSuccess;
			PreviousValue = previousValue;
			NewValue = newValue;
		}

		public static TryExchangeResult<T> Success(T previousValue, T newValue) => new TryExchangeResult<T>(true, previousValue, newValue);

		public static TryExchangeResult<T> Failure(T persistingValue) => new TryExchangeResult<T>(false, persistingValue, persistingValue);

		public override string ToString() {
			if (!ExchangeSuccess) return $"{(PreviousValue != null ? PreviousValue.ToString() : "<null>")} (unchanged)";
			
			return $"{(PreviousValue != null ? PreviousValue.ToString() : "<null>")} => " +
				   $"{(NewValue != null ? NewValue.ToString() : "<null>")}";
		}

		public bool Equals(TryExchangeResult<T> other) {
			return ExchangeSuccess == other.ExchangeSuccess && EqualityComparer<T>.Default.Equals(PreviousValue, other.PreviousValue) && EqualityComparer<T>.Default.Equals(NewValue, other.NewValue);
		}

		public override bool Equals(object obj) {
			return obj is TryExchangeResult<T> other && Equals(other);
		}

		public override int GetHashCode() {
			unchecked {
				var hashCode = ExchangeSuccess.GetHashCode();
				hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(PreviousValue);
				hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(NewValue);
				return hashCode;
			}
		}

		public static bool operator ==(TryExchangeResult<T> left, TryExchangeResult<T> right) => left.Equals(right);
		public static bool operator !=(TryExchangeResult<T> left, TryExchangeResult<T> right) => !left.Equals(right);
	}
}