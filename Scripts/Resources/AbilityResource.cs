using Godot;

[GlobalClass]
public partial class AbilityResource : Resource
{
  [Export]
  public string Name { get; set; }

  [Export]
  public int Cooldown { get; set; }
  [Export]
  public float CastTime { get; set; }

  [Export]
  public int Value { get; set; }
  [Export]
  public int Damage { get; set; }
  [Export]
  public Texture2D Icon { get; set; }
  [Export]
  public bool isSuperAbility { get; set; }
  [Export]
  public int kiRequired { get; set; }

}
