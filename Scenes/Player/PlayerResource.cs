using Godot;
using Godot.Collections;

[GlobalClass]
public partial class PlayerResource : BaseCharacterResource
{
  [Export] public RaceResource Race;
  [Export] public LevelTableResource LevelUpTables;
  [Export] public AbilityDatabaseResource Abilities;


}
