using Godot;
using Godot.Collections;

[GlobalClass]
public partial class BaseCharacterResource : Resource
{
  [Export] public int HP;
  [Export] public int MaxHP;
  [Export] public int Speed;
  [Export] public int Shield;
  [Export] public int MaxShield;
  [Export] public int KI;
  [Export] public int MaxKI;
  [Export] public SpriteFrames AnimatedFrames;
  [Export] public bool ShowHealBar = true;
  [Export] public Texture2D Portrait;
  [Export] public Array<AbilityResource> Abilities;
  [Export] public Array<LootTable> LootTables;

  [Export] public int ExpDropValue = 0;

}
