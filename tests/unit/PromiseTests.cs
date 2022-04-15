using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Jds.TestingUtils.MockHttp;
using RLC.Promises;
using Xunit;

namespace PromiseTests;

public class PromiseTests
{
  public class Resolve
  {
    public class FromValue
    {
      [Fact]
      public void ItShouldReturnResolved()
      {
        IPromise<int> testPromise = Promise<int>.Resolve(1);
        
        Assert.True(testPromise.IsFulfilled);
      }
    }

    public class FromTask
    {
      [Fact]
      public async void ItShouldReturnResolved()
      {
        Func<Task<int>> testTaskGenerator = async () =>
        {
          await Task.Delay(1);
          return 1;
        };
        IPromise<int> testPromise = Promise<int>.Resolve(testTaskGenerator());

        await Task.Delay(100);
        
        Assert.True(testPromise.IsFulfilled);
      }

      [Fact]
      public void ItShouldRejectIfTaskIsFaulted()
      {
        IPromise<int> testPromise = Promise<int>.Resolve(Task.FromException<int>(new ArgumentNullException()));
        
        Assert.True(testPromise.IsRejected);
      }

      [Fact]
      public async void ItShouldRejectIfTaskThrows()
      {
        Task<int> testTask = Task<int>.Factory.StartNew(() => throw new ArgumentNullException());
        IPromise<int> testPromise = Promise<int>.Resolve(testTask);

        await Task.Delay(100);
        
        Assert.True(testPromise.IsRejected);
      }
    }
  }
  
  public class Catch
  {
    public class ForExceptionToT
    {
      [Fact]
      public async void ItShouldReportUnRejectedAfterCatching()
      {
        Func<string, int> testFunc = _ => throw new ArgumentException();
        IPromise<int> testPromise = Promise<string>.Resolve("12345")
          .Then(testFunc)
          .Catch(ex => ex.Message.Length);

        await Task.Delay(100);

        Assert.False(testPromise.IsRejected);
      }

      [Fact]
      public async void ItShouldReportUnRejectedAfterCatchingFromAsync()
      {
        Func<string, Task<int>> testFunc = async _ => { await Task.Delay(1); throw new ArgumentException(); };
        IPromise<int> testPromise = Promise<string>.Resolve("12345")
          .Then(testFunc)
          .Catch(ex => ex.Message.Length);

        await Task.Delay(100);

        Assert.False(testPromise.IsRejected);
      }
      
      [Fact]
      public async void ItShouldReturnTheFulfilledValueAfterCatching()
      {
        Func<string, int> testFunc = _ => throw new ArgumentException();
        int expectedValue = 46;
        int actualValue = await Promise<string>.Resolve("12345")
          .Then(testFunc)
          .Catch(ex => ex.Message.Length);

        await Task.Delay(100);

        Assert.Equal(expectedValue, actualValue);
      }
    }

    public class ForExceptionToTaskT
    {
      [Fact]
      public async void ItShouldReportUnRejectedAfterCatching()
      {
        Func<string, int> testFunc = _ => throw new ArgumentException();
        IPromise<int> testPromise = Promise<string>.Resolve("12345")
          .Then(testFunc)
          .Catch(ex => Task.FromResult(ex.Message.Length));

        await Task.Delay(100);

        Assert.False(testPromise.IsRejected);
      }

      [Fact]
      public async void ItShouldReportUnRejectedAfterCatchingFromAsync()
      {
        Func<string, Task<int>> testFunc = async _ => { await Task.Delay(1); throw new ArgumentException(); };
        IPromise<int> testPromise = Promise<string>.Resolve("12345")
          .Then(testFunc)
          .Catch(ex => Task.FromResult(ex.Message.Length));

        await Task.Delay(100);

        Assert.False(testPromise.IsRejected);
      }
      
      [Fact]
      public async void ItShouldReturnTheFulfilledValueAfterCatching()
      {
        Func<string, int> testFunc = _ => throw new ArgumentException();
        int expectedValue = 46;
        int actualValue = await Promise<string>.Resolve("12345")
          .Then(testFunc)
          .Catch(ex => Task.FromResult(ex.Message.Length));

        await Task.Delay(100);

        Assert.Equal(expectedValue, actualValue);
      }
      
      [Fact]
      public async void ItShouldReturnTheFulfilledValueAfterCatchingFromAsync()
      {
        ArgumentException testException = new();
        Func<string, int> testFunc = _ => throw testException;
        int expectedValue = 46;
        int actualValue = await Promise<string>.Resolve("12345")
          .Then(testFunc)
          .Catch(async ex =>
          {
            await Task.Delay(1);
            return ex.Message.Length;
          });

        await Task.Delay(100);

        Assert.Equal(expectedValue, actualValue);
      }
    }

    public class ForExceptionToIPromiseT
    {
      [Fact]
      public async void ItShouldReportUnRejectedAfterCatching()
      {
        Func<string, int> testFunc = _ => throw new ArgumentException();
        IPromise<int> testPromise = Promise<string>.Resolve("12345")
          .Then(testFunc)
          .Catch(ex => Promise<int>.Resolve(ex.Message.Length));

        await Task.Delay(100);

        Assert.False(testPromise.IsRejected);
      }

      [Fact]
      public async void ItShouldReportUnRejectedAfterCatchingFromAsync()
      {
        Func<string, Task<int>> testFunc = async _ => { await Task.Delay(1); throw new ArgumentException(); };
        IPromise<int> testPromise = Promise<string>.Resolve("12345")
          .Then(testFunc)
          .Catch(ex => Promise<int>.Resolve(ex.Message.Length));

        await Task.Delay(100);

        Assert.False(testPromise.IsRejected);
      }
      
      [Fact]
      public async void ItShouldReturnTheFulfilledValueAfterCatching()
      {
        Func<string, int> testFunc = _ => throw new ArgumentException();
        int expectedValue = 46;
        int actualValue = await Promise<string>.Resolve("12345")
          .Then(testFunc)
          .Catch(ex => Promise<int>.Resolve(ex.Message.Length));

        await Task.Delay(100);

        Assert.Equal(expectedValue, actualValue);
      }
    }
  }

  public class Then
  {
    public class ForTtoTNext
    {
      [Fact]
      public async void ItShouldTransition()
      {
        int expectedValue = 5;
        int actualValue = await Promise<string>.Resolve("12345")
          .Then(str => str.Length);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldCompleteWithoutAwaiting()
      {
        int expectedValue = 5;
        int actualValue = 0;

        _ = Promise<string>.Resolve("12345")
          .Then(str =>
          {
            actualValue = str.Length;

            return actualValue;
          });

        await Task.Delay(100);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldRejectForThrownExceptions()
      {
        Func<string, int> testFunc = _ => throw new Exception();
        
        IPromise<int> testPromise = Promise<string>.Resolve("abc")
          .Then(testFunc);

        await Task.Delay(100);
        
        Assert.True(testPromise.IsRejected);
      }

      [Fact]
      public void ItShouldRethrowForRejections()
      {
        IPromise<int> testPromise = Promise<string>.Reject(new ArgumentNullException("abcde"))
          .Then(value => value.Length);

        Assert.ThrowsAsync<ArgumentNullException>(async () => await testPromise);
      }
      
      [Fact]
      public async void ItShouldNotRunForARejection()
      {
        Exception testException = new(Guid.NewGuid().ToString());

        await Assert.ThrowsAsync<Exception>(
          async () => await Promise<string>.Reject(testException).Then(str => str.Length)
        );
      }

      [Fact]
      public async void ItShouldReportRejectedForThrownException()
      {
        Func<string, int> testFunc = _ => throw new ArgumentException();
        IPromise<int> testPromise = Promise<string>.Resolve("12345").Then(testFunc);

        await Task.Delay(100);

        Assert.True(testPromise.IsRejected);
      }

      [Fact]
      public void ItShouldThrowRejectedException()
      {
        Func<string, int> testFunc = _ => throw new ArgumentException();

        Assert.ThrowsAsync<ArgumentException>(async () => await Promise<string>.Resolve("12345").Then(testFunc));
      }
    }

    public class ForTtoTaskTNext
    {
      [Fact]
      public async void ItShouldTransition()
      {
        int expectedValue = 5;
        int actualValue = await Promise<string>.Resolve("12345")
          .Then(str => Task.FromResult(str.Length));

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldCompleteWithoutAwaiting()
      {
        int expectedValue = 5;
        int actualValue = 0;

        _ = Promise<string>.Resolve("12345")
          .Then(str =>
          {
            actualValue = str.Length;

            return Task.FromResult(actualValue);
          });

        await Task.Delay(100);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldNotRunForARejection()
      {
        Exception testException = new(Guid.NewGuid().ToString());

        await Assert.ThrowsAsync<Exception>(
          async () => await Promise<string>.Reject(testException).Then(str => Task.FromResult(str.Length))
        );
      }

      [Fact]
      public async void ItShouldContinueAsyncTasks()
      {
        int expectedValue = 5;
        int actualValue = 0;

        _ = Promise<string>.Resolve("12345")
          .Then(async str =>
          {
            await Task.Delay(1);

            actualValue = str.Length;

            return Task.FromResult(str.Length);
          });

        await Task.Delay(100);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldContinueAsyncTasksWithoutAwaiting()
      {
        int expectedValue = 5;
        int actualValue = 0;

        _ = Promise<string>.Resolve("12345")
          .Then(async str =>
          {
            await Task.Delay(1);

            actualValue = str.Length;

            return Task.FromResult(actualValue);
          });

        await Task.Delay(100);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldReportRejectedForThrownException()
      {
        Func<string, Task<int>> testFunc = _ => throw new ArgumentException();
        IPromise<int> testPromise = Promise<string>.Resolve("12345").Then(testFunc);

        await Task.Delay(100);

        Assert.True(testPromise.IsRejected);
      }

      [Fact]
      public void ItShouldThrowRejectedException()
      {
        Func<string, Task<int>> testFunc = _ => throw new ArgumentException();

        Assert.ThrowsAsync<ArgumentException>(async () => await Promise<string>.Resolve("12345").Then(testFunc));
      }

      [Fact]
      public async void ItShouldReportRejectedForThrownExceptionFromAsync()
      {
        Func<string, Task<int>> testFunc = async _ =>
        {
          await Task.Delay(1);
          throw new ArgumentException();
        };
        IPromise<int> testPromise = Promise<string>.Resolve("12345").Then(testFunc);

        await Task.Delay(100);

        Assert.True(testPromise.IsRejected);
      }

      [Fact]
      public void ItShouldThrowRejectedExceptionFromAsync()
      {
        Func<string, Task<int>> testFunc = async _ =>
        {
          await Task.Delay(1);
          throw new ArgumentException();
        };

        Assert.ThrowsAsync<ArgumentException>(async () => await Promise<string>.Resolve("12345").Then(testFunc));
      }
      
      [Fact]
      public async void ItShouldRejectForThrownExceptions()
      {
        Func<string, Task<int>> testFunc = _ => throw new Exception();
        
        IPromise<int> testPromise = Promise<string>.Resolve("abc")
          .Then(testFunc);

        await Task.Delay(100);
        
        Assert.True(testPromise.IsRejected);
      }

      [Fact]
      public void ItShouldCaptureTaskCancellation()
      {
        HttpClient testHttpClient = new MockHttpBuilder()
          .WithHandler(messageCaseBuilder => messageCaseBuilder.AcceptAll()
            .RespondWith((responseBuilder, _) => responseBuilder.WithStatusCode(HttpStatusCode.OK))
          )
          .BuildHttpClient();
        ;
        CancellationTokenSource testTokenSource = new(-1);
        IPromise<string> testPromise = Promise<string>.Resolve("http://anything.anywhere")
          .Then(async url => await testHttpClient.GetStringAsync(url, testTokenSource.Token));

        Assert.ThrowsAsync<TaskCanceledException>(async () => await testPromise);
      }
    }

    public class ForTtoIPromiseTNext
    {
      [Fact]
      public async void ItShouldTransition()
      {
        int expectedValue = 5;
        int actualValue = await Promise<string>.Resolve("12345")
          .Then(str => Promise<int>.Resolve(str.Length));

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldCompleteWithoutAwaiting()
      {
        int expectedValue = 5;
        int actualValue = 0;

        _ = Promise<string>.Resolve("12345")
          .Then(str =>
          {
            actualValue = str.Length;

            return Promise<int>.Resolve(actualValue);
          });

        await Task.Delay(100);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldNotRunForARejection()
      {
        Exception testException = new(Guid.NewGuid().ToString());

        await Assert.ThrowsAsync<Exception>(
          async () => await Promise<string>.Reject(testException).Then(str => Promise<int>.Resolve(str.Length))
        );
      }

      [Fact]
      public async void ItShouldContinueAsyncTasks()
      {
        int expectedValue = 5;
        int actualValue = 0;

        _ = Promise<string>.Resolve("12345")
          .Then(async str =>
          {
            await Task.Delay(1);

            actualValue = str.Length;

            return Promise<int>.Resolve(str.Length);
          });

        await Task.Delay(100);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldContinueAsyncTasksWithoutAwaiting()
      {
        int expectedValue = 5;
        int actualValue = 0;

        _ = Promise<string>.Resolve("12345")
          .Then(async str =>
          {
            await Task.Delay(1);

            actualValue = str.Length;

            return Promise<int>.Resolve(actualValue);
          });

        await Task.Delay(100);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldReportRejectedForThrownException()
      {
        Func<string, IPromise<int>> testFunc = _ => throw new ArgumentException();
        IPromise<int> testPromise = Promise<string>.Resolve("12345").Then(testFunc);

        await Task.Delay(100);

        Assert.True(testPromise.IsRejected);
      }

      [Fact]
      public void ItShouldThrowRejectedException()
      {
        Func<string, IPromise<int>> testFunc = _ => throw new ArgumentException();

        Assert.ThrowsAsync<ArgumentException>(async () => await Promise<string>.Resolve("12345").Then(testFunc));
      }
      
      [Fact]
      public async void ItShouldRejectForThrownExceptions()
      {
        Func<string, IPromise<int>> testFunc = _ => throw new Exception();
        
        IPromise<int> testPromise = Promise<string>.Resolve("abc")
          .Then(testFunc);

        await Task.Delay(100);
        
        Assert.True(testPromise.IsRejected);
      }
    }

    public class ForTtoTaskIPromiseTNext
    {
      [Fact]
      public async void ItShouldTransition()
      {
        int expectedValue = 5;
        int actualValue = await Promise<string>.Resolve("12345")
          .Then(str => Task.FromResult(Promise<int>.Resolve(str.Length)));

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldCompleteWithoutAwaiting()
      {
        int expectedValue = 5;
        int actualValue = 0;

        _ = Promise<string>.Resolve("12345")
          .Then(str =>
          {
            actualValue = str.Length;

            return Task.FromResult(Promise<int>.Resolve(actualValue));
          });

        await Task.Delay(100);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldNotRunForARejection()
      {
        Exception testException = new(Guid.NewGuid().ToString());

        await Assert.ThrowsAsync<Exception>(
          async () => await Promise<string>.Reject(testException)
            .Then(str => Task.FromResult(Promise<int>.Resolve(str.Length)))
        );
      }

      [Fact]
      public async void ItShouldContinueAsyncTasks()
      {
        int expectedValue = 5;
        int actualValue = 0;

        _ = Promise<string>.Resolve("12345")
          .Then(async str =>
          {
            await Task.Delay(1);

            actualValue = str.Length;

            return Promise<int>.Resolve(str.Length);
          });

        await Task.Delay(100);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldContinueAsyncTasksWithoutAwaiting()
      {
        int expectedValue = 5;
        int actualValue = 0;

        _ = Promise<string>.Resolve("12345")
          .Then(async str =>
          {
            await Task.Delay(1);

            actualValue = str.Length;

            return Promise<int>.Resolve(actualValue);
          });

        await Task.Delay(100);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldReportRejectedForThrownException()
      {
        Func<string, Task<IPromise<int>>> testFunc = _ => throw new ArgumentException();
        IPromise<int> testPromise = Promise<string>.Resolve("12345").Then(testFunc);

        await Task.Delay(100);

        Assert.True(testPromise.IsRejected);
      }

      [Fact]
      public void ItShouldThrowRejectedException()
      {
        Func<string, Task<IPromise<int>>> testFunc = _ => throw new ArgumentException();

        Assert.ThrowsAsync<ArgumentException>(async () => await Promise<string>.Resolve("12345").Then(testFunc));
      }
      
      [Fact]
      public async void ItShouldRejectForThrownExceptions()
      {
        Func<string, Task<IPromise<int>>> testFunc = _ => throw new Exception();
        
        IPromise<int> testPromise = Promise<string>.Resolve("abc")
          .Then(testFunc);

        await Task.Delay(100);
        
        Assert.True(testPromise.IsRejected);
      }
    }
  }

  public class Tap
  {
    [Fact]
    public async void ItShouldPerformASideEffectOnAResolution()
    {
      int actualValue = 0;
      int expectedValue = 5;

      await Promise<int>.Resolve(5)
        .Tap(value =>
        {
          actualValue = value;
        },
        _ => {}
      );

      Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public async void ItShouldPerformASideEffectOnAResolutionWithoutAwaiting()
    {
      int actualValue = 0;
      int expectedValue = 5;

      _ = Promise<int>.Resolve(5)
        .Tap(value =>
        {
          actualValue = value;
        },
          _ => {}
        );

      await Task.Delay(100);

      Assert.Equal(expectedValue, actualValue);
    }
    
    [Fact]
    public async void ItShouldPerformASideEffectOnARejectionWithoutAwaiting()
    {
      int actualValue = 0;
      int expectedValue = 5;

      _ = Promise<int>.Reject(new ArgumentNullException())
        .Tap(
          _ => {},
          _ =>
          {
            actualValue = 5;
          }
        );

      await Task.Delay(100);

      Assert.Equal(expectedValue, actualValue);
    }
  }

  public class IfFulfilled
  {
    [Fact]
    public async void ItShouldPerformASideEffect()
    {
      int actualValue = 0;
      int expectedValue = 5;

      await Promise<int>.Resolve(5)
        .IfFulfilled(value =>
        {
          actualValue = value;
        });

      Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public async void ItShouldPerformASideEffectWithoutAwaiting()
    {
      int actualValue = 0;
      int expectedValue = 5;

      _ = Promise<int>.Resolve(5)
        .IfFulfilled(value =>
        {
          actualValue = value;
        });

      await Task.Delay(100);

      Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public async void ItShouldNotPerformASideEffectForARejection()
    {
      int actualValue = 0;
      int expectedValue = 0;

      try
      {
        await Promise<int>.Reject(new ArgumentNullException())
          .IfFulfilled(value =>
          {
            actualValue = 5;
          });
      }
      catch(ArgumentNullException)
      {
      }

      Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public async void ItShouldNotPerformASideEffectForARejectionWithoutAwaiting()
    {
      int actualValue = 0;
      int expectedValue = 0;

      _ =Promise<int>.Reject(new ArgumentNullException())
        .IfFulfilled(_ =>
        {
          actualValue = 5;
        });

      await Task.Delay(100);

      Assert.Equal(expectedValue, actualValue);
    }
  }

  public class IfRejected
  {
    [Fact]
    public async void ItShouldPerformASideEffectWithoutAwaiting()
    {
      int actualValue = 0;
      int expectedValue = 5;

      _ = Promise<int>.Reject(new ArgumentNullException())
        .IfRejected(_ =>
        {
          actualValue = 5;
        });

      await Task.Delay(100);

      Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public async void ItShouldNotPerformASideEffectForAResolution()
    {
      int actualValue = 0;
      int expectedValue = 0;

      try
      {
        await Promise<int>.Resolve(5)
          .IfRejected(_ =>
          {
            actualValue = 5;
          });
      }
      catch(ArgumentNullException)
      {
      }

      Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public async void ItShouldNotPerformASideEffectForAResolutionWithoutAwaiting()
    {
      int actualValue = 0;
      int expectedValue = 0;

      _ =Promise<int>.Resolve(5)
        .IfRejected(_ =>
        {
          actualValue = 5;
        });

      await Task.Delay(100);

      Assert.Equal(expectedValue, actualValue);
    }
  }

  public class Constructors
  {
    public class WithRawExecutor
    {
      private Action<Action<int>, Action<Exception>> MakeSuccessfulExecutor(int returnValue)
      {
        return (resolve, _) => resolve(returnValue);
      }

      private Action<Action<int>, Action<Exception>> MakeFailedExecutor(Exception exception)
      {
        return (_, reject) => reject(exception);
      }
      
      private Action<Action<int>, Action<Exception>> MakeThrowingExecutor(Exception exception)
      {
        return (_, _) => throw exception;
      }
      
      [Fact]
      public void ItShouldProduceAResolutionForAValue()
      {
        IPromise<int> testPromise = new Promise<int>(MakeSuccessfulExecutor(1));
        
        Assert.True(testPromise.IsFulfilled);
      }

      [Fact]
      public async void ItShouldHaveTheExpectedValue()
      {
        int expectedValue = 1;
        int actualValue = await new Promise<int>(MakeSuccessfulExecutor(1));

        await Task.Delay(100);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public void ItShouldProduceARejectionForAnException()
      {
        IPromise<int> testPromise = new Promise<int>(MakeFailedExecutor(new ArgumentNullException()));
        
        Assert.True(testPromise.IsRejected);
      }

      [Fact]
      public void ItShouldProduceARejectForThrowing()
      {
        IPromise<int> testPromise = new Promise<int>(MakeThrowingExecutor(new ArgumentNullException()));
        
        Assert.True(testPromise.IsRejected);
      }
    }

    public class WithTaskExecutor
    {
      private Action<Action<Task<int>>, Action<Exception>> MakeSuccessfulExecutor(int returnValue)
      {
        return (resolve, _) => resolve(Task.FromResult(returnValue));
      }

      private Action<Action<Task<int>>, Action<Exception>> MakeFailedExecutor(Exception exception)
      {
        return (_, reject) => reject(exception);
      }
      
      private Action<Action<Task<int>>, Action<Exception>> MakeThrowingExecutor(Exception exception)
      {
        return (_, _) => throw exception;
      }
      
      [Fact]
      public void ItShouldProduceAResolutionForAValue()
      {
        IPromise<int> testPromise = new Promise<int>(MakeSuccessfulExecutor(1));
        
        Assert.True(testPromise.IsFulfilled);
      }

      [Fact]
      public async void ItShouldHaveTheExpectedValue()
      {
        int expectedValue = 1;
        int actualValue = await new Promise<int>(MakeSuccessfulExecutor(1));

        await Task.Delay(100);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public void ItShouldProduceARejectionForAnException()
      {
        IPromise<int> testPromise = new Promise<int>(MakeFailedExecutor(new ArgumentNullException()));
        
        Assert.True(testPromise.IsRejected);
      }

      [Fact]
      public void ItShouldProduceARejectForThrowing()
      {
        IPromise<int> testPromise = new Promise<int>(MakeThrowingExecutor(new ArgumentNullException()));
        
        Assert.True(testPromise.IsRejected);
      }      
    }

    public class WithPromiseExecutor
    {
      private Action<Action<IPromise<int>>, Action<Exception>> MakeSuccessfulExecutor(int returnValue)
      {
        return (resolve, _) => resolve(Promise<int>.Resolve(returnValue));
      }

      private Action<Action<IPromise<int>>, Action<Exception>> MakeFailedExecutor(Exception exception)
      {
        return (_, reject) => reject(exception);
      }
      
      private Action<Action<IPromise<int>>, Action<Exception>> MakeThrowingExecutor(Exception exception)
      {
        return (_, _) => throw exception;
      }
      
      [Fact]
      public async void ItShouldProduceAResolutionForAValue()
      {
        IPromise<int> testPromise = new Promise<int>(MakeSuccessfulExecutor(1));

        await Task.Delay(100);
        
        Assert.True(testPromise.IsFulfilled);
      }

      [Fact]
      public async void ItShouldHaveTheExpectedValue()
      {
        int expectedValue = 1;
        int actualValue = await new Promise<int>(MakeSuccessfulExecutor(1));

        await Task.Delay(100);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public void ItShouldProduceARejectionForAnException()
      {
        IPromise<int> testPromise = new Promise<int>(MakeFailedExecutor(new ArgumentNullException()));
        
        Assert.True(testPromise.IsRejected);
      }

      [Fact]
      public void ItShouldProduceARejectForThrowing()
      {
        IPromise<int> testPromise = new Promise<int>(MakeThrowingExecutor(new ArgumentNullException()));
        
        Assert.True(testPromise.IsRejected);
      }
    }
  }

  public class StaticMethods
  {
    public class All
    {
      [Fact]
      public async void ItShouldCompleteAllTasks()
      {
        List<Func<Task<string>>> testTasks = new()
        {
          () => Task.Factory.StartNew(() =>
          {
            Task.Delay(1).GetAwaiter().GetResult();
            return "1";
          }),
          () => Task.Factory.StartNew(() =>
          {
            Task.Delay(1).GetAwaiter().GetResult();
            return "2";
          }),
          () => Task.Factory.StartNew(() =>
          {
            Task.Delay(1).GetAwaiter().GetResult();
            return "3";
          }),
          () => Task.Factory.StartNew(() =>
          {
            Task.Delay(1).GetAwaiter().GetResult();
            return "4";
          }),
          () => Task.Factory.StartNew(() =>
          {
            Task.Delay(1).GetAwaiter().GetResult();
            return "5";
          })
        };
        IEnumerable<string> expectedValue = new List<string>() { "1", "2", "3", "4", "5" };
        IEnumerable<string> actualValue = await Promise<string>.All(testTasks);

        Assert.Equal(expectedValue, actualValue);
      }

      [Fact]
      public async void ItShouldReturnARejectionIfATaskFails()
      {
        List<Func<Task<string>>> testTasks = new()
        {
          () => Task.Factory.StartNew(() =>
          {
            Task.Delay(1).GetAwaiter().GetResult();
            return "1";
          }),
          () => Task.Factory.StartNew(() =>
          {
            Task.Delay(1).GetAwaiter().GetResult();
            return "2";
          }),
          () => Task<string>.Factory.StartNew(() => throw new ArgumentNullException()),
          () => Task.Factory.StartNew(() =>
          {
            Task.Delay(1).GetAwaiter().GetResult();
            return "4";
          }),
          () => Task.Factory.StartNew(() =>
          {
            Task.Delay(1).GetAwaiter().GetResult();
            return "5";
          })
        };
        IPromise<IEnumerable<string>> testPromise = Promise<string>.All(testTasks);

        await Task.Delay(100);
        
        Assert.True(testPromise.IsRejected);
      }
    }

    public class Any
    {
      [Fact]
      public async void ItShouldReturnASingleResolvedValue()
      {
        List<Func<Task<string>>> testTasks = new()
        {
          () => Task.Factory.StartNew(() =>
          {
            Task.Delay(1).GetAwaiter().GetResult();
            return "1";
          }),
          () => Task.Factory.StartNew(() =>
          {
            Task.Delay(1).GetAwaiter().GetResult();
            return "2";
          }),
          () => Task.Factory.StartNew(() =>
          {
            Task.Delay(1).GetAwaiter().GetResult();
            return "3";
          }),
          () => Task.Factory.StartNew(() =>
          {
            Task.Delay(1).GetAwaiter().GetResult();
            return "4";
          }),
          () => Task.Factory.StartNew(() =>
          {
            Task.Delay(1).GetAwaiter().GetResult();
            return "5";
          }),
        };
        IEnumerable<string> expectedValue = new List<string> { "1", "2", "3", "4", "5" };
        string actualValue = await Promise<string>.Any(testTasks);

        Assert.Contains(actualValue, expectedValue);
      }

      [Fact]
      public async void ItShouldReturnARejectionIfTheWinningTaskThrows()
      {
        List<Func<Task<string>>> testTasks = new()
        {
          () => Task.Factory.StartNew(() =>
          {
            Task.Delay(100).GetAwaiter().GetResult();
            return "1";
          }),
          () => Task.Factory.StartNew(() =>
          {
            Task.Delay(100).GetAwaiter().GetResult();
            return "2";
          }),
          () => Task<string>.Factory.StartNew(() => throw new ArgumentNullException()),
          () => Task.Factory.StartNew(() =>
          {
            Task.Delay(100).GetAwaiter().GetResult();
            return "4";
          }),
          () => Task.Factory.StartNew(() =>
          {
            Task.Delay(100).GetAwaiter().GetResult();
            return "5";
          })
        };
        IPromise<string> testPromise = Promise<string>.Any(testTasks);

        await Task.Delay(100);

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await testPromise);
      }

      [Fact]
      public async void ItShouldNotThrowIfAResolutionHappensFirst()
      {
        List<Func<Task<string>>> testTasks = new()
        {
          () => Task.Factory.StartNew(() =>
          {
            Task.Delay(1).GetAwaiter().GetResult();
            return "1";
          }),
          () => Task.Factory.StartNew(() =>
          {
            Task.Delay(1).GetAwaiter().GetResult();
            return "2";
          }),
          () => Task<string>.Factory.StartNew(() =>
          {
            Task.Delay(100).GetAwaiter().GetResult();
            throw new ArgumentNullException();
          }),
          () => Task.Factory.StartNew(() =>
          {
            Task.Delay(1).GetAwaiter().GetResult();
            return "4";
          }),
          () => Task.Factory.StartNew(() =>
          {
            Task.Delay(1).GetAwaiter().GetResult();
            return "5";
          })
        };
        IEnumerable<string> expectedValue = new List<string> { "1", "2", "3", "4", "5" };
        string actualValue = await Promise<string>.Any(testTasks);

        Assert.Contains(actualValue, expectedValue);
      }

      [Fact]
      public async void ItShouldTakeAnIEnumerableOfPromises()
      {
        List<Promise<string>> testPromises = new()
        {
          (Promise<string>) Promise<string>.Resolve(Task.Factory.StartNew(() =>
          {
            Task.Delay(500).GetAwaiter().GetResult();
            return "1";
          })),
          (Promise<string>) Promise<string>.Resolve(Task.Factory.StartNew(() =>
          {
            Task.Delay(500).GetAwaiter().GetResult();
            return "2";
          })),
          (Promise<string>) Promise<string>.Resolve(Task.Factory.StartNew(() =>
          {
            Task.Delay(500).GetAwaiter().GetResult();
            return "3";
          })),
          (Promise<string>) Promise<string>.Resolve(Task.Factory.StartNew(() =>
          {
            Task.Delay(1).GetAwaiter().GetResult();
            return "4";
          })),
          (Promise<string>) Promise<string>.Resolve(Task.Factory.StartNew(() =>
          {
            Task.Delay(500).GetAwaiter().GetResult();
            return "5";
          }))
        };
        //IEnumerable<string> expectedValue = new List<string> { "1", "2", "3", "4", "5" };
        string expectedValue = "4";
        string actualValue = await Promise<string>.Any(testPromises);

        Assert.Equal(expectedValue, actualValue);
      }
    }

    public class RejectIf
    {
      [Fact]
      public async void ItShouldRejectForAFailedPredicate()
      {
        IPromise<int> testPromise = Promise<int>.RejectIf(value => value % 2 == 0, value => new ArgumentException())(1);
        
        await Task.Delay(100);
        
        Assert.True(testPromise.IsRejected);
      }

      [Fact]
      public async void ItShouldResolveForASuccessfulPredicate()
      {
        IPromise<int> testPromise = Promise<int>.RejectIf(value => value % 2 == 0, value => new ArgumentException())(2);
        
        await Task.Delay(100);
        
        Assert.True(testPromise.IsFulfilled);
      }
    }
  }
}
