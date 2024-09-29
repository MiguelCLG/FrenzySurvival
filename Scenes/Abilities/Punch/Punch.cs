using Algos;
using Godot;
using System;
using System.Threading.Tasks;

public partial class Punch : Ability
{
  [Export] public AbilityResource punchResource;

  double timer = 0;
  [Export] private float coneAngleDegrees = 45.0f;  // Cone's half-angle in degrees
  [Export] private float coneRange = 60.0f;         // Maximum range of the cone

  // Call this function to detect objects in the cone
  public void DetectInCone()
  {
    // Use character's movement direction as forward direction
    Vector2 forward = CurrentVelocity.Normalized();  // Adjusted to use velocity

    // Get all bodies in a circular range (optimized with a CollisionShape2D)
    var spaceState = GetWorld2D().DirectSpaceState;
    var query = new PhysicsShapeQueryParameters2D();
    query.Shape = new CircleShape2D { Radius = coneRange };
    query.Transform = new Transform2D(0, GlobalPosition); // Set the center of the query to the player

    var results = spaceState.IntersectShape(query);

    AnimationPlayer.Play("punch");
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
          TimerUtils.CreateTimer(() =>
        {
          if (healthbar.IsAlive)
            EventRegistry.GetEventPublisher("TakeDamage").RaiseEvent(new object[] {
              healthbar,
              punchResource.Damage
            });

        }, this, 1f);
        }
      }
    }
    TimerUtils.CreateTimer(() =>
    AnimationPlayer.Play("default"),
    this, .3f);
    TimerUtils.CreateTimer(() =>
        {
          EventRegistry.GetEventPublisher("ActionFinished").RaiseEvent(new object[] { });
        }, this, 1f);
  }

  public override void _Process(double delta)
  {
    /* timer += delta;
    if (timer > punchResource.Cooldown)
    {
      DoPunch();
      timer = 0;
    } */
    QueueRedraw();  // Redraw the cone when the punch is initiated

  }

  public override void Action()
  {
    DetectInCone();  // Perform the cone detection
  }

  /* public override void _Draw()
  {
    base._Draw();
    // Use the character's movement direction to determine the forward direction
    Vector2 forward = CurrentVelocity.Normalized();  // Adjusted to use velocity

    if (forward == Vector2.Zero)
      return;  // Avoid drawing the cone if there is no movement

    // Calculate the left and right cone directions
    float angleRad = Mathf.DegToRad(coneAngleDegrees);
    Vector2 leftDir = forward.Rotated(-angleRad) * coneRange;
    Vector2 rightDir = forward.Rotated(angleRad) * coneRange;

    Color lineColor = new(0, 0, 0, 0.2f);

    // Draw the cone as a triangle
    DrawLine(Vector2.Zero, leftDir, lineColor, 2);  // Left line of the cone
    DrawLine(Vector2.Zero, rightDir, lineColor, 2); // Right line of the cone
    DrawLine(leftDir, rightDir, lineColor, 2);      // Closing the cone

    Color color = new(122, 122, 122, 0.2f);
    // Optionally fill the cone area (create a filled shape)
    Vector2[] points = { Vector2.Zero, leftDir, rightDir };
    DrawPolygon(points, new Color[] { color });
  } */

  public override void _Draw()
  {
    base._Draw();

    // Get the forward direction based on movement velocity
    Vector2 forward = CurrentVelocity.Normalized();

    // Calculate the cone's half-angle in radians
    float halfAngleRad = Mathf.DegToRad(coneAngleDegrees);

    // Calculate the left and right boundaries of the cone
    Vector2 leftDir = forward.Rotated(-halfAngleRad) * coneRange;
    Vector2 rightDir = forward.Rotated(halfAngleRad) * coneRange;

    Color lineColor = new(0, 0, 0, 0.1f);
    // Draw the cone as lines
    DrawLine(Position, Position + leftDir, lineColor, 2);  // Left line of the cone
    DrawLine(Position, Position + rightDir, lineColor, 2); // Right line of the cone
    DrawLine(Position + leftDir, Position + rightDir, lineColor, 2);  // Closing line of the cone

    Color color = new(122, 122, 122, 0.1f);

    // Optionally fill the cone area (as a polygon)
    Vector2[] points = { Position, Position + leftDir, Position + rightDir };
    DrawPolygon(points, new Color[] { color });

    // Debugging: Draw circles on the detected enemies within the cone
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
