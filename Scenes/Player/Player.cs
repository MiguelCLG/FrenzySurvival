using Godot;
using System;
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

  public override void _Ready()
  {
    abilityManager = GetNode<AbilityManager>("%Abilities");
    AnimationPlayer = GetNode<AnimatedSprite2D>("Portrait");
    GetNode<Healthbar>("Healthbar").SetInitialValues(playerResource);
    EventRegistry.RegisterEvent("TakeDamage");
    EventSubscriber.SubscribeToEvent("TakeDamage", TakeDamage);
    EventRegistry.RegisterEvent("OnComboFinished");
    EventSubscriber.SubscribeToEvent("OnComboFinished", OnComboFinished);
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
      // Apply friction/damping when there is no input
      float friction = 10f; // Adjust this value to change how fast the character slows down
      Velocity = Velocity.Lerp(Vector2.Zero, friction * (float)delta);
      if (AnimationPlayer.Animation == "move") AnimationPlayer.Play("default");
    }

    // Optionally, use MoveAndSlide to handle movement with collisions
    AnimationPlayer.FlipH = Velocity.X < 0;
    MoveAndSlide();

  }
  public override void _ExitTree()
  {
    EventSubscriber.UnsubscribeFromEvent("TakeDamage", TakeDamage);
    EventSubscriber.UnsubscribeFromEvent("OnComboFinished", OnComboFinished);

  }
}
