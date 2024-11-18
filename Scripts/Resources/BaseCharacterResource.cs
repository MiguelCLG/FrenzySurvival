using Godot;
using Godot.Collections;

[GlobalClass]
public partial class BaseCharacterResource : Resource
{
  [ExportGroup("Character Stats")]
  [Export] public int HP;
  [Export] public int MaxHP;
  [Export] public int Speed;
  [Export] public int Shield;
  [Export] public int MaxShield;
  [Export] public int KI;
  [Export] public int MaxKI;

  [ExportGroup("Mobs Only")]
  [Export] public Array<PackedScene> MobAttacks;
  [Export] public Array<LootTable> LootTables;
  [Export] public bool ShowHealBar = true;
  [Export] public int ExpDropValue = 0;

  [ExportGroup("Image and Sound")]
  [Export] public SpriteFrames AnimatedFrames;
  [Export] public Texture2D Portrait;

  [Export] public Godot.Collections.Dictionary<string, AudioOptionsResource> characterSounds;

}
