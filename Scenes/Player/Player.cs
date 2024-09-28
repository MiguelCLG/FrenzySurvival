using Godot;
using System;

public partial class Player : CharacterBody2D
{
  [Export] public PlayerResource playerResource;

  float maxVelocity = 10f;

  public override void _Ready()
  {

  }

  public override void _PhysicsProcess(double delta)
  {
    Movement(delta);
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
    MoveAndSlide();

    // Debugging information
  }
}
