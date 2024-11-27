using Godot;
public abstract partial class Behavior : Node2D
{
  public abstract void Execute(Mob mob, float delta, int currentPhase = 1);
}
