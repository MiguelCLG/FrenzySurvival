using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Player : CharacterBody2D
{
  [Export] public PlayerResource playerResource;

  [Export] public AnimatedSprite2D AnimationPlayer { get; set; }


  float maxVelocity = 10f;
  AbilityManager abilityManager;
  double timer = 5;
  bool isDoingCombo = false;
  bool isGettingHurt = false;
  int facingDirection = 1;

  public override void _Ready()
  {
    abilityManager = GetNode<AbilityManager>("%Abilities");
    AnimationPlayer = GetNode<AnimatedSprite2D>("Portrait");
    GetNode<Healthbar>("Healthbar").SetInitialValues(playerResource);
    EventRegistry.RegisterEvent("TakeDamage");
    EventSubscriber.SubscribeToEvent("TakeDamage", TakeDamage);
    EventRegistry.RegisterEvent("OnComboFinished");
    EventSubscriber.SubscribeToEvent("OnComboFinished", OnComboFinished);
    //EventRegistry.RegisterEvent("SetKI");
    //EventSubscriber.SubscribeToEvent("SetKI", SetKI);
    EventRegistry.RegisterEvent("IncreaseStatsFromDictionary");
    EventSubscriber.SubscribeToEvent("IncreaseStatsFromDictionary", IncreaseStatsFromDictionary);

    GetTree().CreateTimer(.2f, false, true).Timeout += () =>
    {
      SetInitialKIValue();
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
    if (Velocity.X != 0)
    {
      AnimationPlayer.FlipH = Velocity.X < 0;
      abilityManager.SetFacingDirection(Velocity.X < 0 ? -1 : 1);
    }
    MoveAndSlide();

  }

  public void SetInitialKIValue()
  {
    abilityManager.SetKI(playerResource.KI);
    EventRegistry.GetEventPublisher("SetInitialKIValue").RaiseEvent(new object[] { playerResource.KI, playerResource.MaxKI });
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
    if (args[0] is Dictionary<string, int> statIncreases)
    {
     foreach(var kvp in statIncreases)
     {
      switch (kvp.Key)
      {
        case "ki":
          int newKi = playerResource.KI +kvp.Value < playerResource.MaxKI ? playerResource.KI +kvp.Value : playerResource.MaxKI;
          playerResource.KI = newKi;
          abilityManager.SetKI(newKi);
          EventRegistry.GetEventPublisher("OnKiChanged").RaiseEvent(new object[] { playerResource.KI });
          break;

        case "health":
          int newHelath = playerResource.HP +kvp.Value < playerResource.MaxHP ? playerResource.HP +kvp.Value : playerResource.MaxHP;
          playerResource.HP = newHelath;
          break;

        default:
          break;
      }
      if(kvp.Key == "ki")
      EventRegistry.GetEventPublisher("OnKiChanged").RaiseEvent(new object[] { playerResource.KI });
     }
    }
  }

  public override void _ExitTree()
  {
    EventSubscriber.UnsubscribeFromEvent("TakeDamage", TakeDamage);
    EventSubscriber.UnsubscribeFromEvent("OnComboFinished", OnComboFinished);
    EventSubscriber.UnsubscribeFromEvent("SetKI", SetKI);

  }
}
