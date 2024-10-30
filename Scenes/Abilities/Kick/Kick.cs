using Godot;
using System;

public partial class Kick : Ability
{
  [Export] public AbilityResource kickResource;
  private bool isDoingAction = false;
  double timer = 0;
  [Export] private float coneAngleDegrees = 45.0f;  // Cone's half-angle in degrees
  [Export] private float coneRange = 100.0f;         // Maximum range of the cone

  // Call this function to detect objects in the cone
  public async void DetectInCone()
  {
    // Use character's movement direction as forward direction
    Vector2 forward = CurrentVelocity.Normalized();  // Adjusted to use velocity

    // Get all bodies in a circular range (optimized with a CollisionShape2D)
    var spaceState = GetWorld2D().DirectSpaceState;
    var query = new PhysicsShapeQueryParameters2D();
    query.Shape = new CircleShape2D { Radius = coneRange };
    query.Transform = new Transform2D(0, GlobalPosition); // Set the center of the query to the player

    var results = spaceState.IntersectShape(query);

    AnimationPlayer.Play("kick");
    foreach (var result in results)
    {
      if (result["collider"] is Variant body)
      {
        // The object is within the cone, call its method
        if (!body.As<Node2D>().IsInGroup("Enemies"))
          continue;
        var healthbar = body.As<Node2D>().GetNode<Healthbar>("Healthbar");
        // Vector from the character to the object
        Vector2 toBody = (body.As<Node2D>().GlobalPosition - GlobalPosition).Normalized();

        // Check if the object is within the cone
        float angleToBody = Mathf.RadToDeg(forward.AngleTo(toBody));

        if (Math.Abs(angleToBody) <= coneAngleDegrees)
        {
          // Call Take Damage
          if (healthbar.IsAlive)
            EventRegistry.GetEventPublisher("TakeDamage").RaiseEvent(new object[] {
              healthbar,
              kickResource.Damage
            });
          body.As<Mob>().KnockBack(forward, kickResource.Value);
        }
      }
    }

    await ToSignal(GetTree().CreateTimer(.1f, false, true), "timeout");
    AnimationPlayer.Play("default");
    await ToSignal(GetTree().CreateTimer(kickResource.Cooldown, false, true), "timeout");
    isDoingAction = false;
    EventRegistry.GetEventPublisher("ActionFinished").RaiseEvent(new object[] { });
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
      DrawLine(Position, leftDir, lineColor, 2);   // Left boundary
      DrawLine(Position, rightDir, lineColor, 2);  // Right boundary
      DrawLine(leftDir, rightDir, lineColor, 2);         // Closing line

      // Optionally fill the cone area (as a polygon)
      Color fillColor = new Color(0.8f, 0.8f, 0.8f, 0.1f);  // Light gray with transparency
      Vector2[] points = { Position, leftDir, rightDir };
      DrawPolygon(points, new Color[] { fillColor });

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
        if (!body.As<Node2D>().IsInGroup("Enemies"))
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
