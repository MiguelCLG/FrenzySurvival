using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;

public partial class Punch : Ability
{
  private bool isDoingAction = false;
  double timer = 0;
  [Export] private float coneAngleDegrees = 90.0f;  // Cone's half-angle in degrees
  [Export] private float coneRange = 60.0f;         // Maximum range of the cone

  // Call this function to detect objects in the cone

  public override void _Process(double delta)
  {
    QueueRedraw();  // Redraw the cone when the punch is initiated
  }

  public override void Action()
  {
    if (isDoingAction) return;  // Prevent multiple actions at once

    isDoingAction = true;
    cancellationTokenSource = new CancellationTokenSource();
    CancellationToken token = cancellationTokenSource.Token;

    currentTask = Task.Run(() => DetectInCone(token), token);
  }

  public override void Cancel()
  {
    if (isDoingAction)
    {
      cancellationTokenSource.Cancel();  // Cancel the task
      EventRegistry.GetEventPublisher("ActionCanceled").RaiseEvent(new object[] { this });
    }
  }

  public async Task DetectInCone(CancellationToken token)
  {
    try
    {
      await ToSignal(GetTree().CreateTimer(.2f, false, true), "timeout");

      if (token.IsCancellationRequested) return;  // Handle early cancellation

      AnimationPlayer.Play("default");
      cooldownTimer = GetTree().CreateTimer(abilityResource.Cooldown, false, true);
      await ToSignal(cooldownTimer, "timeout");

      Vector2 forward = CurrentVelocity.Normalized();

      var spaceState = GetWorld2D().DirectSpaceState;
      var query = new PhysicsShapeQueryParameters2D
      {
        Shape = new CircleShape2D { Radius = coneRange },
        Transform = new Transform2D(0, GlobalPosition)
      };

      var results = spaceState.IntersectShape(query);

      if (AnimationPlayer.Animation == "death" || token.IsCancellationRequested) return;

      AnimationPlayer.Play("punch");
      audioManager?.Play(abilitySound, this);

      foreach (var result in results)
      {
        if (result["collider"] is Variant body && body.As<Node2D>().IsInGroup(targetGroup))
        {
          var healthbar = body.As<Node2D>().GetNode<Healthbar>("Healthbar");
          Vector2 toBody = (body.As<Node2D>().GlobalPosition - GlobalPosition).Normalized();
          float angleToBody = Mathf.RadToDeg(forward.AngleTo(toBody));

          if (Math.Abs(angleToBody) <= coneAngleDegrees && healthbar.IsAlive)
          {
            EventRegistry.GetEventPublisher("TakeDamage").RaiseEvent(new object[] {
                            healthbar,
                            abilityResource.Damage
                        });
          }
        }

        if (token.IsCancellationRequested) return;  // Check cancellation inside the loop
      }

      AnimationPlayer.Play("default");
    }
    catch (TaskCanceledException)
    {
      // Handle task cancellation, if needed
      GD.Print("Ability was canceled");
      cooldownTimer.Free();
    }
    finally
    {
      isDoingAction = false;
      EventRegistry.GetEventPublisher("ActionFinished").RaiseEvent(new object[] { this });
    }
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
        if (cancellationTokenSource.IsCancellationRequested)
        {
          DrawLine(Position, leftDir, transparentColor, 2);   // Left boundary
          DrawLine(Position, rightDir, transparentColor, 2);  // Right boundary
          DrawLine(leftDir, rightDir, transparentColor, 2);    //Clearing line
          DrawPolygon(points, new Color[] { transparentColor });
        }
        else if (cooldownTimer.TimeLeft == 0)
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
