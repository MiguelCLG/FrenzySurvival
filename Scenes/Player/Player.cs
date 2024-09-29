using Godot;
using System;

public partial class Player : CharacterBody2D
{
  [Export] public PlayerResource playerResource;

  [Export] public AnimatedSprite2D AnimationPlayer { get; set; }


  float maxVelocity = 10f;
  AbilityManager abilityManager;
  double timer = 5;
  bool isDoingCombo = false;

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


  public void TakeDamage(object sender, object[] args)
  {
    // AnimationPlayer.Play("Hurt");
    GD.Print($"ARGS: {args[1]}");
    if (args[0] is Healthbar healthbar)
      healthbar.TakeDamage(float.Parse(args[1].ToString()));
  }

  public override void _PhysicsProcess(double delta)
  {
    Movement(delta);
    if (!isDoingCombo)
    {
      isDoingCombo = true;
      abilityManager.DoNextAction();
    }
  }

  public void OnComboFinished(object sender, object[] args)
  {
    GD.Print("Player: TERMINOU");

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
    }
    else
    {
      // Apply friction/damping when there is no input
      float friction = 10f; // Adjust this value to change how fast the character slows down
      Velocity = Velocity.Lerp(Vector2.Zero, friction * (float)delta);
    }

    // Optionally, use MoveAndSlide to handle movement with collisions
    AnimationPlayer.FlipH = Velocity.X < 0;
    MoveAndSlide();

    // Debugging information
  }
  public override void _ExitTree()
  {
    EventSubscriber.UnsubscribeFromEvent("TakeDamage", TakeDamage);
    EventSubscriber.UnsubscribeFromEvent("OnComboFinished", OnComboFinished);

  }
}
