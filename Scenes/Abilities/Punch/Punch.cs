using Godot;
using System;

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

    foreach (var result in results)
    {
      if (result["collider"] is Variant body)
      {
        // The object is within the cone, call its method
        if (!body.As<Node2D>().IsInGroup("Enemies"))
          continue;

        // Vector from the character to the object
        Vector2 toBody = (body.As<Node2D>().GlobalPosition - GlobalPosition).Normalized();

        // Check if the object is within the cone
        float angleToBody = Mathf.RadToDeg(forward.AngleTo(toBody));

        if (Math.Abs(angleToBody) <= coneAngleDegrees)
        {
          GD.Print("Punching");
          body.As<Node2D>().GetNode<Healthbar>("Healthbar").TakeDamage(punchResource.Damage);  // Replace with your method
        }
      }
    }
  }

  public override void _Process(double delta)
  {
    timer += delta;
    if (timer > punchResource.Cooldown)
    {
      DoPunch();
      timer = 0;
    }
    QueueRedraw();  // Redraw the cone when the punch is initiated

  }

  public void DoPunch()
  {
    DetectInCone();  // Perform the cone detection
  }

  public override void _Draw()
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

    // Draw the cone as a triangle
    DrawLine(Vector2.Zero, leftDir, Colors.Red, 2);  // Left line of the cone
    DrawLine(Vector2.Zero, rightDir, Colors.Red, 2); // Right line of the cone
    DrawLine(leftDir, rightDir, Colors.Red, 2);      // Closing the cone

    // Optionally fill the cone area (create a filled shape)
    Vector2[] points = { Vector2.Zero, leftDir, rightDir };
    DrawPolygon(points, new Color[] { Colors.Cyan.Darkened(0.5f) });
  }
}
