using Godot;
using System;

public partial class EnergyBarrier : Node2D
{
  [Export] public float radius = 0;
  [Export] public float maxRadius = 100;
  [Export] public CpuParticles2D cpuParticles2D;
  [Export] public CollisionShape2D collisionShape2D;
  [Export] public float speed = 50f;
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
    if (radius < maxRadius)
    {
      radius += speed * (float)delta;
      cpuParticles2D.EmissionSphereRadius = radius;
      (collisionShape2D.Shape as CircleShape2D).Radius = radius;
    }
    else
    {
      GetTree().CreateTimer(2f, false, true).Timeout += () => QueueFree();
    }
  }
}
