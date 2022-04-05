namespace Promises;

using System;
using System.Threading.Tasks;

public interface IPromise<T>
{
  IPromise<T> Catch(Func<Exception, Task<T>> morphism);
  IPromise<U> Then<U>(Func<T, Task<U>> morphism);
}

public class Promise<T>
{
}

internal class RejectedPromise<T> : IPromise<T>
{
  private readonly Exception _exception;

  public RejectedPromise(Exception exception)
  {
    _exception = exception;
  }

  public IPromise<T> Catch(Func<Exception, Task<T>> morphism)
  {
    try
    {
      Task<T> nextValueTask = morphism(_exception);

      nextValueTask.Wait();

      return new ResolvedPromise<T>(nextValueTask.Result);
    }
    catch(Exception exception)
    {
      return new RejectedPromise<T>(exception);
    }
  }

  public IPromise<U> Then<U>(Func<T, Task<U>> morphism)
  {
    return new RejectedPromise<U>(_exception);
  }
}

internal class ResolvedPromise<T> : IPromise<T>
{
  private readonly T _value;

  public ResolvedPromise(T value)
  {
    _value = value;
  }

  public IPromise<T> Catch(Func<Exception, Task<T>> morphism)
  {
    return this;
  }

  public IPromise<U> Then<U>(Func<T, Task<U>> morphism)
  {
    try
    {
      Task<U> nextValueTask = morphism(_value);

      nextValueTask.Wait();

      return new ResolvedPromise<U>(nextValueTask.Result);
    }
    catch(Exception exception)
    {
      return new RejectedPromise<U>(exception);
    }
  }
}