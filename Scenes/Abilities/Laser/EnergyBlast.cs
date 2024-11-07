using System;
using Godot;

public partial class EnergyBlast : Ability
{
  [Export] public float speed = 100.0f;
  [Export] public Vector2 EnergyBallScale = new(1, 1);
  [Export] Gradient energyBlastGradient;
  [Export] public Vector2 EnergyBallPosition = new(13, 0);
  [Export] PackedScene energyBallScene;
  int direction = 1;
  public EnergyBall energyBall;
  public Area2D energyBallArea;

  public override void _Ready()
  {
    SetProcess(false);
    EventRegistry.RegisterEvent("OnEnergyBallHit");
    EventSubscriber.SubscribeToEvent("OnEnergyBallHit", OnEnergyBallHit);
    EventSubscriber.SubscribeToEvent("DirectionChanged", DirectionChanged);

  }
  public override void _Process(double delta)
  {
    energyBall.Position += new Vector2(speed * direction, 0) * (float)delta;
    speed += (float)delta * 2;
  }

  public override void Action()
  {
    SetProcess(false);
    energyBall = energyBallScene.Instantiate<EnergyBall>();
    energyBall.GlobalPosition = GlobalPosition;
    energyBall.Position = EnergyBallPosition;
    AddChild(energyBall);
    energyBallArea = energyBall.GetNode<Area2D>("CollisionArea");
    energyBall.GetNode<CpuParticles2D>("CPUParticles2D").ColorRamp = energyBlastGradient;

    CallDeferred("StartLaunchAsync");
  }
  public async void StartLaunchAsync()
  {
    energyBallArea.Scale = Vector2.Zero;
    energyBall.GetNode<CpuParticles2D>("CPUParticles2D").Emitting = true;
    AnimationPlayer?.Play("beam_charge");
    Vector2 energyBallPosition = new Vector2(EnergyBallPosition.X * -facingDirection, EnergyBallPosition.Y) - Position;
    energyBall.Position = energyBallPosition;

    await ToSignal(GetTree().CreateTimer(2f, false, true), "timeout");
    energyBallArea.Scale = EnergyBallScale;

    RemoveChild(energyBall);
    GetTree().Root.AddChild(energyBall);
    energyBall.GlobalPosition = GlobalPosition + new Vector2(EnergyBallPosition.X * -facingDirection, EnergyBallPosition.Y);

    EventRegistry.GetEventPublisher("IsDoingAction")?.RaiseEvent(new object[] { true }); // locks character in animation
    direction = facingDirection;
    AnimationPlayer?.Play("beam");
    SetProcess(true);
    await ToSignal(GetTree().CreateTimer(abilityResource.CastTime, false, true), "timeout");
    Destroy();
    EventRegistry.GetEventPublisher("IsDoingAction")?.RaiseEvent(new object[] { false }); // unlocks character in animation
    AnimationPlayer?.Play("default");
    await ToSignal(GetTree().CreateTimer(abilityResource.Cooldown, false, true), "timeout");
    EventRegistry.GetEventPublisher("ActionFinished")?.RaiseEvent(new object[] { this });

  }

  public override void _UnhandledInput(InputEvent @event)
  {
    if (@event is InputEventMouseButton)
      StartLaunchAsync();
  }
  public void Destroy()
  {
    SetProcess(false);
    GetTree().Root.RemoveChild(energyBall);
    energyBall.QueueFree();
    energyBall.GlobalPosition = new Vector2(EnergyBallPosition.X * -facingDirection, EnergyBallPosition.Y) + GlobalPosition;
    energyBall.GetNode<CpuParticles2D>("CPUParticles2D").Emitting = false;

  }
  public void OnEnergyBallHit(object sender, object[] args)
  {
    if (args[0] is Node2D bodyEntered)
      if (bodyEntered.IsInGroup(targetGroup))
      {
        var healthbar = bodyEntered.GetNode<Healthbar>("Healthbar");
        if (!healthbar.IsAlive) return;
        if (bodyEntered is Mob mob)
        {
          Vector2 curDirection = facingDirection == 1 ? Vector2.Right : Vector2.Left;
          mob.KnockBack(curDirection, 10);
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
    {
      facingDirection = (int)args[0];
      energyBall.Position = new Vector2(EnergyBallPosition.X * -facingDirection, EnergyBallPosition.Y) - Position;
    }
  }

  public override void _ExitTree()
  {
    EventSubscriber.UnsubscribeFromEvent("OnEnergyBallHit", OnEnergyBallHit);
    EventSubscriber.UnsubscribeFromEvent("DirectionChanged", DirectionChanged);
  }
}
