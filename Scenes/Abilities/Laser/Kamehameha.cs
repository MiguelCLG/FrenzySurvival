using Godot;
using System;

public partial class Kamehameha : Ability
{

  [Export] public AbilityResource LaserResource;

  Laser Laser;
  public override void _Ready()
  {
    Laser = GetNode<Laser>("%LaserRaycast");
    EventRegistry.RegisterEvent("KamehameHit");
    EventSubscriber.SubscribeToEvent("KamehameHit", KamehameHit);
  }

  public async void FireLaser()
  {
    Laser.SetDirection(CurrentVelocity.X > 0 ? 1 : -1);
    AnimationPlayer.Play("punch");
    Laser.SetIsCasting(true);
    await ToSignal(GetTree().CreateTimer(1.5f, false, true), "timeout");
    Laser.SetIsCasting(false);
    AnimationPlayer.Play("default");
    await ToSignal(GetTree().CreateTimer(LaserResource.Cooldown, false, true), "timeout");
    EventRegistry.GetEventPublisher("ActionFinished").RaiseEvent(new object[] { });
  }
  public override void Action()
  {

    CallDeferred("FireLaser");

  }

  public void KamehameHit(object sender, object[] args)
  {

    if (args[0] is Node2D body)
    {
      var healthbar = body.GetNode<Healthbar>("Healthbar");

      // If the object gets out and gets in again, he gets hit again
      // Save a list of elements entered
      // if element is not on the list, he gets hit and added to the list
      // when laser is over, clear the list
      if (healthbar.IsAlive)
        EventRegistry.GetEventPublisher("TakeDamage").RaiseEvent(new object[] {
              healthbar,
              LaserResource.Damage
            });
    }
  }

  public override void _ExitTree()
  {
    EventSubscriber.UnsubscribeFromEvent("KamehameHit", KamehameHit);
    EventRegistry.UnregisterEvent("KamehameHit");
  }
}
