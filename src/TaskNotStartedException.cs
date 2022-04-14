using System;

namespace RLC.Promises;

public class TaskNotStartedException : Exception
{
  public TaskNotStartedException() : base("Task was not started correctly.")
  {
  }
}