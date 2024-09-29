
using System;
using Algos;
using Godot;


public partial class Mob : CharacterBody2D
{
  [Export] public BaseCharacterResource mobResource;
  Node2D target;
  Healthbar healthbar;
  double timer = 0;
  [Export] public AnimatedSprite2D AnimationPlayer { get; set; }


  public override void _Ready()
  {
    target = GetTree().GetFirstNodeInGroup("Player") as Node2D;
    healthbar = GetNode<Healthbar>("Healthbar");
    mobResource ??= ResourceLoader.Load<BaseCharacterResource>("res://Resources/BaseCharacter/Mob_Level_1.tres");
    AnimationPlayer = GetNode<AnimatedSprite2D>("Portrait");

    GetNode<AnimatedSprite2D>("Portrait").SpriteFrames = mobResource.AnimatedFrames;
  }

  public override void _Process(double delta)
  {
    if (!healthbar.IsAlive)
    {
      TimerUtils.CreateTimer(() => QueueFree(), this, 2f);
      SetProcess(false);
      return;
    }
    UpdateTarget();
    Movement(delta);
  }

  private void Movement(double delta)
  {

    var direction = (target.Position - Position).Normalized();

    Position += new Vector2((float)(direction.X * delta * mobResource.Speed), (float)(direction.Y * delta * mobResource.Speed));

    if (MathF.Abs(target.Position.X - Position.X) < 20f && MathF.Abs(target.Position.Y - Position.Y) < 20f)
    {
      timer += delta;
      if (timer > mobResource.Abilities[0].Cooldown)
      {
        Attack();
        timer = 0;
      }
    }
    AnimationPlayer.FlipH = direction.X < 0;

    MoveAndSlide();
  }

  public void Attack()
  {
    //TODO:
    // 1. Find player
    // 2. Call Take Damage
    if (target.HasNode("Healthbar"))
    {
      Callable callable = Callable.From(() =>
        target.GetNode<Healthbar>("Healthbar").TakeDamage(mobResource.Abilities[0].Damage)
      );
      GetNode<AnimatedSprite2D>("Portrait").Play("punch");
      TimerUtils.CreateTimer(() =>
        {
          target.GetNode<Healthbar>("Healthbar").TakeDamage(mobResource.Abilities[0].Damage);
          GetNode<AnimatedSprite2D>("Portrait").Play("default");

        }, this, .1f);

    }
  }

  public void UpdateTarget()
  {
    target = GetTree().GetFirstNodeInGroup("Player") as Node2D;

  }
}
