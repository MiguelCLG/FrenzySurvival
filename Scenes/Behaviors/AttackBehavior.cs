using Godot;

public partial class AttackBehavior : Behavior
{
  public override void Execute(Mob mob, float delta, int currentPhase)
  {
    mob.abilityManager.DoNextActionAsync();
  }
}
