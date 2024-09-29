using Algos;
using Godot;
using System;

public partial class PunchTwo : Ability
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

    AnimationPlayer.Play("punch_2");
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

  public override void Action()
  {
    DetectInCone();  // Perform the cone detection
  }


}
