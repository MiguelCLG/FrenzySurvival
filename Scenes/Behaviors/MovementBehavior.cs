using Godot;

public partial class MovementBehavior : Behavior
{
  public override void Execute(Mob mob, float delta, int currentPhase)
  {
    // Move towards the target X range and adjust Y to align with the player
    Vector2 moveDirection = new Vector2(mob.target.Position.X - mob.Position.X, mob.target.Position.Y - mob.Position.Y).Normalized();

    // OnlY move horizontally until the target range is reached, but also adjust Y if needed
    mob.motion = new Vector2(moveDirection.X, moveDirection.Y) * (float)(mob.mobResource.Speed * delta);

    // Play the move animation
    if (!mob.lockedAnimations.Contains(mob.AnimationPlayer.Animation))
    {
      mob.AnimationPlayer.Play(currentPhase == 1 ? "move" : "phase_2_move");
    }

    mob.AnimationPlayer.FlipH = moveDirection.X < 0;
    EventRegistry.GetEventPublisher("DirectionChanged").RaiseEvent(new object[] { mob.AnimationPlayer.FlipH ? -1 : 1, this });
  }
}
