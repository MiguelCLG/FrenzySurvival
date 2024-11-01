using Godot;
using System;
using System.Collections.Generic;

public partial class EnergyBarrier : Ability
{
  [Export] public float radius = 1;
  [Export] public float maxRadius = 100;
  [Export] public CpuParticles2D cpuParticles2D;
  [Export] public CollisionShape2D collisionShape2D;
  [Export] public float speed = 50f;
  List<Mob> mobsInArea = new();
  bool isAscending = true;
  public override void _Ready()
  {
    cpuParticles2D = GetNode<CpuParticles2D>("%CPUParticles2D");
    collisionShape2D = GetNode<CollisionShape2D>("%CollisionShape2D");
    (collisionShape2D.Shape as CircleShape2D).Radius = radius;
    cpuParticles2D.EmissionSphereRadius = radius;
    GetTree().CreateTimer(.1f, false, true).Timeout += () => { SetEmitting(false); };
    EventSubscriber.SubscribeToEvent("OnMobDeath", OnMobDeath);
  }

  public void SetEmitting(bool emitting)
  {
    SetProcess(emitting);
    cpuParticles2D.Emitting = emitting;
    AnimationPlayer.Play(emitting ? "explosion" : "default");
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
      GetNode<CpuParticles2D>("%Explosion/CPUParticles2D").Emitting = true;
      foreach (Mob mob in mobsInArea)
      {
        if (IsInstanceValid(mob) && !mob.IsQueuedForDeletion())
        {
          mob.TakeDamage(this, new object[] { this, abilityResource.Damage });
          mob.KnockBack(-mob.Velocity, abilityResource.Value);
        }
        SetEmitting(false);
        isAscending = true;
        radius = 1;
        (collisionShape2D.Shape as CircleShape2D).Radius = radius;


      }
      EventRegistry.GetEventPublisher("ActionFinished").RaiseEvent(new object[] { });
    }

  }

  public override void Action()
  {
    CallDeferred("SetEmitting", true);
  }

  public void OnMobDeath(object sender, object[] args)
  {
    if (args[0] is Mob mob)
    {
      mobsInArea.Remove(mob);
    }
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
      mobsInArea.Add(mob);
      Vector2 curDirection = facingDirection == 1 ? Vector2.Right : Vector2.Left;
      mob.KnockBack(curDirection, 5);
    }

    if (healthbar.IsAlive)
      EventRegistry.GetEventPublisher("TakeDamage").RaiseEvent(new object[] {
              healthbar,
              abilityResource.Damage
            });
  }
  public void OnBodyExited(Node2D body)
  {
    if (!body.IsInGroup("Enemies")) return;

    if (body is Mob mob)
    {
      mobsInArea.Remove(mob);

    }
  }

  /* public override void _UnhandledInput(InputEvent @event)
  {
    if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
      Action();
  } */
}
