using Godot;
using System;

public partial class Kick : Ability
{
  private bool isDoingAction = false;
  double timer = 0;
  [Export] private float coneAngleDegrees = 45.0f;  // Cone's half-angle in degrees
  [Export] private float coneRange = 100.0f;         // Maximum range of the cone


  // Call this function to detect objects in the cone
  public async void DetectInCone()
  {
    await ToSignal(GetTree().CreateTimer(.2f, false, true), "timeout");
    AnimationPlayer.Play("default");
    cooldownTimer = GetTree().CreateTimer(abilityResource.Cooldown, false, true);    // Use character's movement direction as forward direction
    await ToSignal(cooldownTimer, "timeout");

    Vector2 forward = CurrentVelocity.Normalized();  // Adjusted to use velocity

    // Get all bodies in a circular range (optimized with a CollisionShape2D)
    var spaceState = GetWorld2D().DirectSpaceState;
    var query = new PhysicsShapeQueryParameters2D();
    query.Shape = new CircleShape2D { Radius = coneRange };
    query.Transform = new Transform2D(0, GlobalPosition); // Set the center of the query to the player

    var results = spaceState.IntersectShape(query);
    if (AnimationPlayer.Animation == "death") return;
    AnimationPlayer.Play("kick");
    audioManager?.Play(abilitySound, this);

    foreach (var result in results)
    {
      if (result["collider"] is Variant body)
      {
        Node2D node = body.As<Node2D>();
        // The object is within the cone, call its method
        if (!node.IsInGroup(targetGroup))
          continue;
        var healthbar = node.GetNode<Healthbar>("Healthbar");
        // Vector from the character to the object
        Vector2 toBody = (node.GlobalPosition - GlobalPosition).Normalized();

        // Check if the object is within the cone
        float angleToBody = Mathf.RadToDeg(forward.AngleTo(toBody));

        if (Math.Abs(angleToBody) <= coneAngleDegrees)
        {
          // Call Take Damage
          if (healthbar.IsAlive)
            EventRegistry.GetEventPublisher("TakeDamage").RaiseEvent(new object[] {
              healthbar,
              abilityResource.Damage
            });
          //TODO: Make the knockback for player (maybe create an Interface to acomudate both the player and Mobs as they both might have knockbacks)
          if (node is Mob mob)
            mob.KnockBack(forward, abilityResource.Value);
        }
      }
    }
    AnimationPlayer.Play("default");
    isDoingAction = false;
    EventRegistry.GetEventPublisher("ActionFinished").RaiseEvent(new object[] { this });
  }

  public override void _Process(double delta)
  {
    QueueRedraw();  // Redraw the cone when the punch is initiated
  }

  public override void Action()
  {
    isDoingAction = true;
    CallDeferred("DetectInCone");  // Perform the cone detection
  }

  public override void _Draw()
  {

    if (isDoingAction)
    {
      // Use character's movement direction as forward direction
      Vector2 forward = CurrentVelocity.Normalized();  // Adjusted to use velocity
      // Calculate the cone's half-angle in radians
      float halfAngleRad = Mathf.DegToRad(coneAngleDegrees / 2);
      // Calculate the left and right boundaries of the cone based on playerPosition
      Vector2 leftDir = Position + forward.Rotated(-halfAngleRad) * coneRange;
      Vector2 rightDir = Position + forward.Rotated(halfAngleRad) * coneRange;

      // Draw the cone boundaries, anchored to the player's position
      Color lineColor = new Color(0, 0, 0, 0.1f);
      Color transparentColor = new Color(0, 0, 0, 0);

      if (cooldownTimer is not null)
      {
        // Optionally fill the cone area (as a polygon)
        Color fillColor = new Color(0.8f, 0.8f, 0.8f, abilityResource.Cooldown - (float)cooldownTimer.TimeLeft);  // Light gray with transparency
        Vector2[] points = { Position, leftDir, rightDir };
        if (cooldownTimer.TimeLeft == 0)
        {
          DrawLine(Position, leftDir, transparentColor, 2);   // Left boundary
          DrawLine(Position, rightDir, transparentColor, 2);  // Right boundary
          DrawLine(leftDir, rightDir, transparentColor, 2);    //Clearing line
          DrawPolygon(points, new Color[] { transparentColor });

        }
        else
        {
          DrawLine(Position, leftDir, lineColor, 2);   // Left boundary
          DrawLine(Position, rightDir, lineColor, 2);  // Right boundary
          DrawLine(leftDir, rightDir, lineColor, 2);         // Closing line
          DrawPolygon(points, new Color[] { fillColor });
        }
      }

    }
    // Visualize enemies detected within the cone
    DetectInConeVisual();
  }
  // Helper method to visualize enemies detected in the cone
  private void DetectInConeVisual()
  {
    // Get the forward direction based on the player's movement
    Vector2 forward = CurrentVelocity.Normalized();

    // Set up the query to find objects in the circular range
    var spaceState = GetWorld2D().DirectSpaceState;
    var query = new PhysicsShapeQueryParameters2D();
    query.Shape = new CircleShape2D { Radius = coneRange };
    query.Transform = new Transform2D(0, GlobalPosition); // Set the center of the query to the player

    var results = spaceState.IntersectShape(query);

    foreach (var result in results)
    {
      if (result["collider"] is Variant body)
      {
        if (!body.As<Node2D>().IsInGroup(targetGroup))
          continue;

        // Vector from the character to the object
        Vector2 toBody = (body.As<Node2D>().GlobalPosition - GlobalPosition).Normalized();

        // Check if the object is within the cone
        float angleToBody = Mathf.RadToDeg(forward.AngleTo(toBody));

        if (Math.Abs(angleToBody) <= coneAngleDegrees)
        {
          // Visual indicator: Draw a circle at the enemy's position to show it's detected in the cone
          DrawCircle(body.As<Node2D>().Position, 10, Colors.Green);
        }
      }
    }
  }
}
