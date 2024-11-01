using Godot;
using System;

public partial class Laser : RayCast2D
{
  [Export] Godot.Environment laserEnvironment;
  WorldEnvironment worldEnvironment;
  bool IsCasting { get; set; }
  Vector2 maxPosition = new(500, 0);
  Vector2 collisionPoint;
  Vector2 initialPos;
  [Export] float beamSpeed = 2000f;
  [Export] float beamWidth = 50f;
  [Export] int direction = 1;
  Line2D beamLine;
  Area2D area;
  GpuParticles2D castingParticlesBegin;
  GpuParticles2D collisionParticles;
  GpuParticles2D beamParticles;
  public override void _Ready()
  {
    SetPhysicsProcess(false);
    beamLine = GetNode<Line2D>("%BeamLine");
    area = GetNode<Area2D>("%CollisionArea");
    area.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
    castingParticlesBegin = GetNode<GpuParticles2D>("%CastingParticlesBegin");
    collisionParticles = GetNode<GpuParticles2D>("%CollisionParticles");
    beamParticles = GetNode<GpuParticles2D>("%BeamParticles");
    worldEnvironment = GetNode<WorldEnvironment>("LaserEnvironment");
    beamLine.Width = 0;
    collisionPoint = beamLine.Points[1];
    initialPos = Position;
    worldEnvironment.Environment = null;

  }

  public void SetDirection(int newValue)
  {
    direction = newValue;
  }

  public override void _PhysicsProcess(double delta)
  {
    ForceRaycastUpdate();

    collisionParticles.Emitting = true;

    if (MathF.Abs(collisionPoint.X) < maxPosition.X)
      collisionPoint += Vector2.Right * direction * (float)delta * beamSpeed;
    collisionParticles.GlobalRotation = collisionPoint.AngleTo(Vector2.Left * direction);
    collisionParticles.Position = collisionPoint;
    if (IsColliding())
    {
      //if (MathF.Abs(collisionPoint.X) < GetCollisionPoint().X * direction)
      //  collisionPoint += Vector2.Right * (float)delta * beamSpeed;
      //collisionParticles.GlobalRotation = GetCollisionNormal().Angle();
      //collisionParticles.Position = collisionPoint;
    }
    Position = new Vector2(initialPos.X * direction, initialPos.Y);
    for (int i = 0; i < beamLine.Points.Length; i++)
      beamLine.RemovePoint(i);
    
    beamLine.AddPoint(Vector2.Zero);
    beamLine.AddPoint(collisionPoint / 4);
    beamLine.AddPoint(collisionPoint - collisionPoint / 4);
    beamLine.AddPoint(collisionPoint);
    beamParticles.Position = collisionPoint * .5f;

    ParticleProcessMaterial material = (ParticleProcessMaterial)beamParticles.ProcessMaterial;
    material.EmissionBoxExtents = new(collisionPoint.X * .5f, material.EmissionBoxExtents.Y, material.EmissionBoxExtents.Z);

    beamParticles.ProcessMaterial = material;
  }

  public void SetIsCasting(bool isCasting)
  {
    IsCasting = isCasting;
    beamParticles.Emitting = IsCasting;
    castingParticlesBegin.Emitting = IsCasting;

    if (IsCasting)
      AppearBeam();
    else
    {
      collisionParticles.Emitting = false;
      DisappearBeam();
    }
    SetPhysicsProcess(IsCasting);

  }

  public override void _UnhandledInput(InputEvent @event)
  {
    if (@event is InputEventMouseButton)
      SetIsCasting(@event.IsPressed());
  }

  public void AppearBeam()
  {
    area.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
    Tween tween;
    Curve curve = new();
    curve.AddPoint(new Vector2(0, 0.75f));
    curve.AddPoint(new Vector2(0.25f, 0.25f));
    curve.AddPoint(new Vector2(0.75f, 0.25f));
    curve.AddPoint(new Vector2(1, 0.75f));
    tween = CreateTween();
    tween.Stop();
    tween.SetParallel(true);
    tween.Parallel().TweenProperty(beamLine, "width", beamWidth, 0.2);
    tween.Parallel().TweenProperty(beamLine, "width_curve", curve, 0.2);
    tween.Parallel().TweenProperty(area, "scale", new Vector2(beamWidth * direction, area.Scale.Y), 0.2);
    tween.Play();
  }

  public async void DisappearBeam()
  {
    Tween tween;
    tween = CreateTween();
    tween.Stop();
    tween.SetParallel(true);
    tween.Parallel().TweenProperty(beamLine, "width", 0, 0.2);
    tween.Parallel().TweenProperty(area, "scale", new Vector2(0, area.Scale.Y), 0.2);
    collisionPoint = Vector2.Zero;
    tween.Play();
    await ToSignal(tween, "finished");
    area.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;

  }

  public void OnAreaEntered(Node2D bodyEntered)
  {
    if (bodyEntered.IsInGroup("Enemies"))
    {
      EventRegistry.GetEventPublisher("KamehameHit").RaiseEvent(new object[] { bodyEntered });
    }
  }

}
