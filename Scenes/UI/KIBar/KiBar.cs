using Godot;
using System;

public partial class KiBar : ProgressBar
{
  public override void _Ready()
  {
    EventRegistry.RegisterEvent("OnKiChanged");
    EventSubscriber.SubscribeToEvent("OnKiChanged", OnKiChanged);
    EventRegistry.RegisterEvent("SetInitialKIValue");
    EventSubscriber.SubscribeToEvent("SetInitialKIValue", SetInitialKIValue);
  }

  private void SetInitialKIValue(object sender, object[] args)
  {
    if (args[2] is not Player) return;

    if (args[0] is int kiValue && args[1] is int maxKI)
    {
      MaxValue = maxKI;
      Value = kiValue;
    }
  }
  private void OnKiChanged(object sender, object[] args)
  {
    if (args[1] is not Player) return;
    if (args[0] is int kiValue)
    {
      Value = kiValue;
    }
  }

  public override void _ExitTree()
  {
    EventSubscriber.UnsubscribeFromEvent("OnKiChanged", OnKiChanged);
    EventSubscriber.UnsubscribeFromEvent("SetInitialKIValue", SetInitialKIValue);
    EventRegistry.UnregisterEvent("SetInitialKIValue");
    EventRegistry.UnregisterEvent("OnKiChanged");
  }
}
