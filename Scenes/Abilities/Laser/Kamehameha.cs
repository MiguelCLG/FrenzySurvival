using Godot;
using System;

public partial class Kamehameha : Ability
{

  Laser Laser;

  public override void _Ready()
  {
    Laser = GetNode<Laser>("%LaserRaycast");
    EventRegistry.RegisterEvent("KamehameHit");
    EventSubscriber.SubscribeToEvent("KamehameHit", KamehameHit);
  }

  public async void FireLaser()
  {
    EnergyBall energyBall = Laser.GetNode<EnergyBall>("EnergyBall");
    AnimationPlayer.Play("beam_charge");

    // Set the energy ball's position based on the facing direction
    Vector2 energyBallPosition = new Vector2(13 * -facingDirection, 0) - Position;
    energyBall.Position = energyBallPosition;

    energyBall.ActivateEnergyBall();
    await ToSignal(GetTree().CreateTimer(2f, false, true), "timeout");
    energyBall.DeactivateEnergyBall();

    Laser.SetDirection(facingDirection);
    AnimationPlayer.Play("beam");
    Laser.SetIsCasting(true);
    Position = new Vector2(-28 * -facingDirection, 0);
    await ToSignal(GetTree().CreateTimer(abilityResource.CastTime, false, true), "timeout");
    Laser.SetIsCasting(false);
    AnimationPlayer.Play("default");
    await ToSignal(GetTree().CreateTimer(abilityResource.Cooldown, false, true), "timeout");
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
      if (body is Mob mob)
      {
        Vector2 curDirection = facingDirection == 1 ? Vector2.Right : Vector2.Left;
        mob.KnockBack(curDirection, 100);
      }

      if (healthbar.IsAlive)
        EventRegistry.GetEventPublisher("TakeDamage").RaiseEvent(new object[] {
              healthbar,
              abilityResource.Damage
            });
    }
  }

  public override void _ExitTree()
  {
    EventSubscriber.UnsubscribeFromEvent("KamehameHit", KamehameHit);
    EventRegistry.UnregisterEvent("KamehameHit");
  }
}
