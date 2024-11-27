using Godot;
using Godot.Collections;

[GlobalClass]
public partial class AbilityResource : Resource
{
  [Export]
  public string Name { get; set; }
  [Export]
  public Array<string> AnimationNames { get; set; }

  [Export]
  public Texture2D Icon { get; set; }

  [Export]
  public float Cooldown { get; set; }
  [Export]
  public float CastTime { get; set; }

  [Export]
  public float Value { get; set; }
  [Export]
  public float Damage { get; set; }
  [Export]
  public float kiRequired { get; set; }
  [Export]
  public bool isSuperAbility { get; set; }
  [Export]
  public bool isRangedAbility { get; set; } = false;
  [Export]
  public Vector2 RangeRequired { get; set; } = Vector2.Zero;
}
