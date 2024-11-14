using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class Ability : Node2D
{
  [Export] public AbilityResource abilityResource;
  [Export] protected AudioOptionsResource abilitySound;
  protected AudioManager audioManager;
  [Export] public string targetGroup = "Enemies";

  public Vector2 CurrentVelocity = Vector2.Zero;

  public AnimatedSprite2D AnimationPlayer;

  private protected SceneTreeTimer cooldownTimer;

  public bool isCanceled = false;

  public Task currentTask;
  public int facingDirection = 1;
  public CancellationTokenSource cancellationTokenSource;

  public override void _Ready()
  {
    audioManager = GetNode<AudioManager>("/root/AudioManager");
  }
  public void SetFacingDirection(int direction)
  {
    facingDirection = direction;
  }

  public virtual void SetTargetGroup(string group)
  {
    targetGroup = group;
  }

  public virtual void Action()
  {
  }

  public virtual void Cancel() { }
  public virtual void SpendKi(int value)
  {
    EventRegistry.GetEventPublisher("IncreaseStatsFromDictionary")
      .RaiseEvent(new object[] { new Dictionary<string, int> { { "ki", -abilityResource.kiRequired } }, this });
  }
}
