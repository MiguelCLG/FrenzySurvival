using System;
using System.Linq;
using Algos;
using Godot;
using System.Collections.Generic;

public partial class Mob : CharacterBody2D
{
  [Export] public BaseCharacterResource mobResource;
  [Export] public AnimatedSprite2D AnimationPlayer { get; set; }
  private Node2D target;
  public Healthbar healthbar;
  private AbilityManager abilityManager;

  private bool isDoingAction = false;
  private double timer = 0;
  bool isGettingHurt = false;

  private float stopDistance = 30f;
  private AudioManager audioManager;


  public override void _Ready()
  {
    target = GetTree().GetFirstNodeInGroup("Player") as Node2D;
    healthbar = GetNode<Healthbar>("Healthbar");
    mobResource ??= ResourceLoader.Load<BaseCharacterResource>("res://Resources/BaseCharacter/Mob_Level_1.tres");
    AnimationPlayer = GetNode<AnimatedSprite2D>("Portrait");
    abilityManager = GetNode<AbilityManager>("AbilityManager");
    AnimationPlayer.SpriteFrames = mobResource.AnimatedFrames;
    //Need an event to change the hp values in the resource
    healthbar.SetInitialValues(mobResource);
    EventSubscriber.SubscribeToEvent("TakeDamage", TakeDamage);
    if (!EventRegistry.HasEventBeenRegistered("OnPlayerDeath"))
      EventRegistry.RegisterEvent("OnPlayerDeath");
    EventSubscriber.SubscribeToEvent("OnPlayerDeath", OnPlayerDeath);
    audioManager = GetNode<AudioManager>("/root/AudioManager");

    if (!EventRegistry.HasEventBeenRegistered("ActionFinished"))
      EventRegistry.RegisterEvent("ActionFinished");
    EventSubscriber.SubscribeToEvent("ActionFinished", ActionFinished);
    if (!EventRegistry.HasEventBeenRegistered("OnComboFinished"))
      EventRegistry.RegisterEvent("OnComboFinished");
    EventSubscriber.SubscribeToEvent("OnComboFinished", ActionFinished);
    LoadAbilities();
  }

  public override void _Process(double delta)
  {
    if (AnimationPlayer.Animation == "death" || isGettingHurt) return;
    UpdatePositions();
    if (!mobResource.ShowHealBar)
      healthbar.Visible = false;
    if (healthbar.IsAlive)
    {
      UpdateTarget();
      if (mobResource.Speed > 0) // TODO: ??? Why? Must have been drunk, right?
        Movement(delta);
    }
    else
    {
      Die();
    }
  }

  public override void _PhysicsProcess(double delta)
  {
    if (AnimationPlayer.Animation == "death") return;

    MoveAndSlide();
  }

  private void UpdatePositions()
  {
    if (GlobalPosition.DistanceTo(target.GlobalPosition) > 700)
    {
      SetProcess(false);
      SetPhysicsProcess(false);
      Vector2 randomPositionPositive = target.GlobalPosition + new Vector2(Random.Shared.Next(100, 400), Random.Shared.Next(100, 400));
      Vector2 randomPositionNegative = target.GlobalPosition + new Vector2(Random.Shared.Next(-400, -100), Random.Shared.Next(-400, -100));
      GlobalPosition = Random.Shared.NextDouble() > 0.5 ? randomPositionPositive : randomPositionNegative;
      SetProcess(true);
      SetPhysicsProcess(true);
    }
  }

  private void LoadAbilities()
  {
    foreach (var abilityScene in mobResource.MobAttacks)
    {
      var ability = abilityScene.Instantiate<Ability>();
      abilityManager.AddAbility(ability);
    }
    abilityManager.UnsubscribeFromEvents();
    abilityManager.SetKI(mobResource.KI);
    abilityManager.SetTargetGroup("Player");
  }

  private void Movement(double delta)
  {
    var ability = abilityManager.GetNextAbility();

    if (ability.isRangedAbility)
    {
      // Calculate the distance on the X-axis to keep the mob at the specific range from the target
      float targetRangeX = 300.0f; // Adjust this to the desired ranged attack distance
      float distanceX = Mathf.Abs(target.Position.X - Position.X);
      float distanceY = Mathf.Abs(target.Position.Y - Position.Y);

      // Check if mob is at the desired range on the X-axis and within the acceptable Y range
      if (distanceX > targetRangeX || distanceY > 5.0f) // Allow small Y-axis variance
      {
        // Move towards the target X range and adjust Y to align with the player
        Vector2 moveDirection = new Vector2(target.Position.X - Position.X, target.Position.Y - Position.Y).Normalized();

        // OnlY move horizontally until the target range is reached, but also adjust Y if needed
        Position += new Vector2(moveDirection.X, moveDirection.Y) * (float)(mobResource.Speed * delta);

        // Play the move animation
        if (AnimationPlayer.Animation == "default")
        {
          AnimationPlayer.Play("move");
        }
      }
      else if (!isDoingAction)
      {
        // Stop moving and play default animation when at range
        if (AnimationPlayer.Animation == "move")
        {
          AnimationPlayer.Play("default");
        }

        // Perform the next action (e.g., ranged attack)
        isDoingAction = true;
        abilityManager.DoNextActionAsync();
      }

      // Flip sprite based on the direction
      AnimationPlayer.FlipH = target.Position.X < Position.X;
    }
    else
    {
      // Melee ability logic (existing code)
      Vector2 direction = (target.Position - Position).Normalized();
      float distanceToTarget = Position.DistanceTo(target.Position);

      if (distanceToTarget > stopDistance)
      {
        Position += direction * (float)(mobResource.Speed * delta);
        if (AnimationPlayer.Animation == "default")
        {
          AnimationPlayer.Play("move");
        }
      }
      else if (!isDoingAction)
      {
        if (AnimationPlayer.Animation == "move")
        {
          AnimationPlayer.Play("default");
        }
        isDoingAction = true;
        abilityManager.DoNextActionAsync();
      }

      AnimationPlayer.FlipH = direction.X < 0;
    }
    timer += delta;
    if (timer > 1)
    {
      abilityManager.SetKI(abilityManager.GetKI() + 1); // regenerate ki 1/second
      timer = 0;
    }
    abilityManager.SetFacingDirection(AnimationPlayer.FlipH ? -1 : 1);
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
              1
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

        isGettingHurt = true;

        AnimationPlayer.Play("hurt");
        if (mobResource.characterSounds is not null)
        {
          AudioOptionsResource sound = mobResource.characterSounds.GetValueOrDefault("hurt");
          if (sound is not null)
            audioManager?.Play(mobResource.characterSounds.GetValueOrDefault("hurt"), this);
        }
        await ToSignal(AnimationPlayer, "animation_finished");
        if (!healthbar.IsAlive)
        {
          Die();
          return;
        }
        AnimationPlayer.Play("default");
        isGettingHurt = true;

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
    SetProcess(false);
    SetPhysicsProcess(false);
  }
  public void ActionFinished(object sender, object[] args)
  {
    // Se nao vier do ability manager ou ability deste mob, entao retorna
    if (args[0] is Node2D node)
    {
      var isInChildren = abilityManager.GetChildren().Contains(node);
      var isThisAbilityManager = node == abilityManager;
      if (!isThisAbilityManager)
        if (!isInChildren)
          return;
      isDoingAction = false;
    }

  }

  public void HandleLootDrop()
  {
    if (mobResource.LootTables is null)
      return;
    if (mobResource.LootTables.Count == 0)
      return;

    foreach (LootTable lootTable in mobResource.LootTables)
    {
      PackedScene dropItem = lootTable.GetDroppedItem();
      if (dropItem is not null)
      {
        Node2D pickup = dropItem.Instantiate<Node2D>();
        pickup.GlobalPosition = GlobalPosition;
        if (pickup is Pickup realPickup)
        {
          realPickup.ChangeStatChangeValue("experience", mobResource.ExpDropValue);
        }
        GetTree().Root.AddChild(pickup);
      }
    }
  }

  public void Die()
  {
    if (mobResource.characterSounds is not null)
    {
      AudioOptionsResource sound = mobResource.characterSounds.GetValueOrDefault("death");
      if (sound is not null)
        audioManager?.Play(mobResource.characterSounds.GetValueOrDefault("death"), this);
    }
    AnimationPlayer.Play("death");
    EventRegistry.GetEventPublisher("OnMobDeath").RaiseEvent(new object[] { this });
  }
  public override void _ExitTree()
  {
    EventSubscriber.UnsubscribeFromEvent("OnPlayerDeath", OnPlayerDeath);
    EventSubscriber.UnsubscribeFromEvent("TakeDamage", TakeDamage);
    EventSubscriber.UnsubscribeFromEvent("ActionFinished", ActionFinished);
    EventSubscriber.UnsubscribeFromEvent("OnComboFinished", ActionFinished);


  }
}
