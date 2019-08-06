![Egodystonic.Atomics](http://www.egodystonic.com/atomicsLogoSmallest.png)

Atomics is a C#/.NET Standard 2.0 library aimed at providing thread-safe wrappers for mutable shared state variables.

Please note: This library is currently in alpha. There is no inline documentation yet and the API is liable to minor changes.

## Library Features

* Provides built-in mechanisms for operating on mutable variables in discrete, atomic operations; including 'basics' like compare-and-swap/increment etc. but also more complex or arbitrary routines, making it easier to reason about potential concurrent accesses and eliminate accidental race conditions.
* Helps ensure that all accesses to wrapped variables are done in a threadsafe manner. Unlike with other synchronization primitives, mutable variables wrapped in an `Atomic<T>` wrapper are much harder to accidentally alter in an unsafe way.
* Almost all operations are "lock-free", resulting in high-scalability for almost any contention level and any number of threads; with a suite of [benchmarks](https://github.com/Egodystonic/Atomics/tree/master/Egodystonic.Atomics.Benchmarks) used to measure performance and guide implementation.
* A full suite of [unit tests](https://github.com/Egodystonic/Atomics/tree/master/Egodystonic.Atomics.Tests) with a custom-built harness for testing the entire library with multiple concurrent threads.
* Support for custom equality (e.g. compare-and-swap with two `IEquatable<>` objects will use their `Equals()` function for comparison).
* Library is targeted to .NET Standard 2.0 which is [supported by most modern .NET platforms](https://github.com/dotnet/standard/blob/master/docs/versions.md).
* MIT licensed.


## Library Advantages

Although the best design for threadsafe code is to have no mutable state at all, sometimes it is a necessity for performance or reasons of complexity.

As [growth in single-core computing power is slowing](https://www.technologyreview.com/s/601441/moores-law-is-dead-now-what/), scaling via parallelism with higher CPU core counts is becoming an increasingly important way to increase application responsiveness. The latest generation of desktop CPUs have more threads than ever before, with the flagship specs currently being Intel's i9 9900k (16 threads) and AMD's Threadripper 2950X (32 threads).

Currently the .NET FCL/BCL offers a great suite of tools for writing parellized/concurrent code with `async/await`, concurrent & immutable collections, `Task`s and the TPL, and more fundamental constructs like locks, semaphores, reader-writer locks, and more. 

However, one potentially missing element is the provision for threadsafe single variables, often used as part of a larger concurrent algorithm or data structure. Many other languages provide a similar library, including [C++](https://en.cppreference.com/w/cpp/atomic/atomic), [Java](https://docs.oracle.com/javase/tutorial/essential/concurrency/atomicvars.html), [Rust](https://doc.rust-lang.org/nomicon/atomics.html), and [Go](https://golang.org/pkg/sync/atomic/).


## Installation

Simply install `Egodystonic.Atomics` via NuGet.

# Examples

Currently the library is in alpha and has no inline documentation. However, the following examples demonstrate common use-cases.

## Atomic Types

* `AtomicRef<T>`: Represents an atomic reference (class instance).
* `AtomicVal<T>`: Represents an atomic value (struct instance).
* `AtomicInt`: Represents an atomic 32-bit signed integer value.
* `AtomicLong`: Represents an atomic 64-bit signed integer value.
* `AtomicFloat`: Represents an atomic 32-bit floating-point value.
* `AtomicDouble`: Represents an atomic 64-bit floating-point value.
* `CopyOnReadRef<T>`: Represents an atomic reference (class instance) where the current value is always copied before being returned from any operation.
* `AtomicDelegate<T>`: Represents an atomic delegate value (e.g. `Action<>`, `Func<>`, or any custom delegate type).
* `AtomicValUnmanaged<T>`: Faster alternative to `AtomicVal<T>` for [unmanaged](https://blogs.msdn.microsoft.com/seteplia/2018/06/12/dissecting-new-generics-constraints-in-c-7-3/) value types. `sizeof(T)` must be <= `sizeof(long)`.
* `AtomicPtr<T>`: Represents an atomic pointer to type T.
* `AtomicBool<T>`: Represents an atomic boolean value (true/false).
* `AtomicEnumVal<T>`: Represents an atomic enum value.

## Common Operations

The following operations are supported on all `Atomic` types.

### Get()/Set()/Value

Atomically get or set the value on the atomic object.
	
	var currentValue = _atomic.Get(); // Atomically get the current value.
	_atomic.Set(newValue); // Atomically set a new value.
	
	var currentValue = _atomic.Value; // Atomically get the current value.
	_atomic.Value = newValue; // Atomically set a new value.
	
### Exchange()

Atomically set a new value and return the previous one.

	var exchangeResult = _atomic.Exchange(newValue); // Set a new value and return the value that was previously set as a single atomic operation.
	
	var exchangeResult = _atomic.Exchange(v => v.Frobnicate()); // Set a new value via a map function that uses the current value, and return that value, as an atomic operation.
	
* `exchangeResult.PreviousValue` returns the value that was set before the exchange operation completed.
* `exchangeResult.CurrentValue` returns the value that is now currently set (after the exchange operation completed).
	
### TryExchange()

Atomically set a new value and return the previous one, depending on the current value.

	var tryExchangeResult = _atomic.TryExchange(newValue, expectedValue); // Set a new value if and only if the current value is equal to "expectedValue". Returns the previous and current values (i.e. the values before/after the operation executes) and whether or not the exchange actually occurred.
	
	var tryExchangeResult = _atomic.TryExchange(v => v.Frobnicate(), expectedValue); // Set a new value via a map function that uses the current value, if and only if the current value is equal to "expectedValue". Returns the previous and current values (i.e. the values before/after the operation executes) and whether or not the exchange actually occurred.
	
	var tryExchangeResult = _atomic.TryExchange(newValue, (vCurrent, vNext) => vNext.SomeProperty > vCurrent.SomeProperty); // Set a new value if and only if the given predicate function returns true. The predicate function takes the currently set value and the new value as inputs. Returns the previous and current values (i.e. the values before/after the operation executes) and whether or not the exchange actually occurred.
	
	var tryExchangeResult = _atomic.TryExchange(v => v.Frobnicate(), (vCurrent, vNext) => vNext.SomeProperty > vCurrent.SomeProperty); // Set a new value via a map function that uses the current value, if and only if the given predicate function returns true. The predicate function takes the currently set value and the potential new value (calculated via the map function) as inputs. Returns the previous and current values (i.e. the values before/after the operation executes) and whether or not the exchange actually occurred.
	
* `tryExchangeResult.ValueWasSet` returns whether or not the exchange actually occurred.
* `tryExchangeResult.PreviousValue` returns the value that was set before the exchange operation was attempted.
* `tryExchangeResult.CurrentValue` returns the value that is currently set, after the exchange operation ended. If `ValueWasSet` is `false`, this is the same as `PreviousValue`. If `ValueWasSet` is `true`, this is the new value that was passed to the method call (or created via the map func).

	
## Numeric Operations

The following operations are supported on all numeric types (e.g. `AtomicInt`, `AtomicLong`, `AtomicFloat`, `AtomicDouble`).

### Increment()/Decrement()

Add or remove 1 from the current value.

	var incResult = _atomic.Increment(); // Atomically raise the value by 1, and return the current and previous value.
	var decResult = _atomic.Decrement(); // Atomically lower the value by 1, and return the current and previous value.
	
* `incResult.PreviousValue`/`decResult.PreviousValue` is the value that was set before the Increment/Decrement call.
* `incResult.CurrentValue`/`decResult.CurrentValue` is the value that is now set after the Increment/Decrement call.

### Add()/Subtract()/MultiplyBy()/DivideBy()

Add to, subtract from, multiply, or divide the current value by a given operand.

	var addResult = _atomic.Add(n); // Atomically add n, and return the current and previous value.
	var subResult = _atomic.Sub(n); // Atomically subtract n, and return the current and previous value.
	var mulResult = _atomic.MultiplyBy(n); // Atomically multiply by n, and return the current and previous value.
	var divResult = _atomic.DivideBy(n); // Atomically divide by n, and return the current and previous value.
	
* `addResult.PreviousValue` (etc.) is the value that was set before the arithmetic operation.
* `addResult.CurrentValue` (etc.) is the value that is now set after the arithmetic operation.

### TryMinimumExchange()/TryMaximumExchange()

Atomically set a new value and return the previous one, depending on the current value.

	var tryMinExchangeResult = _atomic.TryMinimumExchange(newValue, minValue); // Set a new value if and only if the current value is greater than or equal to "minValue". Returns the previous and current values (i.e. the values before/after the operation executes) and whether or not the exchange actually occurred.
	
	var tryMaxExchangeResult = _atomic.TryMaximumExchange(newValue, maxValue); // Set a new value if and only if the current value is less than or equal to "maxValue". Returns the previous and current values (i.e. the values before/after the operation executes) and whether or not the exchange actually occurred.

* `tryMinExchangeResult.ValueWasSet` (etc.) returns whether or not the exchange actually occurred.
* `tryMinExchangeResult.PreviousValue` (etc.) returns the value that was set before the exchange operation was attempted.
* `tryMinExchangeResult.CurrentValue` (etc.) returns the value that is currently set, after the exchange operation ended. If `ValueWasSet` is `false`, this is the same as `PreviousValue`. If `ValueWasSet` is `true`, this is the new value that was passed to the method call (or created via the map func).

### TryBoundedExchange()

Atomically set a new value and return the previous one, depending on the current value.

	var tryBoundedExchangeResult = _atomic.TryBoundedExchange(newValue, lowerBound, upperBound); // Set a new value if and only if lowerBound <= the current value < upperBound. Returns the previous and current values (i.e. the values before/after the operation executes) and whether or not the exchange actually occurred.
	
* `tryBoundedExchangeResult.ValueWasSet` returns whether or not the exchange actually occurred.
* `tryBoundedExchangeResult.PreviousValue` returns the value that was set before the exchange operation was attempted.
* `tryBoundedExchangeResult.CurrentValue` returns the value that is currently set, after the exchange operation ended. If `ValueWasSet` is `false`, this is the same as `PreviousValue`. If `ValueWasSet` is `true`, this is the new value that was passed to the method call (or created via the map func).

### TryExchangeWithMaxDelta() (`AtomicFloat`/`AtomicDouble` only)

Atomically set a new value and return the previous one, depending on the current value.

	var tryExchangeMaxDeltaResult = _atomic.TryExchangeWithMaxDelta(newValue, comparand, maxDelta); // Set a new value if and only if the current value is equal to the given comparand, +/- the given maxDelta. Returns the previous and current values (i.e. the values before/after the operation executes) and whether or not the exchange actually occurred.
	
* `tryExchangeMaxDeltaResult.ValueWasSet` returns whether or not the exchange actually occurred.
* `tryExchangeMaxDeltaResult.PreviousValue` returns the value that was set before the exchange operation was attempted.
* `tryExchangeMaxDeltaResult.CurrentValue` returns the value that is currently set, after the exchange operation ended. If `ValueWasSet` is `false`, this is the same as `PreviousValue`. If `ValueWasSet` is `true`, this is the new value that was passed to the method call (or created via the map func).

## Additional Operations

### `AtomicDelegate<T>.Combine()`/`AtomicDelegate<T>.Remove()`/`AtomicDelegate<T>.RemoveAll()`

Atomically [Combine](https://docs.microsoft.com/en-us/dotnet/api/system.delegate.combine?view=netframework-4.7.2), [Remove](https://docs.microsoft.com/en-us/dotnet/api/system.delegate.remove?view=netframework-4.7.2), or [RemoveAll](https://docs.microsoft.com/en-us/dotnet/api/system.delegate.removeall?view=netframework-4.7.2) the currently set delegate value.

### `AtomicDelegate<T>.TryDynamicInvoke()`

Invoke the delegate value with the given args (via [DynamicInvoke](https://docs.microsoft.com/en-us/dotnet/api/system.delegate.dynamicinvoke?view=netframework-4.7.2)) if it's not null. Returns a tuple containing whether an invocation was made, and if so the result of that invocation.

### `AtomicDelegate<T>.TryInvoke()`

Only supported when `T` is a `Func<>` type or `Action<>` type. Directly invoke the delegate value with the given args if it's not null.

* When `T` is a `Func<>` deriviative, returns a tuple containing whether an invocation was made, and if so the result of that invocation.
* When `T` is an `Action<>` deriviative, returns a boolean indicating whether or not an invocation was made.

### `AtomicDelegate<T>.TryWrappedInvoke()`

Provide a function/lambda to directly (i.e. not dynamically) invoke the delegate value if it is not null. Provided for when dynamic invoke is too slow. Return type/value is the same as `TryDynamicInvoke()`.

### `AtomicBool.Negate()`

Negate the current value of the atomic boolean. Returns the previous value and the new current value.

### `CopyOnReadRef<T>.GetWithoutCopy()`

Return the currently set value without making a copy of it.

## Advanced Operations

The following operations are provided for advanced usage scenarios. Most everyday use of the atomic types won't require these functions.

### `Fast...()`

There are various `Fast...()` versions of methods (e.g. `FastExchange()`) that return less verbose data and in some cases allow circumventing custom equality checks when reference-equality is acceptable, etc. These functions are only necessary in extreme cases, most users are recommended to use the standard versions.

### `Exchange()`/`TryExchange()` variants that consume context objects

These methods can take optional generic context objects to be used in the corresponding map/predicate functions; making it easier to pass context objects in to those functions without the implicit generation of GC pressure/garbage that comes from closure capture in lambdas. For most users, simply providing contextual arguments to map/predicate lambdas as closed-over variables is recommended, as it's simpler and not much slower.

### `GetUnsafe()`/`SetUnsafe()`

These methods are identical to `Get()`/`Set()` but elide any fence instructions or atomic/Interlocked operations; instead just directly reading/writing the internal value in a non-threadsafe way. Useful only for extremely-high-performance algorithm authors who understand the implications.

### `SpinWaitForValue()`

Forces the calling thread to busy-spin in an extremely tight loop waiting for the given value to be set (or predicate to be fulfilled). This is not an alternative for proper cross-thread synchronization, and is intended to be used by lock-free algorithm writers only. Internally uses a [SpinWait](https://docs.microsoft.com/en-us/dotnet/api/system.threading.spinwait?view=netframework-4.7.2) object to ensure correct busy-spin behaviour on any target architecture.

### `SpinWaitForExchange()`

Similar to `SpinWaitForValue()`, but additionally sets a new value once the target value/predicate has been met.

# Threading Model

The following paragraph details the threading guarantees made by this library; and is useful for experts wishing to write lock-free algorithms or data structures.

* All reads (except those marked as `Unsafe` such as `GetUnsafe()`) emit acquire fences or full fences. This includes spin-wait operations such as `SpinWaitForValue()`.
* All writes (including compound read-writes such as `Exchange` operations) emit release fences or full fences (except those marked as `Unsafe` such as `SetUnsafe()`).
* The library does not make any assumptions about need for value stability or 'freshness'. If you require a 'fresh' read (as opposed to a volatile read, which is what this library provides), you are expected to emit the relevant fences yourself or use the `GetUnsafe()` methods (which are inlined) in a spinloop to wait for a stable value.
* For target variables whose size exceeds the native word size (e.g. structs larger than 4 or 8 bytes); `AtomicVal<T>` currently uses a locked write model. Reads are still lock-free. 
* One invariant guarantee in the lib is that writes can not be 'lost' (i.e. when a concurrent `Set()` and `Exchange()` operation occur, either the `Set()` value will eventually be propagated as the current value to all threads, or it will be returned as the `PreviousValue` from the `Exchange()` operation).
* All the methods that take map or predicate functions may call those functions multiple times with different values while attempting to atomically alter the target variable.
