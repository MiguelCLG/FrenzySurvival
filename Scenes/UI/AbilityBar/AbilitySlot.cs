using Godot;
using System;

public partial class AbilitySlot : PanelContainer
{
  public void SpawnUIIcon(Texture2D texture)
  {
    TextureRect icon = new();
    icon.Texture = texture;
    icon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
    icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
    icon.CustomMinimumSize = new Vector2(50, 50);
    GetNode<MarginContainer>("MarginContainer").AddChild(icon);
  }
}
