using System;
using System.Threading.Tasks;

namespace RLC.Promises;

internal static class TaskExtensions
{
  public static Task<TNext> Fold<T, TNext>(this Task<T> task, Func<Exception, TNext> leftMap, Func<T, TNext> rightMap)
  {
    return task.ContinueWith(continuationTask => continuationTask.IsFaulted 
      ? continuationTask.Exception?.InnerException != null
        ? leftMap(continuationTask.Exception.InnerException) 
        : leftMap(continuationTask.Exception!) 
      : rightMap(continuationTask.GetAwaiter().GetResult())
    );
  }
}