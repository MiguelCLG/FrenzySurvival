using Godot;

[GlobalClass]
public partial class RaceResource : Resource
{
  [Export]
  public string Name { get; set; }

  [Export]
  public string Description { get; set; } = string.Empty;

  [Export]
  public Texture2D Icon { get; set; }

}
