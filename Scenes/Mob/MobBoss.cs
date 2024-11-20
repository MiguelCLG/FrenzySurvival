using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class MobBoss : Mob
{
  [Export] int bossPhases = 2;
  [Export] ProgressBar HealthBar;
  [Export] ProgressBar KiBar;
  [Export] CollisionShape2D collision;

  [Export] Array<PackedScene> behaviors = new();
  [Export] public BaseCharacterResource mobResourcePhaseTwo;

  protected Node2D behaviorContainer;
  protected Behavior currentBehavior;
  int currentPhase = 1;
  public override void _Ready()
  {
    base._Ready();
    HealthBar = GetNode<Healthbar>("Healthbar");
    KiBar = GetNode<MobKiBar>("KiBar");
    collision = GetNode<CollisionShape2D>("CollisionShape2D");
    behaviorContainer = GetNode<Node2D>("Behaviors");
    LoadBehaviours();
  }

  public override void _Process(double delta)
  {
    base._Process(delta); // Not sure if it will be the same process, but we do need to update the position from the player
    GD.Print("Process Boss");
  }
  public override void _PhysicsProcess(double delta)
  {

    // Calculate the distance on the X-axis to keep the mob at the specific range from the target
    int index = abilityManager.actionindex < abilityManager.abilityArray.Count ? abilityManager.actionindex : 0;
    Vector2 targetRange = abilityManager.abilityArray[index].abilityResource.RangeRequired; // Adjust this to the desired ranged attack distance
    float distanceX = Mathf.Abs(target.Position.X - Position.X);
    float distanceY = Mathf.Abs(target.Position.Y - Position.Y);
    if (distanceX < targetRange.X || distanceY < targetRange.Y) // Allow small Y-axis variance
    {
      if (isDoingAction) return;
      // Mob is in range for a ranged attack
      // Verify if the mob has KI to do the attack
      if (abilityManager.abilityArray[index].abilityResource.kiRequired <= abilityManager.GetKI())
      {
        SetBehavior("Attack");
        currentBehavior.Execute(this, (float)delta, currentPhase);
      }
    }
    else
    {
      SetBehavior("Movement");
      currentBehavior.Execute(this, (float)delta, currentPhase);
      MoveAndCollide(motion, false, 1f, true);
    }
  }

  private void LoadBehaviours()
  {
    foreach (PackedScene behavior in behaviors)
    {
      var behaviorInstance = behavior.Instantiate<Behavior>();
      GetNode<Node2D>("Behaviors").AddChild(behaviorInstance);
    }
    SetBehavior("Movement");
  }

  private void SetBehavior(string newBehavior)
  {
    currentBehavior = behaviorContainer.GetNode<Behavior>(newBehavior);
  }
  public override async void TakeDamage(object sender, object[] args)
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
        AnimationPlayer.Play(currentPhase == 1 ? "hurt" : "phase_2_hurt");
        if (mobResource.characterSounds is not null)
        {
          AudioOptionsResource sound = mobResource.characterSounds.GetValueOrDefault("hurt");
          if (sound is not null)
            audioManager?.Play(mobResource.characterSounds.GetValueOrDefault("hurt"), this);
        }
        await ToSignal(AnimationPlayer, "animation_finished");

        if (!healthbar.IsAlive)
        {
          if (currentPhase < bossPhases)
          {
            // Go to next phase
            currentPhase++;
            abilityManager.CancelCurrentAbility();
            SetProcess(false);
            SetPhysicsProcess(false);
            AnimationPlayer.Play("transformation");
            healthbar.IsAlive = true;
            healthbar.Heal(mobResourcePhaseTwo.HP);
            HealthBar.Position = new Vector2(HealthBar.Position.X, -40);
            KiBar.Position = new Vector2(KiBar.Position.X, -36);
            collision.Position = Vector2.Zero;
            collision.Scale = Vector2.One * 3;
            await ToSignal(AnimationPlayer, "animation_finished");
            AnimationPlayer.Play("phase_2_default");
            LoadBossAbilities();
            SetProcess(true);
            SetPhysicsProcess(true);
          }
          else
            Die();
          return;
        }
        else if (AnimationPlayer.Animation != "death")
        {
          isGettingHurt = false;
          AnimationPlayer.Play(currentPhase == 1 ? "default" : "phase_2_default");
          SetProcess(true); // Resume processing
        }
      }
    }
  }
  private void LoadBossAbilities()
  {
    foreach (var node in abilityManager.GetChildren())
      abilityManager.RemoveChild(node);
    abilityManager.abilityArray.Clear();
    foreach (var abilityScene in mobResourcePhaseTwo.MobAttacks)
    {
      var ability = abilityScene.Instantiate<Ability>();
      abilityManager.AddAbility(ability);
    }
    abilityManager.UnsubscribeFromEvents();
    abilityManager.SetKI(mobResource.KI);
    abilityManager.SetTargetGroup("Player");
  }

  public override async void Die()
  {
    if (mobResource.characterSounds is not null)
    {
      AudioOptionsResource sound = mobResource.characterSounds.GetValueOrDefault("death");
      if (sound is not null)
        audioManager?.Play(mobResource.characterSounds.GetValueOrDefault("death"), this);
    }
    AnimationPlayer.Play("phase_2_ko");
    UnsubscribeFromEvents();
    SetProcess(false);
    SetPhysicsProcess(false);
    abilityManager.Deactivate();
    if (AnimationPlayer.Frame < 8) // if it hasn't reached the last frame, wait for the animation to finish, otherwise play the death animation
      await ToSignal(AnimationPlayer, "animation_finished");
    AnimationPlayer.Play("phase_2_death");
    EventRegistry.GetEventPublisher("OnMobDeath").RaiseEvent(new object[] { this });
  }
}
