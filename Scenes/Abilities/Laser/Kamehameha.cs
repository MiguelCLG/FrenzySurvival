using Godot;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public partial class Kamehameha : Ability

{
  [Export] public Vector2 EnergyBallPosition = new(13, 0);
  [Export] public Vector2 EnergyBallScale = new(.1f, .5f);
  Laser Laser;
  EnergyBall energyBall;

  public override void _Ready()
  {
    base._Ready();
    Laser = GetNode<Laser>("%LaserRaycast");
    Laser.targetGroup = targetGroup;
    energyBall = Laser.GetNode<EnergyBall>("EnergyBall");

    EventRegistry.RegisterEvent("KamehameHit");
    EventSubscriber.SubscribeToEvent("KamehameHit", KamehameHit);
    if (!EventRegistry.HasEventBeenRegistered("DirectionChanged"))
      EventRegistry.RegisterEvent("DirectionChanged");
    EventSubscriber.SubscribeToEvent("DirectionChanged", DirectionChanged);
  }

  public override void SetTargetGroup(string group)
  {
    base.SetTargetGroup(group);
    Laser.targetGroup = group;
  }

  public async void FireLaser(CancellationToken token)
  {
    try
    {
      await ToSignal(GetTree().CreateTimer(.2f, false, true), "timeout");
      if (token.IsCancellationRequested) return;
      energyBall.GetNode<CpuParticles2D>("CPUParticles2D").ScaleAmountMin = EnergyBallScale.X;
      energyBall.GetNode<CpuParticles2D>("CPUParticles2D").ScaleAmountMax = EnergyBallScale.Y;
      AnimationPlayer.Play("beam_charge");
      audioManager?.Play(abilitySound, this);

      // Set the energy ball's position based on the facing direction
      Vector2 energyBallPosition = new Vector2(EnergyBallPosition.X * -facingDirection, EnergyBallPosition.Y) - Position;
      energyBall.Position = energyBallPosition;

      energyBall.ActivateEnergyBall();
      await ToSignal(GetTree().CreateTimer(2f, false, true), "timeout");
      if (token.IsCancellationRequested) return;

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
      if (token.IsCancellationRequested) return;
      EventRegistry.GetEventPublisher("ActionFinished").RaiseEvent(new object[] { this });
    }
    catch (TaskCanceledException)
    {
      cooldownTimer.Free();
      energyBall.DeactivateEnergyBall();
      Laser.SetIsCasting(false);
    }

  }

  public override void Action()
  {
    cancellationTokenSource = new CancellationTokenSource();
    CancellationToken token = cancellationTokenSource.Token;
    currentTask = Task.Run(() => FireLaser(token), token);
  }

  public override void Cancel()
  {

    cancellationTokenSource.Cancel();  // Cancel the task
    energyBall.DeactivateEnergyBall();
    Laser.SetIsCasting(false);
    EventRegistry.GetEventPublisher("ActionCanceled").RaiseEvent(new object[] { this });
    base.Cancel();
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
        mob.KnockBack(curDirection, abilityResource.Value);
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

    if (IsInstanceValid(energyBall) && !energyBall.IsQueuedForDeletion() && energyBall.GetParent() == this)
    {//Verify that the direction changed was done by the correct unit (mob or player)
      if (args[1] is Node2D node)
      {
        if (!GetParent().GetParent().Equals(node)) return;
      }
      facingDirection = (int)args[0];
      Laser.GetNode<EnergyBall>("EnergyBall").Position = new Vector2(EnergyBallPosition.X * -facingDirection, EnergyBallPosition.Y) - Position;
    }
  }

  public override void _ExitTree()
  {
    EventSubscriber.UnsubscribeFromEvent("KamehameHit", KamehameHit);
    EventSubscriber.UnsubscribeFromEvent("DirectionChanged", DirectionChanged);

  }
}
