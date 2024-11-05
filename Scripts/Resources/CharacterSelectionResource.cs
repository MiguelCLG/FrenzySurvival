using Godot;

[GlobalClass]
public partial class CharacterSelectionResource : Resource
{
  [Export]
  public string Name;
  [Export]
  public Texture2D Portrait;
  [Export]
  public AbilityDatabaseResource Abilities;
  [Export]
  public PlayerResource CharacterResource;

}
