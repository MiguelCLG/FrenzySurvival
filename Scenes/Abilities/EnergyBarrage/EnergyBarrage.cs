using Godot;

public partial class EnergyBarrage : Ability
{
  [Export] public float spawnRate = .1f;
  [Export] PackedScene fireballScene;

  public override void _Ready()
  {
    base._Ready();
    EventRegistry.RegisterEvent("OnFireballHit");
    EventSubscriber.SubscribeToEvent("OnFireballHit", OnFireballHit);
  }
  public async void SpawnFireballs()
  {
    for (int i = 0; i < abilityResource.Value; i++)
    {

      AnimationPlayer.Play(i % 2 == 0 ? "punch" : "punch_2");
      facingDirection = AnimationPlayer.FlipH ? -1 : 1;
      var fireball = fireballScene.Instantiate();
      GetTree().Root.AddChild(fireball);
      Node2D fireballSpawn = GetNode<Node2D>("FireballSpawn");
      fireballSpawn.Position = new Vector2(Position.X * facingDirection, Position.Y);
      (fireball as Fireball).GlobalPosition = fireballSpawn.GlobalPosition;
      (fireball as Fireball).SetFacingDirection(facingDirection);
      await ToSignal(GetTree().CreateTimer(spawnRate, false, true), "timeout");
      audioManager?.Play(abilitySound, this);
      (fireball as Fireball).StartProcess();
    }
    
    AnimationPlayer.Play("default");
    await ToSignal(GetTree().CreateTimer(abilityResource.Cooldown, false, true), "timeout");
    EventRegistry.GetEventPublisher("ActionFinished").RaiseEvent(new object[] { this });

  }
  public override void Action()
  {
    CallDeferred("SpawnFireballs");  // Perform the cone detection
  }

  public void OnFireballHit(object sender, object[] args)
  {
    if (args[0] is Node2D body)
    {
      if (!body.IsInGroup(targetGroup)) return;
      var healthbar = body.GetNode<Healthbar>("Healthbar");
      if (!healthbar.IsAlive) return;
      if (body is Mob mob)
      {
        Vector2 curDirection = facingDirection == 1 ? Vector2.Right : Vector2.Left;
        mob.KnockBack(curDirection, 10);
      }
      if (healthbar.IsAlive)
        EventRegistry.GetEventPublisher("TakeDamage").RaiseEvent(new object[] {
              healthbar,
              abilityResource.Damage
            });
    }
    if (args[1] is Fireball fireball) { fireball.Destroy(); }
  }


  public override void _ExitTree()
  {
    EventSubscriber.UnsubscribeFromEvent("OnFireballHit", OnFireballHit);
    EventRegistry.UnregisterEvent("OnFireballHit");
  }
}
