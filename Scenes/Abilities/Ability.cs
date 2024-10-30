using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class Ability : Node2D
{
  public Vector2 CurrentVelocity = Vector2.Zero;

  public AnimatedSprite2D AnimationPlayer;

  public int facingDirection = 1;

  public void SetFacingDirection(int direction)
  {
    facingDirection = direction;
  }


  public virtual void Action()
  {
    EventRegistry.GetEventPublisher("OnComboFinished").RaiseEvent(new object[] { });
  }


}
