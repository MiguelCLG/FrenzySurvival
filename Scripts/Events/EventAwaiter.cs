using System;
using System.Threading.Tasks;

public static class EventAwaiter
{
  /// <summary>
  /// Method to await an event to be called once and then unsubscribes, something like the ToSignal method from godot but for events
  /// Uses Native threading tasks from C# System
  /// </summary>
  public static Task AwaitEvent(Action<EventHandler<object>> subscribe, Action<EventHandler<object>> unsubscribe)
  {
    var tcs = new TaskCompletionSource<bool>();

    EventHandler<object> handler = null;
    handler = (sender, args) =>
    {
      unsubscribe(handler);
      tcs.SetResult(true);
    };

    subscribe(handler);

    return tcs.Task;
  }
}

