using Godot;
using System;
using System.Collections.Generic;

public partial class Kamehameha : Ability

{
  [Export] public Vector2 EnergyBallPosition = new(13, 0);
  [Export] public Vector2 EnergyBallScale = new(.1f, .5f);
  Laser Laser;

  public override void _Ready()
  {
    base._Ready();
    Laser = GetNode<Laser>("%LaserRaycast");
    Laser.targetGroup = targetGroup;
    EventRegistry.RegisterEvent("KamehameHit");
    EventSubscriber.SubscribeToEvent("KamehameHit", KamehameHit);
    if (!EventRegistry.HasEventBeenRegistered("DirectionChanged"))
      EventRegistry.RegisterEvent("DirectionChanged");
    EventSubscriber.SubscribeToEvent("DirectionChanged", DirectionChanged);
  }

  public async void FireLaser()
  {
    EnergyBall energyBall = Laser.GetNode<EnergyBall>("EnergyBall");
    energyBall.GetNode<CpuParticles2D>("CPUParticles2D").ScaleAmountMin = EnergyBallScale.X;
    energyBall.GetNode<CpuParticles2D>("CPUParticles2D").ScaleAmountMax = EnergyBallScale.Y;
    AnimationPlayer.Play("beam_charge");
    audioManager?.Play(abilitySound, this);

    // Set the energy ball's position based on the facing direction
    Vector2 energyBallPosition = new Vector2(EnergyBallPosition.X * -facingDirection, EnergyBallPosition.Y) - Position;
    energyBall.Position = energyBallPosition;

    energyBall.ActivateEnergyBall();
    await ToSignal(GetTree().CreateTimer(2f, false, true), "timeout");
    EventRegistry.GetEventPublisher("IsDoingAction").RaiseEvent(new object[] { true }); // Locks the character in animation
    energyBall.DeactivateEnergyBall();

    Laser.SetDirection(facingDirection);
    AnimationPlayer.Play("beam");
    Laser.SetIsCasting(true);
    Position = new Vector2(-28 * -facingDirection, 0);
    await ToSignal(GetTree().CreateTimer(abilityResource.CastTime, false, true), "timeout");
    Laser.SetIsCasting(false);
    EventRegistry.GetEventPublisher("IsDoingAction").RaiseEvent(new object[] { false }); // unlocks character in animation
    AnimationPlayer.Play("default");
    await ToSignal(GetTree().CreateTimer(abilityResource.Cooldown, false, true), "timeout");
    EventRegistry.GetEventPublisher("ActionFinished").RaiseEvent(new object[] { this });
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

  public void DirectionChanged(object sender, object[] args)
  {
    facingDirection = (int)args[0];
    Laser.GetNode<EnergyBall>("EnergyBall").Position = new Vector2(EnergyBallPosition.X * -facingDirection, EnergyBallPosition.Y) - Position;
  }

  public override void _ExitTree()
  {
    EventSubscriber.UnsubscribeFromEvent("KamehameHit", KamehameHit);
    EventRegistry.UnregisterEvent("KamehameHit");
    EventSubscriber.UnsubscribeFromEvent("DirectionChanged", DirectionChanged);

  }
}
