using System;
using System.Collections.Generic;
using Algos;
using Godot;
using Godot.Collections;


public partial class Mob : CharacterBody2D
{
  [Export] public BaseCharacterResource mobResource;
  [Export] public AnimatedSprite2D AnimationPlayer { get; set; }
  public Node2D target;
  public Healthbar healthbar;
  public AbilityManager abilityManager;
  public Vector2 motion = Vector2.Zero;

  private double timer = 0;
  protected bool isDoingAction = false;
  protected bool isGettingHurt = false;

  protected AudioManager audioManager;
  private float stopDistance = 50f;

  public Array<string> lockedAnimations = new() { "hurt", "death", "beam", "beam_charge" };

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
    if (!EventRegistry.HasEventBeenRegistered("IncreaseStatsFromDictionary"))
      EventRegistry.RegisterEvent("IncreaseStatsFromDictionary");
    EventSubscriber.SubscribeToEvent("IncreaseStatsFromDictionary", IncreaseStatsFromDictionary);
    LoadAbilities();
  }

  public override void _Process(double delta)
  {
    if (AnimationPlayer.Animation == "death" || isGettingHurt) return;
    UpdatePositions();
    if (!mobResource.ShowHealBar)
      healthbar.Visible = false;
  }

  public override void _PhysicsProcess(double delta)
  {
    if (AnimationPlayer.Animation == "death") return;
    MoveAndCollide(motion, false, 1f, true);
    if(isGettingHurt) return;

    if (healthbar.IsAlive)
    {
      UpdateTarget();
      if (mobResource.Speed > 0 && !isDoingAction)
      {
        Movement(delta);
        MoveAndCollide(motion, false, 1f, true);
      }
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
    EventRegistry.GetEventPublisher("SetInitialKIValue").RaiseEvent(new object[] { mobResource.KI, mobResource.MaxKI, this });
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

  private void Movement(double delta)
  {
    var ability = abilityManager.GetNextAbility();

    // Update velocity for all abilities
    foreach (Ability _ability in GetNode("AbilityManager").GetChildren())
    {
      _ability.CurrentVelocity = motion * 100;
    }
    if (ability.isRangedAbility)
    {
      // Calculate the distance on the X-axis to keep the mob at the specific range from the target
      float targetRangeX = 300.0f; // Adjust this to the desired ranged attack distance
      float distanceX = Mathf.Abs(target.Position.X - Position.X);
      float distanceY = Mathf.Abs(target.Position.Y - Position.Y);

      // Check if mob is at the desired range on the X-axis and within the acceptable Y range
      if (distanceX > targetRangeX || distanceY > 20.0f) // Allow small Y-axis variance
      {
        // Move towards the target X range and adjust Y to align with the player
        Vector2 moveDirection = new Vector2(target.Position.X - Position.X, target.Position.Y - Position.Y).Normalized();

        // OnlY move horizontally until the target range is reached, but also adjust Y if needed
        motion = new Vector2(moveDirection.X, moveDirection.Y) * (float)(mobResource.Speed * delta);

        // Play the move animation
        if (!lockedAnimations.Contains(AnimationPlayer.Animation))
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
      Vector2 direction = (target.Position - Position).Normalized();
      float distanceToTarget = Position.DistanceTo(target.Position);

      if (distanceToTarget > stopDistance)
      {
        motion = direction * (float)(mobResource.Speed * delta);
        if (!lockedAnimations.Contains(AnimationPlayer.Animation))
        {
          AnimationPlayer.Play("move");
        }
      }
      else if (!isDoingAction)
      {
        isDoingAction = true;
        abilityManager.DoNextActionAsync();
      }

      AnimationPlayer.FlipH = direction.X < 0;
    }
    timer += delta;
    if (timer > 1) 
    {
      abilityManager.SetKI(abilityManager.GetKI() + mobResource.RegenKI); // regenerate the mobs  1/second
      EventRegistry.GetEventPublisher("OnKiChanged").RaiseEvent(new object[] { abilityManager.GetKI(), this });

      timer = 0;
    }
    abilityManager.SetFacingDirection(AnimationPlayer.FlipH ? -1 : 1);
    EventRegistry.GetEventPublisher("DirectionChanged").RaiseEvent(new object[] { AnimationPlayer.FlipH ? -1 : 1, this });
  }

  public void UpdateTarget()
  {
    target = GetTree().GetFirstNodeInGroup("Player") as Node2D;
  }

  public virtual async void TakeDamage(object sender, object[] args)
  {
    if (AnimationPlayer.Animation == "death") return;

    if (args[0] is Healthbar healthbar)
    {
      if (healthbar.Equals(GetNode<Healthbar>("Healthbar")))
      {
        SetProcess(false); // Stop current processing
        healthbar.TakeDamage(float.Parse(args[1].ToString()));

        // Cancel the current ability if one is being executed
        if (isDoingAction)
        {
          abilityManager.CancelCurrentAbility(); // Cancel the ongoing ability
        }

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
        else if (AnimationPlayer.Animation != "death")
        {
          isGettingHurt = false;
          AnimationPlayer.Play("default");
          SetProcess(true); // Resume processing
        }
      }
    }
  }

  public async void KnockBack(Vector2 dir, float frc)
  {
    GD.Print($"KnockBack: {this.Name} --- Animation: [{AnimationPlayer.Animation}]");
    if (AnimationPlayer.Animation == "death") return;
    await ToSignal(GetTree().CreateTimer(.1f), "timeout");
    motion = dir * frc;
    await ToSignal(GetTree().CreateTimer(1f), "timeout");
  }

  public virtual void OnPlayerDeath(object sender, object[] args)
  {
    AnimationPlayer.Play("default");
    SetProcess(false);
    SetPhysicsProcess(false);
  }
  public async void ActionFinished(object sender, object[] args)
  {
    // Se nao vier do ability manager ou ability deste mob, entao retorna
    if (AnimationPlayer.Animation == "hurt") await ToSignal(AnimationPlayer, "animation_finished");
    if (AnimationPlayer.Animation == "death") return;
    if (args[0] is Node2D node)
    {
      if (abilityManager != null && !IsInstanceValid(abilityManager))
      {
        var isInChildren = abilityManager.GetChildren().Contains(node);
        var isThisAbilityManager = node == abilityManager;
        if (!isThisAbilityManager)
          if (!isInChildren)
            return;
      }
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

  public void IncreaseStatsFromDictionary(object sender, object[] args)
  {
    if (args[1] is Node2D node)
    {
      var isInChildren = abilityManager.GetChildren().Contains(node);
      var isThisAbilityManager = node == abilityManager;
      if (!isThisAbilityManager)
        if (!isInChildren)
          return;
    }
    if (args[0] is Godot.Collections.Dictionary<string, int> statIncreases)
    {

      foreach (var kvp in statIncreases)
      {
        switch (kvp.Key)
        {
          case "ki":
            float newKi = mobResource.KI + kvp.Value < mobResource.MaxKI ? mobResource.KI + kvp.Value : mobResource.MaxKI;
            abilityManager.SetKI(newKi);
            EventRegistry.GetEventPublisher("OnKiChanged").RaiseEvent(new object[] { abilityManager.GetKI(), this });
            break;
          default:
            break;
        }
      }
    }
  }

  public virtual void Die()
  {
    if (mobResource.characterSounds is not null)
    {
      AudioOptionsResource sound = mobResource.characterSounds.GetValueOrDefault("death");
      if (sound is not null)
        audioManager?.Play(mobResource.characterSounds.GetValueOrDefault("death"), this);
    }
    AnimationPlayer.Play("death");
    UnsubscribeFromEvents();
    SetProcess(false);
    SetPhysicsProcess(false);
    abilityManager.Deactivate();
    EventRegistry.GetEventPublisher("OnMobDeath").RaiseEvent(new object[] { this });
  }

  public void UnsubscribeFromEvents()
  {
    EventSubscriber.UnsubscribeFromEvent("OnPlayerDeath", OnPlayerDeath);
    EventSubscriber.UnsubscribeFromEvent("TakeDamage", TakeDamage);
    EventSubscriber.UnsubscribeFromEvent("ActionFinished", ActionFinished);
    EventSubscriber.UnsubscribeFromEvent("OnComboFinished", ActionFinished);
    EventSubscriber.UnsubscribeFromEvent("IncreaseStatsFromDictionary", IncreaseStatsFromDictionary);

  }
  public override void _ExitTree()
  {
    UnsubscribeFromEvents();
  }
}
