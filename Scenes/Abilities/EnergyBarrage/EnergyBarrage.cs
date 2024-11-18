using System.Threading;
using System.Threading.Tasks;
using Godot;

public partial class EnergyBarrage : Ability
{
  [Export] public float spawnRate = .1f;
  [Export] PackedScene fireballScene;
  Node2D fireballSpawn;
  public override void _Ready()
  {
    base._Ready();
    fireballSpawn = GetNode<Node2D>("FireballSpawn");
    EventRegistry.RegisterEvent("OnFireballHit");
    EventSubscriber.SubscribeToEvent("OnFireballHit", OnFireballHit);
  }
  public async void SpawnFireballs(CancellationToken token)
  {
    try
    {
      await ToSignal(GetTree().CreateTimer(.2f, false, true), "timeout");
      if (token.IsCancellationRequested) return;  // Handle early cancellation
      for (int i = 0; i < abilityResource.Value; i++)
      {

        if (token.IsCancellationRequested) return;  // Handle early cancellation
        AnimationPlayer.Play(i % 2 == 0 ? "punch" : "punch_2");
        facingDirection = AnimationPlayer.FlipH ? -1 : 1;
        var fireball = fireballScene.Instantiate<Fireball>();
        fireball.targetGroup = targetGroup;
        GetTree().Root.AddChild(fireball);
        fireballSpawn.Position = new Vector2(Position.X * facingDirection, Position.Y);
        fireball.GlobalPosition = fireballSpawn.GlobalPosition;
        fireball.SetFacingDirection(facingDirection);
        await ToSignal(GetTree().CreateTimer(spawnRate, false, true), "timeout");
        audioManager?.Play(abilitySound, this);
        fireball.StartProcess();
      }

      AnimationPlayer.Play("default");
      await ToSignal(GetTree().CreateTimer(abilityResource.Cooldown, false, true), "timeout");
      if (token.IsCancellationRequested) return;  // Handle early cancellation
      EventRegistry.GetEventPublisher("ActionFinished").RaiseEvent(new object[] { this });
    }
    catch (TaskCanceledException)
    {
      cooldownTimer.Free();
    }

  }
  public override void Action()
  {
    cancellationTokenSource = new CancellationTokenSource();
    CancellationToken token = cancellationTokenSource.Token;
    currentTask = Task.Run(() => SpawnFireballs(token), token);
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
        mob.KnockBack(curDirection, 1);
      }
      if (healthbar.IsAlive)
        EventRegistry.GetEventPublisher("TakeDamage").RaiseEvent(new object[] {
              healthbar,
              abilityResource.Damage
            });
      if (args[1] is Fireball fireball) { fireball.Destroy(); }
    }
  }

  public override void Cancel()
  {

    cancellationTokenSource.Cancel();  // Cancel the task
    EventRegistry.GetEventPublisher("ActionCanceled").RaiseEvent(new object[] { this });
    base.Cancel();
  }


  public override void _ExitTree()
  {
    EventSubscriber.UnsubscribeFromEvent("OnFireballHit", OnFireballHit);
  }
}
