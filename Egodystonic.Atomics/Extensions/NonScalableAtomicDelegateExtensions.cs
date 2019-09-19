// (c) Egodystonic Studios 2019
// Author: Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;

namespace Egodystonic.Atomics {
	public static class NonScalableAtomicDelegateExtensions {
		#region Action
		public static bool TryInvoke(this INonScalableAtomic<Action> @this) {
			var valueLocal = @this.Value;
			if (valueLocal == null) return false;
			valueLocal();
			return true;
		}

		public static bool TryInvoke<T1>(this INonScalableAtomic<Action<T1>> @this, T1 arg1) {
			var valueLocal = @this.Value;
			if (valueLocal == null) return false;
			valueLocal(arg1);
			return true;
		}

		public static bool TryInvoke<T1, T2>(this INonScalableAtomic<Action<T1, T2>> @this, T1 arg1, T2 arg2) {
			var valueLocal = @this.Value;
			if (valueLocal == null) return false;
			valueLocal(arg1, arg2);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3>(this INonScalableAtomic<Action<T1, T2, T3>> @this, T1 arg1, T2 arg2, T3 arg3) {
			var valueLocal = @this.Value;
			if (valueLocal is null) return false;
			valueLocal(arg1, arg2, arg3);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4>(this INonScalableAtomic<Action<T1, T2, T3, T4>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
			var valueLocal = @this.Value;
			if (valueLocal is null) return false;
			valueLocal(arg1, arg2, arg3, arg4);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5>(this INonScalableAtomic<Action<T1, T2, T3, T4, T5>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
			var valueLocal = @this.Value;
			if (valueLocal is null) return false;
			valueLocal(arg1, arg2, arg3, arg4, arg5);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6>(this INonScalableAtomic<Action<T1, T2, T3, T4, T5, T6>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) {
			var valueLocal = @this.Value;
			if (valueLocal is null) return false;
			valueLocal(arg1, arg2, arg3, arg4, arg5, arg6);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7>(this INonScalableAtomic<Action<T1, T2, T3, T4, T5, T6, T7>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) {
			var valueLocal = @this.Value;
			if (valueLocal is null) return false;
			valueLocal(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8>(this INonScalableAtomic<Action<T1, T2, T3, T4, T5, T6, T7, T8>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) {
			var valueLocal = @this.Value;
			if (valueLocal is null) return false;
			valueLocal(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this INonScalableAtomic<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9) {
			var valueLocal = @this.Value;
			if (valueLocal is null) return false;
			valueLocal(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this INonScalableAtomic<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10) {
			var valueLocal = @this.Value;
			if (valueLocal is null) return false;
			valueLocal(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this INonScalableAtomic<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11) {
			var valueLocal = @this.Value;
			if (valueLocal is null) return false;
			valueLocal(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this INonScalableAtomic<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12) {
			var valueLocal = @this.Value;
			if (valueLocal is null) return false;
			valueLocal(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this INonScalableAtomic<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13) {
			var valueLocal = @this.Value;
			if (valueLocal is null) return false;
			valueLocal(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this INonScalableAtomic<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14) {
			var valueLocal = @this.Value;
			if (valueLocal is null) return false;
			valueLocal(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this INonScalableAtomic<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15) {
			var valueLocal = @this.Value;
			if (valueLocal is null) return false;
			valueLocal(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this INonScalableAtomic<Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16) {
			var valueLocal = @this.Value;
			if (valueLocal is null) return false;
			valueLocal(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
			return true;
		}
		#endregion

		#region Func
		public static bool TryInvoke<TOut>(this INonScalableAtomic<Func<TOut>> @this, out TOut returnValue) {
			var valueLocal = @this.Value;
			if (valueLocal is null) {
				returnValue = default;
				return false;
			}
			returnValue = valueLocal();
			return true;
		}

		public static bool TryInvoke<T1, T2, TOut>(this INonScalableAtomic<Func<T1, T2, TOut>> @this, T1 arg1, T2 arg2, out TOut returnValue) {
			var valueLocal = @this.Value;
			if (valueLocal is null) {
				returnValue = default;
				return false;
			}
			returnValue = valueLocal(arg1, arg2);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, TOut>(this INonScalableAtomic<Func<T1, T2, T3, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, out TOut returnValue) {
			var valueLocal = @this.Value;
			if (valueLocal is null) {
				returnValue = default;
				return false;
			}
			returnValue = valueLocal(arg1, arg2, arg3);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, TOut>(this INonScalableAtomic<Func<T1, T2, T3, T4, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, out TOut returnValue) {
			var valueLocal = @this.Value;
			if (valueLocal is null) {
				returnValue = default;
				return false;
			}
			returnValue = valueLocal(arg1, arg2, arg3, arg4);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, TOut>(this INonScalableAtomic<Func<T1, T2, T3, T4, T5, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, out TOut returnValue) {
			var valueLocal = @this.Value;
			if (valueLocal is null) {
				returnValue = default;
				return false;
			}
			returnValue = valueLocal(arg1, arg2, arg3, arg4, arg5);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, TOut>(this INonScalableAtomic<Func<T1, T2, T3, T4, T5, T6, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, out TOut returnValue) {
			var valueLocal = @this.Value;
			if (valueLocal is null) {
				returnValue = default;
				return false;
			}
			returnValue = valueLocal(arg1, arg2, arg3, arg4, arg5, arg6);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, TOut>(this INonScalableAtomic<Func<T1, T2, T3, T4, T5, T6, T7, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, out TOut returnValue) {
			var valueLocal = @this.Value;
			if (valueLocal is null) {
				returnValue = default;
				return false;
			}
			returnValue = valueLocal(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, TOut>(this INonScalableAtomic<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, out TOut returnValue) {
			var valueLocal = @this.Value;
			if (valueLocal is null) {
				returnValue = default;
				return false;
			}
			returnValue = valueLocal(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOut>(this INonScalableAtomic<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, out TOut returnValue) {
			var valueLocal = @this.Value;
			if (valueLocal is null) {
				returnValue = default;
				return false;
			}
			returnValue = valueLocal(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOut>(this INonScalableAtomic<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, out TOut returnValue) {
			var valueLocal = @this.Value;
			if (valueLocal is null) {
				returnValue = default;
				return false;
			}
			returnValue = valueLocal(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOut>(this INonScalableAtomic<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, out TOut returnValue) {
			var valueLocal = @this.Value;
			if (valueLocal is null) {
				returnValue = default;
				return false;
			}
			returnValue = valueLocal(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOut>(this INonScalableAtomic<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, out TOut returnValue) {
			var valueLocal = @this.Value;
			if (valueLocal is null) {
				returnValue = default;
				return false;
			}
			returnValue = valueLocal(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOut>(this INonScalableAtomic<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, out TOut returnValue) {
			var valueLocal = @this.Value;
			if (valueLocal is null) {
				returnValue = default;
				return false;
			}
			returnValue = valueLocal(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOut>(this INonScalableAtomic<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, out TOut returnValue) {
			var valueLocal = @this.Value;
			if (valueLocal is null) {
				returnValue = default;
				return false;
			}
			returnValue = valueLocal(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOut>(this INonScalableAtomic<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, out TOut returnValue) {
			var valueLocal = @this.Value;
			if (valueLocal is null) {
				returnValue = default;
				return false;
			}
			returnValue = valueLocal(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
			return true;
		}

		public static bool TryInvoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TOut>(this INonScalableAtomic<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TOut>> @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, out TOut returnValue) {
			var valueLocal = @this.Value;
			if (valueLocal is null) {
				returnValue = default;
				return false;
			}
			returnValue = valueLocal(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
			return true;
		}
		#endregion

		public static bool TryDynamicInvoke<T>(this INonScalableAtomic<T> @this, object[] args, out object returnValue) where T : MulticastDelegate {
			var valueLocal = @this.Value;
			if (valueLocal == null) {
				returnValue = default;
				return false;
			}
			returnValue = valueLocal.DynamicInvoke(args);
			return true;
		}

		public static void Combine<T>(this INonScalableAtomic<T> @this, T delegateToCombine) where T : MulticastDelegate {
			@this.Set(v => (T) Delegate.Combine(v, delegateToCombine));
		}

		public static void Remove<T>(this INonScalableAtomic<T> @this, T delegateToRemove) where T : MulticastDelegate {
			@this.Set(v => (T) Delegate.Remove(v, delegateToRemove));
		}

		public static void RemoveAll<T>(this INonScalableAtomic<T> @this, T delegateToRemove) where T : MulticastDelegate {
			@this.Set(v => (T) Delegate.RemoveAll(v, delegateToRemove));
		}
	}
}