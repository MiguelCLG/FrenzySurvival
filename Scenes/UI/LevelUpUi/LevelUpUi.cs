using Godot;
using System;

public partial class LevelUpUi : CanvasLayer
{
  [Export] PackedScene abilityRow;
  public override void _Ready()
  {
    var verticalContainer = GetNode<Control>("%VerticalContainer");
    for (int i = 0; i < 2; i++)
    {
      Control row = abilityRow.Instantiate<Control>();
      verticalContainer.AddChild(row);
      foreach (AbilityUiButton abilityUiButton in row.GetChildren())
      {
        abilityUiButton.SetInitialValues(new Texture2D(), "", "", "", "", ""); //TODO: set values from dictionary
      }
    }
  }
}
