
using System;
using Algos;
using Godot;


public partial class Mob : CharacterBody2D
{
  [Export] public BaseCharacterResource mobResource;
  Node2D target;
  public Healthbar healthbar;
  double timer = 0;
  bool isTargetAlive = true;
  [Export] public AnimatedSprite2D AnimationPlayer { get; set; }
  private float stopDistance = 30f;

  public override void _Ready()
  {
    target = GetTree().GetFirstNodeInGroup("Player") as Node2D;
    healthbar = GetNode<Healthbar>("Healthbar");
    mobResource ??= ResourceLoader.Load<BaseCharacterResource>("res://Resources/BaseCharacter/Mob_Level_1.tres");
    AnimationPlayer = GetNode<AnimatedSprite2D>("Portrait");
    AnimationPlayer.SpriteFrames = mobResource.AnimatedFrames;
    //Need an event to change the hp values in the resource
    healthbar.SetInitialValues(mobResource);
    EventSubscriber.SubscribeToEvent("TakeDamage", TakeDamage);
    EventSubscriber.SubscribeToEvent("OnPlayerDeath", OnPlayerDeath);

  }

  public override void _Process(double delta)
  {
    if (healthbar.IsAlive && isTargetAlive)
    {
      UpdateTarget();
      Movement(delta);
    }
  }

  public override void _PhysicsProcess(double delta)
  {
    MoveAndSlide();
  }

  private void Movement(double delta)
  {
    // Calculate direction towards the target
    Vector2 direction = (target.Position - Position).Normalized();

    // Calculate the distance between the enemy and the player
    float distanceToTarget = Position.DistanceTo(target.Position);

    // Only move if the distance is greater than the stop distance
    if (distanceToTarget > stopDistance)
    {
      // Move towards the player, but maintain the offset (stopDistance)
      Position += direction * (float)(mobResource.Speed * delta);

      // Play the move animation
      if (AnimationPlayer.Animation == "default")
      {
        AnimationPlayer.Play("move");
      }
    }
    else
    {
      // Stop moving and play default animation
      if (AnimationPlayer.Animation == "move")
      {
        AnimationPlayer.Play("default");
      }

      // Optionally perform other actions like attacking when close enough
      timer += delta;
      if (timer > mobResource.Abilities[0].Cooldown)
      {
        Attack();
        timer = 0;
      }
    }

    // Flip sprite based on direction
    AnimationPlayer.FlipH = direction.X < 0;

    // Optionally, you can also use MoveAndSlide for collisions, depending on the physics of your game
    // MoveAndSlide();
  }

  public void Attack()
  {
    if (target.HasNode("Healthbar"))
    {
      var targetHealthbar = target.GetNode<Healthbar>("Healthbar");
      AnimationPlayer.Play("punch");
      TimerUtils.CreateTimer(() =>
        {
          if (!targetHealthbar.IsQueuedForDeletion())
            EventRegistry.GetEventPublisher("TakeDamage").RaiseEvent(new object[] {
              targetHealthbar,
              mobResource.Abilities[0].Damage
            });
          //targetHealthbar.TakeDamage(mobResource.Abilities[0].Damage);
          AnimationPlayer.Play("default");

        }, this, .1f);

    }
  }

  public void UpdateTarget()
  {
    target = GetTree().GetFirstNodeInGroup("Player") as Node2D;

  }

  public async void TakeDamage(object sender, object[] args)
  {
    if (AnimationPlayer.Animation == "death") return;
    if (args[0] is Healthbar healthbar)
    {
      if (healthbar.Equals(GetNode<Healthbar>("Healthbar")))
      {
        SetProcess(false);

        healthbar.TakeDamage(float.Parse(args[1].ToString()));

        AnimationPlayer.Play("hurt");
        await ToSignal(AnimationPlayer, "animation_finished");
        if (!healthbar.IsAlive)
        {
          AnimationPlayer.Play("death");
          EventRegistry.GetEventPublisher("OnMobDeath").RaiseEvent(new object[] { this });
          return;
        }


        AnimationPlayer.Play("default");
        SetProcess(true);
      }
    }
  }

  public async void KnockBack(Vector2 dir, float frc)
  {
    if (AnimationPlayer.Animation == "death") return;
    SetProcess(false);
    SetPhysicsProcess(true);
    await ToSignal(GetTree().CreateTimer(.1f), "timeout");
    Velocity = dir * frc;
    await ToSignal(GetTree().CreateTimer(1f), "timeout");
    Velocity = Vector2.Zero;
    SetPhysicsProcess(false);
    SetProcess(true);
  }

  public void OnPlayerDeath(object sender, object[] args)
  {
    isTargetAlive = false;
  }

  public override void _ExitTree()
  {
    EventSubscriber.UnsubscribeFromEvent("OnPlayerDeath", OnPlayerDeath);
    EventSubscriber.UnsubscribeFromEvent("TakeDamage", TakeDamage);

  }
}
