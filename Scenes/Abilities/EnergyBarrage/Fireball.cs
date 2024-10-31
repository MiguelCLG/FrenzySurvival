using Godot;
using System;

public partial class Fireball : AnimatedSprite2D
{
  [Export] public float speed = 500f;
  private int facingDirection = 1;
  private int lifespan = 10;

  public void SetFacingDirection(int direction)
  {
    facingDirection = direction;
    FlipH = direction < 0;
  }

  public override void _Ready()
  {
    Play("create");
    SetProcess(false);
  }

  public void StartProcess()
  {
    Play("move");
    SetProcess(true);
    GetTree().CreateTimer(lifespan, false, true).Timeout += Destroy;
  }
  public override void _Process(double delta)
  {
    Position += new Vector2(speed * facingDirection, 0) * (float)delta;
    speed += (float)delta * 2;
  }

  public async void Destroy()
  {
    SetProcess(false);
    Play("destroy");
    await ToSignal(this, "animation_finished");
    QueueFree();
  }

  public void OnBodyEntered(Node2D bodyEntered)
  {
    EventRegistry.GetEventPublisher("OnFireballHit").RaiseEvent(new object[] { bodyEntered, this });
  }

}
