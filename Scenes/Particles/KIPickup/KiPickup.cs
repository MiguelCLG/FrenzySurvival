using Godot;
using System;

public partial class KiPickup : Node2D
{

  [Export] public int KiValue = 10;
  public void OnBodyEntered(Node2D body)
  {
    if (body.IsInGroup("Player"))
    {
      EventRegistry.GetEventPublisher("SetKI").RaiseEvent(new object[] { KiValue });
      QueueFree();
    }
  }
}
