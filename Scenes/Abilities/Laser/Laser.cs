using Godot;
using System;
using System.Linq;

public partial class Laser : RayCast2D
{
  [Export] Godot.Environment laserEnvironment;
  [Export] Gradient laserGradient;
  [Export] Vector2 maxPosition = new(500, 0);
  WorldEnvironment worldEnvironment;
  bool IsCasting { get; set; }
  Vector2 collisionPoint;
  Vector2 initialPos;
  [Export] float beamSpeed = 2000f;
  [Export] float beamWidth = 50f;
  [Export] int direction = 1;
  Line2D beamLine;
  Area2D area;
  EnergyBall energyBall;
  GpuParticles2D castingParticlesBegin;
  GpuParticles2D collisionParticles;
  GpuParticles2D beamParticles;
  Vector2[] beamLineOriginalPoints;
  public string targetGroup = "Enemies";
  public override void _Ready()
  {
    TargetPosition = maxPosition;
    SetPhysicsProcess(false);
    beamLine = GetNode<Line2D>("%BeamLine");
    area = GetNode<Area2D>("%CollisionArea");
    area.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
    castingParticlesBegin = GetNode<GpuParticles2D>("%CastingParticlesBegin");
    collisionParticles = GetNode<GpuParticles2D>("%CollisionParticles");
    beamParticles = GetNode<GpuParticles2D>("%BeamParticles");
    worldEnvironment = GetNode<WorldEnvironment>("LaserEnvironment");
    energyBall = GetNode<EnergyBall>("EnergyBall");
    beamLine.Width = 0;
    collisionPoint = beamLine.Points[3];
    initialPos = Position;
    worldEnvironment.Environment = null;
    beamLineOriginalPoints = beamLine.Points;

    castingParticlesBegin.Lifetime = beamWidth / 100f > .3f ? .3f : beamWidth / 100f;
    collisionParticles.Lifetime = beamWidth / 100f > .3f ? .3f : beamWidth / 100f;

    ParticleProcessMaterial castingParticlesBeginMaterial = (ParticleProcessMaterial)castingParticlesBegin.ProcessMaterial;
    castingParticlesBeginMaterial.ColorRamp.Set("gradient", laserGradient);
    castingParticlesBeginMaterial.EmissionSphereRadius = beamWidth / 2;
    castingParticlesBegin.ProcessMaterial = castingParticlesBeginMaterial;

    ParticleProcessMaterial beamParticlesMaterial = (ParticleProcessMaterial)beamParticles.ProcessMaterial;
    beamParticlesMaterial.ColorRamp.Set("gradient", laserGradient);
    beamParticles.ProcessMaterial = beamParticlesMaterial;

    ParticleProcessMaterial collisionParticlesMaterial = (ParticleProcessMaterial)collisionParticles.ProcessMaterial;
    collisionParticlesMaterial.ColorRamp.Set("gradient", laserGradient);
    collisionParticlesMaterial.EmissionSphereRadius = beamWidth / 2;

    collisionParticles.ProcessMaterial = collisionParticlesMaterial;
    beamLine.Gradient = laserGradient;
    energyBall.GetNode<CpuParticles2D>("CPUParticles2D").ColorRamp = laserGradient;

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
      collisionPoint *= (float)delta * beamSpeed;
    collisionParticles.GlobalRotation = collisionPoint.AngleTo(Vector2.Left * direction);
    castingParticlesBegin.GlobalRotation = collisionPoint.AngleTo(Vector2.Right * direction);
    collisionParticles.Position = collisionPoint * direction;
    if (IsColliding())
    {
      //if (MathF.Abs(collisionPoint.X) < GetCollisionPoint().X * direction)
      //  collisionPoint += Vector2.Right * (float)delta * beamSpeed;
      //collisionParticles.GlobalRotation = GetCollisionNormal().Angle();
      //collisionParticles.Position = collisionPoint;
    }
    Position = new Vector2(initialPos.X * direction, initialPos.Y);

    beamLine.ClearPoints();
    beamLine.AddPoint(beamLineOriginalPoints[0]);
    beamLine.AddPoint(TargetPosition * direction / 4);
    beamLine.AddPoint(TargetPosition * direction - TargetPosition * direction / 4);
    beamLine.AddPoint(TargetPosition * direction);

    beamParticles.Position = collisionPoint * direction * .5f;

    ParticleProcessMaterial beamParticlesMaterial = (ParticleProcessMaterial)beamParticles.ProcessMaterial;
    beamParticlesMaterial.EmissionBoxExtents = new(collisionPoint.X * direction * .5f, beamWidth / 3, beamParticlesMaterial.EmissionBoxExtents.Z);

    beamParticles.ProcessMaterial = beamParticlesMaterial;

  }

  public void SetIsCasting(bool isCasting)
  {
    IsCasting = isCasting;
    beamParticles.Emitting = IsCasting;
    castingParticlesBegin.Emitting = IsCasting;
    SetPhysicsProcess(IsCasting);

    if (IsCasting)
      AppearBeam();
    else
    {
      collisionParticles.Emitting = false;
      DisappearBeam();
    }

  }

  public override void _UnhandledInput(InputEvent @event)
  {
    if (@event is InputEventMouseButton)
      SetIsCasting(@event.IsPressed());
  }

  public void AppearBeam()
  {
    // Get the CollisionShape2D and enable it
    area.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;

    // Create a curve for the width animation
    Tween tween;
    Curve curve = new();
    curve.AddPoint(new Vector2(0, 0.50f));
    curve.AddPoint(new Vector2(0.25f, 0.25f));
    curve.AddPoint(new Vector2(0.75f, 0.25f));
    curve.AddPoint(new Vector2(1, 0.75f));

    tween = CreateTween();
    tween.Stop();
    tween.SetParallel(true);

    // Tween the beam's width - This is the line width, we might  just want the particles themselves to be visible
    /*   tween.Parallel().TweenProperty(beamLine, "width", beamWidth, 0.2);
      tween.Parallel().TweenProperty(beamLine, "width_curve", curve, 0.2); */

    // Convert position.x to scale.x based on the initial width
    float scaleX = TargetPosition.X * direction * .045f;
    float scaleY = beamWidth * .04f;

    // Tween the area's scale (will now grow from the left)
    tween.Parallel().TweenProperty(area, "scale", new Vector2(scaleX, scaleY), 0.2);
    tween.Parallel().TweenProperty(area, "position", area.Position + new Vector2(TargetPosition.X * direction * .5f, 0), 0.2);

    // Play the tween
    tween.Play();
    collisionPoint = TargetPosition;
  }


  public async void DisappearBeam()
  {
    Tween tween;
    tween = CreateTween();
    tween.Stop();
    tween.SetParallel(true);
    tween.Parallel().TweenProperty(beamLine, "width", 0, 0.2);
    tween.Parallel().TweenProperty(area, "scale", new Vector2(0, area.Scale.Y), 0.2);
    tween.Parallel().TweenProperty(area, "position", new Vector2(0, 0), 0.2);
    tween.Play();
    await ToSignal(tween, "finished");
    collisionPoint = beamLine.Points[3];
    area.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;

  }

  public void OnAreaEntered(Node2D bodyEntered)
  {
    if (bodyEntered.IsInGroup(targetGroup))
    {
      EventRegistry.GetEventPublisher("KamehameHit").RaiseEvent(new object[] { bodyEntered });
    }
  }

}
