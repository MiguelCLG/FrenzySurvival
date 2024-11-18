using Godot;
using Godot.Collections;
using System;

public partial class MobBoss : Mob
{
  [Export] int bossPhases = 2;

  Dictionary<Ability, float> abilityProbabilityDictionary = new();
  int currentPhase = 1;
  public override void _Ready()
  {
    base._Ready();
    foreach (var mobAbility in mobResource.MobAttacks)
    {
      var probablitiy = (float)GD.Randf();
      abilityProbabilityDictionary.Add(new(), probablitiy);
    }
  }

  public override void _Process(double delta)
  {
    GD.Print("Process Boss");
  }
}
