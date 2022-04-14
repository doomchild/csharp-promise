using System.Runtime.CompilerServices;

namespace RLC.Promises;

using System;
using System.Threading.Tasks;

public interface IPromise<T>
{
  /// <summary>
  /// Returns <code>true</code> if the IPromise is fulfilled, otherwise <code>false</code>.
  /// </summary>
  bool IsFulfilled { get; }
  
  /// <summary>
  /// Returns <code>true</code> if the IPromise is rejected, otherwise <code>false</code>.
  /// </summary>
  bool IsRejected { get; }

  /// <summary>
  /// Calls the provided callback if the IPromise is rejected.
  /// </summary>
  /// <param name="onRejected">Function to call if the IPromise is rejected.</param>
  /// <returns>A resolved IPromise of <typeparamref name="T"/>.</returns>
  IPromise<T> Catch(Func<Exception, T> onRejected);
  
  /// <summary>
  /// Calls the provided callback if the IPromise is rejected.
  /// </summary>
  /// <param name="onRejected">Function to call if the IPromise is rejected.</param>
  /// <returns>A resolved IPromise of <typeparamref name="T"/>.</returns>
  IPromise<T> Catch(Func<Exception, IPromise<T>> onRejected);
  
  /// <summary>
  /// Calls the provided callback if the IPromise is rejected.
  /// </summary>
  /// <param name="onRejected">Function to call if the IPromise is rejected.</param>
  /// <returns>A resolved IPromise of <typeparamref name="T"/>.</returns>
  IPromise<T> Catch(Func<Exception, Task<T>> onRejected);
  
  /// <summary>
  /// Returns a <see cref="TaskAwaiter"/> that allows the IPromise to be awaited.
  /// </summary>
  /// <returns>A <see cref="TaskAwaiter"/>.</returns>
  TaskAwaiter<T> GetAwaiter();
  
  /// <summary>
  /// Calls the provide <see cref="Action"/> if the IPromise is fulfilled.
  /// </summary>
  /// <param name="onFulfilled">Action to be called if the IPromise is fulfilled.</param>
  /// <returns>An IPromise of the original fulfillment.</returns>
  IPromise<T> IfFulfilled(Action<T> onFulfilled);
  
  /// <summary>
  /// Calls the provided <see cref="Action"/> if the IPromise is rejected.
  /// </summary>
  /// <param name="onRejected">Action to be called if the IPromise is rejected.</param>
  /// <returns>An IPromise of the original rejection.</returns>
  IPromise<T> IfRejected(Action<Exception> onRejected);
  
  /// <summary>
  /// Calls one of the two callbacks provided, depending on the fulfillment state of the IPromise, then returns the
  /// original value.
  /// </summary>
  /// <param name="onFulfilled">Action to be called if the IPromise is fulfilled.</param>
  /// <param name="onRejected">Action to be called if the IPromise is rejected.</param>
  /// <returns>An IPromise of the original type <typeparamref name="T"/>.</returns>
  IPromise<T> Tap(Action<T> onFulfilled, Action<Exception> onRejected) => IfFulfilled(onFulfilled)
    .IfRejected(onRejected);

  /// <summary>
  /// Calls the provided callback if the IPromise is fulfilled.
  /// </summary>
  /// <param name="onFulfilled">Function to be called if the IPromise is fulfilled.</param>
  /// <typeparam name="TNext"></typeparam>
  /// <returns>An IPromise of the new type <typeparamref name="TNext"/>.</returns>
  IPromise<TNext> Then<TNext>(Func<T, TNext> onFulfilled);
  
  /// <summary>
  /// Calls the provided callback if the IPromise is fulfilled.
  /// </summary>
  /// <param name="onFulfilled">Function to be called if the IPromise is fulfilled.</param>
  /// <typeparam name="TNext"></typeparam>
  /// <returns>An IPromise of the new type <typeparamref name="TNext"/>.</returns>
  IPromise<TNext> Then<TNext>(Func<T, IPromise<TNext>> onFulfilled);
  
  /// <summary>
  /// Calls the provided callback if the IPromise is fulfilled.
  /// </summary>
  /// <param name="onFulfilled">Function to be called if the IPromise is fulfilled.</param>
  /// <typeparam name="TNext"></typeparam>
  /// <returns>An IPromise of the new type <typeparamref name="TNext"/>.</returns>
  IPromise<TNext> Then<TNext>(Func<T, Task<TNext>> onFulfilled);
  
  /// <summary>
  /// Calls the provided callback if the IPromise is fulfilled.
  /// </summary>
  /// <param name="onFulfilled">Function to be called if the IPromise is fulfilled.</param>
  /// <typeparam name="TNext"></typeparam>
  /// <returns>An IPromise of the new type <typeparamref name="TNext"/>.</returns>
  IPromise<TNext> Then<TNext>(Func<T, Task<IPromise<TNext>>> onFulfilled);

  /// <summary>
  /// Calls one of the two callbacks provided, depending on the fulfillment state of the IPromise.
  /// </summary>
  /// <param name="onFulfilled">Function to be called if the IPromise is fulfilled.</param>
  /// <param name="onRejected">Function to be called if the IPromise is rejected.</param>
  /// <typeparam name="TNext">The type returned by the callbacks.</typeparam>
  /// <returns>An IPromise of the new type <typeparamref name="TNext"/>.</returns>
  IPromise<TNext> Then<TNext>(Func<T, TNext> onFulfilled, Func<Exception, TNext> onRejected);
  
  /// <summary>
  /// Calls one of the two callbacks provided, depending on the fulfillment state of the IPromise.
  /// </summary>
  /// <param name="onFulfilled">Function to be called if the IPromise is fulfilled.</param>
  /// <param name="onRejected">Function to be called if the IPromise is rejected.</param>
  /// <typeparam name="TNext">The type returned by the callbacks.</typeparam>
  /// <returns>An IPromise of the new type <typeparamref name="TNext"/>.</returns>
  IPromise<TNext> Then<TNext>(Func<T, Task<TNext>> onFulfilled, Func<Exception, Task<TNext>> onRejected);
  
  /// <summary>
  /// Calls one of the two callbacks provided, depending on the fulfillment state of the IPromise.
  /// </summary>
  /// <param name="onFulfilled">Function to be called if the IPromise is fulfilled.</param>
  /// <param name="onRejected">Function to be called if the IPromise is rejected.</param>
  /// <typeparam name="TNext">The type returned by the callbacks.</typeparam>
  /// <returns>An IPromise of the new type <typeparamref name="TNext"/>.</returns>
  IPromise<TNext> Then<TNext>(Func<T, IPromise<TNext>> onFulfilled, Func<Exception, IPromise<TNext>> onRejected);
  
  /// <summary>
  /// Calls one of the two callbacks provided, depending on the fulfillment state of the IPromise.
  /// </summary>
  /// <param name="onFulfilled">Function to be called if the IPromise is fulfilled.</param>
  /// <param name="onRejected">Function to be called if the IPromise is rejected.</param>
  /// <typeparam name="TNext">The type returned by the callbacks.</typeparam>
  /// <returns>An IPromise of the new type <typeparamref name="TNext"/>.</returns>
  IPromise<TNext> Then<TNext>(Func<T, Task<IPromise<TNext>>> onFulfilled, Func<Exception, Task<IPromise<TNext>>> onRejected);
}