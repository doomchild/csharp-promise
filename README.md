# RLC.Promises

A Typescript-like Promise library for .NET.

## Rationale

Asynchronous code (particularly in C#) typically relies on using the `async`/`await` feature introduced in C# 5.0.  This has a lot of benefits, but it unfortunately tends to push code into an imperative style.  This library aims to make writing asychronous functional code easier, cleaner, and less error-prone.

## Installation

Install RLC.Promises as a NuGet package, via an IDE package manager, or using the command-line instructions
at [nuget.org][].

## API

### IPromise<T>

This interface is the main interface exported by the library, and should be used as the return value from any Promise-returning functions.  Note that `IPromise<T>` instances _can_ be awaited like `Tasks`, but _cannot_ be passed to functions expecting `Task<T>` instances, as awaiting is not done by descending from an interface or base class, but rather by implementing the "magic" method `GetAwaiter`.

### Promise<T>

#### Creating a new Promise<T>

There are three ways to create a `Promise<T>` instance.

##### Promise<T>.Resolve

###### Raw value

To create a resolved `Promise` from a raw value, `Resolve` is the easiest way to go.

###### Task

`Resolve` will take a `Task<T>`.  However, if the `Task<T>` throws an exception, the resulting `Promise<T>` will be in a rejected state with the exception that was thrown.

##### Promise<T>.Reject

To create a rejected `Promise` from an exception object, `Reject` is the easiest way to go.

##### Promise<T>.RejectIf

`RejectIf` can be used to create a `Promise` based on some function, or can be used for validation, as follows:

```c#
Promise<int>.Resolve(1)
  .Then(Promise<int>.RejectIf(value => value % 2 == 0, value => new ArgumentException($"{nameof(value)} was not even!")))
  .Tap(
    value => _logger.LogInformation("Value was even: {Value}", value),
    exception => _logger.LogException(exception)
  );
```

##### Constructor

The `Promise<T>` constructor takes an `Action<Action<T>, Action<Exception>>` to allow callback-oriented code to resolve or reject a value/exception in order to enter a `Promise<T>`-oriented workflow.  This is analogous to Javascript/Typescript `Promise`s, where you might occasionally need to write something along the lines of:

```typescript
new Promise((resolve, reject) => {
  try {
    performCallback(resolve);
  } catch (Error error) {
    reject(error);
  }
 }
```

#### Chaining

##### Then

Once a `Promise<T>` has been created, successive operations can be chained using the `Then` method.

```c#
HttpClient client = new ();

Promise<string>.Resolve("https://www.google.com/")
  .Then(client.GetAsync)
  .Then(responseMessage => responseMessage.StatusCode);
```

##### Catch

When a `Promise<T>` enters a rejected state, the `Catch` method can be used to deal with the exception.

```c#
HttpClient client = new ();

Promise<string>.Resolve("not-a-url")
  .Then(client.GetAsync)
  .Catch(exception => exception.Message)
  .Then(message => message.Length);
```

##### IfFulfilled/IfRejected/Tap

The `IfFulfilled` and `IfRejected` methods can be used to perform side effects such as logging when the `Promise<T>` is in the fulfilled or rejected state, respectively.

```c#
HttpClient client = new ();

Promise<string>.Resolve("https://www.google.com/")
  .Then(client.GetAsync)
  .IfFulfilled(response => _logger.LogDebug("Got response {Response}", response)
  .Then(response => response.StatusCode);
```

```c#
HttpClient client = new ();

Promise<string>.Resolve("not-a-url")
  .Then(client.GetAsync)
  .IfRejected(exception => _logger.LogException(exception, "Failed to get URL")
  .Catch(exception => exception.Message)
  .Then(message => message.Length);
```

The `Tap` method takes both an `onFulfilled` and `onRejected` `Action` in the event that you want to perform some side effect on both sides of the `Promise` at a single time.

```c#
HttpClient client = new ();

Promise<string>.Resolve(someExternalUrl)
  .Then(client.GetAsync)
  .Tap(
    response => _logger.LogDebug("Got response {Response}", response),
    exception => _logger.LogException(exception, "Failed to get URL")
  )
```

#### Static Methods

`Promise<T>` has some useful static methods for dealing with multiple `Promise<T>`s or `Task<T>`s at once.

##### Promise<T>.All

This method takes an `IEnumerable<Task<T>` of existing `Task<T>` instances or an `IEnumerable<Func<Task<T>>>` of `Task<T>` supplier functions and returns an `IEnumerable<T>` of all of their results or a rejected `Promise<T>` of the first exception thrown.

##### Promise<T>.Any

This method takes an `IEnumerable<Task<T>` of existing `Task<T>` instances or an `IEnumerable<Func<Task<T>>>` of `Task<T>` supplier functions and returns the result of the first to finish or a rejected `Promise<T>` of the first exception thrown.

#### Getting Back Out of a Promise

Occasionally you need to escape from the world of `Promise`, either because existing code requires a raw value or `Task`, or because you're starting a workflow that you want separated from the current one.  In these cases, you should `await` your `IPromise<T>` instance.  This will return a `Task<T>` if the `Promise` was in a fulfilled state, and will throw the exception contained in a rejected state.  Generally, this is something you would do at the top level of a program, or anywhere that you need to return something to framework code (like a WebApi controller).

[nuget.org]: https://www.nuget.org/packages/Jds.LanguageExt.Extras/
