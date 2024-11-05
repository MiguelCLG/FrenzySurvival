using Godot;
using System.Collections.Generic;

public partial class Player : CharacterBody2D
{
  [Export] public PlayerResource playerResource;

  [Export] public AnimatedSprite2D AnimationPlayer { get; set; }


  float maxVelocity = 10f;
  AbilityManager abilityManager;
  double timer = 5;
  bool isDoingCombo = false;
  bool isDoingAction = false;
  bool isGettingHurt = false;
  int facingDirection = 1;
  private int experiencePoints = 0;

  public override void _Ready()
  {
    abilityManager = GetNode<AbilityManager>("%Abilities");
    AnimationPlayer = GetNode<AnimatedSprite2D>("Portrait");
    EventRegistry.RegisterEvent("TakeDamage");
    EventSubscriber.SubscribeToEvent("TakeDamage", TakeDamage);
    EventRegistry.RegisterEvent("OnComboFinished");
    EventSubscriber.SubscribeToEvent("OnComboFinished", OnComboFinished);
    //EventRegistry.RegisterEvent("SetKI");
    //EventSubscriber.SubscribeToEvent("SetKI", SetKI);
    EventRegistry.RegisterEvent("IncreaseStatsFromDictionary");
    EventSubscriber.SubscribeToEvent("IncreaseStatsFromDictionary", IncreaseStatsFromDictionary);
    EventRegistry.RegisterEvent("AbilitySelected");
    EventSubscriber.SubscribeToEvent("AbilitySelected", AbilitySelected);
    EventRegistry.RegisterEvent("IsDoingAction");
    EventSubscriber.SubscribeToEvent("IsDoingAction", SetIsDoingAction);
    EventRegistry.RegisterEvent("DirectionChanged");


    PrepareCharacter();
  }

  public void PrepareCharacter()
  {
    AnimationPlayer.SpriteFrames = playerResource.AnimatedFrames;
    GetNode<Healthbar>("Healthbar").SetInitialValues(playerResource);
    GetTree().CreateTimer(.5f, false, true).Timeout += () =>
    {
      SetInitialKIValue();
      SetInitialExperienceValue();
    };
  }
  public async void TakeDamage(object sender, object[] args)
  {
    if (args[0] is Healthbar healthbar)
    {
      if (healthbar.Equals(GetNode<Healthbar>("Healthbar")))
      {
        if (IsInstanceValid(healthbar))
        {
          isGettingHurt = true;

          AnimationPlayer.Play("hurt");
          await ToSignal(AnimationPlayer, "animation_finished");
          healthbar.TakeDamage(float.Parse(args[1].ToString()));
          if (!healthbar.IsAlive)
          {
            // Corre animacao de morte
            AnimationPlayer.Play("death");
            // Espera x segundos para a morte
            EventRegistry.GetEventPublisher("OnPlayerDeath").RaiseEvent(new object[] { this });
            return;
          }
          isGettingHurt = false;

        }
      }
    }
  }

  private void HandleExpIncrease(int increment)
  {

    int newExp = experiencePoints + increment;
    int levelDepoisExp = playerResource.LevelUpTables.GetCurrentLevel(experiencePoints);

    List<int> levelsGained = playerResource.LevelUpTables.GetLevelUps(experiencePoints, newExp);
    experiencePoints += increment;

    GD.Print();

    int MaxExpLevelAnterior = playerResource.LevelUpTables.levels[levelDepoisExp][0] == 0 ? 0 : playerResource.LevelUpTables.levels[levelDepoisExp][0] - 1;
    int MaxXpBarra = playerResource.LevelUpTables.levels[levelDepoisExp][1];
    EventRegistry.GetEventPublisher("OnExperienceChanged").RaiseEvent(new object[] { newExp, MaxExpLevelAnterior, MaxXpBarra });

    foreach (int level in levelsGained)
    {
      EventRegistry.GetEventPublisher("OnLevelUp").RaiseEvent(new object[] { level });
      GD.Print($"Leveled Up! New level [{level}]!");
    }
  }

  public override void _PhysicsProcess(double delta)
  {
    if (!GetNode<Healthbar>("Healthbar").IsAlive) return;
    Movement(delta);
    if (!isDoingCombo && !isGettingHurt)
    {
      isDoingCombo = true;
      abilityManager.DoNextActionAsync();
    }
  }

  public void OnComboFinished(object sender, object[] args)
  {

    isDoingCombo = false;
    timer = 0;
  }

  public void SetIsDoingAction(object sender, object[] args)
  {
    isDoingAction = (bool)args[0];
  }
  public void Movement(double delta)
  {
    foreach (Ability ability in GetNode("Abilities").GetChildren())
    {
      ability.CurrentVelocity = Velocity;
    }

    var horDir = Input.GetAxis("ui_left", "ui_right") * playerResource.Speed;
    var verDir = Input.GetAxis("ui_up", "ui_down") * playerResource.Speed;

    var direction = new Vector2((float)horDir, (float)verDir);

    if (direction != Vector2.Zero)
    {
      // If there's input, set the velocity towards the new direction
      Velocity = direction;
      if (AnimationPlayer.Animation == "default") AnimationPlayer.Play("move");
    }
    else
    {
      // TODO: Shouldnt the friction be exported? Maybe in the character resource?
      // Apply friction/damping when there is no input
      float friction = 10f; // Adjust this value to change how fast the character slows down
      Velocity = Velocity.Lerp(Vector2.Zero, friction * (float)delta);
      if (AnimationPlayer.Animation == "move") AnimationPlayer.Play("default");
    }

    // Optionally, use MoveAndSlide to handle movement with collisions
    if (Velocity.X != 0 && AnimationPlayer.FlipH == Velocity.X > 0 && !isDoingAction)
    {
      AnimationPlayer.FlipH = Velocity.X < 0;
      abilityManager.SetFacingDirection(Velocity.X < 0 ? -1 : 1);
      EventRegistry.GetEventPublisher("DirectionChanged").RaiseEvent(new object[] { Velocity.X < 0 ? -1 : 1 });
    }
    MoveAndSlide();

  }

  public void SetInitialKIValue()
  {
    abilityManager.SetKI(playerResource.KI);
    EventRegistry.GetEventPublisher("SetInitialKIValue").RaiseEvent(new object[] { playerResource.KI, playerResource.MaxKI });
  }
  public void SetInitialExperienceValue()
  {
    int newExp = experiencePoints;
    int levelDepoisExp = playerResource.LevelUpTables.GetCurrentLevel(experiencePoints);
    int MaxExpLevelAnterior = playerResource.LevelUpTables.levels[levelDepoisExp][0] == 0 ? 0 : playerResource.LevelUpTables.levels[levelDepoisExp][0] - 1;
    int MaxXpBarra = playerResource.LevelUpTables.levels[levelDepoisExp][1] - playerResource.LevelUpTables.levels[levelDepoisExp][0];

    EventRegistry.GetEventPublisher("SetInitialExpValue").RaiseEvent(new object[] { newExp, MaxExpLevelAnterior, MaxXpBarra, levelDepoisExp });
  }

  public void SetKI(object sender, object[] args)
  {
    if (args[0] is int kiValue)
    {
      int newKi = playerResource.KI + kiValue < playerResource.MaxKI ? playerResource.KI + kiValue : playerResource.MaxKI;
      playerResource.KI = newKi;
      abilityManager.SetKI(newKi);
      EventRegistry.GetEventPublisher("OnKiChanged").RaiseEvent(new object[] { playerResource.KI });
    }
  }

  public void IncreaseStatsFromDictionary(object sender, object[] args)
  {
    if (args[0] is Godot.Collections.Dictionary<string, int> statIncreases)
    {
      var healthbar = GetNode<Healthbar>("Healthbar");

      foreach (var kvp in statIncreases)
      {
        switch (kvp.Key)
        {
          case "ki":
            int newKi = playerResource.KI + kvp.Value < playerResource.MaxKI ? playerResource.KI + kvp.Value : playerResource.MaxKI;
            playerResource.KI = newKi;
            abilityManager.SetKI(newKi);
            EventRegistry.GetEventPublisher("OnKiChanged").RaiseEvent(new object[] { playerResource.KI });
            break;

          case "health":
            int newHealth = playerResource.HP + kvp.Value < playerResource.MaxHP ? playerResource.HP + kvp.Value : playerResource.MaxHP;
            playerResource.HP = newHealth;
            healthbar.Heal(kvp.Value);
            break;

          case "damage":
            abilityManager.AddDamage(kvp.Value);
            break;

          case "experience":
            HandleExpIncrease(kvp.Value);
            break;

          default:
            break;
        }
        if (kvp.Key == "ki")
          EventRegistry.GetEventPublisher("OnKiChanged").RaiseEvent(new object[] { playerResource.KI });
      }
    }
  }

  public void AbilitySelected(object sender, object[] args)
  {
    PackedScene a = args[0] as PackedScene;
    Ability ab = a.Instantiate<Ability>();
    abilityManager.AddAbility(ab);
  }


  public override void _ExitTree()
  {
    EventSubscriber.UnsubscribeFromEvent("TakeDamage", TakeDamage);
    EventSubscriber.UnsubscribeFromEvent("OnComboFinished", OnComboFinished);
    EventSubscriber.UnsubscribeFromEvent("SetKI", SetKI);
    EventSubscriber.UnsubscribeFromEvent("IncreaseStatsFromDictionary", IncreaseStatsFromDictionary);
    EventSubscriber.UnsubscribeFromEvent("AbilitySelected", AbilitySelected);
    EventSubscriber.UnsubscribeFromEvent("IsDoingAction", SetIsDoingAction);
    EventRegistry.UnregisterEvent("RegisterEvent");
  }
}
