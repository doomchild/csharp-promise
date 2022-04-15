using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RLC.Promises;

public class Promise<T> : IPromise<T>
{
  private Task<T> _task = Task.FromException<T>(new TaskNotStartedException());

  /// <inheritdoc />
  public bool IsRejected => _task.IsFaulted;
  /// <inheritdoc />
  public bool IsFulfilled => _task.IsCompletedSuccessfully;

  private Promise(Task<T> task)
  {
    _task = task;
  }

  /// <summary>
  /// Creates a new Promise.  The provided Action will receive functions <code>resolve</code> and <code>reject</code> as
  /// its arguments which be called to seal the fate of the created Promise.
  /// </summary>
  /// <param name="executor">The Action that will receive functions for Promise fulfillment and rejection.</param>
  public Promise(Action<Action<T>, Action<Exception>> executor)
  {
    try
    {
      executor(
        value => _task = Task.FromResult(value),
        exception => _task = Task.FromException<T>(exception)
      );
    }
    catch (Exception exception)
    {
      _task = Task.FromException<T>(exception);
    }
  }

  /// <summary>
  /// Creates a new Promise.  The provided Action will receive functions <code>resolve</code> and <code>reject</code> as
  /// its arguments which be called to seal the fate of the created Promise.
  /// </summary>
  /// <param name="executor">The Action that will receive functions for Promise fulfillment and rejection.</param>
  public Promise(Action<Action<Task<T>>, Action<Exception>> executor)
  {
    try
    {
      executor(
        value => _task = value,
        exception => _task = Task.FromException<T>(exception)
      );
    }
    catch (Exception exception)
    {
      _task = Task.FromException<T>(exception);
    }
  }

  /// <summary>
  /// Creates a new Promise.  The provided Action will receive functions <code>resolve</code> and <code>reject</code> as
  /// its arguments which be called to seal the fate of the created Promise.
  /// </summary>
  /// <param name="executor">The Action that will receive functions for Promise fulfillment and rejection.</param>
  public Promise(Action<Action<IPromise<T>>, Action<Exception>> executor)
  {
    try
    {
      executor(
        value => _task = Task.Factory.StartNew(() => value.GetAwaiter().GetResult()),
        exception => _task = Task.FromException<T>(exception)
      );
    }
    catch (Exception exception)
    {
      _task = Task.FromException<T>(exception);
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static U Invoke<U>(Func<U> supplier) => supplier();
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Func<A, C> Pipe2<A, B, C>(Func<A, B> f, Func<B, C> g) => x => g(f(x));
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static Func<TTappedValue, TTappedValue> Tap<TTappedValue>(Action<TTappedValue> consumer)
  {
    return value =>
    {
      consumer(value);

      return value;
    };
  }
  
  /// <summary>
  /// Takes an <see cref="IEnumerable{Task}"/> of <see cref="Task{T}"/> and returns an <see cref="IEnumerable{T}"/>
  /// containing either a fulfillment of all the results, or a rejection from the first <see cref="Task{T}"/> to fail.
  /// </summary>
  /// <param name="tasks">The <see cref="IEnumerable{T}"/> containing the <see cref="Task{T}"/>s that will be
  /// awaited.</param>
  /// <returns>A fulfillment of all of the <see cref="Task{T}"/> inputs, or a rejection if any <see cref="Task{T}"/>
  /// fails.</returns>
  public static IPromise<IEnumerable<T>> All(IEnumerable<Task<T>> tasks) => new Promise<IEnumerable<T>>(
    Task.WhenAll(tasks).ContinueWith(task => task.GetAwaiter().GetResult().AsEnumerable())
  );

  /// <summary>
  /// Takes an <see cref="IEnumerable{Task}"/> of <see cref="Func{T}"/> that return <see cref="Task{T}"/>s
  /// and returns an <see cref="IEnumerable{T}"/> containing either a fulfillment of all the results,
  /// or a rejection from the first <see cref="Task{T}"/> to fail.
  /// </summary>
  /// <param name="suppliers">The <see cref="IEnumerable{T}"/> containing the <see cref="Func{T}"/>s that will be
  /// called to produce the <see cref="Task{T}"/>s that will be awaited.</param>
  /// <returns>A fulfillment of all of the <see cref="Task{T}"/> inputs, or a rejection if any <see cref="Task{T}"/>
  /// fails.</returns>
  public static IPromise<IEnumerable<T>> All(IEnumerable<Func<Task<T>>> suppliers) => All(suppliers.Select(Invoke));
  
  /// <summary>
  /// Takes an <see cref="IEnumerable{T}"/> of <see cref="Task{T}"/> and returns an IPromise of the result of the first
  /// <see cref="Task{T}"/> to finish.
  /// </summary>
  /// <param name="tasks">The <see cref="IEnumerable{T}"/> containing the <see cref="Task{T}"/>s that will race.</param>
  /// <returns>A fulfillment or rejection of the first <see cref="Task{T}"/> to finish running.</returns>
  public static IPromise<T> Any(IEnumerable<Task<T>> tasks) => Resolve(Task.WhenAny(tasks)
    .ContinueWith(task => task.GetAwaiter().GetResult().GetAwaiter().GetResult())
  );

  /// <summary>
  /// Takes an <see cref="IEnumerable{T}"/> of <see cref="Func{T}"/> that will return <see cref="Task{T}"/>s
  /// and returns an IPromise of the result of the first <see cref="Task{T}"/> to finish.
  /// </summary>
  /// <param name="suppliers">The <see cref="IEnumerable{T}"/> containing the <see cref="Func"/>s that will be called to
  /// produce the <see cref="Task{T}"/>s that will race.</param>
  /// <returns>A fulfillment or rejection of the first <see cref="Task{T}"/> to finish running.</returns>
  public static IPromise<T> Any(IEnumerable<Func<Task<T>>> suppliers) => Any(suppliers.Select(Invoke));

  /// <summary>
  /// Takes an <see cref="IEnumerable{T}"/> of <see cref="IPromise{T}"/> and returns an IPromise of the result of the
  /// first Promise to finish.
  /// </summary>
  /// <param name="promises"></param>
  /// <returns>A fulfillment or rejection of the first <see cref="IPromise{T}"/> to finish running.</returns>
  public static IPromise<T> Any(IEnumerable<IPromise<T>> promises) => Any(promises.Select(async p => await p));
  
  /// <summary>
  /// Takes an <see cref="IEnumerable{T}"/> of <see cref="Func{T}"/> that will return <see cref="IPromise{T}"/>s
  /// and returns an IPromise of the result of the first Promise to finish.
  /// </summary>
  /// <param name="promises"></param>
  /// <returns>A fulfillment or rejection of the first <see cref="IPromise{T}"/> to finish running.</returns>
  public static IPromise<T> Any(IEnumerable<Func<IPromise<T>>> promises) => Any(promises.Select(Invoke));

  /// /// <summary>
  /// Creates an IPromise from a <see cref="Task{T}"/>.
  /// </summary>
  /// <param name="value">The raw value to convert into an IPromise.</param>
  /// <returns>The created IPromise.</returns>
  public static IPromise<T> Resolve(T value) => Resolve(Task.FromResult(value));
  
  /// <summary>
  /// Creates an IPromise from a raw value.
  /// </summary>
  /// <param name="valueTask">The <see cref="Task{T}"/> to convert into an IPromise.</param>
  /// <returns>The created IPromise.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static IPromise<T> Resolve(Task<T> valueTask) => new Promise<T>(valueTask);

  /// <summary>
  /// Creates an IPromise from an <see cref="Exception"/>.
  /// </summary>
  /// <param name="exception">The <see cref="Exception"/> to convert into an IPromise.</param>
  /// <returns>The created IPromise.</returns>
  public static IPromise<T> Reject(Exception exception) => new Promise<T>(Task.FromException<T>(exception));

  /// <summary>
  /// Returns a function that that rejects with the result of the <paramref name="rejectionSupplier"/> if the
  /// <paramref name="predicate"/> returns <code>true</code>.
  /// </summary>
  /// <param name="predicate">Determines whether or not to reject the IPromise.</param>
  /// <param name="rejectionSupplier">Provides the <see cref="Exception"/> to reject.</param>
  /// <returns>A <see cref="Func{T}"/> that takes a value and returns an <see cref="IPromise{T}"/>.</returns>
  public static Func<T, IPromise<T>> RejectIf(Predicate<T> predicate, Func<T, Exception> rejectionSupplier) => 
    value => predicate(value) 
      ? Resolve(value) 
      : Reject(rejectionSupplier(value));

    /// <inheritdoc />
  public IPromise<T> Catch(Func<Exception, T> onRejected) => IsFulfilled 
    ? this 
    : Then(
      Task.FromResult,
      Pipe2(onRejected, Task.FromResult)
    );

  /// <inheritdoc />
  public IPromise<T> Catch(Func<Exception, IPromise<T>> onRejected) => IsFulfilled 
    ? this 
    : Then(
      Pipe2<T, Task<T>, IPromise<T>>(Task.FromResult, Resolve),
      onRejected
    );

  /// <inheritdoc />
  public IPromise<T> Catch(Func<Exception, Task<T>> onRejected) => IsFulfilled 
    ? this 
    : Then(
      Task.FromResult,
      onRejected
    );
  
  /// <inheritdoc />
  public TaskAwaiter<T> GetAwaiter() => _task.GetAwaiter();

  /// <inheritdoc />
  public IPromise<T> IfFulfilled(Action<T> onFulfilled) => IsRejected 
    ? this 
    : Then(
      Pipe2(Tap(onFulfilled), Task.FromResult),
      Task.FromException<T>
    );

  /// <inheritdoc />
  public IPromise<T> IfRejected(Action<Exception> onRejected) => IsFulfilled 
    ? this 
    : Then(
      Task.FromResult,
      Pipe2(Tap(onRejected), Task.FromException<T>)
    );

  /// <inheritdoc />
  public IPromise<TNext> Then<TNext>(Func<T, TNext> onFulfilled) => ThenRightRawLeftTask(
    onFulfilled,
    Task.FromException<TNext>
  );

  /// <inheritdoc />
  public IPromise<TNext> Then<TNext>(Func<T, IPromise<TNext>> onFulfilled) => Then(
    onFulfilled,
    Promise<TNext>.Reject
  );

  /// <inheritdoc />
  public IPromise<TNext> Then<TNext>(Func<T, Task<TNext>> onFulfilled) => Then(
    onFulfilled,
    Task.FromException<TNext>
  );

  /// <inheritdoc />
  public IPromise<TNext> Then<TNext>(Func<T, Task<IPromise<TNext>>> onFulfilled) => Then(
    onFulfilled,
    Pipe2<Exception, IPromise<TNext>, Task<IPromise<TNext>>>(Promise<TNext>.Reject, Task.FromResult)
  );

  /// <inheritdoc />
  public IPromise<TNext> Then<TNext>(Func<T, TNext> onFulfilled, Func<Exception, TNext> onRejected) => Then(
    Pipe2(onFulfilled, Task.FromResult),
    Pipe2(onRejected, Task.FromResult)
  );

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IPromise<TNext> Then<TNext>(Func<T, Task<TNext>> onFulfilled, Func<Exception, Task<TNext>> onRejected) =>
    new Promise<TNext>(_task.Fold(
      exception => onRejected(exception).GetAwaiter().GetResult(), 
      value => onFulfilled(value).GetAwaiter().GetResult()
    ));

  /// <inheritdoc />
  public IPromise<TNext> Then<TNext>(
    Func<T, IPromise<TNext>> onFulfilled,
    Func<Exception, IPromise<TNext>> onRejected
  ) => Then(
    value => onFulfilled(value).GetAwaiter().GetResult(),
    exception => onRejected(exception).GetAwaiter().GetResult()
  );

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IPromise<TNext> Then<TNext>(
    Func<T, Task<IPromise<TNext>>> onFulfilled,
    Func<Exception, Task<IPromise<TNext>>> onRejected
  ) => new Promise<TNext>(_task.Fold(
    exception => onRejected(exception).GetAwaiter().GetResult().GetAwaiter().GetResult(), 
    value => onFulfilled(value).GetAwaiter().GetResult().GetAwaiter().GetResult()
  ));

  private IPromise<TNext> ThenRightRawLeftTask<TNext>(
    Func<T, TNext> onFulfilled,
    Func<Exception, Task<TNext>> onRejected
  ) => Then(
    Pipe2(onFulfilled, Task.FromResult),
    onRejected
  );
}