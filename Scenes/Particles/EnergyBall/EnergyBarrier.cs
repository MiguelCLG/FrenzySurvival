using Godot;
using System;

public partial class EnergyBarrier : Ability
{
  [Export] public float radius = 1;
  [Export] public float maxRadius = 100;
  [Export] public CpuParticles2D cpuParticles2D;
  [Export] public CollisionShape2D collisionShape2D;
  [Export] public float speed = 50f;
  bool isAscending = true;
  public override void _Ready()
  {
    cpuParticles2D = GetNode<CpuParticles2D>("%CPUParticles2D");
    collisionShape2D = GetNode<CollisionShape2D>("%CollisionShape2D");
    (collisionShape2D.Shape as CircleShape2D).Radius = radius;
    cpuParticles2D.EmissionSphereRadius = radius;
    SetEmitting(false);
  }

  public void SetEmitting(bool emitting)
  {
    SetProcess(emitting);
    cpuParticles2D.Emitting = emitting;
  }

  public override void _Process(double delta)
  {
    if (radius < maxRadius && isAscending)
    {
      radius += speed * (float)delta;
      cpuParticles2D.EmissionSphereRadius = radius;
      (collisionShape2D.Shape as CircleShape2D).Radius = radius;
      if (radius >= maxRadius)
      {
        isAscending = false;
      }
    }
    else if (radius >= 1 && !isAscending)
    {
      radius -= speed * (float)delta;
      if (radius < 0)
        radius = 0;
      cpuParticles2D.EmissionSphereRadius = radius;
      (collisionShape2D.Shape as CircleShape2D).Radius = radius;
    }
    else
    {
      SetEmitting(false);
      isAscending = true;
      EventRegistry.GetEventPublisher("ActionFinished").RaiseEvent(new object[] { });
    }
  }

  public override void Action()
  {
    CallDeferred("SetEmitting", true);
  }

  public void OnBodyEntered(Node2D body)
  {
    if (!body.IsInGroup("Enemies")) return;
    var healthbar = body.GetNode<Healthbar>("Healthbar");

    // If the object gets out and gets in again, he gets hit again
    // Save a list of elements entered
    // if element is not on the list, he gets hit and added to the list
    // when laser is over, clear the list
    if (body is Mob mob)
    {
      Vector2 curDirection = facingDirection == 1 ? Vector2.Right : Vector2.Left;
      mob.KnockBack(curDirection, 5);
    }

    if (healthbar.IsAlive)
      EventRegistry.GetEventPublisher("TakeDamage").RaiseEvent(new object[] {
              healthbar,
              abilityResource.Damage
            });
  }

  /* public override void _UnhandledInput(InputEvent @event)
  {
    if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
      Action();
  } */
}
